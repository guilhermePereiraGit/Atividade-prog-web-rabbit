using WebApplication1.Application.Services;
using WebApplication1.Infrastructure.Messaging;
using WebApplication1.Infrastructure.Persistence;
using WebApplication1.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RabbitMqOptions>(
	builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.AddSingleton<Banco>();
builder.Services.AddScoped<ProdutoRepository>();
builder.Services.AddScoped<CompraValidationService>();
builder.Services.AddHostedService<RabbitMqConsumer>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
	var repository = scope.ServiceProvider.GetRequiredService<ProdutoRepository>();
	await repository.CriarTabelaAsync();
	await repository.InserirProdutosIniciaisAsync();
	app.Logger.LogInformation("SQLite conectado e tabela Products preparada com sucesso");
}

app.MapGet("/", () => Results.Ok(new
{
	servico = "Validador de compras",
	filaEntrada = builder.Configuration["RabbitMq:QueueName"],
	filaAvisos = builder.Configuration["RabbitMq:AlertQueueName"]
}));

app.Run();