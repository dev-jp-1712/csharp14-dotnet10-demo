namespace OrderService.Tests.Domain.ValueObjects;

public class AddressTests
{
    [Fact]
    public void Create_WithValidInputs_CreatesAddressInstance()
    {
        // Arrange & Act
        var address = Address.Create("123 Main St", "New York", "10001", "USA");

        // Assert
        address.Street.Should().Be("123 Main St");
        address.City.Should().Be("New York");
        address.PostalCode.Should().Be("10001");
        address.Country.Should().Be("USA");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidStreet_ThrowsArgumentException(string? street)
    {
        // Arrange & Act
        var act = () => Address.Create(street!, "New York", "10001", "USA");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidCity_ThrowsArgumentException(string? city)
    {
        // Arrange & Act
        var act = () => Address.Create("123 Main St", city!, "10001", "USA");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidPostalCode_ThrowsArgumentException(string? postalCode)
    {
        // Arrange & Act
        var act = () => Address.Create("123 Main St", "New York", postalCode!, "USA");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidCountry_ThrowsArgumentException(string? country)
    {
        // Arrange & Act
        var act = () => Address.Create("123 Main St", "New York", "10001", country!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        // Arrange
        var address1 = Address.Create("123 Main St", "New York", "10001", "USA");
        var address2 = Address.Create("123 Main St", "New York", "10001", "USA");

        // Act & Assert
        address1.Equals(address2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValues_ReturnsFalse()
    {
        // Arrange
        var address1 = Address.Create("123 Main St", "New York", "10001", "USA");
        var address2 = Address.Create("456 Elm St", "Boston", "02101", "USA");

        // Act & Assert
        address1.Equals(address2).Should().BeFalse();
    }
}
