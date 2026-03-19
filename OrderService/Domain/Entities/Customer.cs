using OrderService.Domain.ValueObjects;

namespace OrderService.Domain.Entities;

public sealed class Customer : Entity
{
    public string FullName { get; private set; }
    public string Email { get; private set; }
    public string PhoneNumber { get; private set; }
    public Address DefaultShippingAddress { get; private set; }
    public bool IsActive { get; private set; }

    private Customer() 
    {
        FullName = string.Empty;
        Email = string.Empty;
        PhoneNumber = string.Empty;
        DefaultShippingAddress = default!;
    }

    public static Customer Create(
        string fullName,
        string email,
        string phoneNumber,
        Address defaultShippingAddress)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(phoneNumber);
        ArgumentNullException.ThrowIfNull(defaultShippingAddress);

        return new Customer
        {
            FullName = fullName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PhoneNumber = phoneNumber.Trim(),
            DefaultShippingAddress = defaultShippingAddress,
            IsActive = true
        };
    }

    public void UpdateContactInfo(string fullName, string phoneNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(phoneNumber);

        FullName = fullName.Trim();
        PhoneNumber = phoneNumber.Trim();
        MarkUpdated();
    }

    public void UpdateShippingAddress(Address address)
    {
        ArgumentNullException.ThrowIfNull(address);
        DefaultShippingAddress = address;
        MarkUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkUpdated();
    }
}
