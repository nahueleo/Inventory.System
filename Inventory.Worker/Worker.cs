using Inventory.Domain.Constants;
using Inventory.Domain.Interfaces;
using Inventory.Infrastructure.Resilience;
using Inventory.Infrastructure.Services;
using Inventory.Worker.Handlers;

namespace Inventory.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IMessageConsumer _consumer;
    private readonly ICircuitBreaker _circuitBreaker;
    private readonly MessageHandlerFactory _handlerFactory;

    public Worker(
        ILogger<Worker> logger,
        IMessageConsumer consumer,
        ICircuitBreaker circuitBreaker,
        MessageHandlerFactory handlerFactory)
    {
        _logger = logger;
        _consumer = consumer;
        _circuitBreaker = circuitBreaker;
        _handlerFactory = handlerFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _circuitBreaker.ExecuteAsync(async () =>
            {
                var queueHandlers = new Dictionary<RoutingKey, Action<string>>
                {
                    { RoutingKey.ProductCreated, message => _handlerFactory(MessageAction.Create).HandleAsync(message).GetAwaiter().GetResult() },
                    { RoutingKey.ProductUpdated, message => _handlerFactory(MessageAction.Update).HandleAsync(message).GetAwaiter().GetResult() },
                    { RoutingKey.ProductDeleted, message => _handlerFactory(MessageAction.Delete).HandleAsync(message).GetAwaiter().GetResult() }
                };

                _consumer.StartConsuming(queueHandlers);
                _logger.LogInformation("Worker started and listening for messages...");

                // Mantener el worker vivo hasta que se cancele
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            });
        }
        catch (CircuitBreakerOpenException)
        {
            _logger.LogWarning("Circuit breaker is open. Waiting before retrying...");
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in worker execution");
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
