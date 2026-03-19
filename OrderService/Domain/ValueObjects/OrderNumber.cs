namespace OrderService.Domain.ValueObjects;

/// <summary>
/// Human-readable order number value object. Encapsulates generation logic.
/// </summary>
public sealed class OrderNumber : IEquatable<OrderNumber>
{
    public string Value
    {
        get;
        // C# 14: field keyword with inline guard
        init => field = !string.IsNullOrWhiteSpace(value)
            ? value.ToUpperInvariant()
            : throw new ArgumentException("Order number cannot be empty.", nameof(value));
    }

    public OrderNumber(string value)
    {
        Value = value;
    }

    /// <summary>Generates a new order number based on a sequential counter and timestamp prefix.</summary>
    public static OrderNumber Generate(long sequence) =>
        new($"ORD-{DateTime.UtcNow:yyyyMM}-{sequence:D6}");

    public bool Equals(OrderNumber? other) =>
        other is not null && Value == other.Value;

    public override bool Equals(object? obj) => obj is OrderNumber o && Equals(o);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    public override string ToString() => Value;
}
