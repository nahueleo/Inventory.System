using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Inventory.Infrastructure.Configuration;
using Inventory.Domain.Interfaces;
using Inventory.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Inventory.Infrastructure.Resilience;

namespace Inventory.Infrastructure.DependencyInjection;

public static class RabbitMQInjection
{
    public static IServiceCollection AddRabbitMQ(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure RabbitMQ settings
        var rabbitMQSettings = configuration.GetSection("RabbitMQ").Get<RabbitMQSettings>();
        services.AddSingleton(rabbitMQSettings ?? new RabbitMQSettings());
        
        // Register Circuit Breaker
        services.AddSingleton<ICircuitBreaker>(sp => 
            new CircuitBreaker(failureThreshold: 3, resetTimeoutSeconds: 30));
        
        // Register RabbitMQ connection
        services.AddSingleton<IConnection>(sp =>
        {
            var settings = sp.GetRequiredService<RabbitMQSettings>();
            var logger = sp.GetRequiredService<ILogger<RabbitMQMessagePublisher>>();
            var circuitBreaker = sp.GetRequiredService<ICircuitBreaker>();
            
            var factory = new ConnectionFactory
            {
                HostName = settings.HostName,
                UserName = settings.UserName,
                Password = settings.Password,
                Port = settings.Port,
                VirtualHost = settings.VirtualHost,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            try
            {
                logger.LogInformation("Creating RabbitMQ connection to {HostName}:{Port}", settings.HostName, settings.Port);
                return circuitBreaker.ExecuteAsync(() => Task.FromResult(factory.CreateConnection())).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create RabbitMQ connection");
                throw new Exception($"Failed to create RabbitMQ connection: {ex.Message}", ex);
            }
        });

        // Register message publisher
        services.AddScoped<IMessagePublisher, RabbitMQMessagePublisher>();

        // Register consumer
        services.AddSingleton<RabbitMQConsumer>();

        return services;
    }
} 