using CleanArchitectureShowcase.Application.Common.Messaging;
using CleanArchitectureShowcase.Domain.Abstractions;
using CleanArchitectureShowcase.Domain.Common;
using CleanArchitectureShowcase.Domain.Entities;
using CleanArchitectureShowcase.Domain.Errors;

namespace CleanArchitectureShowcase.Application.Features.Products.Commands.DeleteProduct;

/// <summary>
/// Handles <see cref="DeleteProductCommand"/> by validating existence, removing from repository,
/// and saving changes.
/// </summary>
internal sealed class DeleteProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteProductCommand>
{
    public async Task<Result> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        Product? product = await productRepository.GetByIdAsync(command.Id, cancellationToken);
        if (product is null)
        {
            return ProductErrors.NotFound(command.Id);
        }

        productRepository.Remove(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
