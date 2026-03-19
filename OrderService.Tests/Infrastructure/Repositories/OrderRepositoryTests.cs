using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Repositories;

namespace OrderService.Tests.Infrastructure.Repositories;

public class OrderRepositoryTests : IDisposable
{
    private readonly OrderDbContext _context;
    private readonly OrderRepository _repository;

    public OrderRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new OrderDbContext(options);
        _repository = new OrderRepository(_context);
    }

    [Fact]
    public async Task AddAsync_AddsOrderToDatabase()
    {
        // Arrange
        var order = CreateTestOrder();

        // Act
        await _repository.AddAsync(order);
        await _repository.SaveChangesAsync();

        // Assert
        var savedOrder = await _repository.FindByIdAsync(order.Id);
        savedOrder.Should().NotBeNull();
        savedOrder!.OrderNumber.Should().Be(order.OrderNumber);
    }

    [Fact]
    public async Task FindByIdAsync_WithExistingId_ReturnsOrder()
    {
        // Arrange
        var order = CreateTestOrder();
        await _repository.AddAsync(order);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.FindByIdAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(order.Id);
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
    public async Task FindByOrderNumberAsync_WithExistingOrderNumber_ReturnsOrder()
    {
        // Arrange
        var order = CreateTestOrder();
        await _repository.AddAsync(order);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.FindByOrderNumberAsync(order.OrderNumber.Value);

        // Assert
        result.Should().NotBeNull();
        result!.OrderNumber.Should().Be(order.OrderNumber);
    }

    [Fact]
    public async Task GetNextSequenceAsync_ReturnsIncrementingSequence()
    {
        // Act
        var sequence1 = await _repository.GetNextSequenceAsync();
        var sequence2 = await _repository.GetNextSequenceAsync();
        var sequence3 = await _repository.GetNextSequenceAsync();

        // Assert
        sequence1.Should().Be(1);
        sequence2.Should().Be(2);
        sequence3.Should().Be(3);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllOrders()
    {
        // Arrange
        var order1 = CreateTestOrder();
        var order2 = CreateTestOrder();
        await _repository.AddAsync(order1);
        await _repository.AddAsync(order2);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCountGreaterOrEqualTo(2);
    }

    [Fact]
    public async Task GetByCustomerIdAsync_ReturnsCustomerOrders()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var order1 = CreateTestOrder(customerId);
        var order2 = CreateTestOrder(customerId);
        var order3 = CreateTestOrder(Guid.NewGuid());
        
        await _repository.AddAsync(order1);
        await _repository.AddAsync(order2);
        await _repository.AddAsync(order3);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByCustomerIdAsync(customerId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(o => o.CustomerId.Should().Be(customerId));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesOrderInDatabase()
    {
        // Arrange
        var order = CreateTestOrder();
        await _repository.AddAsync(order);
        await _repository.SaveChangesAsync();

        var product = Product.Create("Laptop", "High-end laptop", "LAP001", new Money(1000m, "USD"), 10, "Electronics");
        order.AddItem(product, 1);
        order.Confirm();

        // Act
        await _repository.SaveChangesAsync();

        // Assert
        var updatedOrder = await _repository.FindByIdAsync(order.Id);
        updatedOrder!.Status.Should().Be(OrderStatus.Confirmed);
        updatedOrder.Items.Should().HaveCount(1);
    }

    private Order CreateTestOrder(Guid? customerId = null)
    {
        var address = Address.Create("123 Main St", "New York", "10001", "USA");
        var orderNumber = OrderNumber.Generate(Random.Shared.Next(1, 100000));
        return Order.Create(
            orderNumber,
            customerId ?? Guid.NewGuid(),
            address,
            "USD");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
