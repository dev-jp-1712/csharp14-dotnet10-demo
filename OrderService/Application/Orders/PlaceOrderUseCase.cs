using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging;
using OrderService.Application.Abstractions;
using OrderService.Application.DTOs;
using OrderService.Application.Mapping;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;
using OrderService.Domain.ValueObjects;

namespace OrderService.Application.Orders;

public sealed class PlaceOrderUseCase(
    IOrderRepository orderRepository,
    ICustomerRepository customerRepository,
    IProductRepository productRepository,
    ILogger<PlaceOrderUseCase> logger)
{
    public async Task<OrderResponse> ExecuteAsync(
        PlaceOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var customer = await customerRepository.FindByIdAsync(request.CustomerId, cancellationToken)
            ?? throw new CustomerNotFoundException(request.CustomerId);

        if (!customer.IsActive)
            throw new InactiveCustomerException(request.CustomerId);

        var productIds = request.Lines.Select(l => l.ProductId).Distinct();
        var products = await productRepository.GetByIdsAsync(productIds, cancellationToken);
        var productMap = products.ToDictionary(p => p.Id);

        foreach (var line in request.Lines)
        {
            if (!productMap.ContainsKey(line.ProductId))
                throw new ProductNotFoundException(line.ProductId);
        }

        var sequence = await orderRepository.GetNextSequenceAsync(cancellationToken);
        var orderNumber = OrderNumber.Generate(sequence);
        var shippingAddress = DomainMapper.ToDomain(request.ShippingAddress);

        var order = Order.Create(orderNumber, customer.Id, shippingAddress, request.Currency);

        foreach (var line in request.Lines)
        {
            var product = productMap[line.ProductId];
            order.AddItem(product, line.Quantity);
        }

        order.Confirm();

        await orderRepository.AddAsync(order, cancellationToken);
        await orderRepository.SaveChangesAsync(cancellationToken);
        await productRepository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Order {OrderNumber} placed for customer {CustomerId} — {ItemCount} item(s), total {Total}",
            order.OrderNumber, customer.Id, order.Items.Count, order.Subtotal);

        return DomainMapper.ToResponse(order);
    }
}

// C# 14: partial class with partial member — the sealed class definition split allows
// partial property/method declarations in separate files (demonstrated here inline).
internal sealed partial class OrderWorkflowLogger
{
    private readonly ILogger _logger;

    public OrderWorkflowLogger(ILogger logger) => _logger = logger;

    // C# 14: partial member declaration
    public partial void LogTransition(string orderNumber, string from, string to);
    public partial void LogTransition(string orderNumber, string from, string to)
    {
        _logger.LogInformation("Order {OrderNumber} transitioned {From} ? {To}", orderNumber, from, to);
    }
}
