using CleanArchitectureShowcase.Domain.Entities;
using FluentValidation;

namespace CleanArchitectureShowcase.Application.Features.Products.Commands.UpdateProduct;

/// <summary>
/// Validator enforcing constraints on the update product command.
/// </summary>
public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Product identifier is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(Product.NameMaxLength).WithMessage($"Product name must not exceed {Product.NameMaxLength} characters.");

        RuleFor(x => x.Description)
            .MaximumLength(Product.DescriptionMaxLength).WithMessage($"Product description must not exceed {Product.DescriptionMaxLength} characters.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Product price must be non-negative.");
    }
}
