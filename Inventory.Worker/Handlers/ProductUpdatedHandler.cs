using Inventory.Domain.Interfaces;
using Inventory.Domain.Constants;

namespace Inventory.Worker.Handlers;

public class ProductUpdatedHandler : IMessageHandler
{
    private readonly ILogger<ProductUpdatedHandler> _logger;

    public ProductUpdatedHandler(ILogger<ProductUpdatedHandler> logger)
    {
        _logger = logger;
    }

    public string QueueName => RoutingKeys.ProductUpdated;

    public async Task HandleAsync(string message)
    {
        MessageFormatter.WriteFormattedMessage("Product Updated", message, _logger);
        await Task.CompletedTask;
    }
} 