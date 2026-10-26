using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureShowcase.WebAPI.Middleware;

/// <summary>
/// Global exception handler catching unhandled exceptions and translating them into RFC 9457 ProblemDetails.
/// Prevents internal implementation details and stack traces from leaking to clients in production environments.
/// </summary>
public sealed partial class GlobalExceptionHandler(
    IHostEnvironment environment,
    ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        LogUnhandledException(logger, exception.Message, exception);

        string traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
            Detail = environment.IsDevelopment()
                ? exception.ToString()
                : "An unexpected error occurred while processing your request. Please reference the trace ID when contacting support.",
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["code"] = "Server.UnhandledException";
        problemDetails.Extensions["traceId"] = traceId;

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    [LoggerMessage(EventId = 5001, Level = LogLevel.Error, Message = "Unhandled exception occurred: {Message}")]
    private static partial void LogUnhandledException(ILogger logger, string message, Exception exception);
}
