# RetryPolicyOptions

`RetryPolicyOptions` is the configuration object for `ExponentialBackoffRetryPolicy`. It controls how many times a failing operation is attempted, how long the policy waits between attempts, how much random jitter is applied to those waits, and which exceptions are considered transient (worth retrying) before an item is handed to the dead-letter queue.

The class is `sealed` and every property is validated on assignment, so an invalid configuration fails at the point it is set rather than later during a retry.

```csharp
var options = new RetryPolicyOptions
{
    MaxAttempts = 5,
    BaseDelay = TimeSpan.FromMilliseconds(100),
    MaxDelay = TimeSpan.FromSeconds(10),
    JitterFactor = 0.2
};

var policy = new ExponentialBackoffRetryPolicy(options);
```

Passing `null` (or no argument) to `ExponentialBackoffRetryPolicy` uses `new RetryPolicyOptions()` with the defaults listed below.

## Properties

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| `MaxAttempts` | `int` | Maximum number of attempts, including the initial one. Must be ≥ 1. | `3` |
| `BaseDelay` | `TimeSpan` | Delay before the second attempt. Each subsequent attempt doubles it. Must be ≥ `TimeSpan.Zero`. | `200ms` |
| `MaxDelay` | `TimeSpan` | Upper bound applied to the computed backoff delay before jitter. Must be ≥ `TimeSpan.Zero`. | `30s` |
| `JitterFactor` | `double` | Proportion of random jitter applied to each delay (`0` = none, `0.25` = up to ±25%). Must be between `0.0` and `1.0` inclusive. | `0.25` |
| `TransientExceptionPredicate` | `Func<Exception, bool>` | Predicate that decides whether an exception is transient (retryable) or permanent. Must not be `null`. | `DefaultTransientPredicate` |

### `MaxAttempts`

Counts the first execution, so `MaxAttempts = 1` means "no retries". Setting a value below 1 throws `ArgumentOutOfRangeException`.

### `BaseDelay`

The starting point of the exponential schedule. With the default `200ms`, the nominal delays before attempts 2, 3, 4… are `200ms`, `400ms`, `800ms`, and so on, until `MaxDelay` caps them. A zero value is allowed and produces immediate retries (jitter has no effect on a zero delay). Negative values throw `ArgumentOutOfRangeException`.

### `MaxDelay`

Caps the nominal exponential delay. Jitter is applied after the cap, so the actual wait can be up to `MaxDelay * (1 + JitterFactor)`. Negative values throw `ArgumentOutOfRangeException`.

### `JitterFactor`

Randomises each delay so that many failing items do not retry in lockstep. A value of `0` makes the schedule deterministic. Values outside `[0, 1]` (including `NaN`, which fails the range check) throw `ArgumentOutOfRangeException`.

### `TransientExceptionPredicate`

Replaces the default classification with your own rules. Assigning `null` throws `ArgumentNullException`. The predicate is evaluated for every exception thrown by an attempt; returning `true` schedules another attempt (if attempts remain), returning `false` stops retrying at once and the policy reports the failure, so the item is routed to the dead-letter queue.

## Default Transient Predicate

`RetryPolicyOptions.DefaultTransientPredicate(Exception)` is a public static method and can be called directly or reused inside a custom predicate. A `null` argument throws `ArgumentNullException`.

It classifies exceptions as follows:

- **Transient**:
  - `TimeoutException`
  - `IOException`
  - `SocketException`
  - `HttpRequestException`
  - `AggregateException` whose `InnerException` is transient (the inner exception is classified recursively)
  - `PipelineProcessingException` whose `Result.Exception` is transient (the cause is classified recursively)
- **Permanent**:
  - `OperationCanceledException` (cancellation is never retried)
  - `PipelineProcessingException` without a transient cause, including business-rule rejections such as failed validation, which carry no inner exception
  - Every other exception type

Wrapping a transient exception in a custom type does not make it transient by default. Supply your own predicate in that case:

```csharp
options.TransientExceptionPredicate = ex =>
    RetryPolicyOptions.DefaultTransientPredicate(ex) ||
    ex is MyDownstreamUnavailableException;
```

## Backoff Schedule

Delays are computed by `ExponentialBackoffRetryPolicy.GetBackoffDelay(attemptNumber)`:

1. **Exponential growth**: `BaseDelay * 2^(attemptNumber - 1)`. The exponent is capped at 20 to avoid overflow.
2. **Capping**: the result is limited to `MaxDelay`.
3. **Jitter**: a random value in `±(JitterFactor * cappedDelay)` is added, and the final delay is clamped to a minimum of `0`.

For example, with `BaseDelay = 200ms`, `MaxDelay = 1s` and `JitterFactor = 0`, the waits are `200ms`, `400ms`, `800ms`, `1s`, `1s`, and so on. With the default `JitterFactor = 0.25`, the first wait falls between `150ms` and `250ms`.

`GetBackoffDelay` throws `ArgumentOutOfRangeException` when `attemptNumber` is less than 1.

## Validation Summary

| Property | Rule | Violation |
|----------|------|-----------|
| `MaxAttempts` | ≥ 1 | `ArgumentOutOfRangeException` |
| `BaseDelay` | ≥ `TimeSpan.Zero` | `ArgumentOutOfRangeException` |
| `MaxDelay` | ≥ `TimeSpan.Zero` | `ArgumentOutOfRangeException` |
| `JitterFactor` | `0.0` ≤ value ≤ `1.0` | `ArgumentOutOfRangeException` |
| `TransientExceptionPredicate` | not `null` | `ArgumentNullException` |

Options are not re-validated as a whole, so a `MaxDelay` smaller than `BaseDelay` is accepted; in that case every delay is capped at `MaxDelay` from the first retry onward.

## Copying Options

`RetryPolicyOptionsExtensions.Clone()` returns an independent copy with the same property values. `WithMaxAttempts`, `WithBaseDelay`, and related helpers clone the options and change a single property, which is useful when several policies share a base configuration.

```csharp
var baseOptions = new RetryPolicyOptions { JitterFactor = 0.1 };
var aggressive = baseOptions.WithMaxAttempts(10);
var conservative = baseOptions.WithMaxAttempts(2);
```

## Example: Custom Configuration for an HTTP Dependency

```csharp
using DotNetRealtimePipeline.DeadLetter;

var options = new RetryPolicyOptions
{
    MaxAttempts = 4,
    BaseDelay = TimeSpan.FromMilliseconds(250),
    MaxDelay = TimeSpan.FromSeconds(5),
    JitterFactor = 0.2,
    TransientExceptionPredicate = ex =>
        RetryPolicyOptions.DefaultTransientPredicate(ex) ||
        (ex is HttpRequestException http && http.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
};

var policy = new ExponentialBackoffRetryPolicy(options);

// Nominal delays before retries 1-3 (before jitter): 250ms, 500ms, 1s. MaxAttempts=4 allows no fourth wait.
Console.WriteLine(options);
// MaxAttempts=4, BaseDelay=00:00:00.2500000, MaxDelay=00:00:05, JitterFactor=0.20
```

## See Also

- [RetryPolicy](RetryPolicy.md) - backoff strategy and retry lifecycle
- [DeadLetterQueue](DeadLetterQueue.md) - where items go after retries are exhausted
