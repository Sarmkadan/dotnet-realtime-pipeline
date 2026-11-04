# ExponentialBackoffRetryPolicy

`ExponentialBackoffRetryPolicy` is the default implementation of `IRetryPolicy`. It executes an operation, retries it when the thrown exception is classified as transient, and waits between attempts for an exponentially growing, randomly jittered delay. When the attempt budget is exhausted or a permanent exception occurs, it returns a failed `RetryResult<T>` so the caller can route the item to the dead-letter queue.

The class is `sealed` and is configured through [RetryPolicyOptions](RetryPolicyOptions.md). Passing no options uses the defaults shown below.

```csharp
using DotNetRealtimePipeline.DeadLetter;

// Defaults: MaxAttempts = 3, BaseDelay = 200ms, MaxDelay = 30s, JitterFactor = 0.25
var policy = new ExponentialBackoffRetryPolicy();

var result = await policy.ExecuteAsync(
    attempt => FetchFromUpstreamAsync(attempt, cancellationToken),
    cancellationToken);

if (!result.Succeeded)
{
    // Hand result.LastException and result.Attempts to the dead-letter queue.
}
```

## Public Members

| Member | Description |
|--------|-------------|
| `ExponentialBackoffRetryPolicy(RetryPolicyOptions? options = null)` | Creates the policy. A `null` argument is replaced with `new RetryPolicyOptions()`. |
| `int MaxAttempts` | The attempt budget, including the first attempt. Read from the options. |
| `bool IsTransient(Exception exception)` | Classifies an exception by calling the options' `TransientExceptionPredicate`. |
| `TimeSpan GetBackoffDelay(int attemptNumber)` | Computes the delay to wait after the given failed attempt. |
| `Task<RetryResult<T>> ExecuteAsync<T>(Func<int, Task<T>> operation, CancellationToken cancellationToken = default)` | Runs the operation with retries. The delegate receives the 1-based attempt number. |
| `string ToString()` | Returns a summary such as `ExponentialBackoffRetryPolicy: MaxAttempts=3, BaseDelay=00:00:00.2000000, MaxDelay=00:00:30, JitterFactor=0.25`. |

`MaxAttempts` is a read-only property that forwards to the options instance. The policy keeps a reference to that instance rather than a copy, so a later change to the options is seen by the policy on its next call. If you want a policy whose configuration never changes, pass a dedicated options object (for example one produced by `Clone()`) and stop mutating it.

## Delay Formula

`GetBackoffDelay(attemptNumber)` computes the delay in three steps:

```
exponent = min(attemptNumber - 1, 20)
nominal  = min(BaseDelay * 2^exponent, MaxDelay)
delay    = max(0, nominal + nominal * JitterFactor * r),   r uniform in [-1, +1)
```

1. **Exponential growth.** The delay doubles with each attempt: `BaseDelay * 2^(attemptNumber - 1)`.
2. **Cap.** The result is limited to `MaxDelay`. Once the cap is reached, every later retry waits `MaxDelay` (before jitter).
3. **Jitter.** A random offset of up to `±JitterFactor * nominal` is added, so that many items failing at the same moment do not retry in lockstep. The result is clamped to a minimum of `0`. When `nominal` is zero (`BaseDelay = 0`) no jitter is applied.

Because jitter is applied after the cap, the actual wait lies in the range `[nominal * (1 - JitterFactor), nominal * (1 + JitterFactor)]`. With the defaults the longest possible wait is `30s * 1.25 = 37.5s`.

`GetBackoffDelay` throws `ArgumentOutOfRangeException` when `attemptNumber` is less than `1`.

### Exponent Cap

The exponent is capped at `20` (`MaxExponent` in the source). This prevents `Math.Pow` from overflowing `double` range and `TimeSpan` from failing for very large attempt numbers. The practical effect is:

- With `BaseDelay = 200ms`, the nominal delay stops doubling at attempt `21` (`200ms * 2^20`, about 58 hours).
- With the default `MaxDelay = 30s` this cap is never the deciding factor, because `MaxDelay` already limits the delay long before attempt 21.
- It only matters when `MaxDelay` is set to a very large value; then attempts 21 and beyond all produce the same nominal delay.

## Worked Delay Table

Default options: `BaseDelay = 200ms`, `MaxDelay = 30s`, `JitterFactor = 0.25`. The "Nominal" column is the delay before jitter; the "Range" column is the interval a single call to `GetBackoffDelay` can return.

