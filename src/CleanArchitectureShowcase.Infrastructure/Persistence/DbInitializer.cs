using CleanArchitectureShowcase.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CleanArchitectureShowcase.Infrastructure.Persistence;

/// <summary>
/// Database initialization and seed data service.
/// In Development environments, applies EF Core migrations automatically and seeds initial sample records.
/// Seeds products exclusively via <see cref="Product.Create"/> to ensure seed records satisfy all domain invariants.
/// </summary>
public static partial class DbInitializer
{
    public static async Task InitializeAsync(
        ApplicationDbContext context,
        bool applyMigrations,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        if (applyMigrations)
        {
            try
            {
                if (logger is not null)
                {
                    LogApplyingMigrations(logger);
                }

                await context.Database.MigrateAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                if (logger is not null)
                {
                    LogMigrationFallback(logger, ex);
                }

                await context.Database.EnsureCreatedAsync(cancellationToken);
            }
        }

        if (await context.Products.AnyAsync(cancellationToken))
        {
            return;
        }

        if (logger is not null)
        {
            LogSeedingProducts(logger);
        }

        Product[] seedProducts =
        [
            Product.Create(
                "Mechanical Gaming Keyboard",
                "Ergonomic tactile mechanical keyboard featuring custom mechanical switches, per-key RGB backlighting, and aluminum frame.",
                129.99m,
                45).Value,

            Product.Create(
                "Precision Wireless Mouse",
                "Ultra-lightweight ergonomic mouse with 26K DPI optical sensor, optical switches, and sub-millisecond wireless latency.",
                79.99m,
                80).Value,

            Product.Create(
                "34-Inch UltraWide Monitor",
                "WQHD 3440x1440 curved IPS gaming and productivity display with 144Hz refresh rate, 1ms response time, and HDR400.",
                499.99m,
                20).Value,

            Product.Create(
                "Noise-Canceling Wireless Headphones",
                "Premium over-ear headphones with adaptive active noise cancellation, transparency mode, and 30-hour battery life.",
                249.99m,
                35).Value,

            Product.Create(
                "10-in-1 USB-C Docking Station",
                "Universal multiport adapter supporting dual 4K HDMI, Gigabit Ethernet, 100W Power Delivery pass-through, and SD card reader.",
                49.99m,
                120).Value
        ];

        foreach (Product product in seedProducts)
        {
            product.ClearDomainEvents();
            await context.Products.AddAsync(product, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);

        if (logger is not null)
        {
            LogSeededProducts(logger, seedProducts.Length);
        }
    }

    [LoggerMessage(EventId = 3001, Level = LogLevel.Information, Message = "Applying EF Core database migrations...")]
    private static partial void LogApplyingMigrations(ILogger logger);

    [LoggerMessage(EventId = 3002, Level = LogLevel.Warning, Message = "Applying migrations failed; falling back to EnsureCreatedAsync.")]
    private static partial void LogMigrationFallback(ILogger logger, Exception ex);

    [LoggerMessage(EventId = 3003, Level = LogLevel.Information, Message = "Seeding initial catalog products...")]
    private static partial void LogSeedingProducts(ILogger logger);

    [LoggerMessage(EventId = 3004, Level = LogLevel.Information, Message = "Successfully seeded {Count} products.")]
    private static partial void LogSeededProducts(ILogger logger, int count);
}
