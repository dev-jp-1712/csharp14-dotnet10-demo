# 🚀 .NET 10 & C# 14 Demo - OrderService

[![.NET Version](https://img.shields.io/badge/.NET-10.0-purple)](https://dotnet.microsoft.com/)
[![C# Version](https://img.shields.io/badge/C%23-14.0-blue)](https://learn.microsoft.com/dotnet/csharp/)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

A comprehensive demonstration of **.NET 10** platform features and **C# 14** language features through a production-ready Order Management API built with **Clean Architecture** principles.

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Key Features](#-key-features)
- [Technologies](#-technologies)
- [.NET 10 Features Demonstrated](#-net-10-features-demonstrated)
- [C# 14 Features Demonstrated](#-c-14-features-demonstrated)
- [Project Structure](#-project-structure)
- [Getting Started](#-getting-started)
- [API Endpoints](#-api-endpoints)
- [Architecture](#-architecture)
- [Documentation](#-documentation)
- [Contributing](#-contributing)

---

## 🎯 Overview

This project showcases modern .NET development practices using the latest **.NET 10** platform features and **C# 14** language enhancements. It implements a fully functional Order Management system following Clean Architecture, Domain-Driven Design (DDD), and SOLID principles.

### What You'll Learn

- ✅ How to use **.NET 10's native OpenAPI** support
- ✅ Implementing **rate limiting** with multiple algorithms (Fixed Window, Sliding Window, Token Bucket, Concurrency)
- ✅ Using **C# 14's `field` keyword** for property validation
- ✅ Creating **extension members** with the `extension` keyword
- ✅ Applying **IHostApplicationBuilder** pattern for composable DI
- ✅ Building a **Clean Architecture** API with domain modeling
- ✅ Implementing **Domain-Driven Design** patterns

---

## ✨ Key Features

### Business Features
- 📦 **Order Management**: Place, pay, ship, deliver, and cancel orders
- 👥 **Customer Management**: Customer profiles with shipping addresses
- 📦 **Product Catalog**: Product inventory with stock management
- 💰 **Money & Currency**: Type-safe value objects for monetary values
- 🔄 **State Transitions**: Order workflow with enforced business rules
- 📊 **Order Status Tracking**: Full audit trail with timestamps

### Technical Features
- 🚦 **Rate Limiting**: 4 different algorithms protecting API endpoints
- 📖 **Native OpenAPI**: Built-in OpenAPI/Swagger without third-party libraries
- 🏗️ **Clean Architecture**: Separated layers (Domain, Application, Infrastructure, API)
- 🛡️ **Domain Validation**: Business rules enforced at the domain level
- 🎭 **Value Objects**: Immutable types with inline validation
- 🔌 **Extension Members**: Computed properties without modifying types
- 🧪 **In-Memory Database**: Quick setup for demos and testing

---

## 🛠️ Technologies

### Core Stack
- **.NET 10.0** - Latest .NET platform
- **C# 14.0** - Latest language features
- **ASP.NET Core** - Web API framework
- **Entity Framework Core 10.0** - ORM with In-Memory provider
- **Scalar** - Modern OpenAPI UI

### Design Patterns
- Clean Architecture
- Domain-Driven Design (DDD)
- Repository Pattern
- Use Case Pattern
- CQRS (Read/Write separation)
- Value Objects
- Aggregate Roots

---

## 🆕 .NET 10 Features Demonstrated

### 1. **Native OpenAPI Support**
Built-in OpenAPI generation without Swashbuckle:
```csharp
builder.Services.AddOpenApi();
app.MapOpenApi();
```
📄 **File:** `src/OrderService/Program.cs`

### 2. **IHostApplicationBuilder Extensions**
Composable service registration pattern:
```csharp
builder
    .AddInfrastructure()
    .AddApplicationUseCases();
```
📄 **Files:** `src/OrderService/Program.cs`, `src/OrderService/Infrastructure/Extensions/ServiceCollectionExtensions.cs`

### 3. **Enhanced Rate Limiting**
Multiple rate limiting algorithms:
- **Fixed Window** - Order placement (10 req/min)
- **Sliding Window** - General API (100 req/min)
- **Token Bucket** - Read operations (50 tokens, refill 10/10s)
- **Concurrency Limiter** - Max 20 concurrent requests

```csharp
options.AddFixedWindowLimiter("orders", opts => { ... });
options.AddSlidingWindowLimiter("api", opts => { ... });
options.AddTokenBucketLimiter("reads", opts => { ... });
```
📄 **File:** `src/OrderService/API/Configuration/RateLimitingConfiguration.cs`

---

## 🔥 C# 14 Features Demonstrated

### 1. **`field` Keyword - Property Backing Field Access**
Inline property validation:
```csharp
public decimal Amount
{
    get;
    init => field = value >= 0
        ? value
        : throw new ArgumentOutOfRangeException(nameof(value));
}
```
📄 **Files:** `src/OrderService/Domain/ValueObjects/Money.cs`, `Address.cs`, `OrderNumber.cs`

### 2. **Extension Members (`extension` keyword)**
Computed properties without modifying types:
```csharp
extension(Order order)
{
    public bool IsEditable => order.Status == OrderStatus.Pending;
    public string StatusSummary => /* formatted string */;
}
```
📄 **File:** `src/OrderService/Domain/Extensions/OrderExtensions.cs`

### 3. **Null-Conditional Assignment (`??=`)**
Assign only if null:
```csharp
public void SetTrackingIfMissing(string tracking) =>
    TrackingNumber ??= tracking;
```
📄 **File:** `src/OrderService/Domain/Entities/Order.cs`

### 4. **`nameof` with Unbound Generic Types**
Type-safe logging:
```csharp
logger.LogInformation("Seeding {Entity} data...", nameof(Customer));
```
📄 **File:** `src/OrderService/Infrastructure/Persistence/DatabaseSeeder.cs`

### 5. **Partial Members**
Split member definitions:
```csharp
internal sealed partial class OrderWorkflowLogger
{
    public partial void LogTransition(string orderNumber, string from, string to);
}
```
📄 **File:** `src/OrderService/Application/Orders/PlaceOrderUseCase.cs`

### 6. **Primary Constructors**
Simplified dependency injection:
```csharp
public sealed class OrdersController(
    PlaceOrderUseCase placeOrder,
    PayOrderUseCase payOrder,
    ...) : ControllerBase
```
📄 **Files:** All controllers, use cases, and middleware

---

## 📁 Project Structure

```
CSharp14DotNet10Demo/
├── src/
│   └── OrderService/
│       ├── API/
│       │   ├── Configuration/
│       │   │   └── RateLimitingConfiguration.cs    # .NET 10 Rate Limiting
│       │   ├── Controllers/
│       │   │   ├── OrdersController.cs             # Order endpoints
│       │   │   ├── CustomersController.cs
│       │   │   └── ProductsController.cs
│       │   └── Middleware/
│       │       └── ExceptionHandlingMiddleware.cs  # Global error handling
│       ├── Application/
│       │   ├── Abstractions/                       # Repository interfaces
│       │   ├── DTOs/                               # Data transfer objects
│       │   ├── Mapping/                            # Domain-to-DTO mapping
│       │   ├── Orders/                             # Order use cases
│       │   ├── Customers/
│       │   └── Products/
│       ├── Domain/
│       │   ├── Entities/
│       │   │   ├── Order.cs                        # Aggregate root
│       │   │   ├── OrderItem.cs
│       │   │   ├── Customer.cs
│       │   │   └── Product.cs
│       │   ├── ValueObjects/
│       │   │   ├── Money.cs                        # C# 14 field keyword
│       │   │   ├── Address.cs                      # C# 14 field keyword
│       │   │   └── OrderNumber.cs                  # C# 14 field keyword
│       │   ├── Enums/
│       │   │   └── OrderStatus.cs
│       │   ├── Exceptions/
│       │   │   └── DomainExceptions.cs
│       │   └── Extensions/
│       │       └── OrderExtensions.cs              # C# 14 extension members
│       ├── Infrastructure/
│       │   ├── Persistence/
│       │   │   ├── OrderDbContext.cs               # EF Core context
│       │   │   └── DatabaseSeeder.cs               # Sample data
│       │   ├── Repositories/                       # Repository implementations
│       │   └── Extensions/
│       │       └── ServiceCollectionExtensions.cs  # .NET 10 IHostApplicationBuilder
│       └── Program.cs                              # .NET 10 OpenAPI + Rate Limiting
└── docs/
    ├── DotNet10Features.md                         # Rate limiting guide
    ├── FeaturesSummary.md                          # Complete features reference
    └── README-DotNet10Implementation.md            # Implementation notes
```

---

## 🚀 Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Visual Studio 2026+ or Visual Studio Code
- PowerShell (for testing scripts)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/CSharp14DotNet10Demo.git
   cd CSharp14DotNet10Demo
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the project**
   ```bash
   dotnet build
   ```

4. **Run the application**
   ```bash
   dotnet run --project src/OrderService
   ```

5. **Open the API documentation**
   - OpenAPI JSON: `https://localhost:5001/openapi/v1.json`
   - Scalar UI: `https://localhost:5001/scalar/v1`

---

## 📡 API Endpoints

### Orders
| Method | Endpoint | Description | Rate Limit |
|--------|----------|-------------|------------|
| `GET` | `/api/orders` | Get all orders | 50 tokens |
| `GET` | `/api/orders/{id}` | Get order by ID | 50 tokens |
| `GET` | `/api/orders/customer/{customerId}` | Get customer orders | 50 tokens |
| `POST` | `/api/orders` | Place new order | **10/min** |
| `POST` | `/api/orders/{id}/pay` | Record payment | 100/min |
| `POST` | `/api/orders/{id}/ship` | Ship order | 100/min |
| `POST` | `/api/orders/{id}/deliver` | Mark as delivered | 100/min |
| `POST` | `/api/orders/{id}/cancel` | Cancel order | 100/min |

### Customers
| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/customers` | Get all customers |
| `GET` | `/api/customers/{id}` | Get customer by ID |

### Products
| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/products` | Get all products |
| `GET` | `/api/products/{id}` | Get product by ID |

### Sample Request
```bash
POST /api/orders
Content-Type: application/json

{
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "currency": "USD",
  "shippingAddress": {
    "street": "123 Main St",
    "city": "Springfield",
    "postalCode": "62701",
    "country": "US"
  },
  "lines": [
    {
      "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
      "quantity": 2
    }
  ]
}
```

---

## 🏛️ Architecture

### Clean Architecture Layers

```
┌─────────────────────────────────────────┐
│           API Layer                     │
│  (Controllers, Middleware, Config)      │
└─────────────┬───────────────────────────┘
              │
┌─────────────▼───────────────────────────┐
│       Application Layer                 │
│  (Use Cases, DTOs, Interfaces)          │
└─────────────┬───────────────────────────┘
              │
┌─────────────▼───────────────────────────┐
│         Domain Layer                    │
│  (Entities, Value Objects, Rules)       │
└─────────────────────────────────────────┘
              ▲
┌─────────────┴───────────────────────────┐
│      Infrastructure Layer               │
│  (EF Core, Repositories, Persistence)   │
└─────────────────────────────────────────┘
```

### Key Design Principles
- ✅ **Dependency Rule**: Dependencies point inward
- ✅ **Domain-Centric**: Business logic in domain layer
- ✅ **Repository Pattern**: Data access abstraction
- ✅ **Value Objects**: Immutable, validated types
- ✅ **Aggregate Roots**: Consistency boundaries
- ✅ **Use Cases**: Single responsibility per operation

---

## 🧪 Testing Rate Limiting

### Test Fixed Window (Order Placement)
```powershell
# Place 15 orders rapidly (limit: 10/min)
for ($i=1; $i -le 15; $i++) {
    Invoke-RestMethod -Uri "https://localhost:5001/api/orders" `
        -Method Post `
        -ContentType "application/json" `
        -Body '{"customerId":"...","lines":[],"currency":"USD"}' `
        -ErrorAction Continue
    Write-Host "Request $i - $(if ($i -gt 10) {'Expected 429'} else {'OK'})"
}
```

### Expected 429 Response
```json
{
  "error": "RateLimitExceeded",
  "message": "Too many requests. Please try again later.",
  "retryAfter": "60 seconds",
  "timestamp": "2025-01-15T10:30:00Z"
}
```

---

## 📚 Documentation

Comprehensive documentation is available in the `/docs` folder:

- **[DotNet10Features.md](docs/DotNet10Features.md)** - Deep dive into rate limiting
- **[FeaturesSummary.md](docs/FeaturesSummary.md)** - Complete .NET 10 & C# 14 reference
- **[README-DotNet10Implementation.md](docs/README-DotNet10Implementation.md)** - Implementation notes

---

## 🎓 Learning Resources

### Official Documentation
- [C# 14 What's New](https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-14)
- [.NET 10 Release Notes](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10)
- [ASP.NET Core Rate Limiting](https://learn.microsoft.com/aspnet/core/performance/rate-limit)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

### Topics Covered
- C# 14 language features
- .NET 10 platform features
- Clean Architecture
- Domain-Driven Design (DDD)
- SOLID principles
- Repository pattern
- Value objects pattern
- API rate limiting
- OpenAPI/Swagger

---

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

### Development Setup
1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- Microsoft .NET Team for .NET 10 and C# 14
- Clean Architecture by Robert C. Martin (Uncle Bob)
- Domain-Driven Design by Eric Evans
- ASP.NET Core team for excellent documentation

---

## 📧 Contact

**Project Maintainer**: Your Name
- GitHub: [@yourusername](https://github.com/yourusername)
- Email: your.email@example.com

---

## ⭐ Star This Repository

If you found this demo helpful for learning .NET 10 and C# 14, please consider giving it a star! ⭐

---

<div align="center">
  
**Built with ❤️ using .NET 10 & C# 14**

[Report Bug](https://github.com/yourusername/CSharp14DotNet10Demo/issues) · 
[Request Feature](https://github.com/yourusername/CSharp14DotNet10Demo/issues) · 
[Documentation](docs/)

</div>
