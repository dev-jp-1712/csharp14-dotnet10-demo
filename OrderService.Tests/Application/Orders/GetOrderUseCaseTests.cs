using Microsoft.Extensions.Logging;
using OrderService.Application.Orders;

namespace OrderService.Tests.Application.Orders;

public class GetOrderUseCaseTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly GetOrderUseCase _useCase;

    public GetOrderUseCaseTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _useCase = new GetOrderUseCase(_orderRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingOrder_ReturnsOrderResponse()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = CreateConfirmedOrder();

        _orderRepositoryMock
            .Setup(x => x.FindByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _useCase.ExecuteAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Confirmed");
        result.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentOrder_ThrowsOrderNotFoundException()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _orderRepositoryMock
            .Setup(x => x.FindByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        var act = () => _useCase.ExecuteAsync(orderId);

        // Assert
        await act.Should().ThrowAsync<OrderNotFoundException>()
            .WithMessage($"*{orderId}*");
    }

    // Helper methods
    private Order CreateConfirmedOrder()
    {
        var address = Address.Create("123 Main St", "New York", "10001", "USA");
        var order = Order.Create(OrderNumber.Generate(1), Guid.NewGuid(), address, "USD");
        var product = Product.Create("Laptop", "High-end laptop", "LAP001", new Money(1000m, "USD"), 10, "Electronics");
        order.AddItem(product, 1);
        order.Confirm();
        return order;
    }
}
