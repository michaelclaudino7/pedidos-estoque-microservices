using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pedidos.Infrastructure;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace Pedidos.Api.Tests;

/// <summary>
/// Sobe a Pedidos.Api de ponta a ponta (host real do ASP.NET Core, pipeline HTTP real)
/// contra um Postgres e um RabbitMQ reais em containers Docker. Nada aqui é mockado.
/// </summary>
public class PedidosApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("pedidos_db_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly RabbitMqContainer _rabbitMq = new RabbitMqBuilder()
        .WithImage("rabbitmq:3-management-alpine")
        .WithUsername("guest")
        .WithPassword("guest")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:PedidosDb"] = _postgres.GetConnectionString(),
                ["RabbitMq:Host"] = _rabbitMq.Hostname,
                ["RabbitMq:Port"] = _rabbitMq.GetMappedPublicPort(5672).ToString(),
                ["RabbitMq:Username"] = "guest",
                ["RabbitMq:Password"] = "guest"
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await _rabbitMq.StartAsync();

        // Força a criação do host agora para aplicar as migrações antes dos testes rodarem
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PedidosDbContext>();
        // Garante estado limpo e determinístico antes de migrar — elimina qualquer
        // condição de corrida ou resíduo de execuções anteriores no container.
        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await _rabbitMq.DisposeAsync();
        await base.DisposeAsync();
    }
}
