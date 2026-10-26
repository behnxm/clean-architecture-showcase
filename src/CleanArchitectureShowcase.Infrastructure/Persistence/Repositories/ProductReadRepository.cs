using CleanArchitectureShowcase.Application.Abstractions;
using CleanArchitectureShowcase.Application.Common.Models;
using CleanArchitectureShowcase.Application.Features.Products.Contracts;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureShowcase.Infrastructure.Persistence.Repositories;

/// <summary>
/// Read-side repository implementation optimized for query performance.
/// Uses <see cref="EntityFrameworkQueryableExtensions.AsNoTracking"/>, server-side projection to DTOs,
/// safe pattern matching, and database-level pagination.
/// </summary>
public sealed class ProductReadRepository(ApplicationDbContext context) : IProductReadRepository
{
    public async Task<ProductResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProductResponse(
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.Stock,
                p.CreatedAtUtc,
                p.LastModifiedAtUtc))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedList<ProductResponse>> SearchAsync(
        ProductSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        var query = context.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
        {
            string term = criteria.SearchTerm.Trim();
            string escaped = EscapeLikePattern(term);
            string pattern = $"%{escaped}%";

            query = query.Where(p =>
                EF.Functions.Like(p.Name, pattern, @"\") ||
                EF.Functions.Like(p.Description, pattern, @"\"));
        }

        int totalCount = await query.CountAsync(cancellationToken);

        bool descending = string.Equals(criteria.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        string sortBy = criteria.SortBy?.Trim().ToLowerInvariant() ?? "name";

        query = (sortBy, descending) switch
        {
            ("price", false) => query.OrderBy(p => p.Price).ThenBy(p => p.Id),
            ("price", true) => query.OrderByDescending(p => p.Price).ThenBy(p => p.Id),
            ("stock", false) => query.OrderBy(p => p.Stock).ThenBy(p => p.Id),
            ("stock", true) => query.OrderByDescending(p => p.Stock).ThenBy(p => p.Id),
            ("createdatutc", false) => query.OrderBy(p => p.CreatedAtUtc).ThenBy(p => p.Id),
            ("createdatutc", true) => query.OrderByDescending(p => p.CreatedAtUtc).ThenBy(p => p.Id),
            ("name", true) => query.OrderByDescending(p => p.Name).ThenBy(p => p.Id),
            _ => query.OrderBy(p => p.Name).ThenBy(p => p.Id)
        };

        int skip = (criteria.Page - 1) * criteria.PageSize;

        List<ProductResponse> items = await query
            .Skip(skip)
            .Take(criteria.PageSize)
            .Select(p => new ProductResponse(
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.Stock,
                p.CreatedAtUtc,
                p.LastModifiedAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedList<ProductResponse>(items, criteria.Page, criteria.PageSize, totalCount);
    }

    private static string EscapeLikePattern(string term)
    {
        return term
            .Replace(@"\", @"\\", StringComparison.Ordinal)
            .Replace("%", @"\%", StringComparison.Ordinal)
            .Replace("_", @"\_", StringComparison.Ordinal);
    }
}
