using CleanArchitectureShowcase.Domain.Common;

namespace CleanArchitectureShowcase.Domain.Errors;

/// <summary>
/// Domain-level errors specific to the Product aggregate root.
/// Codes follow the 'Entity.Reason' naming convention to align with API ProblemDetails extensions.
/// </summary>
public static class ProductErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Product.NotFound", $"Product with identifier '{id}' was not found.");

    public static readonly Error EmptyName =
        Error.Validation("Product.EmptyName", "Product name cannot be empty.");

    public static readonly Error NameTooLong =
        Error.Validation("Product.NameTooLong", "Product name exceeds maximum length.");

    public static readonly Error DescriptionTooLong =
        Error.Validation("Product.DescriptionTooLong", "Product description exceeds maximum length.");

    public static readonly Error InvalidPrice =
        Error.Validation("Product.InvalidPrice", "Product price cannot be negative.");

    public static readonly Error InvalidStock =
        Error.Validation("Product.InvalidStock", "Product stock cannot be negative.");

    public static readonly Error InsufficientStock =
        Error.Validation("Product.InsufficientStock", "Product stock cannot be reduced below zero.");

    public static readonly Error DuplicateName =
        Error.Conflict("Product.DuplicateName", "A product with the same name already exists.");
}
