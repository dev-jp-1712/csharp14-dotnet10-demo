using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OrderService.Application.DTOs;
using OrderService.Application.Orders;

namespace OrderService.API.Controllers;

[ApiController]
[Route("api/orders")]
[Produces("application/json")]
[EnableRateLimiting("orders")]
public sealed class OrdersController(
    PlaceOrderUseCase placeOrder,
    PayOrderUseCase payOrder,
    ShipOrderUseCase shipOrder,
    CancelOrderUseCase cancelOrder,
    DeliverOrderUseCase deliverOrder,
    GetOrderUseCase getOrder) : ControllerBase
{
    /// <summary>Returns all orders (summary view).</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<OrderSummaryResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var orders = await getOrder.GetAllAsync(cancellationToken);
        return Ok(orders);
    }

    /// <summary>Returns all orders for a specific customer.</summary>
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType<IReadOnlyList<OrderSummaryResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCustomer(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var orders = await getOrder.GetByCustomerAsync(customerId, cancellationToken);
        return Ok(orders);
    }

    /// <summary>Returns a full order detail by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var order = await getOrder.ExecuteAsync(id, cancellationToken);
        return Ok(order);
    }

    /// <summary>Places a new order for a customer.</summary>
    [HttpPost]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemResponse>(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> PlaceOrder(
        [FromBody] PlaceOrderRequest request,
        CancellationToken cancellationToken)
    {
        var order = await placeOrder.ExecuteAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    /// <summary>Records a payment against a confirmed order.</summary>
    [HttpPost("{id:guid}/pay")]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemResponse>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Pay(
        Guid id,
        [FromBody] PayOrderRequest request,
        CancellationToken cancellationToken)
    {
        var order = await payOrder.ExecuteAsync(id, request, cancellationToken);
        return Ok(order);
    }

    /// <summary>Ships a paid order with a tracking number.</summary>
    [HttpPost("{id:guid}/ship")]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemResponse>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Ship(
        Guid id,
        [FromBody] ShipOrderRequest request,
        CancellationToken cancellationToken)
    {
        var order = await shipOrder.ExecuteAsync(id, request, cancellationToken);
        return Ok(order);
    }

    /// <summary>Marks a shipped order as delivered.</summary>
    [HttpPost("{id:guid}/deliver")]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemResponse>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Deliver(
        Guid id,
        CancellationToken cancellationToken)
    {
        var order = await deliverOrder.ExecuteAsync(id, cancellationToken);
        return Ok(order);
    }

    /// <summary>Cancels a pending or confirmed order and restores stock.</summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemResponse>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(
        Guid id,
        [FromBody] CancelOrderRequest request,
        CancellationToken cancellationToken)
    {
        var order = await cancelOrder.ExecuteAsync(id, request, cancellationToken);
        return Ok(order);
    }
}
