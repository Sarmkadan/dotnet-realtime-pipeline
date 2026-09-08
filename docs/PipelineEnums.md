# Pipeline Enums

This document lists the enums defined in `src/Domain/Enums/PipelineEnums.cs`. Meanings are reproduced from the existing XML `<summary>` comments. Where a member has no XML summary, that absence is noted rather than inferred.

## `WindowType`

Represents the different types of windows for time-series aggregation.

| Member | Meaning |
| --- | --- |
| `Tumbling` | Fixed-size windows that do not overlap. |
| `Sliding` | Windows that slide forward by a fixed interval and can overlap. |
| `Session` | Windows that group data by activity sessions with gaps. |
| `Global` | Single window covering the entire dataset. |

## `AggregationType`

Represents the aggregation function to apply within a window.

| Member | Meaning |
| --- | --- |
| `Sum` | Calculates the sum of all values in the window. |
| `Average` | Calculates the average (mean) of all values in the window. |
| `Min` | Finds the minimum value in the window. |
| `Max` | Finds the maximum value in the window. |
| `Count` | Counts the number of items in the window. |
| `StdDev` | Calculates the standard deviation of values in the window. |
| `Percentile` | Calculates a specific percentile of values in the window. |
| `Custom` | Allows custom aggregation logic to be applied. |

## `ProcessingStatus`

Represents the status of a processing operation.

| Member | Meaning |
| --- | --- |
| `Pending` | The operation is waiting to be processed. |
| `InProgress` | The operation is currently being processed. |
| `Completed` | The operation completed successfully. |
| `Failed` | The operation failed and will not be retried. |
| `Retrying` | The operation failed and is being retried. |
| `Skipped` | The operation was skipped due to business rules. |
| `Timeout` | The operation timed out. |

## `HealthStatus`

Represents the health status of a pipeline component.

| Member | Meaning |
| --- | --- |
| `Unknown` | The health status is unknown or not yet determined. |
| `Healthy` | The component is operating normally. |
| `Degraded` | The component is operating but with reduced capacity or performance. |
| `Unhealthy` | The component is not functioning properly. |
| `Critical` | The component requires immediate attention and may be causing system-wide issues. |

## `DataQuality`

Represents different data quality levels.

| Member | Meaning |
| --- | --- |
| `Poor` | No member-level XML summary is provided. |
| `Fair` | No member-level XML summary is provided. |
| `Good` | No member-level XML summary is provided. |
| `Excellent` | No member-level XML summary is provided. |
| `Perfect` | No member-level XML summary is provided. |

## `SeverityLevel`

Represents the severity level of an event or error.

| Member | Meaning |
| --- | --- |
| `Trace` | No member-level XML summary is provided. |
| `Debug` | No member-level XML summary is provided. |
| `Information` | No member-level XML summary is provided. |
| `Warning` | No member-level XML summary is provided. |
| `Error` | No member-level XML summary is provided. |
| `Critical` | No member-level XML summary is provided. |
| `Fatal` | No member-level XML summary is provided. |

## `BackpressureStrategy`

Represents the type of backpressure response.

| Member | Meaning |
| --- | --- |
| `Block` | No member-level XML summary is provided. |
| `DropNewest` | No member-level XML summary is provided. |
| `DropOldest` | No member-level XML summary is provided. |
| `Queue` | No member-level XML summary is provided. |
| `Throttle` | No member-level XML summary is provided. |

## `StageExecutionMode`

Represents the execution order of pipeline stages.

| Member | Meaning |
| --- | --- |
| `Sequential` | No member-level XML summary is provided. |
| `Parallel` | No member-level XML summary is provided. |
| `Conditional` | No member-level XML summary is provided. |

## `DataSourceType`

Represents the type of data source.

| Member | Meaning |
| --- | --- |
| `Kafka` | No member-level XML summary is provided. |
| `EventHub` | No member-level XML summary is provided. |
| `Kinesis` | No member-level XML summary is provided. |
| `Http` | No member-level XML summary is provided. |
| `File` | No member-level XML summary is provided. |
| `Database` | No member-level XML summary is provided. |
| `Custom` | No member-level XML summary is provided. |

## `RetryPolicy`

Represents retry policy behaviors.

| Member | Meaning |
| --- | --- |
| `NoRetry` | No member-level XML summary is provided. |
| `Immediate` | No member-level XML summary is provided. |
| `Linear` | No member-level XML summary is provided. |
| `Exponential` | No member-level XML summary is provided. |
| `Custom` | No member-level XML summary is provided. |
