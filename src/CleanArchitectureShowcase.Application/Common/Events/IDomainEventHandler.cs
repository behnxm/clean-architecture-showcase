using CleanArchitectureShowcase.Domain.Common;

namespace CleanArchitectureShowcase.Application.Common.Events;

/// <summary>
/// Subscribes to domain events raised by domain aggregates.
/// Handlers execute sequentially or asynchronously after database commit in an at-most-once model.
/// </summary>
/// <typeparam name="TEvent">The specific domain event type.</typeparam>
public interface IDomainEventHandler<in TEvent>
    where TEvent : IDomainEvent
{
    /// <summary>
    /// Processes the published domain event.
    /// </summary>
    /// <param name="domainEvent">The domain event instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task Handle(TEvent domainEvent, CancellationToken cancellationToken = default);
}
