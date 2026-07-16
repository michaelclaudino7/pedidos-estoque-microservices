using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pedidos.Infrastructure;

namespace Pedidos.Api.Tests;

public class PedidosEndpointTests : IClassFixture<PedidosApiFactory>
{
    private readonly PedidosApiFactory _factory;
    private readonly HttpClient _client;

    public PedidosEndpointTests(PedidosApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task POST_pedidos_deve_retornar_201_e_persistir_no_banco()
    {
        // Arrange
        var request = new
        {
            Itens = new[]
            {
                new { NomeProduto = "Teclado Mecânico", Quantidade = 2, PrecoUnitario = 250.00m }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/pedidos", request);

        // Assert: contrato HTTP
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var pedidoCriado = await response.Content.ReadFromJsonAsync<PedidoResponse>();
        Assert.NotNull(pedidoCriado);
        Assert.Equal(500.00m, pedidoCriado!.ValorTotal);

        // Assert: dado realmente persistido no Postgres (não só a resposta HTTP)
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PedidosDbContext>();
        var pedidoNoBanco = await context.Pedidos
            .Include("Itens")
            .FirstOrDefaultAsync(p => p.Id == pedidoCriado.Id);

        Assert.NotNull(pedidoNoBanco);
        Assert.Single(pedidoNoBanco!.Itens);
    }

    [Fact]
    public async Task POST_pedidos_com_lista_de_itens_vazia_deve_retornar_400_com_mensagem_clara()
    {
        // Arrange
        var request = new { Itens = Array.Empty<object>() };

        // Act
        var response = await _client.PostAsJsonAsync("/pedidos", request);

        // Assert: com o GlobalExceptionHandler, ArgumentException de domínio
        // agora vira 400 com uma mensagem de erro legível, não mais 500.
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problema = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>();
        Assert.NotNull(problema);
        Assert.Equal("Pedido deve ter pelo menos um item.", problema!.Detail);
    }

    [Fact]
    public async Task POST_pedidos_com_quantidade_invalida_deve_retornar_400()
    {
        // Arrange
        var request = new
        {
            Itens = new[]
            {
                new { NomeProduto = "Teclado Mecânico", Quantidade = 0, PrecoUnitario = 250.00m }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/pedidos", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private record PedidoResponse(Guid Id, decimal ValorTotal);
    private record ProblemDetailsResponse(string? Title, string? Detail);
}
