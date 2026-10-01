using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using WebApplication1.Application.Services;
using WebApplication1.Domain.Messages;
using WebApplication1.Domain.Results;

namespace WebApplication1.Infrastructure.Messaging;

public sealed class RabbitMqConsumer(
    IOptions<RabbitMqOptions> options,
    IServiceScopeFactory scopeFactory,
    ILogger<RabbitMqConsumer> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly RabbitMqOptions rabbitMq = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = rabbitMq.HostName,
            Port = rabbitMq.Port,
            UserName = rabbitMq.UserName,
            Password = rabbitMq.Password,
            VirtualHost = rabbitMq.VirtualHost,
            AutomaticRecoveryEnabled = true
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConsumirAsync(factory, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Falha na conexão com RabbitMQ; nova tentativa em 5 segundos");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task ConsumirAsync(ConnectionFactory factory, CancellationToken cancellationToken)
    {
        await using var connection = await factory.CreateConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await DeclararFilaAsync(channel, rabbitMq.QueueName, cancellationToken);
        await DeclararFilaAsync(channel, rabbitMq.AlertQueueName, cancellationToken);
        await channel.BasicQosAsync(0, 1, false, cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (_, eventArgs) =>
            ProcessarAsync(channel, eventArgs, cancellationToken);

        await channel.BasicConsumeAsync(
            rabbitMq.QueueName,
            autoAck: false,
            consumer,
            cancellationToken);

        logger.LogInformation(
            "Consumindo {InputQueue}; avisos serão publicados em {AlertQueue}",
            rabbitMq.QueueName,
            rabbitMq.AlertQueueName);

        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
    }

    private static Task DeclararFilaAsync(
        IChannel channel,
        string queueName,
        CancellationToken cancellationToken) =>
        channel.QueueDeclareAsync(
            queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

    private async Task ProcessarAsync(
        IChannel channel,
        BasicDeliverEventArgs eventArgs,
        CancellationToken cancellationToken)
    {
        try
        {
            var compra = JsonSerializer.Deserialize<CompraRecebida>(eventArgs.Body.Span, JsonOptions)
                ?? throw new JsonException("A mensagem está vazia.");

            await using var scope = scopeFactory.CreateAsyncScope();
            var validationService = scope.ServiceProvider.GetRequiredService<CompraValidationService>();
            var resultado = await validationService.ValidarAsync(compra, cancellationToken);

            if (resultado.Coerente)
            {
                logger.LogInformation("{Message}", resultado.Mensagem);
            }
            else
            {
                await PublicarAvisoAsync(channel, resultado, cancellationToken);
                logger.LogWarning("{Code}: {Message}", resultado.Codigo, resultado.Mensagem);
            }

            await channel.BasicAckAsync(eventArgs.DeliveryTag, false, cancellationToken);
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "Mensagem inválida descartada");
            await channel.BasicNackAsync(eventArgs.DeliveryTag, false, false, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Falha ao processar mensagem; ela retornará à fila");
            await channel.BasicNackAsync(eventArgs.DeliveryTag, false, true, cancellationToken);
        }
    }

    private async Task PublicarAvisoAsync(
        IChannel channel,
        ResultadoValidacaoCompra resultado,
        CancellationToken cancellationToken)
    {
        var body = JsonSerializer.SerializeToUtf8Bytes(resultado, JsonOptions);
        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: rabbitMq.AlertQueueName,
            body,
            cancellationToken);
    }
}