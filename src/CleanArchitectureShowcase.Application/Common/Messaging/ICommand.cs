namespace CleanArchitectureShowcase.Application.Common.Messaging;

/// <summary>
/// Represents a CQRS command that mutates state and returns a void Result.
/// Command objects encapsulate all intent and parameters required for the mutation.
/// </summary>
public interface ICommand
{
}

/// <summary>
/// Represents a CQRS command that mutates state and returns a typed Result carrying <typeparamref name="TResponse"/>.
/// Typically used when a creation command returns the generated identifier.
/// </summary>
/// <typeparam name="TResponse">The payload type returned on success.</typeparam>
public interface ICommand<TResponse>
{
}
