using System.Text.Json.Serialization;

namespace WebApplication1.Domain.Messages;

public sealed record CompraRecebida(
    [property: JsonPropertyName("nomeProduto")] string NomeProduto,
    [property: JsonPropertyName("quantidade")] int Quantidade,
    [property: JsonPropertyName("valorUnitario")] decimal ValorUnitario);