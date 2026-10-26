using CleanArchitectureShowcase.Domain.Common;

namespace CleanArchitectureShowcase.Application.Common.Messaging;

/// <summary>
/// Intercepts request processing before and after the downstream handler executes.
/// Implements the Decorator and Chain of Responsibility patterns for cross-cutting concerns
/// (e.g., logging, validation, metrics, transaction management) without third-party libraries.
/// </summary>
/// <typeparam name="TRequest">The incoming request type.</typeparam>
/// <typeparam name="TResponse">The response type constrainted to <see cref="Result"/>.</typeparam>
public interface IPipelineBehavior<in TRequest, TResponse>
    where TResponse : Result
{
    /// <summary>
    /// Intercepts the request processing pipeline.
    /// </summary>
    /// <param name="request">The incoming command or query instance.</param>
    /// <param name="next">The continuation delegate for the remainder of the pipeline.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result produced by the pipeline execution.</returns>
    Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken);
}
