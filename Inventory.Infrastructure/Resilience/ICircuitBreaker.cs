namespace Inventory.Infrastructure.Resilience;

public interface ICircuitBreaker
{
    Task<T> ExecuteAsync<T>(Func<Task<T>> action);
    Task ExecuteAsync(Func<Task> action);
}
