# ExportService extensions

`ExportServiceExtensions` adds output validation, retry, stream export, size estimation, and metadata-based naming to [`ExportService`](ExportService.md).

## `ValidateOutputDirectory`

```csharp
bool ValidateOutputDirectory(string outputPath)
```

Checks whether a file can be created and deleted in the directory associated with `outputPath`. If the directory is non-empty and does not exist, the method creates it. The method returns `false` for file-system errors rather than propagating them.

| Parameter | Type | Description |
| --- | --- | --- |
| `exportService` | `ExportService` | Service instance on which the extension is invoked. Cannot be `null`. |
| `outputPath` | `string` | Output file path whose directory is validated. Cannot be `null` or empty. A path without a directory component tests the current directory. |

Returns `true` when the temporary write/delete test succeeds; otherwise, `false`. A `null` service throws `ArgumentNullException`, and a null or empty path throws `ArgumentException` before the file-system test.

## `ExportWithRetryAsync`

```csharp
Task<ExportResult> ExportWithRetryAsync(
    List<DataPoint> dataPoints,
    string outputPath,
    OutputFormat format,
    int maxRetries = 3)
```

Calls `ExportDataPointsAsync` until it returns a successful result, retrying exceptions with delays from a fixed one-, two-, four-, and eight-second delay table. In the current implementation, the retry count advances only when `ExportDataPointsAsync` throws; an unsuccessful returned `ExportResult` does not advance it. The delay is selected after incrementing the retry count, so the first caught exception waits two seconds. Values of `maxRetries` that allow the retry count to exceed the delay table can cause an indexing exception.

| Parameter | Type | Description |
| --- | --- | --- |
| `exportService` | `ExportService` | Service instance on which the extension is invoked. Cannot be `null`. |
| `dataPoints` | `List<DataPoint>` | Data points passed to `ExportDataPointsAsync`. Cannot be `null`. |
| `outputPath` | `string` | Destination file path. Cannot be `null` or empty. |
| `format` | `OutputFormat` | Formatter to use: `Json`, `Csv`, `Table`, or `Html`. |
| `maxRetries` | `int` | Maximum retry count used by the loop. Defaults to `3`; the method does not independently validate its range. |

Returns the first successful `ExportResult`. If the retry loop terminates without success, it throws `InvalidOperationException` with the last recorded error as its inner exception. Argument validation can also throw `ArgumentNullException` or `ArgumentException`.

## `ExportToStreamAsync`

```csharp
Task<ExportResult> ExportToStreamAsync(
    List<DataPoint> dataPoints,
    Stream stream,
    OutputFormat format)
```

Formats the complete list and writes the resulting text to `stream` through a `StreamWriter`. The writer leaves the supplied stream open and flushes it before returning.

| Parameter | Type | Description |
| --- | --- | --- |
| `exportService` | `ExportService` | Service instance on which the extension is invoked. Used for null validation; the formatter performs the export. |
| `dataPoints` | `List<DataPoint>` | Data points to format and write. Cannot be `null`. |
| `stream` | `Stream` | Writable destination stream. Cannot be `null` and remains open after the call. |
| `format` | `OutputFormat` | Formatter to use: `Json`, `Csv`, `Table`, or `Html`. |

On success, the returned `ExportResult` has `Success` set to `true`, `RecordCount` set to the list count, and `FileSizeBytes` set to the formatted string's character count. `StartTime` and `EndTime` use UTC. Formatting and stream exceptions are recorded in the local result and then rethrown; null arguments throw `ArgumentNullException`.

## `EstimateFileSizeAsync`

```csharp
Task<string> EstimateFileSizeAsync(
    List<DataPoint> dataPoints,
    OutputFormat format)
```

Formats all supplied data points in memory, measures the formatted string's character count, and passes that count to `PathHelper.FormatFileSize`.

| Parameter | Type | Description |
| --- | --- | --- |
| `exportService` | `ExportService` | Service instance on which the extension is invoked. Used for null validation. |
| `dataPoints` | `List<DataPoint>` | Complete set of data points to format for the estimate. Cannot be `null`. |
| `format` | `OutputFormat` | Formatter to use: `Json`, `Csv`, `Table`, or `Html`. |

Returns a human-readable size string. Because the calculation uses `string.Length`, it is a formatted character-count estimate rather than a measurement of encoded file bytes. Null service or data-point arguments throw `ArgumentNullException`; formatter errors propagate.

## `ExportWithMetadataAsync`

```csharp
Task<ExportResult> ExportWithMetadataAsync(
    List<DataPoint> dataPoints,
    string baseOutputPath,
    OutputFormat format,
    DateTime? timestamp = null,
    bool includeRecordCount = true)
```

Builds a filename in the directory portion of `baseOutputPath`, then delegates to `ExportDataPointsAsync`. The filename is `export_yyyyMMdd_HHmmss_count.ext` when `includeRecordCount` is `true`, or `export_yyyyMMdd_HHmmss.ext` otherwise. The extension is `json`, `csv`, `html`, or `txt`; both `Table` and unrecognized enum values use `txt`. The original filename portion of `baseOutputPath` is not retained.

| Parameter | Type | Description |
| --- | --- | --- |
| `exportService` | `ExportService` | Service instance used to perform the file export. Cannot be `null`. |
| `dataPoints` | `List<DataPoint>` | Data points to export and, optionally, count in the filename. Cannot be `null`. |
| `baseOutputPath` | `string` | Path whose directory determines the destination directory. Cannot be `null` or empty. |
| `format` | `OutputFormat` | Output formatter and filename extension selector. |
| `timestamp` | `DateTime?` | Timestamp embedded in the filename. Defaults to the current UTC time when `null`. |
| `includeRecordCount` | `bool` | Adds `dataPoints.Count` to the filename when `true`. Defaults to `true`. |

Returns the `ExportResult` produced by `ExportDataPointsAsync`. Null service or data-point arguments throw `ArgumentNullException`; a null or empty base path throws `ArgumentException`.

## Example

```csharp
using DotNetRealtimePipeline.Data;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Formatters;
using Microsoft.Extensions.Logging.Abstractions;

var exportService = new ExportService(NullLogger<ExportService>.Instance);
var dataPoints = new List<DataPoint>
{
    new(1, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 21.5, "sensor-1")
};

var outputPath = Path.Combine("exports", "data.json");
if (exportService.ValidateOutputDirectory(outputPath))
{
    ExportResult result = await exportService.ExportWithRetryAsync(
        dataPoints,
        outputPath,
        OutputFormat.Json);

    Console.WriteLine($"Exported {result.RecordCount} records.");
}

string estimatedSize = await exportService.EstimateFileSizeAsync(
    dataPoints,
    OutputFormat.Csv);
Console.WriteLine($"Estimated CSV size: {estimatedSize}");

await using var stream = new MemoryStream();
await exportService.ExportToStreamAsync(dataPoints, stream, OutputFormat.Json);

ExportResult namedResult = await exportService.ExportWithMetadataAsync(
    dataPoints,
    Path.Combine("exports", "ignored-name.json"),
    OutputFormat.Json,
    timestamp: DateTime.UtcNow,
    includeRecordCount: true);
```
