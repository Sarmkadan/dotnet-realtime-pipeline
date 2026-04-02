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
    Tumbling = 0,
    Sliding = 1,
    Session = 2,
    Global = 3
}

/// <summary>
/// Represents the aggregation function to apply within a window.
/// </summary>
public enum AggregationType
{
    Sum = 0,
    Average = 1,
    Min = 2,
    Max = 3,
    Count = 4,
    StdDev = 5,
    Percentile = 6,
    Custom = 7
}

/// <summary>
/// Represents the status of a processing operation.
/// </summary>
public enum ProcessingStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Failed = 3,
    Retrying = 4,
    Skipped = 5,
    Timeout = 6
}

/// <summary>
/// Represents the health status of a pipeline component.
/// </summary>
public enum HealthStatus
{
    Unknown = 0,
    Healthy = 1,
    Degraded = 2,
    Unhealthy = 3,
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
