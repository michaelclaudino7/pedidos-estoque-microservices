using Xunit;
using Pedidos.Domain;
using Moq;

namespace Pedidos.Application.Tests;

public class CriarPedidoUseCaseTests
{
    [Fact]
    public async Task Deve_criar_pedido_com_sucesso_quando_itens_sao_validos()
    {
        // Arrange
        var repositorioMock = new Mock<IPedidoRepository>();
        var publisherMock = new Mock<IEventPublisher>();
        var useCase = new CriarPedidoUseCase(repositorioMock.Object, publisherMock.Object);

        var itens = new List<ItemPedido>
        {
            new ItemPedido("Teclado Mecânico", 1, 250.00m)
        };

        // Act
        var pedidoCriado = await useCase.Executar(itens);

        // Assert
        Assert.NotNull(pedidoCriado);
        Assert.Equal(250.00m, pedidoCriado.ValorTotal);
        Assert.Single(pedidoCriado.Itens);
        repositorioMock.Verify(r => r.SalvarAsync(pedidoCriado), Times.Once);
    }

    [Fact]
    public async Task Deve_publicar_evento_PedidoCriado_apos_salvar_pedido()
    {
        // Arrange
        var repositorioMock = new Mock<IPedidoRepository>();
        var publisherMock = new Mock<IEventPublisher>();
        var useCase = new CriarPedidoUseCase(repositorioMock.Object, publisherMock.Object);

        var itens = new List<ItemPedido>
        {
            new ItemPedido("Mouse Gamer", 2, 150.00m)
        };

        // Act
        var pedidoCriado = await useCase.Executar(itens);

        // Assert
        publisherMock.Verify(p => p.PublicarAsync(
            It.Is<PedidoCriadoEvent>(e => e.PedidoId == pedidoCriado.Id && e.ValorTotal == pedidoCriado.ValorTotal)
        ), Times.Once);
    }
}