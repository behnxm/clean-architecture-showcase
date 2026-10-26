using CleanArchitectureShowcase.Domain.Common;

namespace CleanArchitectureShowcase.Domain.Events;

/// <summary>
/// Domain event published when a new product aggregate is created.
/// Carries key aggregate details to allow decoupled event handlers (e.g., logging, integration, analytics)
/// to react without querying or mutating the originating aggregate.
/// </summary>
public sealed record ProductCreatedDomainEvent(Guid ProductId, string Name, decimal Price) : IDomainEvent;
