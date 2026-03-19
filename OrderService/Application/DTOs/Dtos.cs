namespace OrderService.Application.DTOs;

// ?? Request DTOs ??????????????????????????????????????????????????????????????

public sealed record PlaceOrderRequest(
    Guid CustomerId,
    AddressDto ShippingAddress,
    List<OrderLineRequest> Lines,
    string Currency = "USD");

public sealed record OrderLineRequest(Guid ProductId, int Quantity);

public sealed record PayOrderRequest(string PaymentReference);

public sealed record ShipOrderRequest(string TrackingNumber);

public sealed record CancelOrderRequest(string Reason);

public sealed record AddressDto(
    string Street,
    string City,
    string PostalCode,
    string Country);

// ?? Response DTOs ?????????????????????????????????????????????????????????????

public sealed record OrderResponse(
    Guid Id,
    string OrderNumber,
    Guid CustomerId,
    string Status,
    string Currency,
    decimal Subtotal,
    string SubtotalDisplay,
    AddressDto ShippingAddress,
    List<OrderItemResponse> Items,
    string? TrackingNumber,
    string? CancellationReason,
    DateTimeOffset? PaidAt,
    DateTimeOffset? ShippedAt,
    DateTimeOffset? DeliveredAt,
    DateTimeOffset? CancelledAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string StatusSummary);

public sealed record OrderItemResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductSku,
    int Quantity,
    decimal UnitPrice,
    string UnitPriceDisplay,
    decimal LineTotal,
    string LineTotalDisplay);

public sealed record OrderSummaryResponse(
    Guid Id,
    string OrderNumber,
    string Status,
    decimal Subtotal,
    string SubtotalDisplay,
    int ItemCount,
    DateTimeOffset CreatedAt);

public sealed record CustomerResponse(
    Guid Id,
    string FullName,
    string Email,
    string PhoneNumber,
    AddressDto DefaultShippingAddress,
    bool IsActive,
    DateTimeOffset CreatedAt);

public sealed record ProductResponse(
    Guid Id,
    string Name,
    string Description,
    string Sku,
    decimal UnitPrice,
    string UnitPriceDisplay,
    string Currency,
    int StockQuantity,
    string Category,
    bool IsAvailable);

public sealed record ProblemResponse(string Title, string Detail, int Status);
