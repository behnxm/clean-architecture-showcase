namespace CleanArchitectureShowcase.WebAPI.Contracts;

/// <summary>
/// HTTP request body for updating descriptive attributes and price of an existing product.
/// </summary>
/// <param name="Name">Updated product title (required, maximum 200 characters).</param>
/// <param name="Description">Updated product description (optional, maximum 2000 characters).</param>
/// <param name="Price">Updated unit price (must be greater than or equal to zero).</param>
public sealed record UpdateProductRequest(
    string Name,
    string Description,
    decimal Price);
