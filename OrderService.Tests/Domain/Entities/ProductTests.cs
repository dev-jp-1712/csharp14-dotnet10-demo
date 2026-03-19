namespace OrderService.Tests.Domain.Entities;

public class ProductTests
{
    [Fact]
    public void Create_WithValidInputs_CreatesProductInstance()
    {
        // Arrange & Act
        var product = Product.Create("Laptop", "High-end laptop", "LAP001", new Money(1000m, "USD"), 10, "Electronics");

        // Assert
        product.Should().NotBeNull();
        product.Name.Should().Be("Laptop");
        product.UnitPrice.Amount.Should().Be(1000m);
        product.UnitPrice.Currency.Should().Be("USD");
        product.StockQuantity.Should().Be(10);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ThrowsArgumentException(string? name)
    {
        // Arrange & Act
        var act = () => Product.Create(name!, "Description", "SKU001", new Money(100m, "USD"), 10, "Category");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNegativePrice_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => new Money(-100m, "USD");

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_WithNegativeStock_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => Product.Create("Laptop", "Description", "SKU001", new Money(100m, "USD"), -5, "Category");

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void TryReserveStock_WithSufficientStock_ReservesAndReturnsTrue()
    {
        // Arrange
        var product = Product.Create("Laptop", "High-end laptop", "LAP001", new Money(1000m, "USD"), 10, "Electronics");

        // Act
        var result = product.TryReserveStock(5);

        // Assert
        result.Should().BeTrue();
        product.StockQuantity.Should().Be(5);
    }

    [Fact]
    public void TryReserveStock_WithExactStock_ReservesAndReturnsTrue()
    {
        // Arrange
        var product = Product.Create("Laptop", "High-end laptop", "LAP001", new Money(1000m, "USD"), 10, "Electronics");

        // Act
        var result = product.TryReserveStock(10);

        // Assert
        result.Should().BeTrue();
        product.StockQuantity.Should().Be(0);
    }

    [Fact]
    public void TryReserveStock_WithInsufficientStock_ReturnsFalse()
    {
        // Arrange
        var product = Product.Create("Laptop", "High-end laptop", "LAP001", new Money(1000m, "USD"), 5, "Electronics");

        // Act
        var result = product.TryReserveStock(10);

        // Assert
        result.Should().BeFalse();
        product.StockQuantity.Should().Be(5);
    }

    [Fact]
    public void TryReserveStock_WithZeroQuantity_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var product = Product.Create("Laptop", "High-end laptop", "LAP001", new Money(1000m, "USD"), 10, "Electronics");

        // Act
        var act = () => product.TryReserveStock(0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void RestoreStock_IncreasesStockQuantity()
    {
        // Arrange
        var product = Product.Create("Laptop", "High-end laptop", "LAP001", new Money(1000m, "USD"), 5, "Electronics");

        // Act
        product.RestoreStock(3);

        // Assert
        product.StockQuantity.Should().Be(8);
    }

    [Fact]
    public void RestoreStock_WithZeroQuantity_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var product = Product.Create("Laptop", "High-end laptop", "LAP001", new Money(1000m, "USD"), 5, "Electronics");

        // Act
        var act = () => product.RestoreStock(0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void UpdatePrice_WithValidPrice_UpdatesPrice()
    {
        // Arrange
        var product = Product.Create("Laptop", "High-end laptop", "LAP001", new Money(1000m, "USD"), 5, "Electronics");
        var newPrice = new Money(1200m, "USD");

        // Act
        product.UpdatePrice(newPrice);

        // Assert
        product.UnitPrice.Should().Be(newPrice);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(100)]
    public void RestoreStock_AddsToExistingStock(int additionalQuantity)
    {
        // Arrange
        var product = Product.Create("Laptop", "High-end laptop", "LAP001", new Money(1000m, "USD"), 10, "Electronics");

        // Act
        product.RestoreStock(additionalQuantity);

        // Assert
        product.StockQuantity.Should().Be(10 + additionalQuantity);
    }
}
