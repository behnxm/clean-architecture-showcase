using CleanArchitectureShowcase.Application.Common.Messaging;

namespace CleanArchitectureShowcase.Application.Features.Products.Commands.UpdateProduct;

/// <summary>
/// Command to update product descriptive attributes and unit price.
/// </summary>
public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    decimal Price) : ICommand;
