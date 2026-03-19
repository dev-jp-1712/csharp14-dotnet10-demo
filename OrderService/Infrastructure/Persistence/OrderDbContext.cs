using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Domain.ValueObjects;

namespace OrderService.Infrastructure.Persistence;

public sealed class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ?? Customer ??????????????????????????????????????????????????????????
        modelBuilder.Entity<Customer>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.FullName).IsRequired().HasMaxLength(200);
            e.Property(c => c.Email).IsRequired().HasMaxLength(320);
            e.Property(c => c.PhoneNumber).IsRequired().HasMaxLength(30);
            e.HasIndex(c => c.Email).IsUnique();

            e.OwnsOne(c => c.DefaultShippingAddress, addr =>
            {
                addr.Property(a => a.Street).IsRequired().HasMaxLength(300);
                addr.Property(a => a.City).IsRequired().HasMaxLength(100);
                addr.Property(a => a.PostalCode).IsRequired().HasMaxLength(20);
                addr.Property(a => a.Country).IsRequired().HasMaxLength(2);
            });
        });

        // ?? Product ???????????????????????????????????????????????????????????
        modelBuilder.Entity<Product>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Name).IsRequired().HasMaxLength(200);
            e.Property(p => p.Sku).IsRequired().HasMaxLength(50);
            e.Property(p => p.Category).IsRequired().HasMaxLength(100);
            e.HasIndex(p => p.Sku).IsUnique();

            e.OwnsOne(p => p.UnitPrice, owned =>
            {
                owned.Property(m => m.Amount).IsRequired();
                owned.Property(m => m.Currency).IsRequired().HasMaxLength(3);
            });
        });

        // ?? Order ?????????????????????????????????????????????????????????????
        modelBuilder.Entity<Order>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.Currency).IsRequired().HasMaxLength(3);
            e.Property(o => o.Status).IsRequired();

            e.OwnsOne(o => o.OrderNumber, owned =>
            {
                owned.Property(n => n.Value).IsRequired().HasMaxLength(30);
                owned.HasIndex(n => n.Value).IsUnique();
            });

            e.OwnsOne(o => o.ShippingAddress, owned =>
            {
                owned.Property(a => a.Street).IsRequired().HasMaxLength(300);
                owned.Property(a => a.City).IsRequired().HasMaxLength(100);
                owned.Property(a => a.PostalCode).IsRequired().HasMaxLength(20);
                owned.Property(a => a.Country).IsRequired().HasMaxLength(2);
            });

            e.HasMany(o => o.Items)
             .WithOne()
             .HasForeignKey(i => i.OrderId)
             .OnDelete(DeleteBehavior.Cascade);

            // shadow property for private _items collection access
            e.Navigation(o => o.Items).HasField("_items");
        });

        // ?? OrderItem ?????????????????????????????????????????????????????????
        modelBuilder.Entity<OrderItem>(e =>
        {
            e.HasKey(i => i.Id);
            e.Property(i => i.ProductName).IsRequired().HasMaxLength(200);
            e.Property(i => i.ProductSku).IsRequired().HasMaxLength(50);

            e.OwnsOne(i => i.UnitPrice, owned =>
            {
                owned.Property(m => m.Amount).IsRequired();
                owned.Property(m => m.Currency).IsRequired().HasMaxLength(3);
            });
        });
    }
}
