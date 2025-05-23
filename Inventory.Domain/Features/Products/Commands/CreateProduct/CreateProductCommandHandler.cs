using MediatR;
using Inventory.Domain.Interfaces;
using Inventory.Domain.Entities;
using Inventory.Domain.Constants;

namespace Inventory.Domain.Features.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, int>
{
    private readonly IProductRepository _productRepository;
    private readonly IMessagePublisher _messagePublisher;

    public CreateProductCommandHandler(IProductRepository productRepository, IMessagePublisher messagePublisher)
    {
        _productRepository = productRepository;
        _messagePublisher = messagePublisher;
    }

    public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock
        };

        var createdProduct = await _productRepository.AddAsync(product);
        await _messagePublisher.PublishAsync(createdProduct, RabbitMQConstants.RoutingKeys.ProductCreated);

        return createdProduct.Id;
    }
} 