using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OrderService.Application.Customers;
using OrderService.Application.DTOs;

namespace OrderService.API.Controllers;

[ApiController]
[Route("api/customers")]
[Produces("application/json")]
[EnableRateLimiting("api")]
public sealed class CustomersController(GetCustomerUseCase getCustomer) : ControllerBase
{
    /// <summary>Returns all customers.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CustomerResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var customers = await getCustomer.GetAllAsync(cancellationToken);
        return Ok(customers);
    }

    /// <summary>Returns a single customer by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<CustomerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var customer = await getCustomer.ExecuteAsync(id, cancellationToken);
        return Ok(customer);
    }
}
