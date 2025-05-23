using Inventory.Domain.Constants;

namespace Inventory.Domain.Interfaces;

public interface IMessagePublisher
{
    Task PublishAsync<T>(T message, RoutingKey routingKey) where T : class;
} 