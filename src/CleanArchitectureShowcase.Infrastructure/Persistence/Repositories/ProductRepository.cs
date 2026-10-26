using CleanArchitectureShowcase.Domain.Abstractions;
using CleanArchitectureShowcase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureShowcase.Infrastructure.Persistence.Repositories;

/// <summary>
/// Write-side repository implementation for the <see cref="Product"/> aggregate root.
/// Entities returned by this repository are tracked by EF Core to support aggregate state mutation.
/// </summary>
public sealed class ProductRepository(ApplicationDbContext context) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Products
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);
        await context.Products.AddAsync(product, cancellationToken);
    }

    public void Remove(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
        context.Products.Remove(product);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        IQueryable<Product> query = context.Products;

        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return await query.AnyAsync(p => p.Name == name, cancellationToken);
    }
}
