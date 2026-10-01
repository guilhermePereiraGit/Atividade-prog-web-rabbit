using WebApplication1.Domain.Messages;

namespace WebApplication1.Domain.Results;

public sealed record ResultadoValidacaoCompra(
    bool Coerente,
    string Codigo,
    string Mensagem,
    CompraRecebida Compra,
    decimal? ValorUnitarioEsperado = null,
    decimal? ValorTotalEsperado = null);