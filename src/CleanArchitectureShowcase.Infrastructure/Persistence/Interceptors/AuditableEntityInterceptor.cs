using CleanArchitectureShowcase.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CleanArchitectureShowcase.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Entity Framework Core interceptor that automatically stamps audit metadata on entities implementing
/// <see cref="IAuditableEntity"/> prior to saving changes.
/// Utilizes the modern .NET <see cref="TimeProvider"/> abstraction to enable deterministic time testing.
/// </summary>
public sealed class AuditableEntityInterceptor(TimeProvider timeProvider) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateAuditableEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateAuditableEntities(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        DateTime utcNow = timeProvider.GetUtcNow().UtcDateTime;

        foreach (EntityEntry<IAuditableEntity> entry in context.ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(nameof(IAuditableEntity.CreatedAtUtc)).CurrentValue = utcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(IAuditableEntity.LastModifiedAtUtc)).CurrentValue = utcNow;
            }
        }
    }
}
