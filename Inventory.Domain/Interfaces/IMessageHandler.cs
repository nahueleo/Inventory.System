namespace Inventory.Domain.Interfaces;

public interface IMessageHandler
{
    string QueueName { get; }
    Task HandleAsync(string message);
} 