namespace OrderService.Domain.Exceptions;

public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}

public sealed class OrderNotFoundException(Guid orderId)
    : DomainException($"Order '{orderId}' was not found.");

public sealed class CustomerNotFoundException(Guid customerId)
    : DomainException($"Customer '{customerId}' was not found.");

public sealed class ProductNotFoundException(Guid productId)
    : DomainException($"Product '{productId}' was not found.");

public sealed class InsufficientStockException(Guid productId, int requested, int available)
    : DomainException($"Product '{productId}' has insufficient stock. Requested: {requested}, Available: {available}.");

public sealed class InvalidOrderTransitionException(string from, string to)
    : DomainException($"Cannot transition order from '{from}' to '{to}'.");

public sealed class OrderAlreadyPaidException(Guid orderId)
    : DomainException($"Order '{orderId}' has already been paid.");

public sealed class EmptyOrderException()
    : DomainException("An order must contain at least one item.");

public sealed class InactiveCustomerException(Guid customerId)
    : DomainException($"Customer '{customerId}' is not active and cannot place orders.");
