using RabbitMQ.Client;

namespace Inventory.Infrastructure.Services;

public interface IRabbitMQConnection
{
    IConnection GetConnection();
} 