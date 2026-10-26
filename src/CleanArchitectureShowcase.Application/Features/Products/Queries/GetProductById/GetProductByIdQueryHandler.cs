using CleanArchitectureShowcase.Application.Abstractions;
using CleanArchitectureShowcase.Application.Common.Messaging;
using CleanArchitectureShowcase.Application.Features.Products.Contracts;
using CleanArchitectureShowcase.Domain.Common;
using CleanArchitectureShowcase.Domain.Errors;

namespace CleanArchitectureShowcase.Application.Features.Products.Queries.GetProductById;

/// <summary>
/// Handles <see cref="GetProductByIdQuery"/> by querying the read repository directly.
/// </summary>
internal sealed class GetProductByIdQueryHandler(IProductReadRepository readRepository)
    : IQueryHandler<GetProductByIdQuery, ProductResponse>
{
    public async Task<Result<ProductResponse>> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        ProductResponse? product = await readRepository.GetByIdAsync(query.Id, cancellationToken);
        if (product is null)
        {
            return ProductErrors.NotFound(query.Id);
        }

        return product;
    }
}
