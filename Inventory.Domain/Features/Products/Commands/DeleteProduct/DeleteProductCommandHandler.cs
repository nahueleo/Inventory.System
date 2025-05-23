using MediatR;
using Inventory.Domain.Interfaces;
using Inventory.Domain.Constants;

namespace Inventory.Domain.Features.Products.Commands.DeleteProduct;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, int>
{
    private readonly IProductRepository _productRepository;
    private readonly IMessagePublisher _messagePublisher;

    public DeleteProductCommandHandler(IProductRepository productRepository, IMessagePublisher messagePublisher)
    {
        _productRepository = productRepository;
        _messagePublisher = messagePublisher;
    }

    public async Task<int> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id);
        if (product == null)
            throw new KeyNotFoundException($"Product with ID {request.Id} not found.");

        await _productRepository.DeleteAsync(product.Id);
        await _messagePublisher.PublishAsync(product, RoutingKey.ProductDeleted);

        return product.Id;
    }
} 