using CleanArchitectureShowcase.Domain.Entities;

namespace CleanArchitectureShowcase.Domain.Abstractions;

/// <summary>
/// Write-side repository abstraction for managing the <see cref="Product"/> aggregate root.
/// In Clean Architecture, defining this interface within the Domain/Application core and implementing it
/// within the Infrastructure layer exemplifies the Dependency Inversion Principle (DIP).
/// High-level domain business logic does not depend on database drivers or ORM libraries; instead,
/// low-level data access implementations depend on domain abstractions.
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Retrieves a product aggregate by its unique identifier, tracked for mutations.
    /// </summary>
    /// <param name="id">The unique identifier of the product aggregate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The tracked Product aggregate root, or null if not found.</returns>
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enrolls a new product aggregate for insertion.
    /// </summary>
    /// <param name="product">The product aggregate root instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(Product product, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enrolls an existing product aggregate for deletion.
    /// </summary>
    /// <param name="product">The product aggregate root instance.</param>
    void Remove(Product product);

    /// <summary>
    /// Checks whether a product with the specified name already exists in the catalog.
    /// </summary>
    /// <param name="name">Product title to evaluate.</param>
    /// <param name="excludeId">Optional aggregate ID to exclude (e.g. during update operations).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if a conflicting name exists; otherwise false.</returns>
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
