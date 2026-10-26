using FluentValidation;

namespace CleanArchitectureShowcase.Application.Features.Products.Queries.GetProducts;

/// <summary>
/// Validator enforcing pagination limits and sort column whitelisting on catalog product queries.
/// </summary>
public sealed class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    private static readonly string[] _allowedSortColumns = ["name", "price", "stock", "createdatutc"];
    private static readonly string[] _allowedSortDirections = ["asc", "desc"];

    public GetProductsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.SortBy)
            .Must(sortBy => string.IsNullOrWhiteSpace(sortBy) || _allowedSortColumns.Contains(sortBy.Trim().ToLowerInvariant()))
            .WithMessage($"SortBy must be one of: {string.Join(", ", _allowedSortColumns)}.");

        RuleFor(x => x.SortDirection)
            .Must(dir => string.IsNullOrWhiteSpace(dir) || _allowedSortDirections.Contains(dir.Trim().ToLowerInvariant()))
            .WithMessage("SortDirection must be either 'asc' or 'desc'.");
    }
}
