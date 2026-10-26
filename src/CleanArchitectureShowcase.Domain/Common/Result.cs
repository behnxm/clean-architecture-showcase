namespace CleanArchitectureShowcase.Domain.Common;

/// <summary>
/// Encapsulates the outcome of a domain or application operation without throwing exceptions.
/// Enforces invariants: a successful result cannot carry an error, and a failure result must carry one.
/// </summary>
public class Result : IFailureResult<Result>
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
        {
            throw new InvalidOperationException("A successful result cannot carry an error.");
        }

        if (!isSuccess && error == Error.None)
        {
            throw new InvalidOperationException("A failure result must carry an error.");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);

    public static implicit operator Result(Error error) => Failure(error);
}

/// <summary>
/// Typed result encapsulation containing a return value on success or an error on failure.
/// </summary>
/// <typeparam name="TValue">The payload type returned on success.</typeparam>
public class Result<TValue> : Result, IFailureResult<Result<TValue>>
{
    private readonly TValue? _value;

    protected internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the payload value when the result is successful; throws when called on failure.
    /// </summary>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access the value of a failed result.");

    public static new Result<TValue> Failure(Error error) => new(default, false, error);

    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure(Error.NullValue);

    public static implicit operator Result<TValue>(Error error) => Failure(error);
}
