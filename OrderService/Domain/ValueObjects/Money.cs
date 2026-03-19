namespace OrderService.Domain.ValueObjects;

/// <summary>
/// Immutable money value object. Uses C# 14 field keyword for validated backing storage.
/// </summary>
public sealed class Money : IEquatable<Money>
{
    // C# 14: field keyword — uses compiler-generated backing field with inline validation
    public decimal Amount
    {
        get;
        init => field = value >= 0
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), "Amount cannot be negative.");
    }

    public string Currency
    {
        get;
        init => field = !string.IsNullOrWhiteSpace(value)
            ? value.ToUpperInvariant()
            : throw new ArgumentException("Currency code cannot be empty.", nameof(value));
    }

    public Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Zero(string currency) => new(0m, currency);

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(int quantity) => new(Amount * quantity, Currency);

    public bool IsGreaterThan(Money other)
    {
        EnsureSameCurrency(other);
        return Amount > other.Amount;
    }

    private void EnsureSameCurrency(Money other)
    {
        if (!Currency.Equals(other.Currency, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Cannot operate on different currencies: {Currency} and {other.Currency}.");
    }

    public bool Equals(Money? other) =>
        other is not null && Amount == other.Amount && Currency == other.Currency;

    public override bool Equals(object? obj) => obj is Money m && Equals(m);
    public override int GetHashCode() => HashCode.Combine(Amount, Currency);
    public override string ToString() => $"{Amount:F2} {Currency}";

    public static bool operator ==(Money left, Money right) => left.Equals(right);
    public static bool operator !=(Money left, Money right) => !left.Equals(right);
}
