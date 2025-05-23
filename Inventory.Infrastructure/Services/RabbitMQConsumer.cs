using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Microsoft.Extensions.Logging;
using Inventory.Domain.Interfaces;
using Inventory.Infrastructure.Configuration;
using Inventory.Domain.Constants;

namespace Inventory.Infrastructure.Services;

public class RabbitMQConsumer : IMessageConsumer
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly RabbitMQSettings _settings;
    private readonly ILogger<RabbitMQConsumer> _logger;

    public RabbitMQConsumer(
        IRabbitMQConnection connectionFactory,
        RabbitMQSettings settings,
        ILogger<RabbitMQConsumer> logger)
    {
        _connection = connectionFactory.GetConnection();
        _settings = settings;
        _logger = logger;
        _channel = _connection.CreateModel();
        SetupExchange();
    }

    private void SetupExchange()
    {
        _channel.ExchangeDeclare(_settings.ExchangeName, ExchangeType.Topic, true);
    }

    public void StartConsuming(Dictionary<RoutingKey, Action<string>> queueHandlers)
    {
        var stringKeyHandlers = queueHandlers.ToDictionary(
            kvp => kvp.Key.ToRoutingKeyString(),
            kvp => kvp.Value
        );
        foreach (var queueHandler in stringKeyHandlers)
        {
            var queue = _channel.QueueDeclare(queueHandler.Key, true, false, false);
            _channel.QueueBind(queue.QueueName, _settings.ExchangeName, queueHandler.Key);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var timestamp = DateTime.Now;

                    _logger.LogInformation("Message received at {Timestamp} from queue {Queue}", timestamp, queueHandler.Key);
                    queueHandler.Value(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message from queue {Queue}", queueHandler.Key);
                }
            };

            _channel.BasicConsume(queue: queueHandler.Key, autoAck: true, consumer: consumer);
        }

        _logger.LogInformation("Started consuming from {Count} queues", stringKeyHandlers.Count);
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
} 