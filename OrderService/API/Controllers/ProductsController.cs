using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OrderService.Application.DTOs;
using OrderService.Application.Products;

namespace OrderService.API.Controllers;

[ApiController]
[Route("api/products")]
[Produces("application/json")]
[EnableRateLimiting("reads")]
public sealed class ProductsController(GetProductUseCase getProduct) : ControllerBase
{
    /// <summary>Returns all products in the catalogue.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ProductResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var products = await getProduct.GetAllAsync(cancellationToken);
        return Ok(products);
    }

    /// <summary>Returns a single product by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await getProduct.ExecuteAsync(id, cancellationToken);
        return Ok(product);
    }
}
