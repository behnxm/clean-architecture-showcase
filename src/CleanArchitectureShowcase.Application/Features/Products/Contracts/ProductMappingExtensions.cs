using CleanArchitectureShowcase.Domain.Entities;

namespace CleanArchitectureShowcase.Application.Features.Products.Contracts;

/// <summary>
/// Explicit, hand-written object projection extensions.
/// Eliminates runtime reflection and third-party mapping libraries.
/// </summary>
public static class ProductMappingExtensions
{
    public static ProductResponse ToResponse(this Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        return new ProductResponse(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Stock,
            product.CreatedAtUtc,
            product.LastModifiedAtUtc);
    }
}
