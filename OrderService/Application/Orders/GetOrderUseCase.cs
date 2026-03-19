using OrderService.Application.Abstractions;
using OrderService.Application.DTOs;
using OrderService.Application.Mapping;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.Orders;

public sealed class GetOrderUseCase(IOrderRepository orderRepository)
{
    public async Task<OrderResponse> ExecuteAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.FindByIdAsync(orderId, cancellationToken)
            ?? throw new OrderNotFoundException(orderId);

        return DomainMapper.ToResponse(order);
    }

    public async Task<IReadOnlyList<OrderSummaryResponse>> GetByCustomerAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var orders = await orderRepository.GetByCustomerIdAsync(customerId, cancellationToken);
        return orders.Select(DomainMapper.ToSummary).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<OrderSummaryResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var orders = await orderRepository.GetAllAsync(cancellationToken);
        return orders.Select(DomainMapper.ToSummary).ToList().AsReadOnly();
    }
}
