# PedidosSolution

Sistema de exemplo com dois microsserviços (**Pedidos** e **Estoque**) comunicando-se
de forma assíncrona via **RabbitMQ** (usando **MassTransit**), cada um com seu próprio
banco **PostgreSQL**. Construído com Clean Architecture (Domain / Application /
Infrastructure / API) e TDD.

## Arquitetura

```
Pedidos.Api  ──HTTP──> CriarPedidoUseCase ──> PedidosDbContext (pedidos_db)
                                │
                                └──publica──> RabbitMQ ("pedido-criado-event")
                                                    │
Estoque.Api  <───────────consome────────────────────┘
     └── PedidoCriadoConsumer ──> ProdutoEstoque.Reservar() ──> EstoqueDbContext (estoque_db)
```

Cada microsserviço mantém sua **própria cópia** do contrato de evento
(`PedidoCriadoEvent`) em vez de referenciar o código do outro serviço — isso evita
acoplamento entre eles. O roteamento no RabbitMQ é garantido por um nome de exchange
configurado explicitamente nos dois lados (`SetEntityName("pedido-criado-event")`),
em vez de depender do nome/namespace da classe.

## Como rodar

### 1. Suba a infraestrutura (Postgres x2 + RabbitMQ)
```bash
docker compose up -d
```
- Postgres do Pedidos: `localhost:5432` (db `pedidos_db`)
- Postgres do Estoque: `localhost:5433` (db `estoque_db`)
- RabbitMQ: `localhost:5672` (management UI em `http://localhost:15672`, guest/guest)

### 2. Aplique as migrações
```bash
dotnet ef database update --project Pedidos.Infrastructure --startup-project Pedidos.Api
dotnet ef database update --project Estoque.Infrastructure --startup-project Estoque.Api
```

### 3. Rode as APIs
```bash
dotnet run --project Pedidos.Api
dotnet run --project Estoque.Api
```

### 4. Rode os testes
```bash
dotnet test
```
Os testes de integração (`*.Infrastructure.Tests` e `Pedidos.Api.Tests`) sobem
**Postgres e RabbitMQ reais via Testcontainers** (precisa do Docker rodando na
máquina/CI). Não usam mocks para o banco — validam o `Repository` e a API de
ponta a ponta contra dependências reais.

## Resiliência e observabilidade

- **Validação**: o domínio (`ItemPedido`, `Pedido`) valida suas próprias regras
  (nome obrigatório, quantidade > 0, preço >= 0, pelo menos 1 item). A API nunca
  deixa uma exceção de validação virar 500 genérico.
- **Middleware de exceção centralizado** (`GlobalExceptionHandler`): captura toda
  exceção não tratada da API. `ArgumentException` (erro de validação de domínio)
  vira `400` com mensagem clara; qualquer outra coisa vira `500` genérico, sem
  vazar detalhes internos, e é logada com `LogError`.
- **Retry policy no consumer do RabbitMQ** (`Estoque.Api`): se o consumo do
  `PedidoCriadoEvent` falhar (erro transiente), o MassTransit tenta de novo 3
  vezes (1s, 5s, 15s de intervalo). Se ainda assim falhar, a mensagem é movida
  automaticamente para a fila `estoque-pedido-criado-queue_error`, sem travar o
  consumo das mensagens seguintes.
- **Logging estruturado com Serilog**: os dois serviços logam em formato
  estruturado no console, com o nome do serviço como propriedade
  (`Servico=Pedidos.Api` / `Servico=Estoque.Api`), incluindo log de cada
  requisição HTTP.
- **Health checks**: `GET /health` nos dois serviços, verificando a conexão com
  o respectivo Postgres.
- **Swagger/OpenAPI**: disponível em `Pedidos.Api` em ambiente de desenvolvimento
  (`/swagger`).
- **Segredos fora do `appsettings.json`**: o `appsettings.json` base não tem mais
  senha nenhuma (`ConnectionStrings`/`RabbitMq` ficam vazios). As credenciais de
  desenvolvimento local (as mesmas do `docker-compose.yml`) ficam em
  `appsettings.Development.json`. Em produção, use variáveis de ambiente
  (`ConnectionStrings__PedidosDb`, `RabbitMq__Password`, etc.) ou
  `dotnet user-secrets` — nunca commitando segredo real.

## Pendências conhecidas / próximos passos

- [x] ~~Bug: colunas faltando em `ItemPedido`~~ — corrigido na migração `CorrecaoColunasItemPedido`.
- [x] ~~Testes de integração com Testcontainers~~ — feito (`Pedidos.Infrastructure.Tests`,
      `Estoque.Infrastructure.Tests`, `Pedidos.Api.Tests`).
- [x] ~~Validações de entrada mais robustas na API (400 com mensagens claras)~~
- [x] ~~Retry policy no consumer do RabbitMQ / fila de erro~~
- [x] ~~Middleware de tratamento de exceções centralizado~~
- [x] ~~Logging estruturado (Serilog) e `/health` nos dois serviços~~
- [x] ~~Swagger/OpenAPI habilitado na API de Pedidos~~
- [x] ~~Tirar senha do `appsettings.json`~~
- [ ] Autenticação/autorização na API
- [ ] Observabilidade avançada (tracing distribuído, métricas/Prometheus)
- [ ] CI/CD (pipeline rodando `dotnet test` a cada push)
