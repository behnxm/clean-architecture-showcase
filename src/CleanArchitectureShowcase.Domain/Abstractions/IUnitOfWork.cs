namespace CleanArchitectureShowcase.Domain.Abstractions;

/// <summary>
/// Unit of Work contract encapsulating transactional consistency boundaries across write operations.
/// In adherence to the Dependency Inversion Principle (DIP), this abstraction resides in the enterprise core
/// so that use cases coordinate transaction completion without referencing EF Core DbContext or database drivers.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists all pending changes to the underlying storage mechanism within a single transaction.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
