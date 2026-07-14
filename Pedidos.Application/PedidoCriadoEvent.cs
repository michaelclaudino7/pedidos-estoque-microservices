namespace Pedidos.Application;

public record PedidoCriadoEvent(
    Guid PedidoId,
    decimal ValorTotal,
    DateTime DataCriacao,
    List<ItemPedidoEvent> Itens
);

public record ItemPedidoEvent(string NomeProduto, int Quantidade);