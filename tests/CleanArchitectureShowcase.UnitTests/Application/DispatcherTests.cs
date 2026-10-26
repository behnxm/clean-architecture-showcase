using CleanArchitectureShowcase.Application.Common.Messaging;
using CleanArchitectureShowcase.Domain.Common;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace CleanArchitectureShowcase.UnitTests.Application;

public sealed class DispatcherTests
{
    private sealed record TestCommand(string Value) : ICommand;
    private sealed record TestCommandWithResponse(int Number) : ICommand<int>;

    private sealed class TestCommandHandler : ICommandHandler<TestCommand>
    {
        public Task<Result> Handle(TestCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result.Success());
        }
    }

    private sealed class TestCommandWithResponseHandler : ICommandHandler<TestCommandWithResponse, int>
    {
        public Task<Result<int>> Handle(TestCommandWithResponse command, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result.Success(command.Number * 2));
        }
    }

    [Fact]
    public async Task Send_DispatchesToRegisteredCommandHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<ICommandHandler<TestCommand>, TestCommandHandler>();
        services.AddScoped<IDispatcher, Dispatcher>();
        using ServiceProvider sp = services.BuildServiceProvider();
        IDispatcher dispatcher = sp.GetRequiredService<IDispatcher>();

        // Act
        Result result = await dispatcher.Send(new TestCommand("Hello"));

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task SendWithResponse_DispatchesToRegisteredHandlerAndReturnsValue()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<ICommandHandler<TestCommandWithResponse, int>, TestCommandWithResponseHandler>();
        services.AddScoped<IDispatcher, Dispatcher>();
        using ServiceProvider sp = services.BuildServiceProvider();
        IDispatcher dispatcher = sp.GetRequiredService<IDispatcher>();

        // Act
        Result<int> result = await dispatcher.Send(new TestCommandWithResponse(21));

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(42);
    }

    private sealed class TraceExecutionLog
    {
        public List<string> Steps { get; } = [];
    }

    private sealed class FirstBehavior(TraceExecutionLog log) : IPipelineBehavior<TestCommand, Result>
    {
        public async Task<Result> Handle(TestCommand request, RequestHandlerDelegate<Result> next, CancellationToken cancellationToken)
        {
            log.Steps.Add("First.Start");
            Result result = await next();
            log.Steps.Add("First.End");
            return result;
        }
    }

    private sealed class SecondBehavior(TraceExecutionLog log) : IPipelineBehavior<TestCommand, Result>
    {
        public async Task<Result> Handle(TestCommand request, RequestHandlerDelegate<Result> next, CancellationToken cancellationToken)
        {
            log.Steps.Add("Second.Start");
            Result result = await next();
            log.Steps.Add("Second.End");
            return result;
        }
    }

    private sealed class TracingCommandHandler(TraceExecutionLog log) : ICommandHandler<TestCommand>
    {
        public Task<Result> Handle(TestCommand command, CancellationToken cancellationToken)
        {
            log.Steps.Add("Handler");
            return Task.FromResult(Result.Success());
        }
    }

    [Fact]
    public async Task Dispatcher_ProvesBehaviorOrder_FirstRegisteredIsOutermost()
    {
        // Arrange
        var log = new TraceExecutionLog();
        var services = new ServiceCollection();
        services.AddSingleton(log);
        services.AddScoped<ICommandHandler<TestCommand>, TracingCommandHandler>();

        // Register behaviors: FirstBehavior then SecondBehavior
        services.AddTransient(typeof(IPipelineBehavior<TestCommand, Result>), typeof(FirstBehavior));
        services.AddTransient(typeof(IPipelineBehavior<TestCommand, Result>), typeof(SecondBehavior));

        services.AddScoped<IDispatcher, Dispatcher>();
        using ServiceProvider sp = services.BuildServiceProvider();
        IDispatcher dispatcher = sp.GetRequiredService<IDispatcher>();

        // Act
        Result result = await dispatcher.Send(new TestCommand("OrderTest"));

        // Assert
        result.IsSuccess.ShouldBeTrue();
        log.Steps.ShouldBe(["First.Start", "Second.Start", "Handler", "Second.End", "First.End"]);
    }

    private sealed class ShortCircuitBehavior : IPipelineBehavior<TestCommand, Result>
    {
        public Task<Result> Handle(TestCommand request, RequestHandlerDelegate<Result> next, CancellationToken cancellationToken)
        {
            // Do NOT call next(); short circuit immediately with failure
            return Task.FromResult<Result>(Error.Validation("ShortCircuit", "Terminated early"));
        }
    }

    [Fact]
    public async Task Dispatcher_WhenBehaviorShortCircuits_DownstreamHandlerNeverExecutes()
    {
        // Arrange
        var log = new TraceExecutionLog();
        var services = new ServiceCollection();
        services.AddSingleton(log);
        services.AddScoped<ICommandHandler<TestCommand>, TracingCommandHandler>();
        services.AddTransient(typeof(IPipelineBehavior<TestCommand, Result>), typeof(ShortCircuitBehavior));

        services.AddScoped<IDispatcher, Dispatcher>();
        using ServiceProvider sp = services.BuildServiceProvider();
        IDispatcher dispatcher = sp.GetRequiredService<IDispatcher>();

        // Act
        Result result = await dispatcher.Send(new TestCommand("ShortCircuitTest"));

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("ShortCircuit");
        log.Steps.ShouldBeEmpty(); // Handler was never called
    }
}
