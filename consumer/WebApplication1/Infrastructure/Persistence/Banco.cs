using Microsoft.Data.Sqlite;

namespace WebApplication1.Infrastructure.Persistence;

public sealed class Banco(IConfiguration configuration)
{
    private readonly string connectionString =
        configuration.GetConnectionString("Sqlite")
        ?? throw new InvalidOperationException("A conexão 'Sqlite' não foi configurada.");

    public async Task<SqliteConnection> OpenConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}