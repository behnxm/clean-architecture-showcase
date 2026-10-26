using CleanArchitectureShowcase.Application.Abstractions;
using CleanArchitectureShowcase.Application.Common.Messaging;
using CleanArchitectureShowcase.Application.Common.Models;
using CleanArchitectureShowcase.Application.Features.Products.Contracts;
using CleanArchitectureShowcase.Domain.Common;

namespace CleanArchitectureShowcase.Application.Features.Products.Queries.GetProducts;

/// <summary>
/// Handles <see cref="GetProductsQuery"/> by mapping query criteria to the read-side repository.
/// </summary>
internal sealed class GetProductsQueryHandler(IProductReadRepository readRepository)
    : IQueryHandler<GetProductsQuery, PagedList<ProductResponse>>
{
    public async Task<Result<PagedList<ProductResponse>>> Handle(GetProductsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var criteria = new ProductSearchCriteria(
            query.SearchTerm,
            query.Page,
            query.PageSize,
            query.SortBy,
            query.SortDirection);

        PagedList<ProductResponse> result = await readRepository.SearchAsync(criteria, cancellationToken);
        return result;
    }
}
