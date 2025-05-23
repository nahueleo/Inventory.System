using Inventory.Infrastructure.Resilience;

namespace Inventory.Infrastructure.Services;

public class CircuitBreaker : ICircuitBreaker
{
    private readonly int _failureThreshold;
    private readonly TimeSpan _resetTimeout;
    private int _failureCount;
    private DateTime _lastFailureTime;
    private CircuitState _state;
    private readonly object _lock = new object();

    public CircuitBreaker(int failureThreshold = 3, int resetTimeoutSeconds = 30)
    {
        _failureThreshold = failureThreshold;
        _resetTimeout = TimeSpan.FromSeconds(resetTimeoutSeconds);
        _state = CircuitState.Closed;
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
    {
        if (_state == CircuitState.Open)
        {
            if (DateTime.UtcNow - _lastFailureTime > _resetTimeout)
            {
                _state = CircuitState.HalfOpen;
            }
            else
            {
                throw new CircuitBreakerOpenException("Circuit breaker is open");
            }
        }

        try
        {
            var result = await action();
            if (_state == CircuitState.HalfOpen)
            {
                Reset();
            }
            return result;
        }
        catch (Exception)
        {
            RecordFailure();
            throw;
        }
    }

    public async Task ExecuteAsync(Func<Task> action)
    {
        await ExecuteAsync(async () =>
        {
            await action();
            return true;
        });
    }

    private void RecordFailure()
    {
        lock (_lock)
        {
            _failureCount++;
            _lastFailureTime = DateTime.UtcNow;

            if (_failureCount >= _failureThreshold)
            {
                _state = CircuitState.Open;
            }
        }
    }

    private void Reset()
    {
        lock (_lock)
        {
            _failureCount = 0;
            _state = CircuitState.Closed;
        }
    }

    private enum CircuitState
    {
        Closed,
        Open,
        HalfOpen
    }
}

public class CircuitBreakerOpenException : Exception
{
    public CircuitBreakerOpenException(string message) : base(message)
    {
    }
} 