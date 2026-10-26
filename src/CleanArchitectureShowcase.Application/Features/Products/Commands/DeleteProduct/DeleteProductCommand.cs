using CleanArchitectureShowcase.Application.Common.Messaging;

namespace CleanArchitectureShowcase.Application.Features.Products.Commands.DeleteProduct;

/// <summary>
/// Command to delete a product from the catalog by its identifier.
/// </summary>
public sealed record DeleteProductCommand(Guid Id) : ICommand;
