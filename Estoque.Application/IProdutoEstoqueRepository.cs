using Estoque.Domain;

namespace Estoque.Application;

public interface IProdutoEstoqueRepository
{
    Task<ProdutoEstoque?> ObterPorNomeAsync(string nomeProduto);
    Task AtualizarAsync(ProdutoEstoque produto);
}
