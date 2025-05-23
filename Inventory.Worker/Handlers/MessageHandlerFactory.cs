using Inventory.Domain.Interfaces;

namespace Inventory.Worker.Handlers;

public delegate IMessageHandler MessageHandlerFactory(MessageAction action);

public static class MessageHandlerFactoryExtensions
{
    public static IServiceCollection AddMessageHandlers(this IServiceCollection services)
    {
        services.AddTransient<MessageHandlerFactory>(sp => 
        {
            return action => action switch
            {
                MessageAction.Create => sp.GetRequiredService<ProductCreatedHandler>(),
                MessageAction.Update => sp.GetRequiredService<ProductUpdatedHandler>(),
                MessageAction.Delete => sp.GetRequiredService<ProductDeletedHandler>(),
                _ => throw new ArgumentException($"No handler found for action: {action}")
            };
        });

        services.AddTransient<ProductCreatedHandler>();
        services.AddTransient<ProductUpdatedHandler>();
        services.AddTransient<ProductDeletedHandler>();

        return services;
    }
} 