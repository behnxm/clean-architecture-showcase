using CleanArchitectureShowcase.Application.Common.Messaging;

namespace CleanArchitectureShowcase.Application.Features.Products.Commands.CreateProduct;

/// <summary>
/// Command to create a new product in the catalog.
/// Returns the generated product identifier upon success.
/// </summary>
public sealed record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    int Stock) : ICommand<Guid>;
