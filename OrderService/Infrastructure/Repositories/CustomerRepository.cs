using Microsoft.EntityFrameworkCore;
using OrderService.Application.Abstractions;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Repositories;

internal sealed class CustomerRepository(OrderDbContext dbContext) : ICustomerRepository
{
    public async Task<Customer?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.Customers.FindAsync([id], cancellationToken);

    public async Task<Customer?> FindByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        await dbContext.Customers.FirstOrDefaultAsync(
            c => c.Email == email.ToLowerInvariant(), cancellationToken);

    public async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var customers = await dbContext.Customers.ToListAsync(cancellationToken);
        return customers.AsReadOnly();
    }

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default) =>
        await dbContext.Customers.AddAsync(customer, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await dbContext.SaveChangesAsync(cancellationToken);
}
