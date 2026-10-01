namespace WebApplication1.Infrastructure.Persistence.Repositories;

public sealed class ProdutoRepository(Banco banco)
{
    public async Task CriarTabelaAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await banco.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();

        command.CommandText = """
            CREATE TABLE IF NOT EXISTS Products (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nome TEXT NOT NULL,
                Preco NUMERIC NOT NULL
            );
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task InserirProdutosIniciaisAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await banco.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();

        command.CommandText = """
            INSERT INTO Products (Nome, Preco)
            VALUES ('Monitor', 250.00),
                   ('Teclado', 100.00),
                   ('Mouse', 50.00),
                   ('GoPro', 300.00),
                   ('Fone', 150.00);
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}