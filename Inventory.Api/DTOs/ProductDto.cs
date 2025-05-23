using System.ComponentModel.DataAnnotations;

namespace Inventory.Api.DTOs;

/// <summary>
/// Represents a product in the inventory system
/// </summary>
public class ProductDto
{
    /// <summary>
    /// The unique identifier of the product
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The name of the product
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// A detailed description of the product
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The price of the product
    /// </summary>
    [Required]
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    /// <summary>
    /// The current stock quantity of the product
    /// </summary>
    [Required]
    [Range(0, int.MaxValue)]
    public int Stock { get; set; }
} 
 