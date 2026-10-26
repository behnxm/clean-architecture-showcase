namespace CleanArchitectureShowcase.Application.Abstractions;

/// <summary>
/// Encapsulates search, pagination, and sorting parameters for querying the product catalog.
/// </summary>
public sealed record ProductSearchCriteria(
    string? SearchTerm,
    int Page,
    int PageSize,
    string? SortBy,
    string? SortDirection);
