# Retry Policy

The retry policy system provides a robust mechanism for handling transient failures in the pipeline before routing items to the dead-letter queue. It supports configurable backoff strategies, jitter, and custom exception classification.

## Configuration

Retry behavior is configured via `RetryPolicyOptions`. The following properties control how retries are executed:

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| `MaxAttempts` | `int` | Maximum number of attempts (including the initial one). Must be ≥ 1. | `3` |
| `BaseDelay` | `TimeSpan` | Initial delay before the second attempt. Doubles with each subsequent attempt. Must be ≥ `TimeSpan.Zero`. | `200ms` |
| `MaxDelay` | `TimeSpan` | Upper bound applied to the computed backoff delay. Must be ≥ `TimeSpan.Zero`. | `30s` |
| `JitterFactor` | `double` | Proportion of random jitter applied to each delay (`0` = none, `0.25` = ±25%). Must be between `0.0` and `1.0`. | `0.25` |
| `TransientExceptionPredicate` | `Func<Exception, bool>` | Predicate that determines whether an exception is transient (retryable) or permanent. | `DefaultTransientPredicate` |

### Default Transient Predicate

`DefaultTransientPredicate` classifies exceptions as follows:
- **Transient**: `TimeoutException`, `IOException`, `SocketException`, `HttpRequestException`.
- **Permanent**: `OperationCanceledException`, `PipelineProcessingException` (unless its `Result.Exception` is transient), and all other exceptions.

## Backoff Formula

The `ExponentialBackoffRetryPolicy` calculates the delay before each retry using the following logic:

1. **Exponential Growth**: `baseDelay * 2^(attempt - 1)`
2. **Capping**: The result is capped at `maxDelay`.
3. **Jitter**: A random value within `±(jitterFactor * cappedDelay)` is added to smooth out thundering herd problems. The final delay is clamped to a minimum of `0`.

Mathematically:
