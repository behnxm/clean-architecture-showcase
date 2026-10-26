namespace CleanArchitectureShowcase.Application.Features.Products.Contracts;

/// <summary>
/// Data Transfer Object representing a product's details returned by queries.
/// </summary>
public sealed record ProductResponse(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int Stock,
    DateTime CreatedAtUtc,
    DateTime? LastModifiedAtUtc);
