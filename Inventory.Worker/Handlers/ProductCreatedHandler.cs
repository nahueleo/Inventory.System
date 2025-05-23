using Inventory.Domain.Interfaces;
using Inventory.Domain.Constants;

namespace Inventory.Worker.Handlers;

public class ProductCreatedHandler : IMessageHandler
{
    private readonly ILogger<ProductCreatedHandler> _logger;

    public ProductCreatedHandler(ILogger<ProductCreatedHandler> logger)
    {
        _logger = logger;
    }

    public string QueueName => RoutingKeys.ProductCreated;

    public async Task HandleAsync(string message)
    {
        MessageFormatter.WriteFormattedMessage("Product Created", message, _logger);
        await Task.CompletedTask;
    }
} 