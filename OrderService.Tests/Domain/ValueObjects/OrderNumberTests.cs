namespace OrderService.Tests.Domain.ValueObjects;

public class OrderNumberTests
{
    [Fact]
    public void Generate_WithSequence_CreatesOrderNumberWithCorrectFormat()
    {
        // Arrange
        var sequence = 12345;

        // Act
        var orderNumber = OrderNumber.Generate(sequence);

        // Assert
        orderNumber.Should().NotBeNull();
        orderNumber.Value.Should().MatchRegex(@"^ORD-\d{4}-\d{2}-\d{2}-\d{5}$");
    }

    [Fact]
    public void Generate_WithDifferentSequences_CreatesDifferentOrderNumbers()
    {
        // Arrange & Act
        var orderNumber1 = OrderNumber.Generate(1);
        var orderNumber2 = OrderNumber.Generate(2);

        // Assert
        orderNumber1.Value.Should().NotBe(orderNumber2.Value);
    }

    [Fact]
    public void Generate_WithSameSequenceOnSameDay_CreatesSameOrderNumber()
    {
        // Arrange & Act
        var orderNumber1 = OrderNumber.Generate(100);
        var orderNumber2 = OrderNumber.Generate(100);

        // Assert
        orderNumber1.Value.Should().Be(orderNumber2.Value);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(99999)]
    [InlineData(12345)]
    public void Generate_WithValidSequence_IncludesSequenceInOrderNumber(int sequence)
    {
        // Arrange & Act
        var orderNumber = OrderNumber.Generate(sequence);

        // Assert
        orderNumber.Value.Should().EndWith($"{sequence:D5}");
    }

    [Fact]
    public void Equals_WithSameValue_ReturnsTrue()
    {
        // Arrange
        var orderNumber1 = OrderNumber.Generate(100);
        var orderNumber2 = OrderNumber.Generate(100);

        // Act & Assert
        orderNumber1.Equals(orderNumber2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValue_ReturnsFalse()
    {
        // Arrange
        var orderNumber1 = OrderNumber.Generate(100);
        var orderNumber2 = OrderNumber.Generate(200);

        // Act & Assert
        orderNumber1.Equals(orderNumber2).Should().BeFalse();
    }

    [Fact]
    public void ToString_ReturnsOrderNumberValue()
    {
        // Arrange
        var orderNumber = OrderNumber.Generate(123);

        // Act
        var result = orderNumber.ToString();

        // Assert
        result.Should().Be(orderNumber.Value);
    }
}
