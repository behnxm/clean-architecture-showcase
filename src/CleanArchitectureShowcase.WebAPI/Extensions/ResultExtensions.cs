using System.Diagnostics;
using CleanArchitectureShowcase.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureShowcase.WebAPI.Extensions;

/// <summary>
/// Extension methods for converting domain and application <see cref="Result"/> outcomes
/// into RFC 9457 compliant ProblemDetails HTTP responses.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Translates a failed Result into an <see cref="ActionResult"/> wrapping an RFC 9457 <see cref="ProblemDetails"/> instance.
    /// Maps validation errors to HTTP 400, not found to 404, conflicts to 409, and general failures to 500.
    /// </summary>
    /// <param name="result">The failed result instance.</param>
    /// <param name="httpContext">The active HTTP context.</param>
    /// <returns>An ActionResult wrapping the configured ProblemDetails payload.</returns>
    public static ActionResult ToProblem(this Result result, HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(httpContext);

        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Cannot convert a successful result to a problem details response.");
        }

        Error error = result.Error;
        string traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        var (statusCode, title, type) = error.Type switch
        {
            ErrorType.Validation => (
                StatusCodes.Status400BadRequest,
                "Bad Request",
                "https://tools.ietf.org/html/rfc9110#section-15.5.1"),

            ErrorType.NotFound => (
                StatusCodes.Status404NotFound,
                "Not Found",
                "https://tools.ietf.org/html/rfc9110#section-15.5.5"),

            ErrorType.Conflict => (
                StatusCodes.Status409Conflict,
                "Conflict",
                "https://tools.ietf.org/html/rfc9110#section-15.5.10"),

            ErrorType.Failure => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "https://tools.ietf.org/html/rfc9110#section-15.6.1"),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "https://tools.ietf.org/html/rfc9110#section-15.6.1")
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = type,
            Detail = error.Description,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["code"] = error.Code;
        problemDetails.Extensions["traceId"] = traceId;

        if (error is ValidationError validationError)
        {
            var errorsDictionary = validationError.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.Description).ToArray());

            problemDetails.Extensions["errors"] = errorsDictionary;
        }

        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode
        };
    }
}
