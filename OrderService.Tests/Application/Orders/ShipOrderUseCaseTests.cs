using Microsoft.Extensions.Logging;
using OrderService.Application.Orders;

namespace OrderService.Tests.Application.Orders;

public class ShipOrderUseCaseTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<ILogger<ShipOrderUseCase>> _loggerMock;
    private readonly ShipOrderUseCase _useCase;

    public ShipOrderUseCaseTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _loggerMock = new Mock<ILogger<ShipOrderUseCase>>();
        _useCase = new ShipOrderUseCase(_orderRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithPaidOrder_ShipsOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = CreatePaidOrder();
        var request = new ShipOrderRequest("TRACK-123456");

        _orderRepositoryMock
            .Setup(x => x.FindByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _useCase.ExecuteAsync(orderId, request);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Shipped");
        result.TrackingNumber.Should().Be("TRACK-123456");
        result.ShippedAt.Should().NotBeNull();

        _orderRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentOrder_ThrowsOrderNotFoundException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new ShipOrderRequest("TRACK-123456");

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
    public async Task ExecuteAsync_WithUnpaidOrder_ThrowsInvalidOrderTransitionException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = CreateConfirmedOrder();
        var request = new ShipOrderRequest("TRACK-123456");

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
