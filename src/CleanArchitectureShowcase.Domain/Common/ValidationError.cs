namespace CleanArchitectureShowcase.Domain.Common;

/// <summary>
/// Specialized error carrying a collection of distinct validation errors,
/// typically aggregated by input validators (e.g. FluentValidation in pipeline behaviors).
/// </summary>
public sealed record ValidationError : Error
{
    public ValidationError(IReadOnlyCollection<Error> errors)
        : base("Validation.General", "One or more validation errors occurred.", ErrorType.Validation)
    {
        Errors = errors ?? [];
    }

    public IReadOnlyCollection<Error> Errors { get; }
}
