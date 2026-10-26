using CleanArchitectureShowcase.Application.Common.Messaging;
using CleanArchitectureShowcase.Domain.Abstractions;
using CleanArchitectureShowcase.Domain.Common;
using CleanArchitectureShowcase.Domain.Entities;
using CleanArchitectureShowcase.Domain.Errors;

namespace CleanArchitectureShowcase.Application.Features.Products.Commands.UpdateProduct;

/// <summary>
/// Handles <see cref="UpdateProductCommand"/> by verifying entity existence,
/// checking for duplicate names excluding the target product, applying domain mutations,
/// and committing the transaction.
/// </summary>
internal sealed class UpdateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateProductCommand>
{
    public async Task<Result> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        Product? product = await productRepository.GetByIdAsync(command.Id, cancellationToken);
        if (product is null)
        {
            return ProductErrors.NotFound(command.Id);
        }

        if (await productRepository.ExistsByNameAsync(command.Name, command.Id, cancellationToken))
        {
            return ProductErrors.DuplicateName;
        }

        Result updateResult = product.Update(command.Name, command.Description, command.Price);
        if (updateResult.IsFailure)
        {
            return updateResult.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
