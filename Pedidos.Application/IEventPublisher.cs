namespace Pedidos.Application;

public interface IEventPublisher
{
    Task PublicarAsync<T>(T evento) where T : class;
}