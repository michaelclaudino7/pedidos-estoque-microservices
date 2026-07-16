using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Pedidos.Infrastructure.Tests;

/// <summary>
/// Sobe um Postgres real em container Docker para os testes de integração.
/// Compartilhado entre todos os testes da coleção "Postgres" para não pagar
/// o custo de subir um container novo a cada teste.
/// </summary>
public class PostgresContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("pedidos_db_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public PedidosDbContext CriarContexto()
    {
        var options = new DbContextOptionsBuilder<PedidosDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new PedidosDbContext(options);
    }
}

[CollectionDefinition("Postgres")]
public class PostgresCollection : ICollectionFixture<PostgresContainerFixture>
{
}
