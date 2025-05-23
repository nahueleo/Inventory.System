using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Microsoft.Extensions.Logging;
using Inventory.Infrastructure.Configuration;

namespace Inventory.Infrastructure.Services;

public class RabbitMQConsumer : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly RabbitMQSettings _settings;
    private readonly ILogger<RabbitMQConsumer> _logger;

    public RabbitMQConsumer(
        IConnection connection,
        RabbitMQSettings settings,
        ILogger<RabbitMQConsumer> logger)
    {
        _connection = connection;
        _settings = settings;
        _logger = logger;
        _channel = _connection.CreateModel();
        SetupQueues();
    }

    private void SetupQueues()
    {
        _channel.ExchangeDeclare(_settings.ExchangeName, ExchangeType.Topic, true);

        var createQueue = _channel.QueueDeclare(Inventory.Domain.Constants.RoutingKeys.ProductCreated, true, false, false);
        var updateQueue = _channel.QueueDeclare(Inventory.Domain.Constants.RoutingKeys.ProductUpdated, true, false, false);
        var deleteQueue = _channel.QueueDeclare(Inventory.Domain.Constants.RoutingKeys.ProductDeleted, true, false, false);

        _channel.QueueBind(createQueue.QueueName, _settings.ExchangeName, Inventory.Domain.Constants.RoutingKeys.ProductCreated);
        _channel.QueueBind(updateQueue.QueueName, _settings.ExchangeName, Inventory.Domain.Constants.RoutingKeys.ProductUpdated);
        _channel.QueueBind(deleteQueue.QueueName, _settings.ExchangeName, Inventory.Domain.Constants.RoutingKeys.ProductDeleted);
    }

    public void StartConsuming(Action<string, string> onMessageReceived)
    {
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;
                var timestamp = DateTime.Now;

                _logger.LogInformation("Message received at {Timestamp} with routing key {RoutingKey}", timestamp, routingKey);
                onMessageReceived(routingKey, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
            }
        };

        _channel.BasicConsume(queue: Inventory.Domain.Constants.RoutingKeys.ProductCreated, autoAck: true, consumer: consumer);
        _channel.BasicConsume(queue: Inventory.Domain.Constants.RoutingKeys.ProductUpdated, autoAck: true, consumer: consumer);
        _channel.BasicConsume(queue: Inventory.Domain.Constants.RoutingKeys.ProductDeleted, autoAck: true, consumer: consumer);
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
} 