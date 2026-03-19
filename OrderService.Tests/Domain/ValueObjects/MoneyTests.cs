namespace OrderService.Tests.Domain.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Constructor_WithValidInputs_CreatesMoneyInstance()
    {
        // Arrange & Act
        var money = new Money(100.50m, "USD");

        // Assert
        money.Amount.Should().Be(100.50m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Constructor_WithLowercaseCurrency_ConvertsToUppercase()
    {
        // Arrange & Act
        var money = new Money(50m, "eur");

        // Assert
        money.Currency.Should().Be("EUR");
    }

    [Fact]
    public void Constructor_WithNegativeAmount_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => new Money(-10m, "USD");

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Amount cannot be negative*");
    }

    [Fact]
    public void Constructor_WithNullCurrency_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => new Money(10m, null!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Currency code cannot be empty*");
    }

    [Fact]
    public void Constructor_WithEmptyCurrency_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => new Money(10m, "");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Currency code cannot be empty*");
    }

    [Fact]
    public void Zero_CreatesMoneyWithZeroAmount()
    {
        // Arrange & Act
        var money = Money.Zero("USD");

        // Assert
        money.Amount.Should().Be(0m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Add_WithSameCurrency_ReturnsCorrectSum()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(50m, "USD");

        // Act
        var result = money1.Add(money2);

        // Assert
        result.Amount.Should().Be(150m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Add_WithDifferentCurrency_ThrowsInvalidOperationException()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(50m, "EUR");

        // Act
        var act = () => money1.Add(money2);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Currency mismatch*");
    }

    [Fact]
    public void Subtract_WithSameCurrency_ReturnsCorrectDifference()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(30m, "USD");

        // Act
        var result = money1.Subtract(money2);

        // Assert
        result.Amount.Should().Be(70m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Subtract_WithDifferentCurrency_ThrowsInvalidOperationException()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(30m, "EUR");

        // Act
        var act = () => money1.Subtract(money2);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Currency mismatch*");
    }

    [Fact]
    public void Multiply_ReturnsCorrectProduct()
    {
        // Arrange
        var money = new Money(25.50m, "USD");

        // Act
        var result = money.Multiply(3);

        // Assert
        result.Amount.Should().Be(76.50m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void IsGreaterThan_WithLargerAmount_ReturnsTrue()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(50m, "USD");

        // Act
        var result = money1.IsGreaterThan(money2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsGreaterThan_WithSmallerAmount_ReturnsFalse()
    {
        // Arrange
        var money1 = new Money(50m, "USD");
        var money2 = new Money(100m, "USD");

        // Act
        var result = money1.IsGreaterThan(money2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(100m, "USD");

        // Act & Assert
        money1.Equals(money2).Should().BeTrue();
        (money1 == money2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentAmount_ReturnsFalse()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(50m, "USD");

        // Act & Assert
        money1.Equals(money2).Should().BeFalse();
        (money1 == money2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentCurrency_ReturnsFalse()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(100m, "EUR");

        // Act & Assert
        money1.Equals(money2).Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10.5)]
    [InlineData(999.99)]
    [InlineData(1000000.00)]
    public void Constructor_WithVariousValidAmounts_CreatesMoneyInstance(decimal amount)
    {
        // Arrange & Act
        var money = new Money(amount, "USD");

        // Assert
        money.Amount.Should().Be(amount);
    }
}
