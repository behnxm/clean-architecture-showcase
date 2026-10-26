using FluentValidation;

namespace CleanArchitectureShowcase.Application.Features.Products.Commands.AdjustProductStock;

/// <summary>
/// Validator enforcing constraints on the stock adjustment command.
/// </summary>
public sealed class AdjustProductStockCommandValidator : AbstractValidator<AdjustProductStockCommand>
{
    public AdjustProductStockCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Product identifier is required.");

        RuleFor(x => x.Delta)
            .NotEqual(0).WithMessage("Stock adjustment delta must not be zero.");
    }
}
