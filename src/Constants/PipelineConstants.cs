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
    public const long DefaultMaxBufferSize = 10000;
    public const long DefaultBufferFlushIntervalMs = 1000;
    public const int DefaultMaxConcurrentConsumers = 4;

    // Window configuration defaults
    public const long DefaultWindowSizeMs = 5000;
    public const long DefaultWindowSlideMs = 1000;
    public const string DefaultWindowType = "TUMBLING";

    // Performance defaults
    public const int DefaultMaxRetries = 3;
    public const long DefaultRetryDelayMs = 100;
    public const long DefaultProcessingTimeoutMs = 30000;
    public const double DefaultBackpressureTriggerThreshold = 80.0;

    // Quality defaults
    public const int DefaultMinDataQualityThreshold = 70;
    public const int MaxDataQualityScore = 100;
    public const int MinDataQualityScore = 0;

    // Backpressure thresholds
    public const double BackpressureHighWaterMark = 80.0;
    public const double BackpressureLowWaterMark = 60.0;
    public const double BackpressureCriticalMark = 95.0;

    // Metric collection
    public const long MetricsCollectionIntervalMs = 5000;
    public const int MaxMetricHistorySize = 1000;
    public const int MaxBackpressureEventHistory = 100;

    // Timeout values
    public const long ProcessingStageTimeoutMs = 30000;
    public const long WindowOperationTimeoutMs = 5000;
    public const long RepositoryOperationTimeoutMs = 10000;

    // Error codes
    public const string ErrorCodeInvalidDataPoint = "INVALID_DATA_POINT";
    public const string ErrorCodeBackpressureExceeded = "BACKPRESSURE_EXCEEDED";
    public const string ErrorCodeStageProcessingFailed = "STAGE_PROCESSING_FAILED";
    public const string ErrorCodeWindowingFailed = "WINDOWING_FAILED";
    public const string ErrorCodeProcessingTimeout = "PROCESSING_TIMEOUT";
    public const string ErrorCodeInvalidConfiguration = "INVALID_CONFIGURATION";
    public const string ErrorCodeResourceNotFound = "RESOURCE_NOT_FOUND";

    // Metadata keys
    public const string MetadataKeySourceId = "SourceId";
    public const string MetadataKeyProcessingTime = "ProcessingTimeMs";
    public const string MetadataKeyStageCount = "StageCount";
    public const string MetadataKeyRetryCount = "RetryCount";
    public const string MetadataKeyCorrelationId = "CorrelationId";
    public const string MetadataKeyWindowId = "WindowId";

    // Stage names (standard)
    public const string StageName_Ingestion = "Ingestion";
    public const string StageName_Validation = "Validation";
    public const string StageName_Transformation = "Transformation";
    public const string StageName_Windowing = "Windowing";
    public const string StageName_Aggregation = "Aggregation";
    public const string StageName_Output = "Output";

    // State values
    public const string StateActive = "ACTIVE";
    public const string StateInactive = "INACTIVE";
    public const string StateDegraded = "DEGRADED";

    // Sorting/grouping
    public const string SortOrderAscending = "ASC";
    public const string SortOrderDescending = "DESC";

    // Pagination defaults
    public const int DefaultPageSize = 100;
    public const int MaxPageSize = 10000;
    public const int MinPageSize = 1;

    // Validation constraints
    public const int MinPipelineNameLength = 1;
    public const int MaxPipelineNameLength = 255;
    public const int MinVersionLength = 1;
    public const int MaxVersionLength = 50;

    // Service defaults
    public const int DefaultServiceStartupTimeoutMs = 10000;
    public const int DefaultServiceShutdownTimeoutMs = 5000;

    // Log levels
    public const string LogLevelTrace = "TRACE";
    public const string LogLevelDebug = "DEBUG";
    public const string LogLevelInfo = "INFO";
    public const string LogLevelWarning = "WARNING";
    public const string LogLevelError = "ERROR";
    public const string LogLevelFatal = "FATAL";
}
