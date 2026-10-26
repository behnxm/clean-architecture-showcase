using System.Reflection;
using CleanArchitectureShowcase.Domain.Abstractions;
using CleanArchitectureShowcase.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CleanArchitectureShowcase.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context implementing the <see cref="IUnitOfWork"/> contract.
/// In accordance with Clean Architecture, high-level use cases interact with this context solely
/// through domain abstractions (<see cref="IUnitOfWork"/> and aggregate repositories).
/// </summary>
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // SQLite Decimal Sorting Caveat:
        // SQLite does not feature a native decimal numeric data type; EF Core defaults to storing decimals
        // as TEXT, which causes ORDER BY queries on decimal columns to sort lexicographically rather than numerically
        // (e.g. "100.0" sorts before "20.0").
        // When using the SQLite provider, we register a value converter transforming decimal to double for accurate
        // numeric comparisons and ordering, while retaining decimal precision at the domain entity boundary.
        if (Database.IsSqlite())
        {
            foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
            {
                IEnumerable<PropertyInfo> decimalProperties = entityType.ClrType.GetProperties()
                    .Where(p => p.PropertyType == typeof(decimal) || p.PropertyType == typeof(decimal?));

                foreach (PropertyInfo? property in decimalProperties)
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property(property.Name)
                        .HasConversion<double>();
                }
            }
        }

        base.OnModelCreating(modelBuilder);
    }
}
