using OrderService.Domain.ValueObjects;

namespace OrderService.Domain.Entities;

/// <summary>
/// OrderItem is an entity owned by the Order aggregate.
/// It captures the price at the time of ordering (snapshot pricing).
/// </summary>
public sealed class OrderItem : Entity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public string ProductSku { get; private set; }
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; }
    public Money LineTotal => UnitPrice.Multiply(Quantity);

    private OrderItem()
    {
        ProductName = string.Empty;
        ProductSku = string.Empty;
        UnitPrice = default!;
    }

    internal static OrderItem Create(Guid orderId, Product product, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be at least 1.");

        // Snapshot the price at time of ordering — creates a new Money instance
        var priceSnapshot = new Money(product.UnitPrice.Amount, product.UnitPrice.Currency);

        return new OrderItem
        {
            OrderId = orderId,
            ProductId = product.Id,
            ProductName = product.Name,
            ProductSku = product.Sku,
            Quantity = quantity,
            UnitPrice = priceSnapshot
        };
    }
}
