using CleanArchitectureShowcase.Application.Common.Models;
using CleanArchitectureShowcase.Application.Features.Products.Contracts;

namespace CleanArchitectureShowcase.Application.Abstractions;

/// <summary>
/// Read-side repository abstraction optimized for high-performance read queries.
/// In Clean Architecture CQRS, queries bypass domain write models and retrieve projected DTOs directly.
/// </summary>
public interface IProductReadRepository
{
    /// <summary>
    /// Retrieves a single product projected as a DTO by its unique identifier.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The product response or null if not found.</returns>
    Task<ProductResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs server-side search, filtering, sorting, and pagination directly against the database.
    /// </summary>
    /// <param name="criteria">Search, sort, and pagination criteria.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated list of product responses.</returns>
    Task<PagedList<ProductResponse>> SearchAsync(ProductSearchCriteria criteria, CancellationToken cancellationToken = default);
}
