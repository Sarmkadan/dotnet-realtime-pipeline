# IPipelineMetrics

`IPipelineMetrics` is the contract for recording event counts and reading pipeline throughput in events per second (EPS). It supports both a pipeline-wide counter and counters keyed by pipeline stage.

`ThroughputCounter` is the built-in, thread-safe implementation. For documentation focused on that concrete type, see [ThroughputCounter](ThroughputCounter.md).

## API

### `void RecordEvents(long count)`

Adds `count` events to the pipeline-wide counter.

In `ThroughputCounter`, a count less than or equal to zero is ignored.

### `void RecordEvents(string stageName, long count)`

Adds `count` events to the counter associated with `stageName`. Stage counters are independent of the pipeline-wide counter, so call both overloads when an event must contribute to both measurements.

In `ThroughputCounter`, a null, empty, or whitespace-only `stageName` causes an `ArgumentException`. A count less than or equal to zero is ignored after the stage name is validated.

### `double GetThroughput()`

Returns the pipeline-wide throughput in events per second. `ThroughputCounter` calculates this value as the total stored event count divided by the configured window length in seconds.

### `double GetThroughput(string stageName)`

Returns the throughput in events per second for `stageName`. `ThroughputCounter` returns `0` when the stage name is null, empty, or whitespace-only, or when no counter exists for that stage.

## ThroughputCounter window

`ThroughputCounter` stores counts in circular, one-second buckets. Its constructor accepts the window length in seconds:

```csharp
var defaultCounter = new ThroughputCounter();           // 60-second window
var shortWindowCounter = new ThroughputCounter(10);     // 10-second window
```

The default window is 60 seconds. Passing zero or a negative value throws `ArgumentOutOfRangeException`.

The pipeline-wide counter advances its sliding window when events are recorded, clearing buckets from the previous rotation before adding the new count. Reads sum the window's buckets and divide by the full configured window length; they do not divide by the number of seconds elapsed since construction. Recording and reading use atomic operations so the implementation can be shared by concurrent pipeline workers.

Each stage is backed by its own circular bucket array. Stage buckets are selected from the current Unix-time second, while the pipeline-wide buckets also track window advancement and stale-bucket cleanup.

## Example

```csharp
using System;
using DotNetRealtimePipeline.Metrics;

IPipelineMetrics metrics = new ThroughputCounter();

const long batchSize = 120;

// Record separately because stage counts do not update the global count.
metrics.RecordEvents(batchSize);
metrics.RecordEvents("transform", batchSize);

double pipelineEps = metrics.GetThroughput();
double transformEps = metrics.GetThroughput("transform");
double unknownStageEps = metrics.GetThroughput("not-registered"); // 0

Console.WriteLine($"Pipeline: {pipelineEps:F2} events/sec");
Console.WriteLine($"Transform: {transformEps:F2} events/sec");
```

With the default 60-second window, recording 120 events produces `2.00` EPS while those events remain in the measured buckets (`120 / 60`).
