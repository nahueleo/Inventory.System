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

/// <summary>
/// Controller for managing products in the inventory system
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets all products in the inventory
    /// </summary>
    /// <returns>A list of products</returns>
    /// <response code="200">Returns the list of products</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
    {
        var query = new GetProductsQuery();
        var products = await _mediator.Send(query);
        return Ok(products.Adapt<IEnumerable<ProductDto>>());
    }

    /// <summary>
    /// Gets a specific product by its ID
    /// </summary>
    /// <param name="id">The ID of the product</param>
    /// <returns>The requested product</returns>
    /// <response code="200">Returns the requested product</response>
    /// <response code="404">If the product is not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Creates a new product
    /// </summary>
    /// <param name="createProductDto">The product data</param>
    /// <returns>The created product</returns>
    /// <response code="201">Returns the newly created product</response>
    /// <response code="400">If the product data is invalid</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

    /// <summary>
    /// Updates an existing product
    /// </summary>
    /// <param name="id">The ID of the product to update</param>
    /// <param name="updateProductDto">The updated product data</param>
    /// <returns>No content</returns>
    /// <response code="204">If the product was successfully updated</response>
    /// <response code="400">If the product data is invalid</response>
    /// <response code="404">If the product is not found</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Deletes a product
    /// </summary>
    /// <param name="id">The ID of the product to delete</param>
    /// <returns>No content</returns>
    /// <response code="204">If the product was successfully deleted</response>
    /// <response code="404">If the product is not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var command = new DeleteProductCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }
} 