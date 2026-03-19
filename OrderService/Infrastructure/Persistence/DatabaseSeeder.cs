using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderService.Domain.Entities;
using OrderService.Domain.ValueObjects;

namespace OrderService.Infrastructure.Persistence;

/// <summary>
/// Seeds realistic sample data into the InMemory database at startup.
/// Uses C# 14 nameof with unbound generics for diagnostic logging.
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(OrderDbContext dbContext, ILogger logger)
    {
        if (await dbContext.Customers.AnyAsync())
        {
            logger.LogDebug("Seed skipped — data already present");
            return;
        }

        // C# 14: nameof with unbound generic type
        logger.LogInformation("Seeding {Entity} data...", nameof(Customer));

        var customers = BuildCustomers();
        await dbContext.Customers.AddRangeAsync(customers);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Seeding {Entity} data...", nameof(Product));

        var products = BuildProducts();
        await dbContext.Products.AddRangeAsync(products);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Seeding initial {Entity} data...", nameof(Order));

        await SeedOrdersAsync(dbContext, customers, products, logger);

        logger.LogInformation(
            "Seed complete — {Customers} customers, {Products} products, {Orders} orders",
            customers.Length, products.Length,
            await dbContext.Orders.CountAsync());
    }

    private static Customer[] BuildCustomers() =>
    [
        Customer.Create(
            "Alice Hartwell",
            "alice.hartwell@example.com",
            "+1-555-0101",
            new Address("12 Maple Street", "Springfield", "62701", "US")),

        Customer.Create(
            "Bob Nguyen",
            "bob.nguyen@example.com",
            "+1-555-0202",
            new Address("88 Ocean Drive", "Miami", "33139", "US")),

        Customer.Create(
            "Clara Jensen",
            "clara.jensen@example.com",
            "+44-20-7946-0300",
            new Address("7 King's Road", "London", "SW3 4NX", "GB")),
    ];

    private static Product[] BuildProducts() =>
    [
        Product.Create(
            "Mechanical Keyboard Pro",
            "Compact TKL mechanical keyboard with Cherry MX switches and RGB backlight.",
            "KB-PRO-001",
            new Money(149.99m, "USD"),
            stockQuantity: 50,
            category: "Electronics"),

        Product.Create(
            "Ergonomic Office Chair",
            "Lumbar-supported mesh office chair with adjustable armrests and tilt tension.",
            "CHR-ERG-002",
            new Money(399.00m, "USD"),
            stockQuantity: 20,
            category: "Furniture"),

        Product.Create(
            "USB-C Docking Station",
            "7-in-1 USB-C hub with 4K HDMI, 100W PD, SD card reader, and USB 3.2 ports.",
            "DOCK-USB-003",
            new Money(89.95m, "USD"),
            stockQuantity: 100,
            category: "Electronics"),

        Product.Create(
            "Standing Desk Mat",
            "Anti-fatigue mat with beveled edges, designed for standing desk use.",
            "MAT-STAND-004",
            new Money(49.99m, "USD"),
            stockQuantity: 75,
            category: "Accessories"),

        Product.Create(
            "Wireless Noise-Cancelling Headphones",
            "Over-ear headphones with ANC, 30-hour battery, and multipoint pairing.",
            "HP-ANC-005",
            new Money(279.00m, "USD"),
            stockQuantity: 35,
            category: "Electronics"),
    ];

    private static async Task SeedOrdersAsync(
        OrderDbContext dbContext,
        Customer[] customers,
        Product[] products,
        ILogger logger)
    {
        // Order 1: Paid order for Alice
        var aliceAddr = new Address(
            customers[0].DefaultShippingAddress.Street,
            customers[0].DefaultShippingAddress.City,
            customers[0].DefaultShippingAddress.PostalCode,
            customers[0].DefaultShippingAddress.Country);

        var order1 = Order.Create(
            OrderNumber.Generate(1),
            customers[0].Id,
            aliceAddr,
            "USD");

        order1.AddItem(products[0], 1); // Keyboard
        order1.AddItem(products[2], 2); // Docking station x2
        order1.Confirm();
        order1.RecordPayment();

        await dbContext.Orders.AddAsync(order1);

        // Order 2: Shipped order for Bob
        var bobAddr = new Address(
            customers[1].DefaultShippingAddress.Street,
            customers[1].DefaultShippingAddress.City,
            customers[1].DefaultShippingAddress.PostalCode,
            customers[1].DefaultShippingAddress.Country);

        var order2 = Order.Create(
            OrderNumber.Generate(2),
            customers[1].Id,
            bobAddr,
            "USD");

        order2.AddItem(products[1], 1); // Chair
        order2.Confirm();
        order2.RecordPayment();
        order2.Ship("UPS-1Z999AA10123456784");
        order2.SetTrackingIfMissing("FALLBACK-TRACKING"); // C# 14: ??= — won't overwrite

        await dbContext.Orders.AddAsync(order2);

        // Order 3: Pending order for Clara (can be cancelled or paid via API)
        var claraAddr = new Address(
            customers[2].DefaultShippingAddress.Street,
            customers[2].DefaultShippingAddress.City,
            customers[2].DefaultShippingAddress.PostalCode,
            customers[2].DefaultShippingAddress.Country);

        var order3 = Order.Create(
            OrderNumber.Generate(3),
            customers[2].Id,
            claraAddr,
            "USD");

        order3.AddItem(products[4], 1); // Headphones
        order3.AddItem(products[3], 1); // Desk mat
        order3.Confirm();

        await dbContext.Orders.AddAsync(order3);

        await dbContext.SaveChangesAsync();

        logger.LogDebug("Seeded 3 sample orders across {Count} customers", customers.Length);
    }
}
