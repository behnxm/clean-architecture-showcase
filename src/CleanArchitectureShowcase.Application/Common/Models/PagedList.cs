namespace CleanArchitectureShowcase.Application.Common.Models;

/// <summary>
/// Paginated list container encapsulating data items and pagination metadata.
/// </summary>
/// <typeparam name="T">The element type of the paginated collection.</typeparam>
public sealed class PagedList<T>(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
{
    public IReadOnlyList<T> Items { get; } = items ?? [];

    public int Page { get; } = page;

    public int PageSize { get; } = pageSize;

    public int TotalCount { get; } = totalCount;

    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

    public bool HasPrevious => Page > 1;

    public bool HasNext => Page < TotalPages;
}
