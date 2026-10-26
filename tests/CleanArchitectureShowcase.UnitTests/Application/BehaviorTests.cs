using CleanArchitectureShowcase.Application.Common.Behaviors;
using CleanArchitectureShowcase.Application.Common.Messaging;
using CleanArchitectureShowcase.Domain.Common;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace CleanArchitectureShowcase.UnitTests.Application;

public sealed class BehaviorTests
{
    private sealed record TestRequest(string Name) : ICommand;

    private sealed class TestRequestValidator : AbstractValidator<TestRequest>
    {
        public TestRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        }
    }

    [Fact]
    public async Task ValidationBehavior_WhenValidationFails_ShortCircuitsAndNeverCallsNext()
    {
        // Arrange
        var validators = new IValidator<TestRequest>[] { new TestRequestValidator() };
        var behavior = new ValidationBehavior<TestRequest, Result>(validators);
        var invalidRequest = new TestRequest("");
        bool nextCalled = false;

        // Act
        Result result = await behavior.Handle(
            invalidRequest,
            () =>
            {
                nextCalled = true;
                return Task.FromResult(Result.Success());
            },
            CancellationToken.None);

        // Assert
        nextCalled.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>();
        var validationError = (ValidationError)result.Error;
        validationError.Errors.ShouldContain(e => e.Code == nameof(TestRequest.Name));
    }

    [Fact]
    public async Task ValidationBehavior_WhenValid_CallsNextAndReturnsSuccess()
    {
        // Arrange
        var validators = new IValidator<TestRequest>[] { new TestRequestValidator() };
        var behavior = new ValidationBehavior<TestRequest, Result>(validators);
        var validRequest = new TestRequest("Valid Name");
        bool nextCalled = false;

        // Act
        Result result = await behavior.Handle(
            validRequest,
            () =>
            {
                nextCalled = true;
                return Task.FromResult(Result.Success());
            },
            CancellationToken.None);

        // Assert
        nextCalled.ShouldBeTrue();
        result.IsSuccess.ShouldBeTrue();
    }

    private sealed class TestLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, string Message)> Logs { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Logs.Add((logLevel, formatter(state, exception)));
        }
    }

    [Fact]
    public async Task LoggingBehavior_WhenRequestExceedsSlowThreshold_LogsWarning()
    {
        // Arrange
        var logger = new TestLogger<LoggingBehavior<TestRequest, Result>>();
        var options = Options.Create(new LoggingBehaviorOptions
        {
            SlowRequestThresholdMilliseconds = 10 // artificially low for testing
        });

        var behavior = new LoggingBehavior<TestRequest, Result>(logger, options);

        // Act
        Result result = await behavior.Handle(
            new TestRequest("SlowTask"),
            async () =>
            {
                await Task.Delay(25); // Exceeds 10ms threshold
                return Result.Success();
            },
            CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        logger.Logs.ShouldContain(log => log.Level == LogLevel.Warning && log.Message.Contains("Slow request alert"));
    }
}
