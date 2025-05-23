using System.ComponentModel.DataAnnotations;

namespace Inventory.Api.DTOs;

/// <summary>
/// Data transfer object for creating a new product
/// </summary>
public class CreateProductDto
{
    /// <summary>
    /// The name of the product
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// A detailed description of the product
    /// </summary>
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The price of the product
    /// </summary>
    [Required]
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    /// <summary>
    /// The initial stock quantity of the product
    /// </summary>
    [Required]
    [Range(0, int.MaxValue)]
    public int Stock { get; set; }
} 