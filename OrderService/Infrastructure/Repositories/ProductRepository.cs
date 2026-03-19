using Microsoft.EntityFrameworkCore;
using OrderService.Application.Abstractions;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Repositories;

internal sealed class ProductRepository(OrderDbContext dbContext) : IProductRepository
{
    public async Task<Product?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.Products.FindAsync([id], cancellationToken);

    public async Task<Product?> FindBySkuAsync(string sku, CancellationToken cancellationToken = default) =>
        await dbContext.Products.FirstOrDefaultAsync(
            p => p.Sku == sku.ToUpperInvariant(), cancellationToken);

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = await dbContext.Products.ToListAsync(cancellationToken);
        return products.AsReadOnly();
    }

    public async Task<IReadOnlyList<Product>> GetByIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        var products = await dbContext.Products
            .Where(p => idList.Contains(p.Id))
            .ToListAsync(cancellationToken);

        return products.AsReadOnly();
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default) =>
        await dbContext.Products.AddAsync(product, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await dbContext.SaveChangesAsync(cancellationToken);
}
