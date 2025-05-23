using MediatR;

namespace Inventory.Domain.Features.Products.Commands.UpdateProduct;

public record UpdateProductCommand : IRequest<int>
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int Stock { get; init; }

} 