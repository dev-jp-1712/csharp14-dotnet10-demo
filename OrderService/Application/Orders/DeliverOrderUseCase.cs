using Microsoft.Extensions.Logging;
using OrderService.Application.Abstractions;
using OrderService.Application.DTOs;
using OrderService.Application.Mapping;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.Orders;

public sealed class DeliverOrderUseCase(
    IOrderRepository orderRepository,
    ILogger<DeliverOrderUseCase> logger)
{
    public async Task<OrderResponse> ExecuteAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.FindByIdAsync(orderId, cancellationToken)
            ?? throw new OrderNotFoundException(orderId);

        order.MarkDelivered();

        await orderRepository.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Order {OrderNumber} marked as delivered at {DeliveredAt}",
            order.OrderNumber, order.DeliveredAt);

        return DomainMapper.ToResponse(order);
    }
}
