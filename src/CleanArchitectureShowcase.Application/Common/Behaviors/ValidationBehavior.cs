using System.Reflection;
using CleanArchitectureShowcase.Application.Common.Messaging;
using CleanArchitectureShowcase.Domain.Common;
using FluentValidation;
using FluentValidation.Results;

namespace CleanArchitectureShowcase.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior running all registered FluentValidation validators for incoming requests.
/// If validation fails, execution short-circuits immediately with a <see cref="ValidationError"/> Result,
/// ensuring downstream handlers never execute with invalid input.
/// </summary>
/// <typeparam name="TRequest">The incoming command or query type.</typeparam>
/// <typeparam name="TResponse">The response type constrained to <see cref="Result"/>.</typeparam>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        ValidationResult[] validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
        {
            return await next();
        }

        var errors = failures
            .Select(f => Error.Validation(f.PropertyName, f.ErrorMessage))
            .ToList();

        var validationError = new ValidationError(errors);

        return CreateFailureResult(validationError);
    }

    private static TResponse CreateFailureResult(ValidationError error)
    {
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error);
        }

        return FailureFactory<TResponse>.Create(error);
    }

    private static class FailureFactory<T>
    {
        private static readonly Func<Error, T> _factory = CreateFactory();

        public static T Create(Error error) => _factory(error);

        private static Func<Error, T> CreateFactory()
        {
            MethodInfo? method = typeof(T).GetMethod(
                nameof(Result.Failure),
                BindingFlags.Public | BindingFlags.Static,
                [typeof(Error)]);

            if (method is not null)
            {
                return (Func<Error, T>)Delegate.CreateDelegate(typeof(Func<Error, T>), method);
            }

            throw new InvalidOperationException($"Type {typeof(T).FullName} does not provide a public static Failure(Error) method.");
        }
    }
}
