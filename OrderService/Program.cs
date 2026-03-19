using System.Threading.RateLimiting;
using OrderService.API.Configuration;
using OrderService.API.Middleware;
using OrderService.Infrastructure.Extensions;
using OrderService.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddControllers();

// .NET 10 native OpenAPI support
builder.Services.AddOpenApi();

// .NET 10: Enhanced Rate Limiting with centralized configuration
builder.Services.AddRateLimitingPolicies();

// .NET 10 IHostApplicationBuilder pattern — composable extension methods
builder
    .AddInfrastructure()
    .AddApplicationUseCases();

var app = builder.Build();

// Seed the in-memory database with realistic sample data
await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await DatabaseSeeder.SeedAsync(dbContext, logger);
}

// Enable OpenAPI and Scalar UI in Development
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();

// .NET 10: Rate Limiting middleware
app.UseRateLimiter();

app.MapControllers();

app.Run();
