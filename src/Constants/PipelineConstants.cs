#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Constants;

/// <summary>
/// Global constants used throughout the pipeline system.
/// </summary>
public static class PipelineConstants
{
    // Buffer configuration defaults
    /// <summary>Default max buffer size.</summary>
    public const long DefaultMaxBufferSize = 10000;
    /// <summary>Default buffer flush interval milliseconds.</summary>
    public const long DefaultBufferFlushIntervalMs = 1000;
    /// <summary>Default max concurrent consumers.</summary>
    public const int DefaultMaxConcurrentConsumers = 4;

    // Window configuration defaults
    /// <summary>Default window size milliseconds.</summary>
    public const long DefaultWindowSizeMs = 5000;
    /// <summary>Default window slide milliseconds.</summary>
    public const long DefaultWindowSlideMs = 1000;
    /// <summary>Default window type.</summary>
    public const string DefaultWindowType = "TUMBLING";

    // Performance defaults
    /// <summary>Default max retries.</summary>
    public const int DefaultMaxRetries = 3;
    /// <summary>Default retry delay milliseconds.</summary>
    public const long DefaultRetryDelayMs = 100;
    /// <summary>Default processing timeout milliseconds.</summary>
    public const long DefaultProcessingTimeoutMs = 30000;
    /// <summary>Default backpressure trigger threshold.</summary>
    public const double DefaultBackpressureTriggerThreshold = 80.0;

    // Quality defaults
    /// <summary>Default min data quality threshold.</summary>
    public const int DefaultMinDataQualityThreshold = 70;
    /// <summary>Max data quality score.</summary>
    public const int MaxDataQualityScore = 100;
    /// <summary>Min data quality score.</summary>
    public const int MinDataQualityScore = 0;

    // Backpressure thresholds
    /// <summary>Backpressure high water mark.</summary>
    public const double BackpressureHighWaterMark = 80.0;
    /// <summary>Backpressure low water mark.</summary>
    public const double BackpressureLowWaterMark = 60.0;
    /// <summary>Backpressure critical mark.</summary>
    public const double BackpressureCriticalMark = 95.0;

    // Metric collection
    /// <summary>Metrics collection interval milliseconds.</summary>
    public const long MetricsCollectionIntervalMs = 5000;
    /// <summary>Max metric history size.</summary>
    public const int MaxMetricHistorySize = 1000;
    /// <summary>Max backpressure event history.</summary>
    public const int MaxBackpressureEventHistory = 100;

    // Timeout values
    /// <summary>Processing stage timeout milliseconds.</summary>
    public const long ProcessingStageTimeoutMs = 30000;
    /// <summary>Window operation timeout milliseconds.</summary>
    public const long WindowOperationTimeoutMs = 5000;
    /// <summary>Repository operation timeout milliseconds.</summary>
    public const long RepositoryOperationTimeoutMs = 10000;

    // Error codes
    /// <summary>Error code invalid data point.</summary>
    public const string ErrorCodeInvalidDataPoint = "INVALID_DATA_POINT";
    /// <summary>Error code backpressure exceeded.</summary>
    public const string ErrorCodeBackpressureExceeded = "BACKPRESSURE_EXCEEDED";
    /// <summary>Error code stage processing failed.</summary>
    public const string ErrorCodeStageProcessingFailed = "STAGE_PROCESSING_FAILED";
    /// <summary>Error code windowing failed.</summary>
    public const string ErrorCodeWindowingFailed = "WINDOWING_FAILED";
    /// <summary>Error code processing timeout.</summary>
    public const string ErrorCodeProcessingTimeout = "PROCESSING_TIMEOUT";
    /// <summary>Error code invalid configuration.</summary>
    public const string ErrorCodeInvalidConfiguration = "INVALID_CONFIGURATION";
    /// <summary>Error code resource not found.</summary>
    public const string ErrorCodeResourceNotFound = "RESOURCE_NOT_FOUND";

    // Metadata keys
    /// <summary>Metadata key source id.</summary>
    public const string MetadataKeySourceId = "SourceId";
    /// <summary>Metadata key processing time.</summary>
    public const string MetadataKeyProcessingTime = "ProcessingTimeMs";
    /// <summary>Metadata key stage count.</summary>
    public const string MetadataKeyStageCount = "StageCount";
    /// <summary>Metadata key retry count.</summary>
    public const string MetadataKeyRetryCount = "RetryCount";
    /// <summary>Metadata key correlation id.</summary>
    public const string MetadataKeyCorrelationId = "CorrelationId";
    /// <summary>Metadata key window id.</summary>
    public const string MetadataKeyWindowId = "WindowId";

    // Stage names (standard)
    /// <summary>Stage name ingestion.</summary>
    public const string StageName_Ingestion = "Ingestion";
    /// <summary>Stage name validation.</summary>
    public const string StageName_Validation = "Validation";
    /// <summary>Stage name transformation.</summary>
    public const string StageName_Transformation = "Transformation";
    /// <summary>Stage name windowing.</summary>
    public const string StageName_Windowing = "Windowing";
    /// <summary>Stage name aggregation.</summary>
    public const string StageName_Aggregation = "Aggregation";
    /// <summary>Stage name output.</summary>
    public const string StageName_Output = "Output";

    // State values
    /// <summary>State active.</summary>
    public const string StateActive = "ACTIVE";
    /// <summary>State inactive.</summary>
    public const string StateInactive = "INACTIVE";
    /// <summary>State degraded.</summary>
    public const string StateDegraded = "DEGRADED";

    // Sorting/grouping
    /// <summary>Sort order ascending.</summary>
    public const string SortOrderAscending = "ASC";
    /// <summary>Sort order descending.</summary>
    public const string SortOrderDescending = "DESC";

    // Pagination defaults
    /// <summary>Default page size.</summary>
    public const int DefaultPageSize = 100;
    /// <summary>Max page size.</summary>
    public const int MaxPageSize = 10000;
    /// <summary>Min page size.</summary>
    public const int MinPageSize = 1;

    // Validation constraints
    /// <summary>Min pipeline name length.</summary>
    public const int MinPipelineNameLength = 1;
    /// <summary>Max pipeline name length.</summary>
    public const int MaxPipelineNameLength = 255;
    /// <summary>Min version length.</summary>
    public const int MinVersionLength = 1;
    /// <summary>Max version length.</summary>
    public const int MaxVersionLength = 50;

    // Service defaults
    /// <summary>Default service startup timeout milliseconds.</summary>
    public const int DefaultServiceStartupTimeoutMs = 10000;
    /// <summary>Default service shutdown timeout milliseconds.</summary>
    public const int DefaultServiceShutdownTimeoutMs = 5000;

    // Log levels
    /// <summary>Log level trace.</summary>
    public const string LogLevelTrace = "TRACE";
    /// <summary>Log level debug.</summary>
    public const string LogLevelDebug = "DEBUG";
    /// <summary>Log level info.</summary>
    public const string LogLevelInfo = "INFO";
    /// <summary>Log level warning.</summary>
    public const string LogLevelWarning = "WARNING";
    /// <summary>Log level error.</summary>
    public const string LogLevelError = "ERROR";
    /// <summary>Log level fatal.</summary>
    public const string LogLevelFatal = "FATAL";
}