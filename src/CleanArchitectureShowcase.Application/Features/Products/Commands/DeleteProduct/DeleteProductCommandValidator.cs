using FluentValidation;

namespace CleanArchitectureShowcase.Application.Features.Products.Commands.DeleteProduct;

/// <summary>
/// Validator enforcing constraints on the delete product command.
/// </summary>
public sealed class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Product identifier is required.");
    }
}
