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
    private static IServiceCollection AddRabbitMQ(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = new RabbitMQSettings();
        configuration.GetSection("RabbitMQ").Bind(settings);
        services.AddSingleton(settings);

        services.AddSingleton<ICircuitBreaker>(sp =>
            new CircuitBreaker(failureThreshold: 3, resetTimeoutSeconds: 30));
        services.AddSingleton<IRabbitMQConnection, RabbitMQConnection>();

        return services;
    }

    public static IServiceCollection AddRabbitMQPublisher(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IMessagePublisher, RabbitMQMessagePublisher>();
        return services
                .AddRabbitMQ(configuration);
    }

    public static IServiceCollection AddRabbitMQConsumer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IMessageConsumer, RabbitMQConsumer>();
        return services
             .AddRabbitMQ(configuration);
    }


}