namespace Estoque.Domain;

public class ProdutoEstoque
{
    public Guid Id { get; }
    public string NomeProduto { get; private set; } = string.Empty;
    public int QuantidadeDisponivel { get; private set; }

    private ProdutoEstoque() { } // Construtor exigido pelo EF Core

    public ProdutoEstoque(string nomeProduto, int quantidadeDisponivel)
    {
        Id = Guid.NewGuid();
        NomeProduto = nomeProduto;
        QuantidadeDisponivel = quantidadeDisponivel;
    }

    public void Reservar(int quantidade)
    {
        if (quantidade > QuantidadeDisponivel)
            throw new InvalidOperationException($"Estoque insuficiente para o produto {NomeProduto}.");

        QuantidadeDisponivel -= quantidade;
    }
}