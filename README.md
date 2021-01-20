# DotNetRealtimePipeline

<!-- existing content ... -->

## DynamicScalingWorker
The `DynamicScalingWorker` class is a background worker that periodically evaluates and adapts the concurrency of pipeline stages in response to observed backpressure. It uses the `DynamicScalingService` to drive scaling decisions. 

## BackgroundProcessingWorker
`BackgroundProcessingWorker` is a long‑running background worker that continuously drives pipeline processing by periodically querying the `PipelineOrchestrator` for status.  
It works together with `MetricsAggregationWorker`, `HealthCheckWorker`, and `WorkerCoordinator` to provide a complete set of background services for a running pipeline.

### Usage example
The following example demonstrates how to create, start, stop, and dispose the workers using their real public members. It assumes that the required services (`PipelineOrchestrator`, `MetricsService`) have parameterless constructors or are otherwise available.

