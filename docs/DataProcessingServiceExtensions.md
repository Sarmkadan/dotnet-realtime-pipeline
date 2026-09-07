# DataProcessingService extensions

`DataProcessingServiceExtensions` adds batch quality filtering, formatted quality reporting, and dictionary-based processing statistics to [`DataProcessingService`](DataProcessingService.md). Import the `DotNetRealtimePipeline.Services` namespace to call these methods with extension-method syntax.

## `ProcessBatchWithQualityFilterAsync`

```csharp
Task<List<ProcessingResult>> ProcessBatchWithQualityFilterAsync(
    this DataProcessingService service,
    List<DataPoint> dataPoints,
    int? minQuality = null)
```

Filters the supplied list before passing it to `DataProcessingService.ProcessBatchAsync`. A point is retained when its `Quality` is greater than or equal to the effective threshold.

- Set `minQuality` to override the service configuration for this call.
- When `minQuality` is `null`, the method uses the configured `MinDataQualityThreshold`. If that configuration cannot be read, it falls back to `50`.
- The returned list contains processing results only for points that passed the filter. Filtering itself does not produce failure results for rejected points.
- A null service or `dataPoints` argument throws `ArgumentNullException`.

## `GenerateQualityReportString`

```csharp
string GenerateQualityReportString(
    this DataProcessingService service,
    List<DataPoint> dataPoints,
    bool includeDetailedStats = true)
```

Runs `DataProcessingService.AnalyzeDataQuality` and formats the result as a multiline report suitable for logs or dashboards. The report includes the total point count, quality score, pass rate, high- and low-quality counts, average and range, and unique source count.

When `includeDetailedStats` is `true`, the report also appends minimum quality, maximum quality, and the configured threshold. Pass rate and average quality are formatted to two decimal places using the invariant culture. A null service or list throws `ArgumentNullException`; an empty list produces the service's empty-analysis values.

## `GetProcessingStatisticsAsync`

```csharp
Task<IReadOnlyDictionary<string, object>> GetProcessingStatisticsAsync(
    this DataProcessingService service)
```

Calls `DataProcessingService.GetStatisticsAsync` and exposes a read-only dictionary with these entries:

| Key | Value type | Meaning |
| --- | --- | --- |
| `TotalDataPoints` | `int` | Number of persisted data points |
| `ConfiguredMaxRetries` | `int` | Configured retry limit |
| `QualityThreshold` | `int` | Configured minimum quality |
| `ProcessingTimeoutMs` | `long` | Configured processing timeout in milliseconds |

The dictionary is a snapshot of the values returned by the service. A null service throws `ArgumentNullException`.

## Example

```csharp
using System;
using System.Collections.Generic;
using DotNetRealtimePipeline.Data.Repositories;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;

var config = new PipelineConfig
{
    MinDataQualityThreshold = 70,
    MaxRetries = 3,
    ProcessingTimeoutMs = 30_000
};

var repository = new InMemoryDataPointRepository();
var service = new DataProcessingService(repository, config);
var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

var points = new List<DataPoint>
{
    new(1, now, 21.5, "sensor-a") { Quality = 92 },
    new(2, now, 18.1, "sensor-b") { Quality = 65 },
    new(3, now, 19.8, "sensor-a") { Quality = 80 }
};

// The explicit threshold means only the points with quality 92 and 80
// are passed to ProcessBatchAsync.
var results = await service.ProcessBatchWithQualityFilterAsync(points, minQuality: 75);

var report = service.GenerateQualityReportString(points);
Console.WriteLine(report);

var statistics = await service.GetProcessingStatisticsAsync();
Console.WriteLine($"Stored points: {statistics["TotalDataPoints"]}");
Console.WriteLine($"Configured threshold: {statistics["QualityThreshold"]}");
```

The example reports on the original three-point list, while the processing statistics reflect points persisted by the filtered batch.
