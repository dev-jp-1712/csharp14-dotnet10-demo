namespace OrderService.Domain.ValueObjects;

/// <summary>
/// Shipping address value object. Demonstrates C# 14 field keyword with validation on each property.
/// </summary>
public sealed class Address : IEquatable<Address>
{
    public string Street
    {
        get;
        init => field = !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : throw new ArgumentException("Street cannot be empty.", nameof(value));
    }

    public string City
    {
        get;
        init => field = !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : throw new ArgumentException("City cannot be empty.", nameof(value));
    }

    public string PostalCode
    {
        get;
        init => field = !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : throw new ArgumentException("PostalCode cannot be empty.", nameof(value));
    }

    public string Country
    {
        get;
        init => field = !string.IsNullOrWhiteSpace(value)
            ? value.Trim().ToUpperInvariant()
            : throw new ArgumentException("Country cannot be empty.", nameof(value));
    }

    public Address(string street, string city, string postalCode, string country)
    {
        Street = street;
        City = city;
        PostalCode = postalCode;
        Country = country;
    }

    /// <summary>Static factory method for creating Address instances.</summary>
    public static Address Create(string street, string city, string postalCode, string country)
        => new(street, city, postalCode, country);

    public bool Equals(Address? other) =>
        other is not null &&
        Street == other.Street &&
        City == other.City &&
        PostalCode == other.PostalCode &&
        Country == other.Country;

    public override bool Equals(object? obj) => obj is Address a && Equals(a);
    public override int GetHashCode() => HashCode.Combine(Street, City, PostalCode, Country);
    public override string ToString() => $"{Street}, {City} {PostalCode}, {Country}";
}
