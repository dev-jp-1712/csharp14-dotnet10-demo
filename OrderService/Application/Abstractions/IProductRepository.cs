using OrderService.Domain.Entities;

namespace OrderService.Application.Abstractions;

public interface IProductRepository
{
    Task<Product?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Product?> FindBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
