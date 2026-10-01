using WebApplication1.Domain.Entities;

namespace WebApplication1.Infrastructure.Persistence.Repositories;

public sealed class ProdutoRepository(Banco banco)
{
    public async Task CriarTabelaAsync()
    {
        await using var connection = await banco.OpenConnectionAsync();
        await using var command = connection.CreateCommand();

        command.CommandText = """
            CREATE TABLE IF NOT EXISTS Products (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nome TEXT NOT NULL COLLATE NOCASE,
                Preco NUMERIC NOT NULL
            );

            CREATE UNIQUE INDEX IF NOT EXISTS IX_Products_Nome
            ON Products (Nome);
            """;

        await command.ExecuteNonQueryAsync();
    }

    public async Task InserirProdutosIniciaisAsync()
    {
        await using var connection = await banco.OpenConnectionAsync();
        await using var command = connection.CreateCommand();

        command.CommandText = """
            INSERT OR IGNORE INTO Products (Nome, Preco) VALUES
                ('Monitor', 250.00),
                ('Teclado', 100.00),
                ('Mouse', 50.00),
                ('GoPro', 300.00),
                ('Fone', 150.00);
            """;

        await command.ExecuteNonQueryAsync();
    }

    public async Task<Produto?> BuscarPorNomeAsync(string nome)
    {
        await using var connection = await banco.OpenConnectionAsync();
        await using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT Id, Nome, Preco
            FROM Products
            WHERE Nome = $nome;
            """;
        command.Parameters.AddWithValue("$nome", nome);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new Produto(
            reader.GetInt64(0),
            reader.GetString(1),
            reader.GetDecimal(2));
    }
}