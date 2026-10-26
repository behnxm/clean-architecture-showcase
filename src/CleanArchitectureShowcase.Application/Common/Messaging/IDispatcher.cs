using CleanArchitectureShowcase.Domain.Common;

namespace CleanArchitectureShowcase.Application.Common.Messaging;

/// <summary>
/// Central mediator interface coordinating the dispatch of commands and queries to their respective handlers
/// through an extensible, decoratable pipeline.
/// </summary>
public interface IDispatcher
{
    /// <summary>
    /// Dispatches an <see cref="ICommand"/> use case returning a void Result.
    /// </summary>
    /// <param name="command">The command instance to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result indicating operation success or failure.</returns>
    Task<Result> Send(ICommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dispatches an <see cref="ICommand{TResponse}"/> use case returning a typed Result.
    /// </summary>
    /// <typeparam name="TResponse">The payload type returned on success.</typeparam>
    /// <param name="command">The command instance to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result carrying the created resource payload or failure details.</returns>
    Task<Result<TResponse>> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dispatches an <see cref="IQuery{TResponse}"/> use case returning a typed Result.
    /// </summary>
    /// <typeparam name="TResponse">The payload type returned on success.</typeparam>
    /// <param name="query">The query parameters to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result carrying the queried view model or failure details.</returns>
    Task<Result<TResponse>> Query<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
}
