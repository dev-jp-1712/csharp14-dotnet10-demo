using Microsoft.Extensions.Logging;
using OrderService.Application.Orders;

namespace OrderService.Tests.Application.Orders;

public class PlaceOrderUseCaseTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ILogger<PlaceOrderUseCase>> _loggerMock;
    private readonly PlaceOrderUseCase _useCase;

    public PlaceOrderUseCaseTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _loggerMock = new Mock<ILogger<PlaceOrderUseCase>>();

        _useCase = new PlaceOrderUseCase(
            _orderRepositoryMock.Object,
            _customerRepositoryMock.Object,
            _productRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_PlacesOrder()
    {
        // Arrange
        var address = Address.Create("123 Main St", "New York", "10001", "USA");
        var customer = Customer.Create("John Doe", "john@example.com", "+1234567890", address);
        var product = Product.Create("Laptop", "High-end laptop", "LAP001", new Money(1000m, "USD"), 10, "Electronics");

        var request = new PlaceOrderRequest(
            customer.Id, // Use customer.Id from created entity
            new AddressDto("123 Main St", "New York", "10001", "USA"),
            [new OrderLineRequest(product.Id, 2)], // Use product.Id from created entity
            "USD");

        _customerRepositoryMock
            .Setup(x => x.FindByIdAsync(customer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _productRepositoryMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([product]);

        _orderRepositoryMock
            .Setup(x => x.GetNextSequenceAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.CustomerId.Should().Be(customer.Id); // Use customer.Id from created entity
        result.Status.Should().Be("Confirmed");
        result.Items.Should().HaveCount(1);
        result.Items[0].Quantity.Should().Be(2);

        _orderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _orderRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentCustomer_ThrowsCustomerNotFoundException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var request = new PlaceOrderRequest(
            customerId,
            new AddressDto("123 Main St", "New York", "10001", "USA"),
            [new OrderLineRequest(Guid.NewGuid(), 1)],
            "USD");

        _customerRepositoryMock
            .Setup(x => x.FindByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        // Act
        var act = () => _useCase.ExecuteAsync(request);

        // Assert
        await act.Should().ThrowAsync<CustomerNotFoundException>()
            .WithMessage($"*{customerId}*");
    }

    [Fact]
    public async Task ExecuteAsync_WithInactiveCustomer_ThrowsInactiveCustomerException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var address = Address.Create("123 Main St", "New York", "10001", "USA");
        var customer = Customer.Create("John Doe", "john@example.com", "+1234567890", address);
        customer.Deactivate();

        var request = new PlaceOrderRequest(
            customerId,
            new AddressDto("123 Main St", "New York", "10001", "USA"),
            [new OrderLineRequest(Guid.NewGuid(), 1)],
            "USD");

        _customerRepositoryMock
            .Setup(x => x.FindByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // Act
        var act = () => _useCase.ExecuteAsync(request);

        // Assert
        await act.Should().ThrowAsync<InactiveCustomerException>()
            .WithMessage($"*{customerId}*");
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentProduct_ThrowsProductNotFoundException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var address = Address.Create("123 Main St", "New York", "10001", "USA");
        var customer = Customer.Create("John Doe", "john@example.com", "+1234567890", address);

        var request = new PlaceOrderRequest(
            customerId,
            new AddressDto("123 Main St", "New York", "10001", "USA"),
            [new OrderLineRequest(productId, 1)],
            "USD");

        _customerRepositoryMock
            .Setup(x => x.FindByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _productRepositoryMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var act = () => _useCase.ExecuteAsync(request);

        // Assert
        await act.Should().ThrowAsync<ProductNotFoundException>()
            .WithMessage($"*{productId}*");
    }

    [Fact]
    public async Task ExecuteAsync_WithInsufficientStock_ThrowsInsufficientStockException()
    {
        // Arrange
        var address = Address.Create("123 Main St", "New York", "10001", "USA");
        var customer = Customer.Create("John Doe", "john@example.com", "+1234567890", address);
        var product = Product.Create("Laptop", "High-end laptop", "LAP001", new Money(1000m, "USD"), 2, "Electronics");

        var request = new PlaceOrderRequest(
            customer.Id, // Use customer.Id from created entity
            new AddressDto("123 Main St", "New York", "10001", "USA"),
            [new OrderLineRequest(product.Id, 10)], // Use product.Id from created entity
            "USD");

        _customerRepositoryMock
            .Setup(x => x.FindByIdAsync(customer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _productRepositoryMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([product]);

        _orderRepositoryMock
            .Setup(x => x.GetNextSequenceAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var act = () => _useCase.ExecuteAsync(request);

        // Assert
        await act.Should().ThrowAsync<InsufficientStockException>()
            .WithMessage($"*{product.Id}*"); // Use product.Id from created entity
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleProducts_PlacesOrderWithAllItems()
    {
        // Arrange
        var address = Address.Create("123 Main St", "New York", "10001", "USA");
        var customer = Customer.Create("John Doe", "john@example.com", "+1234567890", address);
        var product1 = Product.Create("Laptop", "High-end laptop", "LAP001", new Money(1000m, "USD"), 10, "Electronics");
        var product2 = Product.Create("Mouse", "Wireless mouse", "MOU001", new Money(50m, "USD"), 20, "Accessories");

        var request = new PlaceOrderRequest(
            customer.Id, // Use customer.Id from created entity
            new AddressDto("123 Main St", "New York", "10001", "USA"),
            [
                new OrderLineRequest(product1.Id, 2), // Use product1.Id from created entity
                new OrderLineRequest(product2.Id, 3)  // Use product2.Id from created entity
            ],
            "USD");

        _customerRepositoryMock
            .Setup(x => x.FindByIdAsync(customer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _productRepositoryMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([product1, product2]);

        _orderRepositoryMock
            .Setup(x => x.GetNextSequenceAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Subtotal.Should().Be(2150m);
    }
}
