# PipelineConstants Reference

This document provides a reference table for the constants defined in `src/Constants/PipelineConstants.cs`, grouped by their functional categories.

## Buffer Configuration Defaults
| Name | Value | Meaning |
|------|-------|---------|
| `DefaultMaxBufferSize` | `10000` | Maximum number of items allowed in the processing buffer before flushing. |
| `DefaultBufferFlushIntervalMs` | `1000` | Interval in milliseconds between automatic buffer flushes. |
| `DefaultMaxConcurrentConsumers` | `4` | Maximum number of concurrent consumers allowed to process buffer items. |

## Window Configuration Defaults
| Name | Value | Meaning |
|------|-------|---------|
| `DefaultWindowSizeMs` | `5000` | Duration in milliseconds for each data window. |
| `DefaultWindowSlideMs` | `1000` | Interval in milliseconds by which the window slides forward. |
| `DefaultWindowType` | `"TUMBLING"` | Default type of windowing strategy used for data aggregation. |

## Performance Defaults (Retry/Timeout)
| Name | Value | Meaning |
|------|-------|---------|
| `DefaultMaxRetries` | `3` | Maximum number of retry attempts for transient failures. |
| `DefaultRetryDelayMs` | `100` | Initial delay in milliseconds between retry attempts. |
| `DefaultProcessingTimeoutMs` | `30000` | Maximum allowed time in milliseconds for a single processing operation. |

## Quality Defaults
| Name | Value | Meaning |
|------|-------|---------|
| `DefaultMinDataQualityThreshold` | `70` | Minimum acceptable data quality score for processing. |
| `MaxDataQualityScore` | `100` | Highest possible data quality score. |
| `MinDataQualityScore` | `0` | Lowest possible data quality score. |

## Backpressure Watermarks
| Name | Value | Meaning |
|------|-------|---------|
| `BackpressureHighWaterMark` | `80.0` | Threshold percentage at which backpressure mitigation begins. |
| `BackpressureLowWaterMark` | `60.0` | Threshold percentage at which backpressure is considered resolved. |
| `BackpressureCriticalMark` | `95.0` | Critical threshold percentage indicating severe system overload. |

## Metric Collection
| Name | Value | Meaning |
|------|-------|---------|
| `MetricsCollectionIntervalMs` | `5000` | Interval in milliseconds between metric collection cycles. |
| `MaxMetricHistorySize` | `1000` | Maximum number of historical metric data points to retain. |
| `MaxBackpressureEventHistory` | `100` | Maximum number of backpressure events to log in history. |

## Timeout Values
| Name | Value | Meaning |
|------|-------|---------|
| `ProcessingStageTimeoutMs` | `30000` | Maximum time in milliseconds allowed for a single pipeline stage. |
| `WindowOperationTimeoutMs` | `5000` | Maximum time in milliseconds allowed for windowing operations. |
| `RepositoryOperationTimeoutMs` | `10000` | Maximum time in milliseconds allowed for repository/database operations. |

## Error Codes
| Name | Value | Meaning |
|------|-------|---------|
| `ErrorCodeInvalidDataPoint` | `"INVALID_DATA_POINT"` | Code returned when input data fails validation. |
| `ErrorCodeBackpressureExceeded` | `"BACKPRESSURE_EXCEEDED"` | Code returned when system load exceeds backpressure limits. |
| `ErrorCodeStageProcessingFailed` | `"STAGE_PROCESSING_FAILED"` | Code returned when a pipeline stage fails to process data. |
| `ErrorCodeWindowingFailed` | `"WINDOWING_FAILED"` | Code returned when windowing operations fail. |
| `ErrorCodeProcessingTimeout` | `"PROCESSING_TIMEOUT"` | Code returned when processing exceeds the allowed timeout. |
| `ErrorCodeInvalidConfiguration` | `"INVALID_CONFIGURATION"` | Code returned when pipeline configuration is invalid. |
| `ErrorCodeResourceNotFound` | `"RESOURCE_NOT_FOUND"` | Code returned when a required resource is missing. |

## Metadata Keys
| Name | Value | Meaning |
|------|-------|---------|
| `MetadataKeySourceId` | `"SourceId"` | Key for identifying the data source in metadata. |
| `MetadataKeyProcessingTime` | `"ProcessingTimeMs"` | Key for recording processing duration in milliseconds. |
| `MetadataKeyStageCount` | `"StageCount"` | Key for tracking the number of pipeline stages processed. |
| `MetadataKeyRetryCount` | `"RetryCount"` | Key for tracking the number of retry attempts. |
| `MetadataKeyCorrelationId` | `"CorrelationId"` | Key for tracing related events across the pipeline. |
| `MetadataKeyWindowId` | `"WindowId"` | Key for identifying the specific data window. |

## Stage Names
| Name | Value | Meaning |
|------|-------|---------|
| `StageName_Ingestion` | `"Ingestion"` | Name of the initial data ingestion stage. |
| `StageName_Validation` | `"Validation"` | Name of the data validation stage. |
| `StageName_Transformation` | `"Transformation"` | Name of the data transformation stage. |
| `StageName_Windowing` | `"Windowing"` | Name of the data windowing stage. |
| `StageName_Aggregation` | `"Aggregation"` | Name of the data aggregation stage. |
| `StageName_Output` | `"Output"` | Name of the final data output stage. |

## State Values
| Name | Value | Meaning |
|------|-------|---------|
| `StateActive` | `"ACTIVE"` | Indicates the pipeline or component is running normally. |
| `StateInactive` | `"INACTIVE"` | Indicates the pipeline or component is stopped. |
| `StateDegraded` | `"DEGRADED"` | Indicates the pipeline or component is operating with reduced functionality. |

## Sort Orders
| Name | Value | Meaning |
|------|-------|---------|
| `SortOrderAscending` | `"ASC"` | Specifies ascending sort order. |
| `SortOrderDescending` | `"DESC"` | Specifies descending sort order. |

## Pagination Defaults
| Name | Value | Meaning |
|------|-------|---------|
| `DefaultPageSize` | `100` | Default number of items returned per page. |
| `MaxPageSize` | `10000` | Maximum allowed number of items per page. |
| `MinPageSize` | `1` | Minimum allowed number of items per page. |

## Validation Constraints
| Name | Value | Meaning |
|------|-------|---------|
| `MinPipelineNameLength` | `1` | Minimum required length for a pipeline name. |
| `MaxPipelineNameLength` | `255` | Maximum allowed length for a pipeline name. |
| `MinVersionLength` | `1` | Minimum required length for a version string. |
| `MaxVersionLength` | `50` | Maximum allowed length for a version string. |

## Service Timeouts
| Name | Value | Meaning |
|------|-------|---------|
| `DefaultServiceStartupTimeoutMs` | `10000` | Maximum time in milliseconds allowed for service startup. |
| `DefaultServiceShutdownTimeoutMs` | `5000` | Maximum time in milliseconds allowed for service shutdown. |

## Log Levels
| Name | Value | Meaning |
|------|-------|---------|
| `LogLevelTrace` | `"TRACE"` | Most detailed logging level for debugging. |
| `LogLevelDebug` | `"DEBUG"` | Detailed logging for development and debugging. |
| `LogLevelInfo` | `"INFO"` | General informational messages. |
| `LogLevelWarning` | `"WARNING"` | Potentially harmful situations. |
| `LogLevelError` | `"ERROR"` | Error events that might allow the application to continue. |
| `LogLevelFatal` | `"FATAL"` | Critical errors causing application termination. |
