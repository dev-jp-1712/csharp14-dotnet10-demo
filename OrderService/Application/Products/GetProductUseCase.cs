using OrderService.Application.Abstractions;
using OrderService.Application.DTOs;
using OrderService.Application.Mapping;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.Products;

public sealed class GetProductUseCase(IProductRepository productRepository)
{
    public async Task<ProductResponse> ExecuteAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var product = await productRepository.FindByIdAsync(productId, cancellationToken)
            ?? throw new ProductNotFoundException(productId);

        return DomainMapper.ToResponse(product);
    }

    public async Task<IReadOnlyList<ProductResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var products = await productRepository.GetAllAsync(cancellationToken);
        return products.Select(DomainMapper.ToResponse).ToList().AsReadOnly();
    }
}
