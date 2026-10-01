namespace WebApplication1.Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string HostName { get; init; } = "localhost";
    public int Port { get; init; } = 5672;
    public string UserName { get; init; } = "admin";
    public string Password { get; init; } = "admin";
    public string VirtualHost { get; init; } = "/";
    public string QueueName { get; init; } = "produtos.recebidos";
    public string AlertQueueName { get; init; } = "produtos.avisos";
}