namespace CleanArchitectureShowcase.Domain.Common;

/// <summary>
/// Encapsulates error information in the Result pattern.
/// Replaces throwing domain exceptions for expected business rule violations,
/// guaranteeing explicit, branch-safe, and high-performance error propagation.
/// </summary>
public record Error(string Code, string Description, ErrorType Type = ErrorType.Failure)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
    public static readonly Error NullValue = new("General.NullValue", "A null value was provided.", ErrorType.Failure);

    public static Error Failure(string code, string description) => new(code, description, ErrorType.Failure);
    public static Error Validation(string code, string description) => new(code, description, ErrorType.Validation);
    public static Error NotFound(string code, string description) => new(code, description, ErrorType.NotFound);
    public static Error Conflict(string code, string description) => new(code, description, ErrorType.Conflict);
}
