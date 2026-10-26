using CleanArchitectureShowcase.Application.Common.Messaging;
using CleanArchitectureShowcase.Application.Common.Models;
using CleanArchitectureShowcase.Application.Features.Products.Contracts;

namespace CleanArchitectureShowcase.Application.Features.Products.Queries.GetProducts;

/// <summary>
/// Query to search, filter, sort, and paginate products in the catalog.
/// </summary>
public sealed record GetProductsQuery(
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 10,
    string? SortBy = "name",
    string? SortDirection = "asc") : IQuery<PagedList<ProductResponse>>;
