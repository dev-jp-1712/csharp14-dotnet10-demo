using OrderService.Application.DTOs;
using OrderService.Domain.Entities;
using OrderService.Domain.Extensions;
using OrderService.Domain.ValueObjects;

namespace OrderService.Application.Mapping;

/// <summary>
/// Pure static mapping between domain entities and DTOs.
/// Keeps domain objects free of serialization concerns.
/// </summary>
internal static class DomainMapper
{
    public static OrderResponse ToResponse(Order order) => new(
        Id: order.Id,
        OrderNumber: order.OrderNumber.Value,
        CustomerId: order.CustomerId,
        Status: order.Status.ToString(),
        Currency: order.Currency,
        Subtotal: order.Subtotal.Amount,
        SubtotalDisplay: order.Subtotal.DisplayValue,
        ShippingAddress: ToDto(order.ShippingAddress),
        Items: order.Items.Select(ToResponse).ToList(),
        TrackingNumber: order.TrackingNumber,
        CancellationReason: order.CancellationReason,
        PaidAt: order.PaidAt,
        ShippedAt: order.ShippedAt,
        DeliveredAt: order.DeliveredAt,
        CancelledAt: order.CancelledAt,
        CreatedAt: order.CreatedAt,
        UpdatedAt: order.UpdatedAt,
        StatusSummary: order.StatusSummary);

    public static OrderSummaryResponse ToSummary(Order order) => new(
        Id: order.Id,
        OrderNumber: order.OrderNumber.Value,
        Status: order.Status.ToString(),
        Subtotal: order.Subtotal.Amount,
        SubtotalDisplay: order.Subtotal.DisplayValue,
        ItemCount: order.Items.Count,
        CreatedAt: order.CreatedAt);

    public static OrderItemResponse ToResponse(OrderItem item) => new(
        Id: item.Id,
        ProductId: item.ProductId,
        ProductName: item.ProductName,
        ProductSku: item.ProductSku,
        Quantity: item.Quantity,
        UnitPrice: item.UnitPrice.Amount,
        UnitPriceDisplay: item.UnitPrice.DisplayValue,
        LineTotal: item.LineTotal.Amount,
        LineTotalDisplay: item.LineTotal.DisplayValue);

    public static CustomerResponse ToResponse(Customer customer) => new(
        Id: customer.Id,
        FullName: customer.FullName,
        Email: customer.Email,
        PhoneNumber: customer.PhoneNumber,
        DefaultShippingAddress: ToDto(customer.DefaultShippingAddress),
        IsActive: customer.IsActive,
        CreatedAt: customer.CreatedAt);

    public static ProductResponse ToResponse(Product product) => new(
        Id: product.Id,
        Name: product.Name,
        Description: product.Description,
        Sku: product.Sku,
        UnitPrice: product.UnitPrice.Amount,
        UnitPriceDisplay: product.UnitPrice.DisplayValue,
        Currency: product.UnitPrice.Currency,
        StockQuantity: product.StockQuantity,
        Category: product.Category,
        IsAvailable: product.IsAvailable);

    public static Address ToDomain(AddressDto dto) =>
        new(dto.Street, dto.City, dto.PostalCode, dto.Country);

    private static AddressDto ToDto(Address address) =>
        new(address.Street, address.City, address.PostalCode, address.Country);
}
