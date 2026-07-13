using Xunit;

namespace Pedidos.Domain.Tests;

public class PedidoTests
{
    [Fact]
    public void Deve_lancar_excecao_ao_criar_pedido_sem_itens()
    {
        // Arrange
        var itensVazios = new List<ItemPedido>();

        // Act & Assert
        var excecao = Assert.Throws<ArgumentException>(
            () => new Pedido(itensVazios)
        );

        Assert.Equal("Pedido deve ter pelo menos um item.", excecao.Message);
    }

    [Fact]
    public void Deve_calcular_valor_total_do_pedido_corretamente()
    {
        // Arrange
        var itens = new List<ItemPedido>
    {
        new ItemPedido("Teclado Mecânico", 2, 250.00m),
        new ItemPedido("Mouse Gamer", 1, 150.00m)
    };

        // Act
        var pedido = new Pedido(itens);

        // Assert
        Assert.Equal(650.00m, pedido.ValorTotal);
    }
}