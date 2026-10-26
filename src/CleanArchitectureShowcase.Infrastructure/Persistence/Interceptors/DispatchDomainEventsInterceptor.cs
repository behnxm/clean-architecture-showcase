using CleanArchitectureShowcase.Application.Common.Events;
using CleanArchitectureShowcase.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CleanArchitectureShowcase.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor responsible for collecting, clearing, and dispatching domain events
/// after changes have successfully committed to the database.
///
/// Separation of Concerns Note:
/// By executing in <see cref="SavedChangesAsync"/>, this interceptor guarantees that domain events
/// are dispatched only if database persistence succeeds. This delivers an at-most-once delivery model.
/// Audit stamping and event dispatching are strictly separated across two independent interceptors,
/// respecting the Single Responsibility Principle (SRP).
/// </summary>
public sealed class DispatchDomainEventsInterceptor(IDomainEventDispatcher dispatcher) : SaveChangesInterceptor
{
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            await DispatchDomainEventsAsync(eventData.Context, cancellationToken);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private async Task DispatchDomainEventsAsync(DbContext context, CancellationToken cancellationToken)
    {
        var entitiesWithEvents = context.ChangeTracker
            .Entries<BaseEntity<Guid>>()
            .Where(entry => entry.Entity.DomainEvents.Count > 0)
            .Select(entry => entry.Entity)
            .ToList();

        var domainEvents = entitiesWithEvents
            .SelectMany(entity => entity.DomainEvents)
            .ToList();

        foreach (BaseEntity<Guid> entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }

        foreach (IDomainEvent domainEvent in domainEvents)
        {
            await dispatcher.DispatchAsync(domainEvent, cancellationToken);
        }
    }
}
