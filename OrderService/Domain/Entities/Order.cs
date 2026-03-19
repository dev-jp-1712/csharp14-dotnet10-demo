using OrderService.Domain.Enums;
using OrderService.Domain.Exceptions;
using OrderService.Domain.ValueObjects;

namespace OrderService.Domain.Entities;

/// <summary>
/// Order aggregate root. All order state transitions go through this class.
/// Domain rules are enforced here — no service or controller bypasses this logic.
/// </summary>
public sealed class Order : Entity
{
    private readonly List<OrderItem> _items = [];

    public OrderNumber OrderNumber { get; private set; }
    public Guid CustomerId { get; private set; }
    public Address ShippingAddress { get; private set; }
    public OrderStatus Status { get; private set; }
    public string Currency { get; private set; }
    public string? CancellationReason { get; private set; }
    public string? TrackingNumber { get; private set; }
    public DateTimeOffset? PaidAt { get; private set; }
    public DateTimeOffset? ShippedAt { get; private set; }
    public DateTimeOffset? DeliveredAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }

    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    public Money Subtotal => _items.Aggregate(
        Money.Zero(Currency),
        (acc, item) => acc.Add(item.LineTotal));

    private Order()
    {
        OrderNumber = default!;
        ShippingAddress = default!;
        Currency = "USD";
    }

    /// <summary>Factory: creates a confirmed order from validated inputs.</summary>
    public static Order Create(
        OrderNumber orderNumber,
        Guid customerId,
        Address shippingAddress,
        string currency)
    {
        ArgumentNullException.ThrowIfNull(orderNumber);
        ArgumentNullException.ThrowIfNull(shippingAddress);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        return new Order
        {
            OrderNumber = orderNumber,
            CustomerId = customerId,
            ShippingAddress = shippingAddress,
            Currency = currency.ToUpperInvariant(),
            Status = OrderStatus.Pending
        };
    }

    /// <summary>Adds a product line to this order while it is still pending.</summary>
    public OrderItem AddItem(Product product, int quantity)
    {
        EnsureStatus(OrderStatus.Pending, "add items to");

        if (!product.TryReserveStock(quantity))
            throw new InsufficientStockException(product.Id, quantity, product.StockQuantity + quantity);

        var existing = _items.Find(i => i.ProductId == product.Id);
        if (existing is not null)
        {
            // Re-create item with combined quantity (snapshot price stays from first add)
            _items.Remove(existing);
        }

        var item = OrderItem.Create(Id, product, quantity);
        _items.Add(item);
        MarkUpdated();
        return item;
    }

    /// <summary>Confirms a pending order — transitions Pending ? Confirmed.</summary>
    public void Confirm()
    {
        EnsureStatus(OrderStatus.Pending, "confirm");
        if (_items.Count == 0)
            throw new EmptyOrderException();

        Status = OrderStatus.Confirmed;
        MarkUpdated();
    }

    /// <summary>Records payment — transitions Confirmed ? Paid.</summary>
    public void RecordPayment()
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOrderTransitionException(Status.ToString(), OrderStatus.Paid.ToString());

        Status = OrderStatus.Paid;
        PaidAt = DateTimeOffset.UtcNow;
        MarkUpdated();
    }

    /// <summary>Ships the order — transitions Paid ? Shipped.</summary>
    public void Ship(string trackingNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trackingNumber);

        if (Status != OrderStatus.Paid)
            throw new InvalidOrderTransitionException(Status.ToString(), OrderStatus.Shipped.ToString());

        Status = OrderStatus.Shipped;
        TrackingNumber = trackingNumber;
        ShippedAt = DateTimeOffset.UtcNow;
        MarkUpdated();
    }

    /// <summary>Marks an order as delivered — transitions Shipped ? Delivered.</summary>
    public void MarkDelivered()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOrderTransitionException(Status.ToString(), OrderStatus.Delivered.ToString());

        Status = OrderStatus.Delivered;
        DeliveredAt = DateTimeOffset.UtcNow;
        MarkUpdated();
    }

    /// <summary>Cancels an order. Only Pending or Confirmed orders can be cancelled.</summary>
    public void Cancel(string reason, IEnumerable<Product> productsToRestock)
    {
        if (Status is not (OrderStatus.Pending or OrderStatus.Confirmed))
            throw new InvalidOrderTransitionException(Status.ToString(), OrderStatus.Cancelled.ToString());

        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        foreach (var item in _items)
        {
            var product = productsToRestock.FirstOrDefault(p => p.Id == item.ProductId);
            product?.RestoreStock(item.Quantity);
        }

        Status = OrderStatus.Cancelled;
        CancellationReason = reason;
        CancelledAt = DateTimeOffset.UtcNow;
        MarkUpdated();
    }

    // C# 14: null-conditional assignment — only assign if TrackingNumber is currently null
    public void SetTrackingIfMissing(string tracking) =>
        TrackingNumber ??= tracking;

    private void EnsureStatus(OrderStatus expected, string action)
    {
        if (Status != expected)
            throw new InvalidOrderTransitionException(Status.ToString(), $"cannot {action} order in '{Status}' state");
    }
}
