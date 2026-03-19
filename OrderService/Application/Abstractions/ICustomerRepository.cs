using OrderService.Domain.Entities;

namespace OrderService.Application.Abstractions;

public interface ICustomerRepository
{
    Task<Customer?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Customer?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
