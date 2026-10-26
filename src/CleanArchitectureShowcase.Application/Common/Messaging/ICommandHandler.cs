using CleanArchitectureShowcase.Domain.Common;

namespace CleanArchitectureShowcase.Application.Common.Messaging;

/// <summary>
/// Handles an <see cref="ICommand"/> use case returning a void Result.
/// Enforces Single Responsibility: each command handler handles exactly one business operation.
/// </summary>
/// <typeparam name="TCommand">The command type to process.</typeparam>
public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    /// <summary>
    /// Executes the business logic for the specified command.
    /// </summary>
    /// <param name="command">The command instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result indicating operation success or failure.</returns>
    Task<Result> Handle(TCommand command, CancellationToken cancellationToken);
}

/// <summary>
/// Handles an <see cref="ICommand{TResponse}"/> use case returning a typed Result.
/// </summary>
/// <typeparam name="TCommand">The command type to process.</typeparam>
/// <typeparam name="TResponse">The payload type returned on success.</typeparam>
public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    /// <summary>
    /// Executes the business logic for the specified command.
    /// </summary>
    /// <param name="command">The command instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result carrying the return value or failure details.</returns>
    Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken);
}
