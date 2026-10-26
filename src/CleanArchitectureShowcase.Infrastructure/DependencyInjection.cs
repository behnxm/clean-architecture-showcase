using CleanArchitectureShowcase.Application.Abstractions;
using CleanArchitectureShowcase.Domain.Abstractions;
using CleanArchitectureShowcase.Infrastructure.Persistence;
using CleanArchitectureShowcase.Infrastructure.Persistence.Interceptors;
using CleanArchitectureShowcase.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitectureShowcase.Infrastructure;

/// <summary>
/// Dependency injection registrations for the Infrastructure layer.
/// Configures EF Core persistence with SQLite, audit and domain event interceptors,
/// repositories, unit of work, and database health checks.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers Infrastructure persistence and services into the application service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);

        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<DispatchDomainEventsInterceptor>();

        string connectionString = configuration.GetConnectionString("Database")
            ?? "Data Source=productcatalog.db";

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseSqlite(connectionString);
            options.AddInterceptors(
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<DispatchDomainEventsInterceptor>());
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductReadRepository, ProductReadRepository>();

        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>(name: "database");

        return services;
    }
}
