#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Domain.Enums;

/// <summary>
/// Represents the different types of windows for time-series aggregation.
/// </summary>
public enum WindowType
{
    /// <summary>Fixed-size windows that do not overlap.</summary>
    Tumbling = 0,

    /// <summary>Windows that slide forward by a fixed interval and can overlap.</summary>
    Sliding = 1,

    /// <summary>Windows that group data by activity sessions with gaps.</summary>
    Session = 2,

    /// <summary>Single window covering the entire dataset.</summary>
    Global = 3
}

/// <summary>
/// Represents the aggregation function to apply within a window.
/// </summary>
public enum AggregationType
{
    /// <summary>Calculates the sum of all values in the window.</summary>
    Sum = 0,

    /// <summary>Calculates the average (mean) of all values in the window.</summary>
    Average = 1,

    /// <summary>Finds the minimum value in the window.</summary>
    Min = 2,

    /// <summary>Finds the maximum value in the window.</summary>
    Max = 3,

    /// <summary>Counts the number of items in the window.</summary>
    Count = 4,

    /// <summary>Calculates the standard deviation of values in the window.</summary>
    StdDev = 5,

    /// <summary>Calculates a specific percentile of values in the window.</summary>
    Percentile = 6,

    /// <summary>Allows custom aggregation logic to be applied.</summary>
    Custom = 7
}

/// <summary>
/// Represents the status of a processing operation.
/// </summary>
public enum ProcessingStatus
{
    /// <summary>The operation is waiting to be processed.</summary>
    Pending = 0,

    /// <summary>The operation is currently being processed.</summary>
    InProgress = 1,

    /// <summary>The operation completed successfully.</summary>
    Completed = 2,

    /// <summary>The operation failed and will not be retried.</summary>
    Failed = 3,

    /// <summary>The operation failed and is being retried.</summary>
    Retrying = 4,

    /// <summary>The operation was skipped due to business rules.</summary>
    Skipped = 5,

    /// <summary>The operation timed out.</summary>
    Timeout = 6
}

/// <summary>
/// Represents the health status of a pipeline component.
/// </summary>
public enum HealthStatus
{
    /// <summary>The health status is unknown or not yet determined.</summary>
    Unknown = 0,

    /// <summary>The component is operating normally.</summary>
    Healthy = 1,

    /// <summary>The component is operating but with reduced capacity or performance.</summary>
    Degraded = 2,

    /// <summary>The component is not functioning properly.</summary>
    Unhealthy = 3,

    /// <summary>The component requires immediate attention and may be causing system-wide issues.</summary>
    Critical = 4
}

/// <summary>
/// Represents different data quality levels.
/// </summary>
public enum DataQuality
{
    Poor = 0,
    Fair = 25,
    Good = 50,
    Excellent = 75,
    Perfect = 100
}

/// <summary>
/// Represents the severity level of an event or error.
/// </summary>
public enum SeverityLevel
{
    Trace = 0,
    Debug = 1,
    Information = 2,
    Warning = 3,
    Error = 4,
    Critical = 5,
    Fatal = 6
}

/// <summary>
/// Represents the type of backpressure response.
/// </summary>
public enum BackpressureStrategy
{
    Block = 0,
    DropNewest = 1,
    DropOldest = 2,
    Queue = 3,
    Throttle = 4
}

/// <summary>
/// Represents the execution order of pipeline stages.
/// </summary>
public enum StageExecutionMode
{
    Sequential = 0,
    Parallel = 1,
    Conditional = 2
}

/// <summary>
/// Represents the type of data source.
/// </summary>
public enum DataSourceType
{
    Kafka = 0,
    EventHub = 1,
    Kinesis = 2,
    Http = 3,
    File = 4,
    Database = 5,
    Custom = 6
}

/// <summary>
/// Represents retry policy behaviors.
/// </summary>
public enum RetryPolicy
{
    NoRetry = 0,
    Immediate = 1,
    Linear = 2,
    Exponential = 3,
    Custom = 4
}
