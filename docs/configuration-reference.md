# Configuration Reference

This document provides a comprehensive reference of all configurable values across the pipeline components.

## PipelineConfig

| Name                          | Type                     | Default Value | Effect                                                                                     |
|-------------------------------|--------------------------|----------------|--------------------------------------------------------------------------------------------|
| ConfigId                      | long                     | -              | Unique identifier for the configuration.                                                    |
| PipelineName                  | string                   | ""             | Name of the pipeline.                                                                       |
| Version                       | string                   | ""             | Version of the pipeline configuration.                                                      |
| MaxBufferSize                 | long                     | 10000          | Maximum buffer size before backpressure is applied.                                         |
| BufferFlushIntervalMs         | long                     | 1000           | Buffer flush interval in milliseconds.                                                      |
| MaxConcurrentConsumers        | int                      | 4              | Maximum number of concurrent consumers for parallel processing.                            |
| WindowSizeMs                  | long                     | 5000           | Window size in milliseconds.                                                               |
| WindowSlideMs                 | long                     | 1000           | Window slide interval in milliseconds.                                                      |
| WindowType                    | string                   | "TUMBLING"     | Type of windowing algorithm (TUMBLING, SLIDING, SESSION).                                   |
| MaxRetries                    | int                      | 3              | Maximum number of retry attempts for failed operations.                                     |
| RetryDelayMs                  | long                     | 100            | Delay between retry attempts in milliseconds.                                               |
| ProcessingTimeoutMs          | long                     | 30000          | Processing timeout in milliseconds.                                                         |
| BackpressureTriggerThreshold  | double                   | 80.0           | Threshold percentage (0-100) at which backpressure is triggered.                             |
| MinDataQualityThreshold       | int                      | 70             | Minimum data quality threshold (0-100) for processing.                                       |
| ValidateOnIngestion           | bool                     | true           | Whether to validate data on ingestion.                                                      |
| EnableMetricsCollection       | bool                     | true           | Whether metrics collection is enabled.                                                      |
| Stages                        | List<PipelineStageDef>    | new()          | List of pipeline stage definitions.                                                         |
| CustomSettings                | Dictionary<string, object> | new()        | Custom configuration settings as key-value pairs.                                           |
| CreatedAt                     | DateTime                 | -              | Creation timestamp.                                                                         |
| LastModifiedAt                | DateTime                 | -              | Last modification timestamp.                                                               |
| IsActive                      | bool                     | true           | Whether this configuration is active.                                                       |

## WorkerOptions

| Name                          | Type                     | Default Value | Effect                                                                                     |
|-------------------------------|--------------------------|----------------|--------------------------------------------------------------------------------------------|
| MetricsAggregationIntervalMs  | int                      | 5000           | Interval, in milliseconds, between metrics aggregation passes.                              |
| HealthCheckIntervalMs         | int                      | 10000          | Interval, in milliseconds, between health check passes.                                    |
| EnableProcessingWorker        | bool                     | true           | Whether the processing worker is enabled.                                                  |
| EnableMetricsWorker           | bool                     | true           | Whether the metrics worker is enabled.                                                     |
| EnableHealthCheckWorker       | bool                     | true           | Whether the health check worker is enabled.                                               |

## SubscriberOptions

| Name                          | Type                     | Default Value | Effect                                                                                     |
|-------------------------------|--------------------------|----------------|--------------------------------------------------------------------------------------------|
| MaxQueueSize                  | int                      | 1000           | Maximum number of events that can be queued per subscriber.                                |
| MaxQueueSizeBehavior          | MaxQueueSizeBehavior     | DropNew        | Behavior when the queue is full.                                                           |
| DispatchMode                 | SubscriberDispatchMode  | Sequential    | Dispatch mode for event handling.                                                          |
| ErrorPolicy                   | SubscriberErrorPolicy    | SwallowAndCount| Error handling policy when an event handler throws.                                        |
| MaxDegreeOfParallelism       | int                      | 1              | Maximum degree of parallelism when DispatchMode is Parallel.                               |
| Name                          | string                   | null           | Custom name for this subscriber configuration.                                               |

## RetryPolicyOptions

| Name                          | Type                     | Default Value | Effect                                                                                     |
|-------------------------------|--------------------------|----------------|--------------------------------------------------------------------------------------------|
| MaxAttempts                   | int                      | 3              | Maximum number of attempts (including the first one).                                      |
| BaseDelay                     | TimeSpan                 | 200 ms         | Delay before the second attempt; each subsequent attempt doubles it.                      |
| MaxDelay                      | TimeSpan                 | 30 s           | Upper bound applied to the computed backoff delay.                                         |
| JitterFactor                  | double                   | 0.25           | Proportion of random jitter applied to each delay (0 = none, 0.25 = up to ±25%).           |
| TransientExceptionPredicate   | Func<Exception, bool>     | DefaultTransientPredicate | Predicate that decides whether an exception is transient and therefore worth retrying. |

## HttpClientConfiguration

| Name                          | Type                     | Default Value | Effect                                                                                     |
|-------------------------------|--------------------------|----------------|--------------------------------------------------------------------------------------------|
| Timeout                       | TimeSpan                 | 30 s           | Default request timeout.                                                                    |
| MaxRetries                    | int                      | 3              | Maximum number of retry attempts for failed requests.                                     |
| RetryDelay                    | TimeSpan                 | 1 s            | Delay between retry attempts.                                                              |
| MaxConnectionsPerHost         | int                      | 10             | Maximum number of connections per host.                                                   |
| UseCompression                | bool                     | true           | Whether compression is enabled for HTTP clients.                                           |
| UserAgent                     | string                   | "DotNetRealtimePipeline/1.0" | Default user agent string for HTTP clients. |
| DefaultHeaders                | Dictionary<string, string> | new()        | Default headers that will be added to all HTTP clients.                                   |
