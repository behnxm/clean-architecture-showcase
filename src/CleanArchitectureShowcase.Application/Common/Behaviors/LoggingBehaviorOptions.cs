namespace CleanArchitectureShowcase.Application.Common.Behaviors;

/// <summary>
/// Configuration options for the pipeline logging behavior.
/// </summary>
public sealed class LoggingBehaviorOptions
{
    public const string SectionName = "LoggingBehavior";

    /// <summary>
    /// Gets or sets the threshold in milliseconds above which request execution
    /// logs a warning indicating slow processing.
    /// </summary>
    public int SlowRequestThresholdMilliseconds { get; set; } = 500;
}
