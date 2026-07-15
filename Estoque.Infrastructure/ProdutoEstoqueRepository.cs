using Estoque.Application;
using Estoque.Domain;
using Microsoft.EntityFrameworkCore;

namespace Estoque.Infrastructure;

public class ProdutoEstoqueRepository : IProdutoEstoqueRepository
{
    private readonly EstoqueDbContext _context;

    public ProdutoEstoqueRepository(EstoqueDbContext context)
    {
        _context = context;
    }

    public async Task<ProdutoEstoque?> ObterPorNomeAsync(string nomeProduto)
    {
        return await _context.Produtos
            .FirstOrDefaultAsync(p => p.NomeProduto == nomeProduto);
    }

    public async Task AtualizarAsync(ProdutoEstoque produto)
    {
        _context.Produtos.Update(produto);
        await _context.SaveChangesAsync();
    }
}
