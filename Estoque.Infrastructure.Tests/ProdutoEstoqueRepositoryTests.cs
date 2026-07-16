using Estoque.Domain;
using Microsoft.EntityFrameworkCore;

namespace Estoque.Infrastructure.Tests;

[Collection("Postgres")]
public class ProdutoEstoqueRepositoryTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;

    public ProdutoEstoqueRepositoryTests(PostgresContainerFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await using var context = _fixture.CriarContexto();
        await context.Database.MigrateAsync();

        // Limpa entre testes (mesmo container é reaproveitado na coleção inteira)
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Produtos\"");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Deve_persistir_reserva_de_estoque_no_banco_real()
    {
        // Arrange
        var produto = new ProdutoEstoque("Teclado Mecânico", quantidadeDisponivel: 10);

        await using (var contextEscrita = _fixture.CriarContexto())
        {
            contextEscrita.Produtos.Add(produto);
            await contextEscrita.SaveChangesAsync();
        }

        // Act: recupera numa nova instância, reserva, e persiste via repository
        await using (var contextReserva = _fixture.CriarContexto())
        {
            var repositorio = new ProdutoEstoqueRepository(contextReserva);

            var produtoRecuperado = await repositorio.ObterPorNomeAsync("Teclado Mecânico");
            Assert.NotNull(produtoRecuperado);

            produtoRecuperado!.Reservar(4);
            await repositorio.AtualizarAsync(produtoRecuperado);
        }

        // Assert: uma terceira instância confirma que o UPDATE realmente foi pro banco
        await using var contextLeitura = _fixture.CriarContexto();
        var produtoFinal = await contextLeitura.Produtos
            .FirstOrDefaultAsync(p => p.NomeProduto == "Teclado Mecânico");

        Assert.NotNull(produtoFinal);
        Assert.Equal(6, produtoFinal!.QuantidadeDisponivel);
    }

    [Fact]
    public async Task Deve_retornar_null_quando_produto_nao_existe_no_estoque()
    {
        await using var context = _fixture.CriarContexto();
        var repositorio = new ProdutoEstoqueRepository(context);

        var resultado = await repositorio.ObterPorNomeAsync("Produto Que Não Existe");

        Assert.Null(resultado);
    }

    [Fact]
    public async Task Nao_deve_permitir_dois_produtos_com_o_mesmo_nome_indice_unico()
    {
        // Essa regra só existe no banco (índice único), então só um teste de
        // integração real consegue validar que ela está de fato aplicada.
        await using var context = _fixture.CriarContexto();

        context.Produtos.Add(new ProdutoEstoque("Monitor 24pol", 5));
        await context.SaveChangesAsync();

        context.Produtos.Add(new ProdutoEstoque("Monitor 24pol", 3));

        await Assert.ThrowsAnyAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }
}
