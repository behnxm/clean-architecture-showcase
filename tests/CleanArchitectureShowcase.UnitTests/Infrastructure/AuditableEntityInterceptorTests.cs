using CleanArchitectureShowcase.Domain.Entities;
using CleanArchitectureShowcase.Infrastructure.Persistence;
using CleanArchitectureShowcase.Infrastructure.Persistence.Interceptors;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Shouldly;
using Xunit;

namespace CleanArchitectureShowcase.UnitTests.Infrastructure;

public sealed class AuditableEntityInterceptorTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public AuditableEntityInterceptorTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    [Fact]
    public async Task AuditableEntityInterceptor_SetsCreatedAtAndLastModifiedAtViaTimeProvider()
    {
        // Arrange
        var initialTime = new DateTimeOffset(2026, 1, 15, 10, 0, 0, TimeSpan.Zero);
        var fakeTimeProvider = new FakeTimeProvider(initialTime);
        var interceptor = new AuditableEntityInterceptor(fakeTimeProvider);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .AddInterceptors(interceptor)
            .Options;

        await using (var context = new ApplicationDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();

            var product = Product.Create("Audit Test Product", "Desc", 99m, 10).Value;
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            product.CreatedAtUtc.ShouldBe(initialTime.UtcDateTime);
            product.LastModifiedAtUtc.ShouldBeNull();
        }

        // Advance time
        fakeTimeProvider.Advance(TimeSpan.FromHours(2));
        DateTime expectedModifiedTime = fakeTimeProvider.GetUtcNow().UtcDateTime;

        await using (var context = new ApplicationDbContext(options))
        {
            Product? product = await context.Products.FirstAsync();
            product.Update("Updated Name", "Updated Desc", 120m);
            await context.SaveChangesAsync();

            product.CreatedAtUtc.ShouldBe(initialTime.UtcDateTime);
            product.LastModifiedAtUtc.ShouldBe(expectedModifiedTime);
        }
    }
}
