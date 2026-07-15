using Estoque.Application;
using Estoque.Infrastructure;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ===== Logging estruturado (Serilog) =====
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Servico", "Estoque.Api")
    .WriteTo.Console());

// Configura o EF Core com PostgreSQL (banco próprio do serviço de Estoque)
var estoqueConnectionString = builder.Configuration.GetConnectionString("EstoqueDb")
    ?? throw new InvalidOperationException("Connection string 'EstoqueDb' nao configurada.");

builder.Services.AddDbContext<EstoqueDbContext>(options =>
    options.UseNpgsql(estoqueConnectionString));

builder.Services.AddScoped<IProdutoEstoqueRepository, ProdutoEstoqueRepository>();

// ===== Health checks =====
builder.Services.AddHealthChecks()
    .AddNpgSql(estoqueConnectionString, name: "postgres-estoque");

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PedidoCriadoConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitPort = builder.Configuration.GetValue<ushort?>("RabbitMq:Port") ?? 5672;

        cfg.Host(builder.Configuration["RabbitMq:Host"] ?? "localhost", rabbitPort, "/", h =>
        {
            h.Username(builder.Configuration["RabbitMq:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMq:Password"] ?? "guest");
        });

        // Nome de exchange explícito: precisa bater com o configurado no Pedidos.Api.
        cfg.Message<PedidoCriadoEvent>(m => m.SetEntityName("pedido-criado-event"));

        cfg.ReceiveEndpoint("estoque-pedido-criado-queue", e =>
        {
            // ===== Retry policy =====
            // Se o consumer falhar (erro transiente: banco fora do ar, deadlock, etc),
            // tenta de novo 3 vezes com intervalo crescente antes de desistir.
            // Se mesmo assim continuar falhando, o MassTransit move a mensagem
            // automaticamente para a fila "estoque-pedido-criado-queue_error",
            // sem perder a mensagem e sem travar o consumo das próximas.
            e.UseMessageRetry(r => r.Intervals(
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(15)));

            e.ConfigureConsumer<PedidoCriadoConsumer>(context);
        });
    });
});

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.MapHealthChecks("/health");

app.Run();

// Necessário para o WebApplicationFactory (testes de integração) enxergar o Program
public partial class Program { }