| Attempt (`attemptNumber`) | Exponent | Uncapped `200ms * 2^exp` | Nominal (after `MaxDelay` cap) | Range with ±25% jitter |
|---:|---:|---:|---:|---|
| 1 | 0 | 200 ms | 200 ms | 150 ms – 250 ms |
| 2 | 1 | 400 ms | 400 ms | 300 ms – 500 ms |
| 3 | 2 | 800 ms | 800 ms | 600 ms – 1 s |
| 4 | 3 | 1.6 s | 1.6 s | 1.2 s – 2 s |
| 5 | 4 | 3.2 s | 3.2 s | 2.4 s – 4 s |
| 6 | 5 | 6.4 s | 6.4 s | 4.8 s – 8 s |
| 7 | 6 | 12.8 s | 12.8 s | 9.6 s – 16 s |
| 8 | 7 | 25.6 s | 25.6 s | 19.2 s – 32 s |
| 9 | 8 | 51.2 s | **30 s** | 22.5 s – 37.5 s |
| 10 | 9 | 102.4 s | **30 s** | 22.5 s – 37.5 s |
| 11 and later | 10 … 20 | grows to 200 ms × 2^20 | **30 s** | 22.5 s – 37.5 s |

Notes on the table:

- The default `MaxAttempts = 3` means only the first two rows (attempts 1 and 2) are ever used with default options: the policy waits after attempt 1 (150–250 ms) and after attempt 2 (300–500 ms), then makes its third and final attempt. The total wait before the final attempt is therefore 450–750 ms.
- Attempt 8 is the first row whose range can exceed `MaxDelay`: its upper bound is 32 s, above the 30 s cap, because jitter is applied after the cap.
- Attempts 9 and later are flat at `MaxDelay`; they only occur when `MaxAttempts` is raised.

## Transient Classification

`IsTransient(exception)` throws `ArgumentNullException` for a `null` argument and otherwise returns the result of `RetryPolicyOptions.TransientExceptionPredicate(exception)`. The policy itself does not hold any classification rules. By default that predicate is `RetryPolicyOptions.DefaultTransientPredicate`, which treats `TimeoutException`, `IOException`, `SocketException`, and `HttpRequestException` as transient, unwraps `AggregateException` and `PipelineProcessingException` to their inner cause, and treats everything else (including `OperationCanceledException`) as permanent. See [RetryPolicyOptions](RetryPolicyOptions.md#default-transient-predicate) for the full list.

To change the classification, set `TransientExceptionPredicate` on the options before constructing the policy; `IsTransient` picks up that predicate.

## MaxAttempts and ExecuteAsync Lifecycle

`MaxAttempts` counts the first execution. `MaxAttempts = 1` means the operation runs once and is never retried; `MaxAttempts = 3` means at most three executions and at most two waits.

`ExecuteAsync` runs as follows:

1. Increment the attempt counter and check the cancellation token. A cancelled token throws `OperationCanceledException` before the operation runs.
2. Run `operation(attempt)`. On success, return `RetryResult<T>.Success(value, attempt)`.
3. If the operation throws:
   - An `OperationCanceledException` raised while `cancellationToken` is cancelled is rethrown unchanged.
   - Any other exception is recorded as the last exception. If it is not transient, or the attempt counter has reached `MaxAttempts`, the method returns `RetryResult<T>.Failure(attempt, exception)` immediately.
   - Otherwise the policy waits `GetBackoffDelay(attempt)` (skipped when the delay is zero) using `Task.Delay` with the same cancellation token, and then starts the next attempt.

Cancelling the token while the policy is waiting between attempts causes `Task.Delay` to throw `TaskCanceledException`, which propagates to the caller. A cancelled operation is never converted into a failed `RetryResult`.

The `RetryResult<T>` returned on failure carries `Attempts` (how many executions were made) and `LastException` (the exception from the final attempt), which the dead-letter queue stores alongside the item. See [RetryPolicy](RetryPolicy.md) for the wider retry lifecycle and [DeadLetterQueue](DeadLetterQueue.md) for what happens after the budget is exhausted.

## Example: Inspecting the Schedule

```csharp
using DotNetRealtimePipeline.DeadLetter;

var policy = new ExponentialBackoffRetryPolicy(new RetryPolicyOptions
{
    MaxAttempts = 5,
    BaseDelay = TimeSpan.FromMilliseconds(100),
    MaxDelay = TimeSpan.FromSeconds(2),
    JitterFactor = 0   // deterministic schedule
});

for (var attempt = 1; attempt < policy.MaxAttempts; attempt++)
{
    Console.WriteLine($"after attempt {attempt}: {policy.GetBackoffDelay(attempt).TotalMilliseconds} ms");
}
// after attempt 1: 100 ms
// after attempt 2: 200 ms
// after attempt 3: 400 ms
// after attempt 4: 800 ms
```

Setting `JitterFactor = 0` makes `GetBackoffDelay` deterministic, which is useful in unit tests that assert on the schedule.

## See Also

- [RetryPolicyOptions](RetryPolicyOptions.md) - configuration, validation, and the default transient predicate
- [RetryPolicy](RetryPolicy.md) - backoff strategy and retry lifecycle
- [DeadLetterQueue](DeadLetterQueue.md) - where items go after retries are exhausted
