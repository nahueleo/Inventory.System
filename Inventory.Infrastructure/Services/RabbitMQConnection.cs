using Inventory.Infrastructure.Configuration;
using Inventory.Infrastructure.Resilience;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Inventory.Infrastructure.Services;

public class RabbitMQConnection : IRabbitMQConnection
{
    private readonly RabbitMQSettings _settings;
    private readonly ILogger<RabbitMQConnection> _logger;
    private readonly ICircuitBreaker _circuitBreaker;
    private IConnection? _connection;

    public RabbitMQConnection(
        RabbitMQSettings settings,
        ILogger<RabbitMQConnection> logger,
        ICircuitBreaker circuitBreaker)
    {
        _settings = settings;
        _logger = logger;
        _circuitBreaker = circuitBreaker;
    }

    public IConnection GetConnection()
    {
        if (_connection?.IsOpen ?? false)
            return _connection;

        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            UserName = _settings.UserName,
            Password = _settings.Password,
            Port = _settings.Port,
            VirtualHost = _settings.VirtualHost,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        try
        {
            _logger.LogInformation("Creating RabbitMQ connection to {HostName}:{Port}", _settings.HostName, _settings.Port);
            _connection = _circuitBreaker.ExecuteAsync(() => Task.FromResult(factory.CreateConnection())).GetAwaiter().GetResult();
            return _connection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create RabbitMQ connection");
            throw new Exception($"Failed to create RabbitMQ connection: {ex.Message}", ex);
        }
    }
} 