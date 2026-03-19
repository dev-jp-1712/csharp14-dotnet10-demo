using Microsoft.EntityFrameworkCore;
using OrderService.Application.Abstractions;
using OrderService.Application.Customers;
using OrderService.Application.Orders;
using OrderService.Application.Products;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Repositories;

namespace OrderService.Infrastructure.Extensions;

/// <summary>
/// .NET 10 pattern: extension method on IHostApplicationBuilder for composable service registration.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDbContext<OrderDbContext>(options =>
            options.UseInMemoryDatabase("OrderServiceDb"));

        builder.Services.AddScoped<IOrderRepository, OrderRepository>();
        builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
        builder.Services.AddScoped<IProductRepository, ProductRepository>();

        return builder;
    }

    public static IHostApplicationBuilder AddApplicationUseCases(this IHostApplicationBuilder builder)
    {
        // Order workflows
        builder.Services.AddScoped<PlaceOrderUseCase>();
        builder.Services.AddScoped<PayOrderUseCase>();
        builder.Services.AddScoped<ShipOrderUseCase>();
        builder.Services.AddScoped<CancelOrderUseCase>();
        builder.Services.AddScoped<DeliverOrderUseCase>();
        builder.Services.AddScoped<GetOrderUseCase>();

        // Read models
        builder.Services.AddScoped<GetCustomerUseCase>();
        builder.Services.AddScoped<GetProductUseCase>();

        return builder;
    }
}
