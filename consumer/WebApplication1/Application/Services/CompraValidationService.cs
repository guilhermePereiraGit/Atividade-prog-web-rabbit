using WebApplication1.Domain.Messages;
using WebApplication1.Domain.Results;
using WebApplication1.Infrastructure.Persistence.Repositories;

namespace WebApplication1.Application.Services;

public sealed class CompraValidationService(ProdutoRepository repository)
{
    public async Task<ResultadoValidacaoCompra> ValidarAsync(
        CompraRecebida compra,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(compra.NomeProduto))
        {
            return new(false, "PRODUTO_INVALIDO", "O nome do produto é obrigatório.", compra);
        }

        if (compra.Quantidade <= 0)
        {
            return new(false, "QUANTIDADE_INVALIDA", "A quantidade deve ser maior que zero.", compra);
        }

        if (compra.ValorUnitario <= 0)
        {
            return new(false, "VALOR_INVALIDO", "O valor unitário deve ser maior que zero.", compra);
        }

        var produto = await repository.BuscarPorNomeAsync(compra.NomeProduto.Trim());

        if (produto is null)
        {
            return new(
                false,
                "PRODUTO_NAO_CADASTRADO",
                $"O produto '{compra.NomeProduto}' não existe no catálogo.",
                compra);
        }

        var valorTotalEsperado = produto.PrecoUnitario * compra.Quantidade;
        if (compra.ValorUnitario != produto.PrecoUnitario)
        {
            var valorTotalRecebido = compra.ValorUnitario * compra.Quantidade;
            return new(
                false,
                "VALOR_DIVERGENTE",
                $"Valor incoerente para '{produto.Nome}': recebido {compra.Quantidade} x " +
                $"R$ {compra.ValorUnitario:F2} = R$ {valorTotalRecebido:F2}; esperado " +
                $"{compra.Quantidade} x R$ {produto.PrecoUnitario:F2} = R$ {valorTotalEsperado:F2}.",
                compra,
                produto.PrecoUnitario,
                valorTotalEsperado);
        }

        return new(
            true,
            "COMPRA_COHERENTE",
            $"Compra de '{produto.Nome}' validada com sucesso.",
            compra,
            produto.PrecoUnitario,
            valorTotalEsperado);
    }
}