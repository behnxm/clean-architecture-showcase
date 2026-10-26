namespace CleanArchitectureShowcase.Domain.Common;

/// <summary>
/// Static-abstract interface (C# 11+) allowing pipeline behaviors and dispatchers
/// to construct strongly typed failure results without reflection or boxing allocations.
/// </summary>
/// <typeparam name="TSelf">The concrete result type implementing this interface.</typeparam>
public interface IFailureResult<TSelf> where TSelf : IFailureResult<TSelf>
{
    /// <summary>
    /// Creates a failure instance of <typeparamref name="TSelf"/> with the specified error.
    /// </summary>
    /// <param name="error">The error payload.</param>
    /// <returns>A failure result instance.</returns>
    static abstract TSelf Failure(Error error);
}
