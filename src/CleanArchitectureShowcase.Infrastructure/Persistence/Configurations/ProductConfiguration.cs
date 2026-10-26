using CleanArchitectureShowcase.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitectureShowcase.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core entity configuration for <see cref="Product"/>.
/// Reuses constants defined on the domain entity as the single source of truth for column lengths.
/// </summary>
public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.Name)
            .HasMaxLength(Product.NameMaxLength)
            .IsRequired();

        builder.HasIndex(p => p.Name)
            .IsUnique();

        builder.Property(p => p.Description)
            .HasMaxLength(Product.DescriptionMaxLength)
            .IsRequired();

        builder.Property(p => p.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.Stock)
            .IsRequired();

        builder.Property(p => p.CreatedAtUtc)
            .IsRequired();

        builder.Property(p => p.LastModifiedAtUtc)
            .IsRequired(false);

        // DomainEvents are in-memory domain notifications, not persistent relational attributes
        builder.Ignore(p => p.DomainEvents);
    }
}
