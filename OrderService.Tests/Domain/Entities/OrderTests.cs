namespace OrderService.Tests.Domain.Entities;

public class OrderTests
{
    private readonly Address _shippingAddress;

    public OrderTests()
    {
        _shippingAddress = Address.Create("123 Main St", "New York", "10001", "USA");
    }

    [Fact]
    public void Create_WithValidInputs_CreatesOrderInPendingStatus()
    {
        // Arrange
        var orderNumber = OrderNumber.Generate(1);
        var customerId = Guid.NewGuid();

        // Act
        var order = Order.Create(orderNumber, customerId, _shippingAddress, "USD");

        // Assert
        order.Should().NotBeNull();
        order.OrderNumber.Should().Be(orderNumber);
        order.CustomerId.Should().Be(customerId);
        order.ShippingAddress.Should().Be(_shippingAddress);
        order.Currency.Should().Be("USD");
        order.Status.Should().Be(OrderStatus.Pending);
        order.Items.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithLowercaseCurrency_ConvertsToUppercase()
    {
        // Arrange
        var orderNumber = OrderNumber.Generate(1);
        var customerId = Guid.NewGuid();

        // Act
        var order = Order.Create(orderNumber, customerId, _shippingAddress, "eur");

        // Assert
        order.Currency.Should().Be("EUR");
    }

    [Fact]
    public void Create_WithNullOrderNumber_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => Order.Create(null!, Guid.NewGuid(), _shippingAddress, "USD");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_WithNullAddress_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => Order.Create(OrderNumber.Generate(1), Guid.NewGuid(), null!, "USD");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidCurrency_ThrowsArgumentException(string? currency)
    {
        // Arrange & Act
        var act = () => Order.Create(OrderNumber.Generate(1), Guid.NewGuid(), _shippingAddress, currency!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddItem_WhenPending_AddsItemSuccessfully()
    {
        // Arrange
        var order = CreatePendingOrder();
        var product = CreateProduct("Laptop", 1000m, 10);

        // Act
        var item = order.AddItem(product, 2);

        // Assert
        order.Items.Should().HaveCount(1);
        order.Items[0].Should().Be(item);
        order.Items[0].Quantity.Should().Be(2);
        product.StockQuantity.Should().Be(8);
    }

    [Fact]
    public void AddItem_SameProductTwice_CombinesQuantity()
    {
        // Arrange
        var order = CreatePendingOrder();
        var product = CreateProduct("Laptop", 1000m, 10);

        // Act
        order.AddItem(product, 2);
        order.AddItem(product, 3);

        // Assert
        order.Items.Should().HaveCount(1);
        order.Items[0].Quantity.Should().Be(5);
        product.StockQuantity.Should().Be(5);
    }

    [Fact]
    public void AddItem_WithInsufficientStock_ThrowsInsufficientStockException()
    {
        // Arrange
        var order = CreatePendingOrder();
        var product = CreateProduct("Laptop", 1000m, 5);

        // Act
        var act = () => order.AddItem(product, 10);

        // Assert
        act.Should().Throw<InsufficientStockException>()
            .WithMessage($"*{product.Id}*");
    }

    [Fact]
    public void AddItem_WhenNotPending_ThrowsInvalidOrderTransitionException()
    {
        // Arrange
        var order = CreateConfirmedOrder();
        var product = CreateProduct("Laptop", 1000m, 10);

        // Act
        var act = () => order.AddItem(product, 2);

        // Assert
        act.Should().Throw<InvalidOrderTransitionException>();
    }

    [Fact]
    public void Confirm_WhenPendingWithItems_TransitionsToConfirmed()
    {
        // Arrange
        var order = CreatePendingOrder();
        var product = CreateProduct("Laptop", 1000m, 10);
        order.AddItem(product, 1);

        // Act
        order.Confirm();

        // Assert
        order.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public void Confirm_WhenPendingWithoutItems_ThrowsEmptyOrderException()
    {
        // Arrange
        var order = CreatePendingOrder();

        // Act
        var act = () => order.Confirm();

        // Assert
        act.Should().Throw<EmptyOrderException>();
    }

    [Fact]
    public void Confirm_WhenNotPending_ThrowsInvalidOrderTransitionException()
    {
        // Arrange
        var order = CreateConfirmedOrder();

        // Act
        var act = () => order.Confirm();

        // Assert
        act.Should().Throw<InvalidOrderTransitionException>();
    }

    [Fact]
    public void RecordPayment_WhenConfirmed_TransitionsToPaid()
    {
        // Arrange
        var order = CreateConfirmedOrder();

        // Act
        order.RecordPayment();

        // Assert
        order.Status.Should().Be(OrderStatus.Paid);
        order.PaidAt.Should().NotBeNull();
        order.PaidAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void RecordPayment_WhenNotConfirmed_ThrowsInvalidOrderTransitionException()
    {
        // Arrange
        var order = CreatePendingOrder();

        // Act
        var act = () => order.RecordPayment();

        // Assert
        act.Should().Throw<InvalidOrderTransitionException>();
    }

    [Fact]
    public void Ship_WhenPaidWithTrackingNumber_TransitionsToShipped()
    {
        // Arrange
        var order = CreatePaidOrder();
        var trackingNumber = "TRACK123456";

        // Act
        order.Ship(trackingNumber);

        // Assert
        order.Status.Should().Be(OrderStatus.Shipped);
        order.TrackingNumber.Should().Be(trackingNumber);
        order.ShippedAt.Should().NotBeNull();
        order.ShippedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Ship_WithInvalidTrackingNumber_ThrowsArgumentException(string? trackingNumber)
    {
        // Arrange
        var order = CreatePaidOrder();

        // Act
        var act = () => order.Ship(trackingNumber!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Ship_WhenNotPaid_ThrowsInvalidOrderTransitionException()
    {
        // Arrange
        var order = CreateConfirmedOrder();

        // Act
        var act = () => order.Ship("TRACK123");

        // Assert
        act.Should().Throw<InvalidOrderTransitionException>();
    }

    [Fact]
    public void MarkDelivered_WhenShipped_TransitionsToDelivered()
    {
        // Arrange
        var order = CreateShippedOrder();

        // Act
        order.MarkDelivered();

        // Assert
        order.Status.Should().Be(OrderStatus.Delivered);
        order.DeliveredAt.Should().NotBeNull();
        order.DeliveredAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MarkDelivered_WhenNotShipped_ThrowsInvalidOrderTransitionException()
    {
        // Arrange
        var order = CreatePaidOrder();

        // Act
        var act = () => order.MarkDelivered();

        // Assert
        act.Should().Throw<InvalidOrderTransitionException>();
    }

    [Fact]
    public void Cancel_WhenPending_TransitionsToCancelled()
    {
        // Arrange
        var order = CreatePendingOrder();
        var product = CreateProduct("Laptop", 1000m, 10);
        order.AddItem(product, 2);
        var reason = "Customer changed mind";
        var initialStock = product.StockQuantity;

        // Act
        order.Cancel(reason, [product]);

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CancellationReason.Should().Be(reason);
        order.CancelledAt.Should().NotBeNull();
        product.StockQuantity.Should().Be(initialStock + 2); // Stock restored
    }

    [Fact]
    public void Cancel_WhenConfirmed_TransitionsToCancelled()
    {
        // Arrange
        var order = CreateConfirmedOrder();
        var reason = "Out of stock";

        // Act
        order.Cancel(reason, []);

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CancellationReason.Should().Be(reason);
    }

    [Fact]
    public void Cancel_WhenPaid_ThrowsInvalidOrderTransitionException()
    {
        // Arrange
        var order = CreatePaidOrder();

        // Act
        var act = () => order.Cancel("Reason", []);

        // Assert
        act.Should().Throw<InvalidOrderTransitionException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Cancel_WithInvalidReason_ThrowsArgumentException(string? reason)
    {
        // Arrange
        var order = CreatePendingOrder();

        // Act
        var act = () => order.Cancel(reason!, []);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Subtotal_WithMultipleItems_CalculatesCorrectly()
    {
        // Arrange
        var order = CreatePendingOrder();
        var product1 = CreateProduct("Laptop", 1000m, 10);
        var product2 = CreateProduct("Mouse", 50m, 20);

        // Act
        order.AddItem(product1, 2); // 2000
        order.AddItem(product2, 3); // 150

        // Assert
        order.Subtotal.Amount.Should().Be(2150m);
        order.Subtotal.Currency.Should().Be("USD");
    }

    [Fact]
    public void Subtotal_WithNoItems_ReturnsZero()
    {
        // Arrange
        var order = CreatePendingOrder();

        // Act & Assert
        order.Subtotal.Amount.Should().Be(0m);
    }

    // Helper methods
    private Order CreatePendingOrder()
    {
        return Order.Create(
            OrderNumber.Generate(1),
            Guid.NewGuid(),
            _shippingAddress,
            "USD");
    }

    private Order CreateConfirmedOrder()
    {
        var order = CreatePendingOrder();
        var product = CreateProduct("Laptop", 1000m, 10);
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

    private Order CreateShippedOrder()
    {
        var order = CreatePaidOrder();
        order.Ship("TRACK123456");
        return order;
    }

    private Product CreateProduct(string name, decimal price, int stockQuantity)
    {
        return Product.Create(name, $"Description for {name}", $"SKU-{name.ToUpperInvariant()}", new Money(price, "USD"), stockQuantity, "General");
    }
}
