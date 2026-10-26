using CleanArchitectureShowcase.Domain.Common;

namespace CleanArchitectureShowcase.Application.Common.Events;

/// <summary>
/// Dispatcher abstraction responsible for routing published domain events to their registered handlers.
/// Owned by the Application layer to keep the Domain layer free of dispatching machinery.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Dispatches a domain event to all registered domain event handlers.
    /// </summary>
    /// <param name="domainEvent">The domain event to dispatch.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
