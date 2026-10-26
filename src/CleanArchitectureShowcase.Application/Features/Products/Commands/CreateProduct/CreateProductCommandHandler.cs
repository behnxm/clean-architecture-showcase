using CleanArchitectureShowcase.Application.Common.Messaging;
using CleanArchitectureShowcase.Domain.Abstractions;
using CleanArchitectureShowcase.Domain.Common;
using CleanArchitectureShowcase.Domain.Entities;
using CleanArchitectureShowcase.Domain.Errors;

namespace CleanArchitectureShowcase.Application.Features.Products.Commands.CreateProduct;

/// <summary>
/// Handles <see cref="CreateProductCommand"/> by enforcing domain business logic,
/// verifying name uniqueness, persisting through repository, and committing via Unit of Work.
/// </summary>
internal sealed class CreateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateProductCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (await productRepository.ExistsByNameAsync(command.Name, cancellationToken: cancellationToken))
        {
            return ProductErrors.DuplicateName;
        }

        Result<Product> productResult = Product.Create(command.Name, command.Description, command.Price, command.Stock);
        if (productResult.IsFailure)
        {
            return productResult.Error;
        }

        Product product = productResult.Value;
        await productRepository.AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}
