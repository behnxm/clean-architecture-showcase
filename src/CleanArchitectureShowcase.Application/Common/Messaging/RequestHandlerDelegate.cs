using CleanArchitectureShowcase.Domain.Common;

namespace CleanArchitectureShowcase.Application.Common.Messaging;

/// <summary>
/// Continuation delegate invoked to proceed to the next component or handler in the request pipeline.
/// </summary>
/// <typeparam name="TResponse">The response type produced by the pipeline.</typeparam>
/// <returns>A task returning the response result.</returns>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>()
    where TResponse : Result;
