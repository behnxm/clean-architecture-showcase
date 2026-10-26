namespace CleanArchitectureShowcase.WebAPI.Contracts;

/// <summary>
/// HTTP request body for creating a new catalog product.
/// </summary>
/// <param name="Name">Product title (required, maximum 200 characters).</param>
/// <param name="Description">Detailed product description (optional, maximum 2000 characters).</param>
/// <param name="Price">Unit price (must be greater than or equal to zero).</param>
/// <param name="Stock">Initial inventory stock count (must be greater than or equal to zero).</param>
public sealed record CreateProductRequest(
    string Name,
    string Description,
    decimal Price,
    int Stock);
