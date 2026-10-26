using CleanArchitectureShowcase.Domain.Common;
using CleanArchitectureShowcase.Domain.Errors;
using CleanArchitectureShowcase.Domain.Events;

namespace CleanArchitectureShowcase.Domain.Entities;

/// <summary>
/// Aggregate root representing a catalog product.
/// Encapsulates all domain invariants and state mutations.
/// Setter accessibility is strictly private to ensure state cannot be modified
/// from outside the aggregate boundaries.
/// </summary>
public sealed class Product : BaseEntity<Guid>, IAuditableEntity
{
    public const int NameMaxLength = 200;
    public const int DescriptionMaxLength = 2000;

    private Product(Guid id, string name, string description, decimal price, int stock)
        : base(id)
    {
        Name = name;
        Description = description;
        Price = price;
        Stock = stock;
    }

    /// <summary>
    /// Parameterless constructor required by Entity Framework Core for entity materialization.
    /// Kept private to prevent arbitrary instantiation without invariant verification.
    /// </summary>
    private Product()
    {
        Name = null!;
        Description = null!;
    }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public decimal Price { get; private set; }

    public int Stock { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? LastModifiedAtUtc { get; private set; }

    /// <summary>
    /// Factory method enforcing aggregate invariants and generating a time-ordered UUIDv7 identifier.
    /// </summary>
    /// <param name="name">Product title or display name.</param>
    /// <param name="description">Detailed product description.</param>
    /// <param name="price">Unit price, must be non-negative.</param>
    /// <param name="stock">Initial inventory stock count, must be non-negative.</param>
    /// <returns>A Result containing the instantiated Product or a domain error.</returns>
    public static Result<Product> Create(string name, string description, decimal price, int stock)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return ProductErrors.EmptyName;
        }

        string trimmedName = name.Trim();
        if (trimmedName.Length > NameMaxLength)
        {
            return ProductErrors.NameTooLong;
        }

        string trimmedDescription = description?.Trim() ?? string.Empty;
        if (trimmedDescription.Length > DescriptionMaxLength)
        {
            return ProductErrors.DescriptionTooLong;
        }

        if (price < 0)
        {
            return ProductErrors.InvalidPrice;
        }

        if (stock < 0)
        {
            return ProductErrors.InvalidStock;
        }

        var product = new Product(Guid.CreateVersion7(), trimmedName, trimmedDescription, price, stock);
        product.AddDomainEvent(new ProductCreatedDomainEvent(product.Id, product.Name, product.Price));

        return product;
    }

    /// <summary>
    /// Updates product descriptive attributes and unit price while enforcing domain invariants.
    /// </summary>
    public Result Update(string name, string description, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return ProductErrors.EmptyName;
        }

        string trimmedName = name.Trim();
        if (trimmedName.Length > NameMaxLength)
        {
            return ProductErrors.NameTooLong;
        }

        string trimmedDescription = description?.Trim() ?? string.Empty;
        if (trimmedDescription.Length > DescriptionMaxLength)
        {
            return ProductErrors.DescriptionTooLong;
        }

        if (price < 0)
        {
            return ProductErrors.InvalidPrice;
        }

        Name = trimmedName;
        Description = trimmedDescription;
        Price = price;

        return Result.Success();
    }

    /// <summary>
    /// Adjusts inventory stock by a signed delta (positive to restock, negative to consume).
    /// Enforces the invariant that inventory cannot go below zero.
    /// </summary>
    /// <param name="delta">Amount to change stock by.</param>
    /// <returns>A Result indicating success or InsufficientStock failure.</returns>
    public Result AdjustStock(int delta)
    {
        if (Stock + delta < 0)
        {
            return ProductErrors.InsufficientStock;
        }

        Stock += delta;
        return Result.Success();
    }
}
