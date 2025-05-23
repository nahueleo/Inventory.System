using MediatR;
using Inventory.Domain.Entities;

namespace Inventory.Domain.Features.Products.Queries.GetProductById;

public record GetProductByIdQuery : IRequest<Product?>
{
    public int Id { get; init; }
} 