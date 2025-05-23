using Microsoft.AspNetCore.Mvc;
using Inventory.Domain.Features.Products.Commands.CreateProduct;
using Inventory.Domain.Features.Products.Commands.UpdateProduct;
using Inventory.Domain.Features.Products.Commands.DeleteProduct;
using Inventory.Domain.Features.Products.Queries.GetProducts;
using Inventory.Domain.Features.Products.Queries.GetProductById;
using Inventory.Api.DTOs;
using Mapster;
using MediatR;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
    {
        var query = new GetProductsQuery();
        var products = await _mediator.Send(query);
        return Ok(products.Adapt<IEnumerable<ProductDto>>());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var query = new GetProductByIdQuery { Id = id };
        var product = await _mediator.Send(query);
        
        if (product == null)
        {
            return NotFound();
        }
        
        return Ok(product.Adapt<ProductDto>());
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto createProductDto)
    {
        var command = new CreateProductCommand
        {
            Name = createProductDto.Name,
            Description = createProductDto.Description,
            Price = createProductDto.Price,
            Stock = createProductDto.Stock
        };

        var productId = await _mediator.Send(command);
        var product = await _mediator.Send(new GetProductByIdQuery { Id = productId });
        
        return CreatedAtAction(nameof(GetProduct), new { id = productId }, product.Adapt<ProductDto>());
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, UpdateProductDto updateProductDto)
    {
        var command = new UpdateProductCommand
        {
            Id = id,
            Name = updateProductDto.Name,
            Description = updateProductDto.Description,
            Price = updateProductDto.Price,
            Stock = updateProductDto.Stock
        };

        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var command = new DeleteProductCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }
} 