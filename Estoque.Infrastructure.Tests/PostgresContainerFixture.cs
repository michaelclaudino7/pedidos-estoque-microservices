using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Estoque.Infrastructure.Tests;

public class PostgresContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("estoque_db_test")
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

    public EstoqueDbContext CriarContexto()
    {
        var options = new DbContextOptionsBuilder<EstoqueDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new EstoqueDbContext(options);
    }
}

[CollectionDefinition("Postgres")]
public class PostgresCollection : ICollectionFixture<PostgresContainerFixture>
{
}
