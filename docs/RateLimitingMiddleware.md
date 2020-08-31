# RateLimitingMiddleware

Middleware component that enforces rate limiting based on token bucket algorithm with configurable capacity and refill intervals. Designed for high-throughput pipelines where per-request or per-stage rate limits are required.

## API

### `RateLimitingMiddleware`
Initializes a new rate limiter with specified capacity and refill interval. Tokens are refilled at fixed intervals, and requests consume tokens until capacity is exhausted.

| Parameter | Type | Description |
|-----------|------|-------------|
| `capacity` | `int` | Maximum number of tokens the bucket can hold. Must be positive. |
| `refillInterval` | `TimeSpan` | Duration between token refills. Must be positive. |

Throws `ArgumentOutOfRangeException` if `capacity` ≤ 0 or `refillInterval` ≤ `TimeSpan.Zero`.

---

### `TryAcquire`
Attempts to acquire a single token without blocking. Returns `true` if a token is available and consumed; otherwise `false`.

| Return | Type | Description |
|--------|------|-------------|
| Success | `bool` | `true` if token was acquired and consumed; `false` otherwise. |

No exceptions.

---

### `GetStatus`
Returns the current status of the global rate limiter, including token count, capacity, and refill timing.

| Return | Type | Description |
|--------|------|-------------|
| Status | `RateLimitStatus` | Structure containing `AvailableTokens`, `Capacity`, `ResetTime`, and `NextRefillTime`. |

No exceptions.

---
### `Reset`
Resets the token bucket to full capacity immediately. Useful for recovery or manual overrides.

No parameters. No return value. No exceptions.

---
### `GetAllStatuses`
Returns a dictionary of all registered stage-specific rate limiters and their current statuses.

| Return | Type | Description |
|--------|------|-------------|
| Statuses | `Dictionary<string, RateLimitStatus>` | Keys are stage names; values are current status objects. |

No exceptions.

---
### `AvailableTokens`
Gets the current number of available tokens in the global bucket.

| Return | Type | Description |
|--------|------|-------------|
| Tokens | `int` | Number of tokens remaining. |

No exceptions.

---
### `Capacity`
Gets the maximum capacity of the global token bucket.

| Return | Type | Description |
|--------|------|-------------|
| Capacity | `int` | Maximum number of tokens. |

No exceptions.

---
### `ResetTime`
Gets the next time the global bucket will be reset to full capacity.

| Return | Type | Description |
|--------|------|-------------|
| ResetTime | `DateTime` | UTC timestamp of next reset. |

No exceptions.

---
### `NextRefillTime`
Gets the next time tokens will be refilled in the global bucket.

| Return | Type | Description |
|--------|------|-------------|
| NextRefillTime | `DateTime` | UTC timestamp of next refill. |

No exceptions.

---
### `RateLimitBucket`
Gets the underlying token bucket instance used for global rate limiting.

| Return | Type | Description |
|--------|------|-------------|
| Bucket | `object` | Internal bucket reference. Type not exposed publicly. |

No exceptions.

---
### `TryConsume`
Attempts to consume a specified number of tokens from the global bucket. Returns `true` if all tokens were available and consumed; otherwise `false`.

| Parameter | Type | Description |
|-----------|------|-------------|
| `tokens` | `int` | Number of tokens to consume. Must be positive. |

| Return | Type | Description |
|--------|------|-------------|
| Success | `bool` | `true` if tokens were consumed; `false` otherwise. |

Throws `ArgumentOutOfRangeException` if `tokens` ≤ 0.

---
### `RegisterStageLimit`
Registers a new stage-specific rate limiter with a given capacity and refill interval.

| Parameter | Type | Description |
|-----------|------|-------------|
| `stageName` | `string` | Unique identifier for the stage. Must not be null or empty. |
| `capacity` | `int` | Maximum tokens for this stage. Must be positive. |
| `refillInterval` | `TimeSpan` | Duration between refills. Must be positive. |

Throws `ArgumentNullException` if `stageName` is null.
Throws `ArgumentException` if `stageName` is empty or whitespace.
Throws `ArgumentOutOfRangeException` if `capacity` ≤ 0 or `refillInterval` ≤ `TimeSpan.Zero`.
Throws `InvalidOperationException` if a stage with the same name is already registered.

---
### `CanProcessInStage`
Checks whether the specified stage can currently process a request without exceeding its rate limit.

| Parameter | Type | Description |
|-----------|------|-------------|
| `stageName` | `string` | Name of the stage to check. Must not be null or empty. |

| Return | Type | Description |
|--------|------|-------------|
| CanProcess | `bool` | `true` if the stage has at least one token available; `false` otherwise. |

Throws `KeyNotFoundException` if `stageName` is not registered.

---
### `GetStageLimitStatuses`
Returns a dictionary of all registered stage-specific rate limiters and their current statuses.

| Return | Type | Description |
|--------|------|-------------|
| Statuses | `Dictionary<string, RateLimitStatus>` | Keys are stage names; values are current status objects. |

No exceptions.

## Usage

### Example 1: Global Rate Limiting
