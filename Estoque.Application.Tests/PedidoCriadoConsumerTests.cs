using Estoque.Domain;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Estoque.Application.Tests;

public class PedidoCriadoConsumerTests
{
    private static async Task<(ITestHarness Harness, ServiceProvider Provider)> CriarHarnessAsync(
        IProdutoEstoqueRepository repositorio)
    {
        var provider = new ServiceCollection()
            .AddSingleton(repositorio)
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<PedidoCriadoConsumer>();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        return (harness, provider);
    }

    [Fact]
    public async Task Deve_reservar_estoque_do_produto_ao_consumir_evento()
    {
        // Arrange
        var produto = new ProdutoEstoque("Teclado Mecânico", quantidadeDisponivel: 10);

        var repositorioMock = new Mock<IProdutoEstoqueRepository>();
        repositorioMock
            .Setup(r => r.ObterPorNomeAsync("Teclado Mecânico"))
            .ReturnsAsync(produto);

        var (harness, provider) = await CriarHarnessAsync(repositorioMock.Object);
        await using var _ = provider;

        var evento = new PedidoCriadoEvent(
            Guid.NewGuid(),
            250.00m,
            DateTime.UtcNow,
            new List<ItemPedidoEvent> { new("Teclado Mecânico", 3) });

        // Act
        await harness.Bus.Publish(evento);

        // Assert
        Assert.True(await harness.Consumed.Any<PedidoCriadoEvent>());
        Assert.Equal(7, produto.QuantidadeDisponivel);
        repositorioMock.Verify(r => r.AtualizarAsync(produto), Times.Once);
    }

    [Fact]
    public async Task Deve_reservar_todos_os_itens_do_pedido()
    {
        // Arrange
        var teclado = new ProdutoEstoque("Teclado Mecânico", quantidadeDisponivel: 10);
        var mouse = new ProdutoEstoque("Mouse Gamer", quantidadeDisponivel: 5);

        var repositorioMock = new Mock<IProdutoEstoqueRepository>();
        repositorioMock.Setup(r => r.ObterPorNomeAsync("Teclado Mecânico")).ReturnsAsync(teclado);
        repositorioMock.Setup(r => r.ObterPorNomeAsync("Mouse Gamer")).ReturnsAsync(mouse);

        var (harness, provider) = await CriarHarnessAsync(repositorioMock.Object);
        await using var _ = provider;

        var evento = new PedidoCriadoEvent(
            Guid.NewGuid(),
            550.00m,
            DateTime.UtcNow,
            new List<ItemPedidoEvent>
            {
                new("Teclado Mecânico", 2),
                new("Mouse Gamer", 1)
            });

        // Act
        await harness.Bus.Publish(evento);

        // Assert
        Assert.True(await harness.Consumed.Any<PedidoCriadoEvent>());
        Assert.Equal(8, teclado.QuantidadeDisponivel);
        Assert.Equal(4, mouse.QuantidadeDisponivel);
    }

    [Fact]
    public async Task Nao_deve_falhar_quando_produto_nao_esta_cadastrado_no_estoque()
    {
        // Arrange: repositório não encontra o produto (retorna null)
        var repositorioMock = new Mock<IProdutoEstoqueRepository>();
        repositorioMock
            .Setup(r => r.ObterPorNomeAsync(It.IsAny<string>()))
            .ReturnsAsync((ProdutoEstoque?)null);

        var (harness, provider) = await CriarHarnessAsync(repositorioMock.Object);
        await using var _ = provider;

        var evento = new PedidoCriadoEvent(
            Guid.NewGuid(),
            100.00m,
            DateTime.UtcNow,
            new List<ItemPedidoEvent> { new("Produto Inexistente", 1) });

        // Act
        await harness.Bus.Publish(evento);

        // Assert: consumido sem exceção, e nada foi atualizado
        Assert.True(await harness.Consumed.Any<PedidoCriadoEvent>());
        repositorioMock.Verify(r => r.AtualizarAsync(It.IsAny<ProdutoEstoque>()), Times.Never);
    }
}
