using Inventory.Domain.Entities;
using Inventory.Infrastructure.Data;
using Inventory.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Inventory.Tests.Infrastructure.Repositories;

public class ProductRepositoryTests
{
    private readonly InventoryDbContext _context;
    private readonly ProductRepository _repository;

    public ProductRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new InventoryDbContext(options);
        _repository = new ProductRepository(_context);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Name = "Product 1", Description = "Description 1", Price = 10.99m, Stock = 100 },
            new() { Name = "Product 2", Description = "Description 2", Price = 20.99m, Stock = 200 }
        };
        await _context.Products.AddRangeAsync(products);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, p => p.Name == "Product 1");
        Assert.Contains(result, p => p.Name == "Product 2");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnProduct_WhenProductExists()
    {
        // Arrange
        var product = new Product { Name = "Test Product", Description = "Test Description", Price = 15.99m, Stock = 50 };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(product.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(product.Name, result.Name);
        Assert.Equal(product.Description, result.Description);
        Assert.Equal(product.Price, result.Price);
        Assert.Equal(product.Stock, result.Stock);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenProductDoesNotExist()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_ShouldAddProduct()
    {
        // Arrange
        var product = new Product { Name = "New Product", Description = "New Description", Price = 25.99m, Stock = 75 };

        // Act
        var result = await _repository.AddAsync(product);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        var savedProduct = await _context.Products.FindAsync(result.Id);
        Assert.NotNull(savedProduct);
        Assert.Equal(product.Name, savedProduct.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateProduct()
    {
        // Arrange
        var product = new Product { Name = "Original Name", Description = "Original Description", Price = 10.99m, Stock = 100 };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        product.Name = "Updated Name";
        product.Price = 20.99m;
        var result = await _repository.UpdateAsync(product);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal(20.99m, result.Price);
        var updatedProduct = await _context.Products.FindAsync(product.Id);
        Assert.NotNull(updatedProduct);
        Assert.Equal("Updated Name", updatedProduct.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteProduct()
    {
        // Arrange
        var product = new Product { Name = "Product to Delete", Description = "Description", Price = 10.99m, Stock = 100 };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(product.Id);

        // Assert
        var deletedProduct = await _context.Products.FindAsync(product.Id);
        Assert.Null(deletedProduct);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenProductExists()
    {
        // Arrange
        var product = new Product { Name = "Test Product", Description = "Description", Price = 10.99m, Stock = 100 };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(product.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenProductDoesNotExist()
    {
        // Act
        var result = await _repository.ExistsAsync(999);

        // Assert
        Assert.False(result);
    }
} 