using Pedidos.Domain;

namespace Pedidos.Application;

public interface IPedidoRepository
{
    Task SalvarAsync(Pedido pedido);
}