using WebApplication1.Infrastructure.Persistence;
using WebApplication1.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<Banco>();
builder.Services.AddScoped<ProdutoRepository>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
	var repository = scope.ServiceProvider.GetRequiredService<ProdutoRepository>();
	await repository.CriarTabelaAsync();
	app.Logger.LogInformation("SQLite conectado e tabela Products preparada com sucesso");
}

app.MapGet("/", () => "SQLite conectado com sucesso!");

app.Run();