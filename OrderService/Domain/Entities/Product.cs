using OrderService.Domain.ValueObjects;

namespace OrderService.Domain.Entities;

public sealed class Product : Entity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Sku { get; private set; }
    public Money UnitPrice { get; private set; }
    public int StockQuantity { get; private set; }
    public string Category { get; private set; }
    public bool IsAvailable { get; private set; }

    private Product()
    {
        Name = string.Empty;
        Description = string.Empty;
        Sku = string.Empty;
        UnitPrice = default!;
        Category = string.Empty;
    }

    public static Product Create(
        string name,
        string description,
        string sku,
        Money unitPrice,
        int stockQuantity,
        string category)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentNullException.ThrowIfNull(unitPrice);

        if (stockQuantity < 0)
            throw new ArgumentOutOfRangeException(nameof(stockQuantity), "Stock quantity cannot be negative.");

        return new Product
        {
            Name = name.Trim(),
            Description = description?.Trim() ?? string.Empty,
            Sku = sku.Trim().ToUpperInvariant(),
            UnitPrice = unitPrice,
            StockQuantity = stockQuantity,
            Category = category?.Trim() ?? "General",
            IsAvailable = stockQuantity > 0
        };
    }

    /// <summary>Reserves stock for an order line. Returns false if insufficient.</summary>
    public bool TryReserveStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");

        if (StockQuantity < quantity)
            return false;

        StockQuantity -= quantity;
        IsAvailable = StockQuantity > 0;
        MarkUpdated();
        return true;
    }

    /// <summary>Restores stock when an order is cancelled.</summary>
    public void RestoreStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");

        StockQuantity += quantity;
        IsAvailable = true;
        MarkUpdated();
    }

    public void UpdatePrice(Money newPrice)
    {
        ArgumentNullException.ThrowIfNull(newPrice);
        UnitPrice = newPrice;
        MarkUpdated();
    }
}
