using System.Net;
using System.Text.Json;
using OrderService.Application.DTOs;
using OrderService.Domain.Exceptions;

namespace OrderService.API.Middleware;

/// <summary>
/// Central exception handler that maps domain and application exceptions to
/// structured HTTP responses. Keeps controllers free of try/catch boilerplate.
/// </summary>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            OrderNotFoundException
            or CustomerNotFoundException
            or ProductNotFoundException      => (HttpStatusCode.NotFound,       "Resource Not Found"),
            InvalidOrderTransitionException  => (HttpStatusCode.Conflict,       "Invalid State Transition"),
            InsufficientStockException       => (HttpStatusCode.Conflict,       "Insufficient Stock"),
            EmptyOrderException              => (HttpStatusCode.UnprocessableEntity, "Order Is Empty"),
            OrderAlreadyPaidException        => (HttpStatusCode.Conflict,       "Order Already Paid"),
            DomainException                  => (HttpStatusCode.BadRequest,     "Business Rule Violation"),
            ArgumentException
            or ArgumentNullException
            or ArgumentOutOfRangeException   => (HttpStatusCode.BadRequest,     "Invalid Request"),
            _                                => (HttpStatusCode.InternalServerError, "Unexpected Error")
        };

        var logLevel = statusCode == HttpStatusCode.InternalServerError
            ? LogLevel.Error
            : LogLevel.Warning;

        logger.Log(logLevel, exception,
            "Request {Method} {Path} failed with {StatusCode}: {Message}",
            context.Request.Method,
            context.Request.Path,
            (int)statusCode,
            exception.Message);

        var response = new ProblemResponse(
            Title: title,
            Detail: exception.Message,
            Status: (int)statusCode);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}
