using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Repositories;

namespace OrderService.Tests.Infrastructure.Repositories;

public class ProductRepositoryTests : IDisposable
{
    private readonly OrderDbContext _context;
    private readonly ProductRepository _repository;

    public ProductRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new OrderDbContext(options);
        _repository = new ProductRepository(_context);
    }

    [Fact]
    public async Task AddAsync_AddsProductToDatabase()
    {
        // Arrange
        var product = Product.Create("Laptop", "High-end laptop", "LAP001", new Money(1000m, "USD"), 10, "Electronics");

        // Act
        await _repository.AddAsync(product);
        await _repository.SaveChangesAsync();

        // Assert
        var savedProduct = await _repository.FindByIdAsync(product.Id);
        savedProduct.Should().NotBeNull();
        savedProduct!.Name.Should().Be("Laptop");
    }

    [Fact]
    public async Task FindByIdAsync_WithExistingId_ReturnsProduct()
    {
        // Arrange
        var product = Product.Create("Laptop", "High-end laptop", "LAP001", new Money(1000m, "USD"), 10, "Electronics");
        await _repository.AddAsync(product);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.FindByIdAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
        result.Name.Should().Be("Laptop");
    }

    [Fact]
    public async Task FindByIdAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.FindByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllProducts()
    {
        // Arrange
        var product1 = Product.Create("Laptop", "High-end laptop", "LAP001", new Money(1000m, "USD"), 10, "Electronics");
        var product2 = Product.Create("Mouse", "Wireless mouse", "MOU001", new Money(50m, "USD"), 20, "Accessories");
        await _repository.AddAsync(product1);
        await _repository.AddAsync(product2);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCountGreaterOrEqualTo(2);
        result.Should().Contain(p => p.Name == "Laptop");
        result.Should().Contain(p => p.Name == "Mouse");
    }

    [Fact]
    public async Task GetByIdsAsync_ReturnsMatchingProducts()
    {
        // Arrange
        var product1 = Product.Create("Laptop", "High-end laptop", "LAP001", new Money(1000m, "USD"), 10, "Electronics");
        var product2 = Product.Create("Mouse", "Wireless mouse", "MOU001", new Money(50m, "USD"), 20, "Accessories");
        var product3 = Product.Create("Keyboard", "Mechanical keyboard", "KEY001", new Money(100m, "USD"), 15, "Accessories");
        
        await _repository.AddAsync(product1);
        await _repository.AddAsync(product2);
        await _repository.AddAsync(product3);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdsAsync([product1.Id, product3.Id]);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Id == product1.Id);
        result.Should().Contain(p => p.Id == product3.Id);
        result.Should().NotContain(p => p.Id == product2.Id);
    }

    [Fact]
    public async Task GetByIdsAsync_WithEmptyList_ReturnsEmptyList()
    {
        // Act
        var result = await _repository.GetByIdsAsync([]);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_UpdatesProductInDatabase()
    {
        // Arrange
        var product = Product.Create("Laptop", "High-end laptop", "LAP001", new Money(1000m, "USD"), 10, "Electronics");
        await _repository.AddAsync(product);
        await _repository.SaveChangesAsync();

        var newPrice = new Money(1200m, "USD");
        product.UpdatePrice(newPrice);

        // Act
        await _repository.SaveChangesAsync();

        // Assert
        var updatedProduct = await _repository.FindByIdAsync(product.Id);
        updatedProduct!.UnitPrice.Amount.Should().Be(1200m);
    }

    [Fact(Skip = "GetLowStockProductsAsync method not yet implemented")]
    public async Task GetLowStockProductsAsync_ReturnsProductsBelowThreshold()
    {
        // TODO: Implement GetLowStockProductsAsync in ProductRepository
        await Task.CompletedTask;
        /*
        // Arrange
        var product1 = Product.Create("Laptop", "High-end laptop", "LAP001", new Money(1000m, "USD"), 2, "Electronics");
        var product2 = Product.Create("Mouse", "Wireless mouse", "MOU001", new Money(50m, "USD"), 20, "Accessories");
        var product3 = Product.Create("Keyboard", "Mechanical keyboard", "KEY001", new Money(100m, "USD"), 4, "Accessories");

        await _repository.AddAsync(product1);
        await _repository.AddAsync(product2);
        await _repository.AddAsync(product3);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetLowStockProductsAsync(5);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Name == "Laptop");
        result.Should().Contain(p => p.Name == "Keyboard");
        result.Should().NotContain(p => p.Name == "Mouse");
        */
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
