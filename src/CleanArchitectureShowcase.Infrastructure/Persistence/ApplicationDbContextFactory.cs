using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CleanArchitectureShowcase.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for <see cref="ApplicationDbContext"/> enabling EF Core CLI tooling
/// (<c>dotnet ef</c>) to discover entity configurations and scaffold migrations
/// independently from runtime WebAPI startup configuration.
/// </summary>
public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlite("Data Source=productcatalog.db");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
