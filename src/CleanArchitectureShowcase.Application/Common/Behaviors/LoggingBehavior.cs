using System.Diagnostics;
using CleanArchitectureShowcase.Application.Common.Messaging;
using CleanArchitectureShowcase.Domain.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CleanArchitectureShowcase.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior measuring request execution duration, structured diagnostic logging,
/// and slow-request threshold alerting.
/// </summary>
/// <typeparam name="TRequest">The incoming command or query type.</typeparam>
/// <typeparam name="TResponse">The result type returned by the pipeline.</typeparam>
public sealed partial class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger,
    IOptions<LoggingBehaviorOptions> options)
    : IPipelineBehavior<TRequest, TResponse>
    where TResponse : Result
{
    private readonly LoggingBehaviorOptions _options = options.Value;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;
        long startingTimestamp = Stopwatch.GetTimestamp();

        LogRequestExecuting(logger, requestName);

        TResponse response = await next();

        TimeSpan elapsed = Stopwatch.GetElapsedTime(startingTimestamp);
        double elapsedMilliseconds = elapsed.TotalMilliseconds;

        if (elapsedMilliseconds > _options.SlowRequestThresholdMilliseconds)
        {
            LogSlowRequest(logger, requestName, elapsedMilliseconds, _options.SlowRequestThresholdMilliseconds);
        }
        else
        {
            LogRequestExecuted(logger, requestName, elapsedMilliseconds);
        }

        return response;
    }

    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Information,
        Message = "Executing request {RequestName}")]
    private static partial void LogRequestExecuting(ILogger logger, string requestName);

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Information,
        Message = "Executed request {RequestName} in {ElapsedMilliseconds:F2}ms")]
    private static partial void LogRequestExecuted(ILogger logger, string requestName, double elapsedMilliseconds);

    [LoggerMessage(
        EventId = 2003,
        Level = LogLevel.Warning,
        Message = "Slow request alert: {RequestName} executed in {ElapsedMilliseconds:F2}ms, exceeding configured threshold of {ThresholdMilliseconds}ms")]
    private static partial void LogSlowRequest(ILogger logger, string requestName, double elapsedMilliseconds, int thresholdMilliseconds);
}
