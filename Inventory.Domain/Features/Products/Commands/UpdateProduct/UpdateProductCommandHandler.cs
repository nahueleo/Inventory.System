using MediatR;
using Inventory.Domain.Interfaces;
using Inventory.Domain.Constants;

namespace Inventory.Domain.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, int>
{
    private readonly IProductRepository _productRepository;
    private readonly IMessagePublisher _messagePublisher;

    public UpdateProductCommandHandler(IProductRepository productRepository, IMessagePublisher messagePublisher)
    {
        _productRepository = productRepository;
        _messagePublisher = messagePublisher;
    }

    public async Task<int> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id);
        if (product == null)
            throw new KeyNotFoundException($"Product with ID {request.Id} not found.");

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.UpdatedAt = DateTime.Now;

        await _productRepository.UpdateAsync(product);
        await _messagePublisher.PublishAsync(product, RoutingKey.ProductUpdated);

        return product.Id;
    }
} 