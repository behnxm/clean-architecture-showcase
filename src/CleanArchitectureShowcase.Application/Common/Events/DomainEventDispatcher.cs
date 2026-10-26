using System.Collections.Concurrent;
using CleanArchitectureShowcase.Domain.Common;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitectureShowcase.Application.Common.Events;

/// <summary>
/// Application-level domain event dispatcher implementing a per-event cached invoker.
///
/// Reliability Note:
/// In this reference implementation, domain events are dispatched immediately after the Unit of Work
/// successfully commits changes to the persistent store. This delivers at-most-once semantics:
/// if a crash occurs after the database write but prior to handler completion, the event is not retried.
/// In mission-critical systems requiring at-least-once delivery, an Outbox table should be used to atomically
/// record events within the aggregate transaction and publish them asynchronously via a background worker.
/// </summary>
internal sealed class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    private static readonly ConcurrentDictionary<Type, DomainEventInvoker> _eventInvokers = new();

    public Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        Type eventType = domainEvent.GetType();
        DomainEventInvoker invoker = _eventInvokers.GetOrAdd(
            eventType,
            static type => (DomainEventInvoker)Activator.CreateInstance(typeof(DomainEventInvokerImpl<>).MakeGenericType(type))!);

        return invoker.Invoke(domainEvent, serviceProvider, cancellationToken);
    }

    private abstract class DomainEventInvoker
    {
        public abstract Task Invoke(IDomainEvent domainEvent, IServiceProvider sp, CancellationToken ct);
    }

    private sealed class DomainEventInvokerImpl<TEvent> : DomainEventInvoker
        where TEvent : IDomainEvent
    {
        public override async Task Invoke(IDomainEvent domainEvent, IServiceProvider sp, CancellationToken ct)
        {
            IEnumerable<IDomainEventHandler<TEvent>> handlers = sp.GetServices<IDomainEventHandler<TEvent>>();
            foreach (IDomainEventHandler<TEvent> handler in handlers)
            {
                await handler.Handle((TEvent)domainEvent, ct);
            }
        }
    }
}
