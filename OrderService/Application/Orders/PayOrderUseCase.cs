using Microsoft.Extensions.Logging;
using OrderService.Application.Abstractions;
using OrderService.Application.DTOs;
using OrderService.Application.Mapping;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.Orders;

public sealed class PayOrderUseCase(
    IOrderRepository orderRepository,
    ILogger<PayOrderUseCase> logger)
{
    public async Task<OrderResponse> ExecuteAsync(
        Guid orderId,
        PayOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.PaymentReference);

        var order = await orderRepository.FindByIdAsync(orderId, cancellationToken)
            ?? throw new OrderNotFoundException(orderId);

        var previousStatus = order.Status.ToString();
        order.RecordPayment();

        await orderRepository.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Order {OrderNumber} paid (ref: {PaymentRef}). Transition: {From} ? {To}",
            order.OrderNumber, request.PaymentReference, previousStatus, order.Status);

        return DomainMapper.ToResponse(order);
    }
}
