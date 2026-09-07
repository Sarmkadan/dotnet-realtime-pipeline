# ProcessingResultExtensions

Extension methods for inspecting, combining, copying, and serializing [`ProcessingResult`](ProcessingResult.md) instances in pipeline code.

## API

### `IsRetryableFailure`

```csharp
bool IsRetryableFailure(this ProcessingResult result, int maxRetryCount = 3)
```

Returns `true` when all of the following are true:

- `Success` is `false`.
- `Exception` is not `null`, or `ErrorMessage` contains non-whitespace text.
- `RetryCount` is less than `maxRetryCount`.

The default maximum retry count is `3`. A result whose retry count equals the limit is not retryable. Throws `ArgumentNullException` when `result` is `null`.

### `MergeOutputData`

```csharp
void MergeOutputData(
    this ProcessingResult result,
    ProcessingResult source,
    bool overwriteExisting = false)
```

Copies entries from `source.OutputData` into `result.OutputData`. Existing keys are preserved by default; pass `overwriteExisting: true` to replace their values. The method modifies `result` and does not modify `source`.

Throws `ArgumentNullException` when `result` or `source` is `null`.

### `ToDictionary`

```csharp
Dictionary<string, object> ToDictionary(this ProcessingResult result)
```

Creates a case-insensitive dictionary suitable for structured logging or serialization. It always contains these keys:

- `ResultId`
- `Success`
- `StageName`
- `ProcessingTimeMs`
- `ProcessedAt`, formatted as an invariant-culture ISO 8601 round-trip string
- `RetryCount`
- `OutputData`, copied into a new dictionary
- `IsValid`, evaluated by calling `ProcessingResult.IsValid()`

It conditionally includes `ErrorMessage` when it contains non-whitespace text, `ExceptionType` and `ExceptionMessage` when an exception is present, and `CorrelationId` when it contains non-whitespace text. Throws `ArgumentNullException` when `result` is `null`.

### `WithProcessingTime`

```csharp
ProcessingResult WithProcessingTime(
    this ProcessingResult result,
    long processingTimeMs)
```

Returns a cloned result with `ProcessingTimeMs` set to the supplied value. The clone keeps the same `ResultId` and has its own `OutputData` dictionary, so adding or replacing entries on the clone does not change the original dictionary. Other property values are copied according to `ProcessingResult.Clone`.

Throws `ArgumentNullException` when `result` is `null`.

### `IsTimeout`

```csharp
bool IsTimeout(this ProcessingResult result, long timeoutThresholdMs = 5000)
```

Returns `true` when `ProcessingTimeMs` is strictly greater than the threshold. The default threshold is `5000` milliseconds, so a processing time of exactly `5000` milliseconds is not considered a timeout. Throws `ArgumentNullException` when `result` is `null`.

## Example

```csharp
using System;
using System.Collections.Generic;
using DotNetRealtimePipeline.Domain.Models;

var validation = new ProcessingResult(101, success: false, stageName: "Validation")
{
    ErrorMessage = "The upstream service was unavailable.",
    RetryCount = 1,
    ProcessingTimeMs = 5_250,
    CorrelationId = "order-42",
    OutputData = new Dictionary<string, object>
    {
        ["status"] = "pending"
    }
};

bool shouldRetry = validation.IsRetryableFailure(maxRetryCount: 3); // true
bool timedOut = validation.IsTimeout();                             // true

var enrichment = new ProcessingResult(102, success: true, stageName: "Enrichment")
{
    OutputData = new Dictionary<string, object>
    {
        ["status"] = "enriched",
        ["region"] = "eu-west"
    }
};

validation.MergeOutputData(enrichment);
// "status" remains "pending"; "region" is added.

validation.MergeOutputData(enrichment, overwriteExisting: true);
// "status" is now "enriched".

ProcessingResult measured = validation.WithProcessingTime(875);
Dictionary<string, object> logFields = measured.ToDictionary();

Console.WriteLine(logFields["ProcessingTimeMs"]); // 875
Console.WriteLine(measured.IsTimeout());           // false
```
