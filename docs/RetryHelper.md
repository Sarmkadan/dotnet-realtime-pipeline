# RetryHelper

A utility class that provides configurable retry policies for executing operations with automatic retries on transient failures. Supports both synchronous and asynchronous execution with customizable delay strategies, exception handling, and event tracking.

## API

### `RetryAsync<T>`

Executes the provided asynchronous operation with retry logic according to the configured policy.

- **Parameters**:
  - `operation`: The asynchronous operation to execute.
  - `cancellationToken`: Optional cancellation token.
- **Return value**: `Task<T>` representing the result of the operation.
- **Throws**: `RetryPolicyException` if all retry attempts are exhausted without success.

### `Retry<T>`

Executes the provided synchronous operation with retry logic according to the configured policy.

- **Parameters**:
  - `operation`: The synchronous operation to execute.
- **Return value**: `T` representing the result of the operation.
- **Throws**: `RetryPolicyException` if all retry attempts are exhausted without success.

### `RetryPolicyBuilder WithMaxAttempts`

Sets the maximum number of retry attempts.

- **Parameters**:
  - `maxAttempts`: The maximum number of attempts (must be ≥ 1).
- **Return value**: `RetryPolicyBuilder` for fluent chaining.
- **Throws**: `ArgumentOutOfRangeException` if `maxAttempts` is less than 1.

### `RetryPolicyBuilder WithInitialDelay`

Sets the initial delay between retry attempts in milliseconds.

- **Parameters**:
  - `initialDelayMs`: The initial delay in milliseconds (must be ≥ 0).
- **Return value**: `RetryPolicyBuilder` for fluent chaining.
- **Throws**: `ArgumentOutOfRangeException` if `initialDelayMs` is negative.

### `RetryPolicyBuilder WithMaxDelay`

Sets the maximum delay between retry attempts in milliseconds.

- **Parameters**:
  - `maxDelayMs`: The maximum delay in milliseconds (must be ≥ 0).
- **Return value**: `RetryPolicyBuilder` for fluent chaining.
- **Throws**: `ArgumentOutOfRangeException` if `maxDelayMs` is negative.

### `RetryPolicyBuilder WithJitter`

Enables or disables random jitter in delay calculations.

- **Parameters**:
  - `useJitter`: `true` to enable jitter; `false` to disable.
- **Return value**: `RetryPolicyBuilder` for fluent chaining.

### `RetryPolicyBuilder RetryOn<TException>`

Adds an exception type to the list of retryable exceptions.

- **Type Parameter**: `TException`: The exception type to retry on.
- **Return value**: `RetryPolicyBuilder` for fluent chaining.

### `RetryPolicy Build`

Finalizes the policy configuration and returns a `RetryPolicy` instance.

- **Return value**: `RetryPolicy` configured with the specified settings.

### `MaxAttempts`

Gets the maximum number of retry attempts configured for the policy.

- **Return value**: `int` representing the maximum attempts.

### `InitialDelayMs`

Gets the initial delay in milliseconds between retry attempts.

- **Return value**: `int` representing the initial delay.

### `MaxDelayMs`

Gets the maximum delay in milliseconds between retry attempts.

- **Return value**: `int` representing the maximum delay.

### `UseJitter`

Gets whether random jitter is enabled for delay calculations.

- **Return value**: `bool` indicating if jitter is enabled.

### `RetryableExceptions`

Gets the list of exception types that will trigger a retry.

- **Return value**: `List<Type>` of retryable exception types.

### `ExecuteAsync<T>`

Executes the provided asynchronous operation with retry logic according to the configured policy and tracks metrics.

- **Parameters**:
  - `operation`: The asynchronous operation to execute.
  - `cancellationToken`: Optional cancellation token.
- **Return value**: `Task<T>` representing the result of the operation.
- **Throws**: `RetryPolicyException` if all retry attempts are exhausted without success.

### `TotalAttempts`

Gets the total number of attempts made during the last execution.

- **Return value**: `int` representing the total attempts.

### `SuccessfulAttempts`

Gets the number of successful attempts during the last execution.

- **Return value**: `int` representing the successful attempts.

### `FailedAttempts`

Gets the number of failed attempts during the last execution.

- **Return value**: `int` representing the failed attempts.

### `Events`

Gets the list of retry events recorded during the last execution.

- **Return value**: `List<RetryEvent>` of recorded events.

### `RecordAttempt`

Records an attempt with the provided status and exception.

- **Parameters**:
  - `success`: `true` if the attempt succeeded; `false` otherwise.
  - `exception`: The exception that occurred, if any.

### `Timestamp`

Gets the timestamp of the last recorded attempt.

- **Return value**: `DateTime` representing the timestamp.
