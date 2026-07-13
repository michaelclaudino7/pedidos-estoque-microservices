namespace Pedidos.Domain;

public class Pedido
{
    public Guid Id { get; }
    public IReadOnlyCollection<ItemPedido> Itens { get; }
    public decimal ValorTotal { get; }

    private Pedido() { }

    public Pedido(List<ItemPedido> itens)
    {
        if (itens == null || itens.Count == 0)
            throw new ArgumentException("Pedido deve ter pelo menos um item.");

        Id = Guid.NewGuid();
        Itens = itens.AsReadOnly();
        ValorTotal = itens.Sum(item => item.Quantidade * item.PrecoUnitario);
    }
}