using FluentValidation;

namespace CleanArchitectureShowcase.Application.Features.Products.Queries.GetProductById;

/// <summary>
/// Validator enforcing constraints on the get product by identifier query.
/// </summary>
public sealed class GetProductByIdQueryValidator : AbstractValidator<GetProductByIdQuery>
{
    public GetProductByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Product identifier is required.");
    }
}
