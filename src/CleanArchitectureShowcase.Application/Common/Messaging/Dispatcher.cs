using System.Collections.Concurrent;
using CleanArchitectureShowcase.Domain.Common;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitectureShowcase.Application.Common.Messaging;

/// <summary>
/// In-process, zero-dependency mediator implementation that dispatches commands and queries
/// through a configured pipeline of behaviors to their designated handlers.
///
/// Design note:
/// To eliminate recurring reflection and dynamic invocation overhead, this dispatcher employs
/// a generic invoker cache pattern. Upon the first request for a given message type, a strongly typed
/// invoker is instantiated once via reflection and cached in a <see cref="ConcurrentDictionary{TKey,TValue}"/>.
/// Subsequent dispatches invoke statically typed code with zero reflection overhead.
/// In environments requiring Native AOT, a source-generated approach (such as martinothamar/Mediator)
/// can replace this runtime cache with compile-time generated registrations.
/// </summary>
internal sealed class Dispatcher(IServiceProvider serviceProvider) : IDispatcher
{
    private static readonly ConcurrentDictionary<Type, CommandInvoker> _commandInvokers = new();
    private static readonly ConcurrentDictionary<Type, object> _commandWithResultInvokers = new();
    private static readonly ConcurrentDictionary<Type, object> _queryInvokers = new();

    public Task<Result> Send(ICommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Type commandType = command.GetType();
        CommandInvoker invoker = _commandInvokers.GetOrAdd(
            commandType,
            static type => (CommandInvoker)Activator.CreateInstance(typeof(CommandInvokerImpl<>).MakeGenericType(type))!);

        return invoker.Invoke(command, serviceProvider, cancellationToken);
    }

    public Task<Result<TResponse>> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Type commandType = command.GetType();
        var invoker = (CommandWithResultInvoker<TResponse>)_commandWithResultInvokers.GetOrAdd(
            commandType,
            static type => Activator.CreateInstance(typeof(CommandWithResultInvokerImpl<,>).MakeGenericType(type, typeof(TResponse)))!);

        return invoker.Invoke(command, serviceProvider, cancellationToken);
    }

    public Task<Result<TResponse>> Query<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        Type queryType = query.GetType();
        var invoker = (QueryInvoker<TResponse>)_queryInvokers.GetOrAdd(
            queryType,
            static type => Activator.CreateInstance(typeof(QueryInvokerImpl<,>).MakeGenericType(type, typeof(TResponse)))!);

        return invoker.Invoke(query, serviceProvider, cancellationToken);
    }

    #region Invoker Cache Implementations

    private abstract class CommandInvoker
    {
        public abstract Task<Result> Invoke(ICommand command, IServiceProvider sp, CancellationToken ct);
    }

    private sealed class CommandInvokerImpl<TCommand> : CommandInvoker
        where TCommand : ICommand
    {
        public override async Task<Result> Invoke(ICommand command, IServiceProvider sp, CancellationToken ct)
        {
            ICommandHandler<TCommand> handler = sp.GetRequiredService<ICommandHandler<TCommand>>();
            IEnumerable<IPipelineBehavior<TCommand, Result>> behaviors = sp.GetServices<IPipelineBehavior<TCommand, Result>>().Reverse();

            RequestHandlerDelegate<Result> pipeline = () => handler.Handle((TCommand)command, ct);

            foreach (IPipelineBehavior<TCommand, Result>? behavior in behaviors)
            {
                RequestHandlerDelegate<Result> next = pipeline;
                IPipelineBehavior<TCommand, Result> currentBehavior = behavior;
                pipeline = () => currentBehavior.Handle((TCommand)command, next, ct);
            }

            return await pipeline();
        }
    }

    private abstract class CommandWithResultInvoker<TResponse>
    {
        public abstract Task<Result<TResponse>> Invoke(ICommand<TResponse> command, IServiceProvider sp, CancellationToken ct);
    }

    private sealed class CommandWithResultInvokerImpl<TCommand, TResponse> : CommandWithResultInvoker<TResponse>
        where TCommand : ICommand<TResponse>
    {
        public override async Task<Result<TResponse>> Invoke(ICommand<TResponse> command, IServiceProvider sp, CancellationToken ct)
        {
            ICommandHandler<TCommand, TResponse> handler = sp.GetRequiredService<ICommandHandler<TCommand, TResponse>>();
            IEnumerable<IPipelineBehavior<TCommand, Result<TResponse>>> behaviors = sp.GetServices<IPipelineBehavior<TCommand, Result<TResponse>>>().Reverse();

            RequestHandlerDelegate<Result<TResponse>> pipeline = () => handler.Handle((TCommand)command, ct);

            foreach (IPipelineBehavior<TCommand, Result<TResponse>>? behavior in behaviors)
            {
                RequestHandlerDelegate<Result<TResponse>> next = pipeline;
                IPipelineBehavior<TCommand, Result<TResponse>> currentBehavior = behavior;
                pipeline = () => currentBehavior.Handle((TCommand)command, next, ct);
            }

            return await pipeline();
        }
    }

    private abstract class QueryInvoker<TResponse>
    {
        public abstract Task<Result<TResponse>> Invoke(IQuery<TResponse> query, IServiceProvider sp, CancellationToken ct);
    }

    private sealed class QueryInvokerImpl<TQuery, TResponse> : QueryInvoker<TResponse>
        where TQuery : IQuery<TResponse>
    {
        public override async Task<Result<TResponse>> Invoke(IQuery<TResponse> query, IServiceProvider sp, CancellationToken ct)
        {
            IQueryHandler<TQuery, TResponse> handler = sp.GetRequiredService<IQueryHandler<TQuery, TResponse>>();
            IEnumerable<IPipelineBehavior<TQuery, Result<TResponse>>> behaviors = sp.GetServices<IPipelineBehavior<TQuery, Result<TResponse>>>().Reverse();

            RequestHandlerDelegate<Result<TResponse>> pipeline = () => handler.Handle((TQuery)query, ct);

            foreach (IPipelineBehavior<TQuery, Result<TResponse>>? behavior in behaviors)
            {
                RequestHandlerDelegate<Result<TResponse>> next = pipeline;
                IPipelineBehavior<TQuery, Result<TResponse>> currentBehavior = behavior;
                pipeline = () => currentBehavior.Handle((TQuery)query, next, ct);
            }

            return await pipeline();
        }
    }

    #endregion
}
