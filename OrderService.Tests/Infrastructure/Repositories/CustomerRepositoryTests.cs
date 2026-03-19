using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Repositories;

namespace OrderService.Tests.Infrastructure.Repositories;

public class CustomerRepositoryTests : IDisposable
{
    private readonly OrderDbContext _context;
    private readonly CustomerRepository _repository;

    public CustomerRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new OrderDbContext(options);
        _repository = new CustomerRepository(_context);
    }

    [Fact]
    public async Task AddAsync_AddsCustomerToDatabase()
    {
        // Arrange
        var address = Address.Create("123 Main St", "New York", "10001", "USA");
        var customer = Customer.Create("John Doe", "john@example.com", "+1234567890", address);

        // Act
        await _repository.AddAsync(customer);
        await _repository.SaveChangesAsync();

        // Assert
        var savedCustomer = await _repository.FindByIdAsync(customer.Id);
        savedCustomer.Should().NotBeNull();
        savedCustomer!.FullName.Should().Be("John Doe");
        savedCustomer.Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task FindByIdAsync_WithExistingId_ReturnsCustomer()
    {
        // Arrange
        var address = Address.Create("123 Main St", "New York", "10001", "USA");
        var customer = Customer.Create("John Doe", "john@example.com", "+1234567890", address);
        await _repository.AddAsync(customer);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.FindByIdAsync(customer.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(customer.Id);
        result.FullName.Should().Be("John Doe");
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
    public async Task FindByEmailAsync_WithExistingEmail_ReturnsCustomer()
    {
        // Arrange
        var address = Address.Create("123 Main St", "New York", "10001", "USA");
        var customer = Customer.Create("John Doe", "john@example.com", "+1234567890", address);
        await _repository.AddAsync(customer);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.FindByEmailAsync("john@example.com");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task FindByEmailAsync_WithNonExistentEmail_ReturnsNull()
    {
        // Act
        var result = await _repository.FindByEmailAsync("nonexistent@example.com");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllCustomers()
    {
        // Arrange
        var address1 = Address.Create("123 Main St", "New York", "10001", "USA");
        var address2 = Address.Create("456 Elm St", "Boston", "02101", "USA");
        var customer1 = Customer.Create("John Doe", "john@example.com", "+1234567890", address1);
        var customer2 = Customer.Create("Jane Smith", "jane@example.com", "+0987654321", address2);
        await _repository.AddAsync(customer1);
        await _repository.AddAsync(customer2);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCountGreaterOrEqualTo(2);
        result.Should().Contain(c => c.FullName == "John Doe");
        result.Should().Contain(c => c.FullName == "Jane Smith");
    }

    [Fact(Skip = "GetActiveCustomersAsync method not yet implemented")]
    public async Task GetActiveCustomersAsync_ReturnsOnlyActiveCustomers()
    {
        // TODO: Implement GetActiveCustomersAsync in CustomerRepository
        await Task.CompletedTask;
        /*
        // Arrange
        var address1 = Address.Create("123 Main St", "New York", "10001", "USA");
        var address2 = Address.Create("456 Elm St", "Boston", "02101", "USA");
        var customer1 = Customer.Create("Active Customer", "active@example.com", "+1234567890", address1);
        var customer2 = Customer.Create("Inactive Customer", "inactive@example.com", "+0987654321", address2);
        customer2.Deactivate();

        await _repository.AddAsync(customer1);
        await _repository.AddAsync(customer2);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveCustomersAsync();

        // Assert
        result.Should().Contain(c => c.FullName == "Active Customer");
        result.Should().NotContain(c => c.FullName == "Inactive Customer");
        */
    }

    [Fact]
    public async Task UpdateAsync_UpdatesCustomerInDatabase()
    {
        // Arrange
        var address = Address.Create("123 Main St", "New York", "10001", "USA");
        var customer = Customer.Create("John Doe", "john@example.com", "+1234567890", address);
        await _repository.AddAsync(customer);
        await _repository.SaveChangesAsync();

        customer.UpdateContactInfo("John Smith", "+1111111111");

        // Act
        await _repository.SaveChangesAsync();

        // Assert
        var updatedCustomer = await _repository.FindByIdAsync(customer.Id);
        updatedCustomer!.FullName.Should().Be("John Smith");
        updatedCustomer.PhoneNumber.Should().Be("+1111111111");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
