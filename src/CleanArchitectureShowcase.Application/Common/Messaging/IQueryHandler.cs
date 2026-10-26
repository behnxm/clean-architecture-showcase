using CleanArchitectureShowcase.Domain.Common;

namespace CleanArchitectureShowcase.Application.Common.Messaging;

/// <summary>
/// Handles an <see cref="IQuery{TResponse}"/> use case returning a typed Result.
/// Enforces CQRS separation: queries read state and return projected DTOs directly, bypassing write-side aggregates.
/// </summary>
/// <typeparam name="TQuery">The query type to process.</typeparam>
/// <typeparam name="TResponse">The payload type returned on success.</typeparam>
public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    /// <summary>
    /// Executes the read logic for the specified query.
    /// </summary>
    /// <param name="query">The query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result carrying the query response or failure details.</returns>
    Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken);
}
