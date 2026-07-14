using MassTransit;

namespace Estoque.Application;

public class PedidoCriadoConsumer : IConsumer<PedidoCriadoEvent>
{
    private readonly IProdutoEstoqueRepository _repositorio;

    public PedidoCriadoConsumer(IProdutoEstoqueRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task Consume(ConsumeContext<PedidoCriadoEvent> context)
    {
        var evento = context.Message;

        foreach (var item in evento.Itens)
        {
            var produto = await _repositorio.ObterPorNomeAsync(item.NomeProduto);

            if (produto is null)
            {
                // Produto não cadastrado no estoque: por ora apenas ignoramos o item.
                // Numa evolução futura isso pode virar um evento de compensação
                // (ex: EstoqueIndisponivelEvent) para o Pedidos reagir.
                continue;
            }

            produto.Reservar(item.Quantidade);
            await _repositorio.AtualizarAsync(produto);
        }
    }
}
