using OrderService.Domain.Enums;
using OrderService.Domain.Entities;
using OrderService.Domain.ValueObjects;

namespace OrderService.Domain.Extensions;

/// <summary>
/// C# 14 extension members — static extension block that adds computed behaviors
/// to domain entities without modifying them directly.
/// </summary>
public static class OrderExtensions
{
    extension(Order order)
    {
        /// <summary>Returns true if the order can still be modified (items added/removed).</summary>
        public bool IsEditable => order.Status == OrderStatus.Pending;

        /// <summary>Returns true if the order is in a terminal state.</summary>
        public bool IsTerminal =>
            order.Status is OrderStatus.Delivered or OrderStatus.Cancelled or OrderStatus.Refunded;

        /// <summary>Returns true if the order has been paid and fulfillment can proceed.</summary>
        public bool IsReadyForFulfillment =>
            order.Status is OrderStatus.Paid or OrderStatus.Shipped;

        /// <summary>Formats a human-readable status summary for logging or display.</summary>
        public string StatusSummary =>
            order.Status switch
            {
                OrderStatus.Pending    => $"[{order.OrderNumber}] Awaiting confirmation ({order.Items.Count} item(s))",
                OrderStatus.Confirmed  => $"[{order.OrderNumber}] Confirmed, pending payment",
                OrderStatus.Paid       => $"[{order.OrderNumber}] Paid — {order.Subtotal}",
                OrderStatus.Shipped    => $"[{order.OrderNumber}] Shipped (tracking: {order.TrackingNumber})",
                OrderStatus.Delivered  => $"[{order.OrderNumber}] Delivered on {order.DeliveredAt:d}",
                OrderStatus.Cancelled  => $"[{order.OrderNumber}] Cancelled: {order.CancellationReason}",
                _                      => $"[{order.OrderNumber}] Status: {order.Status}"
            };
    }

    extension(Money money)
    {
        /// <summary>Returns true when the money amount represents a zero value.</summary>
        public bool IsZero => money.Amount == 0m;

        /// <summary>Formats the amount with currency symbol in a display-friendly way.</summary>
        public string DisplayValue => money.Currency switch
        {
            "USD" => $"${money.Amount:F2}",
            "EUR" => $"€{money.Amount:F2}",
            "GBP" => $"£{money.Amount:F2}",
            _     => $"{money.Amount:F2} {money.Currency}"
        };
    }
}
