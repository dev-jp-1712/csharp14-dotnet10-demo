using Microsoft.Extensions.Logging;
using OrderService.Application.Abstractions;
using OrderService.Application.DTOs;
using OrderService.Application.Mapping;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.Orders;

public sealed class CancelOrderUseCase(
    IOrderRepository orderRepository,
    IProductRepository productRepository,
    ILogger<CancelOrderUseCase> logger)
{
    public async Task<OrderResponse> ExecuteAsync(
        Guid orderId,
        CancelOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Reason);

        var order = await orderRepository.FindByIdAsync(orderId, cancellationToken)
            ?? throw new OrderNotFoundException(orderId);

        var productIds = order.Items.Select(i => i.ProductId);
        var products = await productRepository.GetByIdsAsync(productIds, cancellationToken);

        order.Cancel(request.Reason, products);

        await orderRepository.SaveChangesAsync(cancellationToken);
        await productRepository.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Order {OrderNumber} cancelled. Reason: {Reason}. Stock restored for {Count} product(s)",
            order.OrderNumber, request.Reason, products.Count);

        return DomainMapper.ToResponse(order);
    }
}
