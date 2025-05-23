using Inventory.Domain.Interfaces;
using Inventory.Domain.Constants;

namespace Inventory.Worker.Handlers;

public class ProductDeletedHandler : IMessageHandler
{
    private readonly ILogger<ProductDeletedHandler> _logger;

    public ProductDeletedHandler(ILogger<ProductDeletedHandler> logger)
    {
        _logger = logger;
    }

    public string QueueName => RoutingKeys.ProductDeleted;

    public async Task HandleAsync(string message)
    {
        MessageFormatter.WriteFormattedMessage("Product Deleted", message, _logger);
        await Task.CompletedTask;
    }
} 