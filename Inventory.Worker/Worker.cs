using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Inventory.Infrastructure.Services;
using Inventory.Infrastructure.Configuration;
using Inventory.Domain.Interfaces;
using Inventory.Infrastructure.Resilience;

namespace Inventory.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly RabbitMQSettings _settings;
    private readonly ICircuitBreaker _circuitBreaker;
    private const int MaxRetries = 3;
    private const int RetryDelayMs = 2000;

    public Worker(
        ILogger<Worker> logger,
        RabbitMQSettings settings,
        ICircuitBreaker circuitBreaker)
    {
        _logger = logger;
        _settings = settings;
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
                    await ConnectAndConsumeAsync(stoppingToken);
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

    private async Task ConnectAndConsumeAsync(CancellationToken stoppingToken)
    {
        var retryCount = 0;
        while (retryCount < MaxRetries)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _settings.HostName,
                    UserName = _settings.UserName,
                    Password = _settings.Password,
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
                };

                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                // Declare the exchange
                channel.ExchangeDeclare(_settings.ExchangeName, ExchangeType.Topic, true);

                // Declare and bind the queues
                var createQueue = channel.QueueDeclare("product.create", true, false, false);
                var updateQueue = channel.QueueDeclare("product.update", true, false, false);
                var deleteQueue = channel.QueueDeclare("product.delete", true, false, false);

                channel.QueueBind(createQueue.QueueName, _settings.ExchangeName, "product.created");
                channel.QueueBind(updateQueue.QueueName, _settings.ExchangeName, "product.updated");
                channel.QueueBind(deleteQueue.QueueName, _settings.ExchangeName, "product.deleted");

                var consumer = new EventingBasicConsumer(channel);

                consumer.Received += (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        var routingKey = ea.RoutingKey;
                        var timestamp = DateTime.Now;

                        Console.WriteLine("\n=== New Event Received ===");
                        Console.WriteLine($"Timestamp: {timestamp:yyyy-MM-dd HH:mm:ss}");
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
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message");
                        Console.WriteLine($"Error processing message: {ex.Message}");
                    }
                };

                // Consume from the three queues
                channel.BasicConsume(queue: "product.create", autoAck: true, consumer: consumer);
                channel.BasicConsume(queue: "product.update", autoAck: true, consumer: consumer);
                channel.BasicConsume(queue: "product.delete", autoAck: true, consumer: consumer);

                _logger.LogInformation("Worker started and listening for messages...");
                Console.WriteLine("Worker started and listening for messages...");

                // Keep the connection alive
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }

                return; // If we reach here, the connection was successful
            }
            catch (Exception ex)
            {
                retryCount++;
                _logger.LogError(ex, $"Error in attempt {retryCount} of {MaxRetries}");
                
                if (retryCount < MaxRetries)
                {
                    await Task.Delay(RetryDelayMs, stoppingToken);
                }
                else
                {
                    throw; // Re-throw the exception for the Circuit Breaker to handle
                }
            }
        }
    }
}
