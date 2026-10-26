namespace CleanArchitectureShowcase.Domain.Common;

/// <summary>
/// Categorizes domain errors to facilitate deterministic RFC 9457 ProblemDetails HTTP mapping
/// in the presentation layer without coupling domain logic to HTTP semantics.
/// </summary>
public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3
}
