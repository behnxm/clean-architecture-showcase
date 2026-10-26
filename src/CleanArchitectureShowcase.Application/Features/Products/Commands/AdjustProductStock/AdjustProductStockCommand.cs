using CleanArchitectureShowcase.Application.Common.Messaging;

namespace CleanArchitectureShowcase.Application.Features.Products.Commands.AdjustProductStock;

/// <summary>
/// Command to adjust the inventory stock of a product by a positive or negative delta.
/// </summary>
public sealed record AdjustProductStockCommand(Guid Id, int Delta) : ICommand;
