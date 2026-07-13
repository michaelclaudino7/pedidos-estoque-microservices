using Xunit;

namespace Estoque.Domain.Tests;

public class ProdutoEstoqueTests
{
    [Fact]
    public void Deve_reservar_quantidade_quando_ha_estoque_suficiente()
    {
        // Arrange
        var produto = new ProdutoEstoque("Teclado Mecânico", quantidadeDisponivel: 10);

        // Act
        produto.Reservar(3);

        // Assert
        Assert.Equal(7, produto.QuantidadeDisponivel);
    }

    [Fact]
    public void Nao_deve_reservar_quantidade_maior_que_disponivel()
    {
        // Arrange
        var produto = new ProdutoEstoque("Mouse Gamer", quantidadeDisponivel: 5);

        // Act & Assert
        var excecao = Assert.Throws<InvalidOperationException>(
            () => produto.Reservar(10)
        );

        Assert.Equal("Estoque insuficiente para o produto Mouse Gamer.", excecao.Message);
    }
}