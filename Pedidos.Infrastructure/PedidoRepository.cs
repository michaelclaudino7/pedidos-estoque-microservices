using Pedidos.Application;
using Pedidos.Domain;

namespace Pedidos.Infrastructure;

public class PedidoRepository : IPedidoRepository
{
    private readonly PedidosDbContext _context;

    public PedidoRepository(PedidosDbContext context)
    {
        _context = context;
    }

    public async Task SalvarAsync(Pedido pedido)
    {
        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();
    }
}