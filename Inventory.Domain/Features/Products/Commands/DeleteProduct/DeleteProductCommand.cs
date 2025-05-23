using MediatR;

namespace Inventory.Domain.Features.Products.Commands.DeleteProduct;

public record DeleteProductCommand : IRequest<int>
{
    public int Id { get; init; }
} 