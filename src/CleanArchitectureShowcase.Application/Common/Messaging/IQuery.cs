namespace CleanArchitectureShowcase.Application.Common.Messaging;

/// <summary>
/// Represents a CQRS query that retrieves state without side effects or mutations.
/// </summary>
/// <typeparam name="TResponse">The payload type returned on success.</typeparam>
public interface IQuery<TResponse>
{
}
