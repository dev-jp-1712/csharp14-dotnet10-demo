using OrderService.Application.Abstractions;
using OrderService.Application.DTOs;
using OrderService.Application.Mapping;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.Customers;

public sealed class GetCustomerUseCase(ICustomerRepository customerRepository)
{
    public async Task<CustomerResponse> ExecuteAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var customer = await customerRepository.FindByIdAsync(customerId, cancellationToken)
            ?? throw new CustomerNotFoundException(customerId);

        return DomainMapper.ToResponse(customer);
    }

    public async Task<IReadOnlyList<CustomerResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var customers = await customerRepository.GetAllAsync(cancellationToken);
        return customers.Select(DomainMapper.ToResponse).ToList().AsReadOnly();
    }
}
