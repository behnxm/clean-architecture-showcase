using CleanArchitectureShowcase.Application.Common.Messaging;
using CleanArchitectureShowcase.Application.Features.Products.Contracts;

namespace CleanArchitectureShowcase.Application.Features.Products.Queries.GetProductById;

/// <summary>
/// Query to retrieve a product by its unique identifier.
/// </summary>
public sealed record GetProductByIdQuery(Guid Id) : IQuery<ProductResponse>;
