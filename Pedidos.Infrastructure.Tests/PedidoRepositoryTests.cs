using Microsoft.EntityFrameworkCore;
using Pedidos.Domain;

namespace Pedidos.Infrastructure.Tests;

[Collection("Postgres")]
public class PedidoRepositoryTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;

    public PedidoRepositoryTests(PostgresContainerFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        // Garante o schema atualizado (todas as migrações, incluindo a de correção) antes de cada teste
        await using var context = _fixture.CriarContexto();
        await context.Database.MigrateAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Deve_salvar_e_recuperar_pedido_com_todos_os_dados_dos_itens()
    {
        // Arrange
        var itens = new List<ItemPedido>
        {
            new("Teclado Mecânico", 2, 250.00m),
            new("Mouse Gamer", 1, 150.00m)
        };
        var pedido = new Pedido(itens);

        // Act: salva usando uma instância do DbContext...
        await using (var contextEscrita = _fixture.CriarContexto())
        {
            var repositorio = new PedidoRepository(contextEscrita);
            await repositorio.SalvarAsync(pedido);
        }

        // ...e recupera usando outra instância nova, simulando uma nova requisição.
        // Isso é o que um teste com mock NUNCA pega: se os dados realmente foram
        // persistidos no banco (e não só ficaram vivos em memória).
        await using var contextLeitura = _fixture.CriarContexto();
        var pedidoRecuperado = await contextLeitura.Pedidos
            .Include("Itens")
            .FirstOrDefaultAsync(p => p.Id == pedido.Id);

        // Assert
        Assert.NotNull(pedidoRecuperado);
        Assert.Equal(pedido.ValorTotal, pedidoRecuperado!.ValorTotal);
        Assert.Equal(2, pedidoRecuperado.Itens.Count);

        var teclado = pedidoRecuperado.Itens.Single(i => i.NomeProduto == "Teclado Mecânico");
        Assert.Equal(2, teclado.Quantidade);
        Assert.Equal(250.00m, teclado.PrecoUnitario);
    }

    [Fact]
    public async Task Deve_persistir_pedidos_diferentes_de_forma_isolada()
    {
        // Arrange
        var pedido1 = new Pedido(new List<ItemPedido> { new("Produto A", 1, 10.00m) });
        var pedido2 = new Pedido(new List<ItemPedido> { new("Produto B", 5, 20.00m) });

        await using (var context = _fixture.CriarContexto())
        {
            var repositorio = new PedidoRepository(context);
            await repositorio.SalvarAsync(pedido1);
            await repositorio.SalvarAsync(pedido2);
        }

        // Act
        await using var contextLeitura = _fixture.CriarContexto();
        var total = await contextLeitura.Pedidos.CountAsync(p => p.Id == pedido1.Id || p.Id == pedido2.Id);

        // Assert
        Assert.Equal(2, total);
    }
}
