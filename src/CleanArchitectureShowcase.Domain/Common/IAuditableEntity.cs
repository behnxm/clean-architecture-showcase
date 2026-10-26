namespace CleanArchitectureShowcase.Domain.Common;

/// <summary>
/// Defines audit tracking metadata for domain entities.
/// Properties are declared get-only in the contract so that callers outside persistence cannot mutate them,
/// while persistence interceptors mutate them via EF Core property access or backing fields.
/// </summary>
public interface IAuditableEntity
{
    DateTime CreatedAtUtc { get; }
    DateTime? LastModifiedAtUtc { get; }
}
