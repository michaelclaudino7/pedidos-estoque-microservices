namespace Estoque.Application;

// Cópia local do contrato de integração publicado pelo Pedidos.
// Duplicar o evento (em vez de referenciar o projeto Pedidos.Application)
// é proposital: cada microsserviço deve ser dono do seu próprio contrato
// de entrada e não deve depender do código interno de outro serviço.
// O que garante que Pedidos e Estoque conversem corretamente não é o
// namespace/nome da classe ser igual, e sim o nome da exchange configurado
// explicitamente nos dois lados (veja Program.cs: SetEntityName("pedido-criado-event")).
public record PedidoCriadoEvent(
    Guid PedidoId,
    decimal ValorTotal,
    DateTime DataCriacao,
    List<ItemPedidoEvent> Itens
);

public record ItemPedidoEvent(string NomeProduto, int Quantidade);
