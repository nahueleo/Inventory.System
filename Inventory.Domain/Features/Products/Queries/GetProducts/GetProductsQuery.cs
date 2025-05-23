using MediatR;
using Inventory.Domain.Entities;

namespace Inventory.Domain.Features.Products.Queries.GetProducts;

public record GetProductsQuery : IRequest<IEnumerable<Product>>; 