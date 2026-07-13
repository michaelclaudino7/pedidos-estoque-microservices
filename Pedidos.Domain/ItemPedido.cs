namespace Pedidos.Domain;

public class ItemPedido
{
    public string NomeProduto { get; }
    public int Quantidade { get; }
    public decimal PrecoUnitario { get; }

    private ItemPedido() { }

    public ItemPedido(string nomeProduto, int quantidade, decimal precoUnitario)
    {
        if (string.IsNullOrWhiteSpace(nomeProduto))
            throw new ArgumentException("Nome do produto e obrigatorio.");

        if (quantidade <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero.");

        if (precoUnitario < 0)
            throw new ArgumentException("Preco unitario nao pode ser negativo.");

        NomeProduto = nomeProduto;
        Quantidade = quantidade;
        PrecoUnitario = precoUnitario;
    }
}
