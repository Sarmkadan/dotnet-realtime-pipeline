# PipelineOrchestratorExtensions

`PipelineOrchestratorExtensions` adds convenience methods for reporting on and interacting with a `PipelineOrchestrator`. The extensions validate the orchestrator argument and then delegate to the corresponding orchestrator APIs. For lifecycle, ingestion, and other core APIs, see [PipelineOrchestrator](PipelineOrchestrator.md).

Import the service and model namespaces to use the extension methods and `DataPoint`:

```csharp
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;
```

## GetStatusSummary

```csharp
string GetStatusSummary(this PipelineOrchestrator orchestrator)
```

Returns a one-line snapshot based on `orchestrator.GetStatus()`. The result has this shape:

```text
Pipeline[Running={value}, Processed={value}, Failed={value}, Pending={value}, Health={value}, Timestamp={round-trip timestamp}]
```

`Health` comes from the status's backpressure report, and `Timestamp` uses the round-trip (`o`) date/time format. The method throws `ArgumentNullException` when `orchestrator` is `null`.

## GetHealthReportAsync

```csharp
Task<HealthReport> GetHealthReportAsync(this PipelineOrchestrator orchestrator)
```

Returns the task produced by `PipelineOrchestrator.GetHealthReportAsync()`. The report includes the current health status, throughput, success and error rates, latency percentiles, backpressure percentage, totals, and generation time. The extension throws `ArgumentNullException` when `orchestrator` is `null`; exceptions from the underlying operation are propagated.

## GetPerformanceTrendAsync

```csharp
Task<PerformanceTrend> GetPerformanceTrendAsync(this PipelineOrchestrator orchestrator)
```

Returns the task produced by `PipelineOrchestrator.GetPerformanceTrendAsync()`. The trend describes its direction and changes in throughput, latency, and error rate, together with the sample count and time span. The extension throws `ArgumentNullException` when `orchestrator` is `null`; exceptions from the underlying operation are propagated.

## ProcessBatchAsync

```csharp
Task<BatchProcessingResult> ProcessBatchAsync(
    this PipelineOrchestrator orchestrator,
    IEnumerable<DataPoint> dataPoints)
```

Validates and materializes `dataPoints` as a list, then passes it to `PipelineOrchestrator.ProcessBatchDataPointsAsync()`. The returned result exposes `SuccessfulCount` and `FailedCount`.

The method throws:

- `ArgumentNullException` when `orchestrator` or `dataPoints` is `null`.
- `ArgumentException` when `dataPoints` is empty.
- Any exception raised by the underlying batch-processing operation.

Because the sequence is checked with `Any()` before it is converted with `ToList()`, callers should provide a repeatable enumerable such as an array or list rather than a single-use sequence.

## Example

The following method assumes the supplied orchestrator has already been configured. It starts the pipeline, uses all four extensions, and stops the pipeline in a `finally` block.

```csharp
using System;
using System.Threading.Tasks;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;

public static class PipelineDiagnostics
{
    public static async Task RunAsync(PipelineOrchestrator orchestrator)
    {
        await orchestrator.StartAsync();

        try
        {
            DataPoint[] batch =
            {
                new(1, DateTime.UtcNow.Ticks, 21.5, "sensor-01"),
                new(2, DateTime.UtcNow.Ticks, 22.0, "sensor-01")
            };

            BatchProcessingResult result = await orchestrator.ProcessBatchAsync(batch);
            HealthReport health = await orchestrator.GetHealthReportAsync();
            PerformanceTrend trend = await orchestrator.GetPerformanceTrendAsync();
            string summary = orchestrator.GetStatusSummary();

            Console.WriteLine(summary);
            Console.WriteLine($"Batch: {result.SuccessfulCount} succeeded, {result.FailedCount} failed");
            Console.WriteLine($"Health: {health.Status}; throughput: {health.ThroughputItemsPerSecond:F2}/s");
            Console.WriteLine($"Trend: {trend.TrendDirection}; samples: {trend.SamplesAnalyzed}");
        }
        finally
        {
            await orchestrator.StopAsync();
        }
    }
}
```
