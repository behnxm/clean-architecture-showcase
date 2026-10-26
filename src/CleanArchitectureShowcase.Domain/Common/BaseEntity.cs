namespace CleanArchitectureShowcase.Domain.Common;

/// <summary>
/// Base class for all domain entities providing identity-based equality and domain event encapsulation.
/// In accordance with Domain-Driven Design (DDD), entity identity is defined exclusively by its identifier,
/// not by transient database row attributes.
/// </summary>
/// <typeparam name="TId">The type of the entity's unique identifier.</typeparam>
public abstract class BaseEntity<TId> : IEquatable<BaseEntity<TId>>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected BaseEntity(TId id)
    {
        Id = id;
    }

    /// <summary>
    /// Parameterless constructor required by Entity Framework Core during entity materialization.
    /// Protected to ensure aggregate instantiation remains restricted to domain factory methods.
    /// </summary>
    protected BaseEntity()
    {
        Id = default!;
    }

    public TId Id { get; protected set; }

    /// <summary>
    /// Gets a read-only snapshot of recorded domain events awaiting dispatch after transaction commit.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Records a domain event to be dispatched when the current aggregate transaction completes successfully.
    /// </summary>
    /// <param name="domainEvent">The domain event instance to append.</param>
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all recorded domain events once they have been collected for dispatching.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    public override bool Equals(object? obj) => obj is BaseEntity<TId> other && Equals(other);

    public bool Equals(BaseEntity<TId>? other)
    {
        if (other is null)
        {
            return false;
        }

        return ReferenceEquals(this, other) || (GetType() == other.GetType() && EqualityComparer<TId>.Default.Equals(Id, other.Id));
    }

    public override int GetHashCode() => EqualityComparer<TId>.Default.GetHashCode(Id);

    public static bool operator ==(BaseEntity<TId>? left, BaseEntity<TId>? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(BaseEntity<TId>? left, BaseEntity<TId>? right)
    {
        return !Equals(left, right);
    }
}
