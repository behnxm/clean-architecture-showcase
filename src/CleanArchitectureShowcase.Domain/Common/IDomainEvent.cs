namespace CleanArchitectureShowcase.Domain.Common;

/// <summary>
/// Marker interface for all domain events raised by domain aggregates.
/// In Clean Architecture, domain events capture state transitions within the enterprise core
/// without coupling the domain to any event dispatching library, broker, or persistence engine.
/// </summary>
public interface IDomainEvent
{
}
