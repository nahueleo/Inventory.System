using System.Text;
using System.Text.Json;
using Inventory.Domain.Interfaces;
using Inventory.Infrastructure.Configuration;
using RabbitMQ.Client;
using Microsoft.Extensions.Logging;
using Inventory.Domain.Constants;

namespace Inventory.Infrastructure.Services;

public class RabbitMQMessagePublisher : IMessagePublisher
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly RabbitMQSettings _settings;
    private readonly CircuitBreaker _circuitBreaker;
    private readonly ILogger<RabbitMQMessagePublisher> _logger;
    private readonly int _maxRetries = 3;
    private readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(2);

    public RabbitMQMessagePublisher(
        IRabbitMQConnection connectionFactory,
        RabbitMQSettings settings,
        ILogger<RabbitMQMessagePublisher> logger)
    {
        _connection = connectionFactory.GetConnection();
        _settings = settings;
        _logger = logger;
        _channel = _connection.CreateModel();
        _circuitBreaker = new CircuitBreaker(failureThreshold: 3, resetTimeoutSeconds: 30);
        SetupExchange();
    }

    public async Task PublishAsync<T>(T message, RoutingKey routingKey) where T : class
    {
        var routingKeyString = routingKey.ToRoutingKeyString();
        // Validate routing key
        if (!IsValidRoutingKey(routingKeyString))
        {
            throw new ArgumentException($"Invalid routing key: {routingKeyString}", nameof(routingKeyString));
        }

        var retryCount = 0;
        var lastException = default(Exception);

        while (retryCount < _maxRetries)
        {
            try
            {
                await _circuitBreaker.ExecuteAsync(async () =>
                {
                    using var channel = _connection.CreateModel();
                    channel.ExchangeDeclare(_settings.ExchangeName, ExchangeType.Topic, true);

                    var json = JsonSerializer.Serialize(message);
                    var body = Encoding.UTF8.GetBytes(json);

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true; // Make messages persistent
                    properties.MessageId = Guid.NewGuid().ToString();
                    properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                    channel.BasicPublish(
                        exchange: _settings.ExchangeName,
                        routingKey: routingKeyString,
                        basicProperties: properties,
                        body: body);

                    _logger.LogInformation("Message published successfully. MessageId: {MessageId}", properties.MessageId);
                    return true;
                });

                return; // Success, exit the retry loop
            }
            catch (CircuitBreakerOpenException ex)
            {
                _logger.LogWarning(ex, "Circuit breaker is open. Retry attempt {RetryCount} of {MaxRetries}", retryCount + 1, _maxRetries);
                lastException = ex;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing message. Retry attempt {RetryCount} of {MaxRetries}", retryCount + 1, _maxRetries);
                lastException = ex;
            }

            retryCount++;
            if (retryCount < _maxRetries)
            {
                await Task.Delay(_retryDelay);
            }
        }

        // If we get here, all retries failed
        _logger.LogError(lastException, "Failed to publish message after {MaxRetries} attempts", _maxRetries);
        throw new Exception($"Failed to publish message after {_maxRetries} attempts", lastException);
    }

    private bool IsValidRoutingKey(string routingKey)
    {
        return routingKey == Inventory.Domain.Constants.RoutingKeys.ProductCreated ||
               routingKey == Inventory.Domain.Constants.RoutingKeys.ProductUpdated ||
               routingKey == Inventory.Domain.Constants.RoutingKeys.ProductDeleted;
    }

    private void SetupExchange()
    {
        _channel.ExchangeDeclare(_settings.ExchangeName, ExchangeType.Topic, true);
    }
} 