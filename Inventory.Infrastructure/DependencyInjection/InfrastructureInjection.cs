using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.Infrastructure.DependencyInjection;

public static class InfrastructureInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Database
        services.AddDatabase(configuration);

        // Add RabbitMQ
        services.AddRabbitMQ(configuration);

        return services;
    }
} 