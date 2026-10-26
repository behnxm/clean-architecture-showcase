using CleanArchitectureShowcase.Application.Common.Messaging;
using CleanArchitectureShowcase.Domain.Abstractions;
using CleanArchitectureShowcase.Domain.Common;
using CleanArchitectureShowcase.Domain.Entities;
using CleanArchitectureShowcase.Domain.Errors;

namespace CleanArchitectureShowcase.Application.Features.Products.Commands.AdjustProductStock;

/// <summary>
/// Handles <see cref="AdjustProductStockCommand"/> by retrieving the product, applying the delta,
/// and committing the state change.
/// </summary>
internal sealed class AdjustProductStockCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AdjustProductStockCommand>
{
    public async Task<Result> Handle(AdjustProductStockCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        Product? product = await productRepository.GetByIdAsync(command.Id, cancellationToken);
        if (product is null)
        {
            return ProductErrors.NotFound(command.Id);
        }

        Result adjustResult = product.AdjustStock(command.Delta);
        if (adjustResult.IsFailure)
        {
            return adjustResult.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
