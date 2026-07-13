using Xunit;

namespace Pedidos.Domain.Tests;

public class ItemPedidoTests
{
    [Fact]
    public void Deve_lancar_excecao_quando_nome_produto_e_vazio()
    {
        var excecao = Assert.Throws<ArgumentException>(
            () => new ItemPedido("", 1, 10.00m));

        Assert.Equal("Nome do produto e obrigatorio.", excecao.Message);
    }

    [Fact]
    public void Deve_lancar_excecao_quando_quantidade_e_menor_ou_igual_a_zero()
    {
        var excecao = Assert.Throws<ArgumentException>(
            () => new ItemPedido("Teclado", 0, 10.00m));

        Assert.Equal("Quantidade deve ser maior que zero.", excecao.Message);
    }

    [Fact]
    public void Deve_lancar_excecao_quando_preco_unitario_e_negativo()
    {
        var excecao = Assert.Throws<ArgumentException>(
            () => new ItemPedido("Teclado", 1, -10.00m));

        Assert.Equal("Preco unitario nao pode ser negativo.", excecao.Message);
    }

    [Fact]
    public void Deve_criar_item_valido_com_sucesso()
    {
        var item = new ItemPedido("Teclado", 2, 250.00m);

        Assert.Equal("Teclado", item.NomeProduto);
        Assert.Equal(2, item.Quantidade);
        Assert.Equal(250.00m, item.PrecoUnitario);
    }
}
