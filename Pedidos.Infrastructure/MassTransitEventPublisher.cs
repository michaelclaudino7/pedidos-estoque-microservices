using MassTransit;
using Pedidos.Application;

namespace Pedidos.Infrastructure;

public class MassTransitEventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitEventPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublicarAsync<T>(T evento) where T : class
    {
        await _publishEndpoint.Publish(evento);
    }
}