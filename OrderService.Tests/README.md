# OrderService Tests

This is a comprehensive test project for the OrderService application, targeting **.NET 10** with the latest C# 14 features.

## Test Framework

- **xUnit** 2.9.2 - Modern test framework
- **FluentAssertions** 7.0.0 - Fluent assertion library for more readable tests
- **Moq** 4.20.72 - Mocking library for isolating dependencies
- **Microsoft.EntityFrameworkCore.InMemory** 10.0.0 - In-memory database for testing

## Test Structure

### Domain Layer Tests

#### Value Objects (`Domain/ValueObjects/`)
- **MoneyTests.cs** - Tests for Money value object
  - Currency validation and conversion
  - Arithmetic operations (Add, Subtract, Multiply)
  - Currency mismatch validation
  - Equality comparisons

- **AddressTests.cs** - Tests for Address value object
  - Address creation validation
  - Required field validation
  - Equality comparisons

- **OrderNumberTests.cs** - Tests for OrderNumber value object
  - Order number generation with sequence
  - Format validation
  - Uniqueness tests

#### Entities (`Domain/Entities/`)
- **OrderTests.cs** - Tests for Order aggregate root
  - Order lifecycle: Pending → Confirmed → Paid → Shipped → Delivered
  - Order cancellation logic
  - Order item management
  - Subtotal calculation
  - Business rule enforcement

- **ProductTests.cs** - Tests for Product entity
  - Product creation and validation
  - Stock reservation and restoration
  - Price updates
  - Stock management

- **CustomerTests.cs** - Tests for Customer entity
  - Customer creation and validation
  - Customer activation/deactivation
  - Contact information updates

### Application Layer Tests

#### Use Cases (`Application/Orders/`)
- **PlaceOrderUseCaseTests.cs** - Tests for placing new orders
  - Valid order placement
  - Customer validation (existence, active status)
  - Product validation (existence, stock availability)
  - Multiple product handling

- **PayOrderUseCaseTests.cs** - Tests for order payment
  - Payment recording for confirmed orders
  - Invalid state transition handling

- **ShipOrderUseCaseTests.cs** - Tests for order shipping
  - Shipping paid orders
  - Tracking number validation

- **CancelOrderUseCaseTests.cs** - Tests for order cancellation
  - Cancellation of pending/confirmed orders
  - Stock restoration
  - Invalid state transition handling

- **GetOrderUseCaseTests.cs** - Tests for retrieving orders
  - Order retrieval by ID
  - Not found handling

### Infrastructure Layer Tests

#### Repositories (`Infrastructure/Repositories/`)
- **OrderRepositoryTests.cs** - Tests for OrderRepository
  - CRUD operations
  - Order sequence generation
  - Query operations (by customer, by order number)

- **ProductRepositoryTests.cs** - Tests for ProductRepository
  - CRUD operations
  - Bulk product retrieval
  - Low stock queries

- **CustomerRepositoryTests.cs** - Tests for CustomerRepository
  - CRUD operations
  - Query operations (by email, active customers)

## Running Tests

### Run all tests:
```bash
dotnet test
```

### Run tests with detailed output:
```bash
dotnet test --verbosity detailed
```

### Run tests for a specific class:
```bash
dotnet test --filter "FullyQualifiedName~OrderTests"
```

### Run tests with code coverage:
```bash
dotnet test /p:CollectCoverage=true
```

## Test Patterns

### Arrange-Act-Assert (AAA)
All tests follow the AAA pattern for clarity:
```csharp
[Fact]
public void TestName()
{
    // Arrange - Set up test data and dependencies
    var product = Product.Create(...);
    
    // Act - Execute the operation being tested
    var result = product.TryReserveStock(5);
    
    // Assert - Verify the expected outcome
    result.Should().BeTrue();
}
```

### Test Naming Convention
Tests follow the pattern: `MethodName_Scenario_ExpectedBehavior`
```csharp
public void Create_WithNegativePrice_ThrowsArgumentOutOfRangeException()
```

### Mocking with Moq
Dependencies are mocked using Moq to isolate the system under test:
```csharp
var mockRepository = new Mock<IOrderRepository>();
mockRepository
    .Setup(x => x.FindByIdAsync(orderId, It.IsAny<CancellationToken>()))
    .ReturnsAsync(order);
```

### In-Memory Database Testing
Repository tests use Entity Framework Core's InMemory provider:
```csharp
var options = new DbContextOptionsBuilder<OrderDbContext>()
    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
    .Options;
```

## .NET 10 Features Used

- **ImplicitUsings** - Reduced boilerplate with global usings
- **Nullable reference types** - Enhanced null safety
- **Collection expressions** - Modern syntax `[]` for collections
- **Primary constructors** - Simplified dependency injection
- **Record types** - Immutable DTOs

## Test Coverage

The test project provides comprehensive coverage of:
- ✅ Domain entities and value objects
- ✅ Business logic and invariants
- ✅ State transitions and workflows
- ✅ Data validation
- ✅ Exception handling
- ✅ Repository operations
- ✅ Use case orchestration

## Notes

- The test project uses `InternalsVisibleTo` attribute in the OrderService project to test internal repository implementations
- Tests are isolated and can run in parallel
- Each test uses unique in-memory databases to prevent data interference
- FluentAssertions provides readable and expressive assertion syntax
