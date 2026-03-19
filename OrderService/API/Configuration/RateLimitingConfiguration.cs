using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace OrderService.API.Configuration;

/// <summary>
/// .NET 10: Centralized rate limiting configuration with multiple policies
/// demonstrating Fixed Window, Sliding Window, and Token Bucket algorithms.
/// </summary>
public static class RateLimitingConfiguration
{
    public static IServiceCollection AddRateLimitingPolicies(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Policy 1: Fixed Window - Strict limits for order placement
            // Resets exactly at window boundaries
            options.AddFixedWindowLimiter("orders", opts =>
            {
                opts.Window = TimeSpan.FromMinutes(1);
                opts.PermitLimit = 10;
                opts.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opts.QueueLimit = 2;
            });

            // Policy 2: Sliding Window - Smooth rate limiting for general API
            // More flexible than fixed window, considers rolling time periods
            options.AddSlidingWindowLimiter("api", opts =>
            {
                opts.Window = TimeSpan.FromMinutes(1);
                opts.PermitLimit = 100;
                opts.SegmentsPerWindow = 6; // Divides window into 6 segments of 10 seconds
                opts.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opts.QueueLimit = 5;
            });

            // Policy 3: Token Bucket - Allows burst traffic for read operations
            // Tokens refill over time, good for APIs with occasional spikes
            options.AddTokenBucketLimiter("reads", opts =>
            {
                opts.TokenLimit = 50;
                opts.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
                opts.TokensPerPeriod = 10;
                opts.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opts.QueueLimit = 3;
            });

            // Policy 4: Concurrency Limiter - Limits concurrent requests
            // Useful for resource-intensive operations
            options.AddConcurrencyLimiter("concurrent", opts =>
            {
                opts.PermitLimit = 20;
                opts.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opts.QueueLimit = 10;
            });

            // Global configuration
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // .NET 10: Enhanced rejection handler with structured response
            options.OnRejected = async (context, token) =>
            {
                var retryAfter = GetRetryAfterValue(context);
                context.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString();

                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    error = "RateLimitExceeded",
                    message = "Too many requests. Please try again later.",
                    retryAfter = $"{retryAfter} seconds",
                    timestamp = DateTimeOffset.UtcNow
                }, cancellationToken: token);
            };
        });

        return services;
    }

    private static int GetRetryAfterValue(Microsoft.AspNetCore.RateLimiting.OnRejectedContext context)
    {
        // Try to get retry-after from lease metadata
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            return (int)retryAfter.TotalSeconds;
        }

        // Default retry-after: 60 seconds
        return 60;
    }
}
