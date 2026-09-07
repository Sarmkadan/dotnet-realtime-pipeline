# DeadLetterEntryExtensions

`DeadLetterEntryExtensions` provides path-string helpers and convenience methods for inspecting a [`DeadLetterEntry`](DeadLetterEntry.md). The type is in the `DotNetRealtimePipeline.DeadLetter` namespace.

## API

### `string? SanitizeForPath(this string? input)`

Removes the path-navigation sequences `../`, `./../`, `.\..\`, `..\`, `./`, and `.\` from the input by repeatedly applying ordinal string replacements in that order.

- Returns `null` when `input` is `null`.
- Returns the resulting string otherwise; it may be empty.
- Does not validate that the result is safe beneath a particular root directory and does not remove rooted-path prefixes. Treat it as a normalization helper, not as a complete path-security boundary.

### `string? GetSanitizedFilename(this string? input)`

Calls `SanitizeForPath`, then passes the result to `Path.GetFileName` so that only the filename portion recognized by the current operating system remains.

- Returns `null` when `input` is `null`.
- Can return an empty string when the sanitized value is empty or ends in a directory separator.
- Filename and directory-separator interpretation follows the platform's `System.IO.Path` behavior.

### `bool IsResolved(this DeadLetterEntry entry)`

Returns `true` only when `entry.Status` is `DeadLetterStatus.Resolved`; otherwise, returns `false`.

Throws `ArgumentNullException` when `entry` is `null`.

### `bool IsPermanentFailure(this DeadLetterEntry entry)`

Returns `true` only when `entry.Status` is `DeadLetterStatus.PermanentFailure`; otherwise, returns `false`.

Throws `ArgumentNullException` when `entry` is `null`.

### `double GetRetryProgress(this DeadLetterEntry entry)`

Returns `RetryCount / MaxRetries` as a `double` when `MaxRetries` is greater than zero. Returns `0.0` when `MaxRetries` is zero or negative.

The result is not clamped: it can be greater than `1.0` when `RetryCount` exceeds `MaxRetries`, and it can be negative when `RetryCount` is negative.

Throws `ArgumentNullException` when `entry` is `null`.

### `DateTime GetLastActivity(this DeadLetterEntry entry)`

Returns `LastRetryAt` when it has a value; otherwise, returns `EnqueuedAt`.

Throws `ArgumentNullException` when `entry` is `null`.

## Example

```csharp
using DotNetRealtimePipeline.DeadLetter;

var entry = new DeadLetterEntry
{
    FailureStageName = "Validation",
    FailureReason = "Invalid payload",
    RetryCount = 1,
    MaxRetries = 4,
    EnqueuedAt = DateTime.UtcNow.AddMinutes(-10),
    LastRetryAt = DateTime.UtcNow.AddMinutes(-2),
    Status = DeadLetterStatus.Pending
};

double progress = entry.GetRetryProgress();       // 0.25
DateTime activity = entry.GetLastActivity();      // the LastRetryAt value
bool resolved = entry.IsResolved();               // false
bool permanent = entry.IsPermanentFailure();      // false

string? segment = @"../imports/./batch-01".SanitizeForPath();
string? filename = "imports/batch-01.json".GetSanitizedFilename();

Console.WriteLine($"Retry progress: {progress:P0}");
Console.WriteLine($"Last activity: {activity:O}");
Console.WriteLine($"Resolved: {resolved}; permanent failure: {permanent}");
Console.WriteLine($"Sanitized segment: {segment}");
Console.WriteLine($"Filename: {filename}");
```

## Notes

- The four `DeadLetterEntry` extensions only inspect the entry; they do not change its status, retry count, or timestamps.
- `GetLastActivity` does not compare its two timestamps. When `LastRetryAt` is present, it is returned even if it is earlier than `EnqueuedAt`.
- The path helpers use ordinal, case-sensitive replacements.
