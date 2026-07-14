using Pedidos.Domain;

namespace Pedidos.Application;

public class CriarPedidoUseCase
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IEventPublisher _eventPublisher;

    public CriarPedidoUseCase(IPedidoRepository pedidoRepository, IEventPublisher eventPublisher)
    {
        _pedidoRepository = pedidoRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<Pedido> Executar(List<ItemPedido> itens)
    {
        var pedido = new Pedido(itens);
        await _pedidoRepository.SalvarAsync(pedido);

        var itensEvento = pedido.Itens
            .Select(i => new ItemPedidoEvent(i.NomeProduto, i.Quantidade))
            .ToList();

        var evento = new PedidoCriadoEvent(pedido.Id, pedido.ValorTotal, DateTime.UtcNow, itensEvento);
        await _eventPublisher.PublicarAsync(evento);

        return pedido;
    }
}