namespace OrderService.Tests.Domain.Entities;

public class CustomerTests
{
    [Fact]
    public void Create_WithValidInputs_CreatesCustomerInstance()
    {
        // Arrange
        var address = Address.Create("123 Main St", "New York", "10001", "USA");

        // Act
        var customer = Customer.Create("John Doe", "john@example.com", "+1234567890", address);

        // Assert
        customer.Should().NotBeNull();
        customer.FullName.Should().Be("John Doe");
        customer.Email.Should().Be("john@example.com");
        customer.PhoneNumber.Should().Be("+1234567890");
        customer.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ThrowsArgumentException(string? name)
    {
        // Arrange
        var address = Address.Create("123 Main St", "New York", "10001", "USA");

        // Act
        var act = () => Customer.Create(name!, "john@example.com", "+1234567890", address);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidEmail_ThrowsArgumentException(string? email)
    {
        // Arrange
        var address = Address.Create("123 Main St", "New York", "10001", "USA");

        // Act
        var act = () => Customer.Create("John Doe", email!, "+1234567890", address);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deactivate_MarksCustomerAsInactive()
    {
        // Arrange
        var address = Address.Create("123 Main St", "New York", "10001", "USA");
        var customer = Customer.Create("John Doe", "john@example.com", "+1234567890", address);

        // Act
        customer.Deactivate();

        // Assert
        customer.IsActive.Should().BeFalse();
    }
}
