# RetryPolicyBuilder

`RetryPolicyBuilder` is a fluent builder that assembles a `RetryPolicy` (defined in the same file, `src/Utilities/RetryHelper.cs`). It is the recommended way to configure retries when you want a policy object you can pass around, as opposed to the static `RetryHelper.RetryAsync` / `RetryHelper.Retry` helpers.

The same file also defines two companion types used for monitoring retries:

- `RetryStatistics` - aggregates attempt outcomes and exposes `SuccessRate`.
- `RetryEvent` - a single recorded attempt (timestamp, outcome, delay).

```csharp
using DotNetRealtimePipeline.Utilities;

var policy = new RetryPolicyBuilder()
    .WithMaxAttempts(5)
    .WithInitialDelay(100)
    .WithMaxDelay(5000)
    .WithJitter(true)
    .RetryOn<TimeoutException>()
    .RetryOn<HttpRequestException>()
    .Build();

var stats = new RetryStatistics();

var result = await policy.ExecuteAsync(() => FetchAsync(url));
```

`RetryPolicy.ExecuteAsync` does not record anything into `RetryStatistics` on its own; recording is done by the caller (see [RetryStatistics](#retrystatistics)).

## Fluent methods

All `With*` and `RetryOn` methods return the same builder instance, so calls can be chained in any order. Each call overwrites the previous value for the same setting, except `RetryOn`, which appends.

| Method | Argument | Effect | Throws |
|--------|----------|--------|--------|
| `WithMaxAttempts(int attempts)` | Total attempts, including the first | Sets `MaxAttempts` | `ArgumentOutOfRangeException` if `attempts` ≤ 0 |
| `WithInitialDelay(int delayMs)` | Base delay in milliseconds | Sets `InitialDelayMs` | `ArgumentOutOfRangeException` if `delayMs` < 0 |
| `WithMaxDelay(int delayMs)` | Delay cap in milliseconds | Sets `MaxDelayMs` | `ArgumentOutOfRangeException` if `delayMs` < 0 |
| `WithJitter(bool enabled)` | Whether to randomise delays | Sets `UseJitter` | None |
| `RetryOn<TException>()` | Exception type `TException : Exception` | Appends `typeof(TException)` to the retryable list | None |
| `Build()` | - | Creates a new `RetryPolicy` from the current settings | None |

### `WithMaxAttempts`

Counts the initial execution, so `WithMaxAttempts(1)` means "no retries". Zero and negative values are rejected.

### `WithInitialDelay`

The base delay used for the exponential schedule. `0` is accepted and produces immediate retries (jitter has no effect on a zero delay).

### `WithMaxDelay`

Caps the exponential delay. The cap is applied before jitter, so with jitter enabled the actual wait never exceeds `MaxDelayMs`.

### `WithJitter`

Defaults to **enabled** in the builder. This differs from `RetryHelper`, whose `UseJitter` property defaults to `false`. Pass `false` for deterministic delays.

### `RetryOn<TException>`

Adds a type to the retryable list. Matching is by assignability, so `RetryOn<IOException>()` also matches `FileNotFoundException`. The builder does not de-duplicate: calling `RetryOn<TimeoutException>()` twice adds two identical entries, which is harmless but redundant.

If **no** `RetryOn` call is made, the built policy's list is empty, and `RetryPolicy` treats **every** exception as retryable (up to `MaxAttempts`). This is the opposite of `RetryHelper`'s defaults, which retry only `TimeoutException`, `HttpRequestException` and `InvalidOperationException`. Always call `RetryOn` when you want a restricted set.

## Build()

`Build()` copies the current settings into a new `RetryPolicy` and returns it. Points to be aware of:

- **No validation beyond the fluent setters.** `Build()` never throws. `RetryPolicy` exposes public setters, so values can still be changed on the returned object afterwards.
- **The retryable list is shared, not copied.** The built policy's `RetryableExceptions` is the same `List<Type>` instance the builder holds. A later `RetryOn` call on the same builder changes the policy that was already built. Build a fresh builder (or fresh policy) if you need an independent copy.
- **The builder is reusable.** Each `Build()` creates a new `RetryPolicy` object from the current values; the builder is not reset.

## RetryPolicy execution timing

For reference when choosing delays, `RetryPolicy.ExecuteAsync` computes the wait before each retry as follows:

1. Increment the retry counter.
2. Double the current delay, capped at `MaxDelayMs`. The first wait is therefore `2 × InitialDelayMs` (capped), not `InitialDelayMs`.
3. If jitter is enabled, multiply the delay by a random factor in `[0.5, 1.0)`.
4. `Task.Delay` for the resulting value.

With `InitialDelayMs = 100` and `MaxDelayMs = 5000`, the nominal waits before retries 1, 2, 3, ... are `200`, `400`, `800`, `1600`, `3200`, `5000`, `5000` ms, before jitter. `OperationCanceledException` is never retried.

## RetryStatistics

`RetryStatistics` is a plain, mutable aggregate for tracking how an operation behaved across attempts. It is not connected to `RetryPolicy`; the caller records outcomes.

```csharp
var stats = new RetryStatistics();

stats.RecordAttempt(success: false, delayMs: 0);
stats.RecordAttempt(success: false, delayMs: 200);
stats.RecordAttempt(success: true, delayMs: 400);

Console.WriteLine(stats.SuccessRate); // 33.33...
```

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `TotalAttempts` | `int` | Number of recorded attempts. Incremented by `RecordAttempt`. |
| `SuccessfulAttempts` | `int` | Number of attempts recorded with `success: true`. |
| `FailedAttempts` | `int` | Number of attempts recorded with `success: false`. |
| `SuccessRate` | `double` | Read-only. Percentage of successful attempts, `0`-`100`. See below. |
| `Events` | `List<RetryEvent>` | Every recorded attempt, in order. Grows without bound. |

All counters and `Events` have public setters, so they can be reset or populated directly. Nothing keeps `TotalAttempts`, `SuccessfulAttempts` and `FailedAttempts` consistent with each other when set manually.

### `RecordAttempt(bool success, int delayMs)`

Records one attempt:

1. Validates `delayMs`; a negative value throws `ArgumentOutOfRangeException` before any state changes.
2. Increments `TotalAttempts`.
3. Increments `SuccessfulAttempts` if `success` is `true`, otherwise `FailedAttempts`.
4. Appends a `RetryEvent` with `Timestamp = DateTime.UtcNow`, the given `Success`, and the given `DelayMs`.

`delayMs` is stored as provided; the class does not compute or verify it. Pass the delay that was applied before the attempt (`0` for the first attempt, if you are not tracking one).

The class is not thread-safe. Use external synchronisation if several threads record into the same instance.

### `SuccessRate`

```csharp
public double SuccessRate => TotalAttempts > 0 ? (SuccessfulAttempts * 100.0) / TotalAttempts : 0;
```

- Returns a **percentage**, not a fraction: `75.0` means 75%.
- Returns `0` when `TotalAttempts` is `0`, so a fresh instance reports `0` rather than `NaN`. Note that this makes "no data" indistinguishable from "all attempts failed"; check `TotalAttempts` if the distinction matters.
- It is computed from `SuccessfulAttempts` and `TotalAttempts`, so it reflects manual edits to those fields as well.

### ToString

`ToString()` returns a summary such as:

```
RetryStatistics { TotalAttempts = 3, SuccessfulAttempts = 1, FailedAttempts = 2, SuccessRate = 33.33%, Events = 3 }
```

## RetryEvent

`RetryEvent` is a simple record of one attempt, created by `RetryStatistics.RecordAttempt`.

| Property | Type | Description |
|----------|------|-------------|
| `Timestamp` | `DateTime` | Time of recording, set by `RecordAttempt` to `DateTime.UtcNow`. |
| `Success` | `bool` | Whether the attempt succeeded. |
| `DelayMs` | `int` | Delay value passed to `RecordAttempt`. |

All properties have public setters, so events can also be constructed directly. `ToString()` formats the timestamp with the round-trip (`"O"`) specifier:

```
RetryEvent { Timestamp = 2026-11-04T10:15:30.1234567Z, Success = true, DelayMs = 400 }
```

## See also

- [RetryHelper](RetryHelper.md) - static `Retry` / `RetryAsync` helpers.
- [RetryPolicy](RetryPolicy.md) - the policy type produced by `Build()`.
- [RetryPolicyOptions](RetryPolicyOptions.md) - options for `ExponentialBackoffRetryPolicy`, a separate retry implementation in the dead-letter subsystem.
