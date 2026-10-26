using CleanArchitectureShowcase.Domain.Entities;
using FluentValidation;

namespace CleanArchitectureShowcase.Application.Features.Products.Commands.CreateProduct;

/// <summary>
/// Validator enforcing request-level constraints for creating a product.
/// Reuses domain entity constants as the single source of truth for constraints.
/// </summary>
public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(Product.NameMaxLength).WithMessage($"Product name must not exceed {Product.NameMaxLength} characters.");

        RuleFor(x => x.Description)
            .MaximumLength(Product.DescriptionMaxLength).WithMessage($"Product description must not exceed {Product.DescriptionMaxLength} characters.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Product price must be non-negative.");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Product stock must be non-negative.");
    }
}
