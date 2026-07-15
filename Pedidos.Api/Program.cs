using MassTransit;
using Microsoft.EntityFrameworkCore;
using Pedidos.Api;
using Pedidos.Application;
using Pedidos.Domain;
using Pedidos.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ===== Logging estruturado (Serilog) =====
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Servico", "Pedidos.Api")
    .WriteTo.Console());

// Configura o EF Core com PostgreSQL
var pedidosConnectionString = builder.Configuration.GetConnectionString("PedidosDb")
    ?? throw new InvalidOperationException("Connection string 'PedidosDb' nao configurada.");

builder.Services.AddDbContext<PedidosDbContext>(options =>
    options.UseNpgsql(pedidosConnectionString));

// Registra o repositório, o publisher de eventos e o caso de uso
builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<IEventPublisher, MassTransitEventPublisher>();
builder.Services.AddScoped<CriarPedidoUseCase>();

// ===== Tratamento de exceções centralizado =====
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// ===== Health checks =====
builder.Services.AddHealthChecks()
    .AddNpgSql(pedidosConnectionString, name: "postgres-pedidos");

// ===== Swagger/OpenAPI =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configura o MassTransit com RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitPort = builder.Configuration.GetValue<ushort?>("RabbitMq:Port") ?? 5672;

        cfg.Host(builder.Configuration["RabbitMq:Host"] ?? "localhost", rabbitPort, "/", h =>
        {
            h.Username(builder.Configuration["RabbitMq:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMq:Password"] ?? "guest");
        });

        // Precisa bater com o nome configurado no Estoque.Api (consumer).
        cfg.Message<PedidoCriadoEvent>(m => m.SetEntityName("pedido-criado-event"));
    });
});

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapHealthChecks("/health");

// Endpoint para criar um pedido
app.MapPost("/pedidos", async (CriarPedidoRequest request, CriarPedidoUseCase useCase) =>
{
    var itens = request.Itens
        .Select(i => new ItemPedido(i.NomeProduto, i.Quantidade, i.PrecoUnitario))
        .ToList();

    var pedido = await useCase.Executar(itens);

    return Results.Created($"/pedidos/{pedido.Id}", pedido);
})
.WithName("CriarPedido")
.WithOpenApi();

app.Run();

// DTOs (objetos simples só para transportar dados da requisição)
public record CriarPedidoRequest(List<ItemPedidoRequest> Itens);
public record ItemPedidoRequest(string NomeProduto, int Quantidade, decimal PrecoUnitario);

// Necessário para o WebApplicationFactory (testes de integração) enxergar o Program
public partial class Program { }
