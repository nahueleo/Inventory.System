using System.Text.Json;
using Inventory.Infrastructure.Services;
using Inventory.Infrastructure.Resilience;

namespace Inventory.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly RabbitMQConsumer _consumer;
    private readonly ICircuitBreaker _circuitBreaker;

    public Worker(
        ILogger<Worker> logger,
        RabbitMQConsumer consumer,
        ICircuitBreaker circuitBreaker)
    {
        _logger = logger;
        _consumer = consumer;
        _circuitBreaker = circuitBreaker;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _circuitBreaker.ExecuteAsync(async () =>
                {
                    _consumer.StartConsuming((routingKey, message) =>
                    {
                        Console.WriteLine("\n=== New Event Received ===");
                        Console.WriteLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                        Console.WriteLine($"Event Type: {routingKey}");
                        Console.WriteLine("Product Data:");
                        
                        try
                        {
                            var productData = JsonSerializer.Deserialize<object>(message);
                            var formattedJson = JsonSerializer.Serialize(productData, new JsonSerializerOptions { WriteIndented = true });
                            Console.WriteLine(formattedJson);
                        }
                        catch
                        {
                            Console.WriteLine(message);
                        }
                        
                        Console.WriteLine("===========================\n");
                    });

                    _logger.LogInformation("Worker started and listening for messages...");
                    Console.WriteLine("Worker started and listening for messages...");

                    // Keep the connection alive
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
}
