using Microsoft.Extensions.Logging;
using OrderService.Application.Orders;

namespace OrderService.Tests.Application.Orders;

public class CancelOrderUseCaseTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ILogger<CancelOrderUseCase>> _loggerMock;
    private readonly CancelOrderUseCase _useCase;

    public CancelOrderUseCaseTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _loggerMock = new Mock<ILogger<CancelOrderUseCase>>();
        _useCase = new CancelOrderUseCase(
            _orderRepositoryMock.Object,
            _productRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithPendingOrder_CancelsOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = CreatePendingOrderWithItems();
        var product = Product.Create("Laptop", "High-end laptop", "LAP001", new Money(1000m, "USD"), 10, "Electronics");
        var request = new CancelOrderRequest("Customer changed mind");

        _orderRepositoryMock
            .Setup(x => x.FindByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _productRepositoryMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([product]);

        // Act
        var result = await _useCase.ExecuteAsync(orderId, request);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Cancelled");
        result.CancellationReason.Should().Be("Customer changed mind");
        result.CancelledAt.Should().NotBeNull();

        _orderRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _productRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithConfirmedOrder_CancelsOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = CreateConfirmedOrder();
        var product = Product.Create("Laptop", "High-end laptop", "LAP001", new Money(1000m, "USD"), 10, "Electronics");
        var request = new CancelOrderRequest("Out of stock");

        _orderRepositoryMock
            .Setup(x => x.FindByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _productRepositoryMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([product]);

        // Act
        var result = await _useCase.ExecuteAsync(orderId, request);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Cancelled");
        result.CancellationReason.Should().Be("Out of stock");
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentOrder_ThrowsOrderNotFoundException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new CancelOrderRequest("Reason");

        _orderRepositoryMock
            .Setup(x => x.FindByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        var act = () => _useCase.ExecuteAsync(orderId, request);

        // Assert
        await act.Should().ThrowAsync<OrderNotFoundException>()
            .WithMessage($"*{orderId}*");
    }

    [Fact]
    public async Task ExecuteAsync_WithPaidOrder_ThrowsInvalidOrderTransitionException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = CreatePaidOrder();
        var request = new CancelOrderRequest("Reason");

        _orderRepositoryMock
            .Setup(x => x.FindByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var act = () => _useCase.ExecuteAsync(orderId, request);

        // Assert
        await act.Should().ThrowAsync<InvalidOrderTransitionException>();
    }

    // Helper methods
    private Order CreatePendingOrder()
    {
        var address = Address.Create("123 Main St", "New York", "10001", "USA");
        return Order.Create(OrderNumber.Generate(1), Guid.NewGuid(), address, "USD");
    }

    private Order CreatePendingOrderWithItems()
    {
        var order = CreatePendingOrder();
        var product = Product.Create("Laptop", "High-end laptop", "LAP001", new Money(1000m, "USD"), 10, "Electronics");
        order.AddItem(product, 1);
        return order;
    }

    private Order CreateConfirmedOrder()
    {
        var order = CreatePendingOrder();
        var product = Product.Create("Laptop", "High-end laptop", "LAP001", new Money(1000m, "USD"), 10, "Electronics");
        order.AddItem(product, 1);
        order.Confirm();
        return order;
    }

    private Order CreatePaidOrder()
    {
        var order = CreateConfirmedOrder();
        order.RecordPayment();
        return order;
    }
}
