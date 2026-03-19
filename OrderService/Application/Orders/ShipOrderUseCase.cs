using Microsoft.Extensions.Logging;
using OrderService.Application.Abstractions;
using OrderService.Application.DTOs;
using OrderService.Application.Mapping;
using OrderService.Domain.Exceptions;
using OrderService.Domain.Extensions;

namespace OrderService.Application.Orders;

public sealed class ShipOrderUseCase(
    IOrderRepository orderRepository,
    ILogger<ShipOrderUseCase> logger)
{
    public async Task<OrderResponse> ExecuteAsync(
        Guid orderId,
        ShipOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.TrackingNumber);

        var order = await orderRepository.FindByIdAsync(orderId, cancellationToken)
            ?? throw new OrderNotFoundException(orderId);

        if (!order.IsReadyForFulfillment)
            throw new InvalidOrderTransitionException(order.Status.ToString(), "Shipped");

        order.Ship(request.TrackingNumber);

        await orderRepository.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Order {OrderNumber} shipped. Tracking: {Tracking}. Summary: {Summary}",
            order.OrderNumber, request.TrackingNumber, order.StatusSummary);

        return DomainMapper.ToResponse(order);
    }
}
