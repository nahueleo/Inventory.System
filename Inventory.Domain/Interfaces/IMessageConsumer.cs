using Inventory.Domain.Constants;

namespace Inventory.Domain.Interfaces;

public interface IMessageConsumer : IDisposable
{
    void StartConsuming(Dictionary<RoutingKey, Action<string>> queueHandlers);
} 