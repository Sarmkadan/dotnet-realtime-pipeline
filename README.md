# dotnet-realtime-pipeline

An in-process, in-memory real-time data pipeline library for .NET with backpressure
management, tumbling/sliding windowing, metrics collection, and a console demo entry
point (`Program.cs`).

## Quick Start

```bash
dotnet build dotnet-realtime-pipeline.csproj
dotnet run --project dotnet-realtime-pipeline.csproj
```

The demo configures the pipeline via `AddPipelineServices(...)`, ingests 500 sample
sensor points, and prints processing stats and a health report.

## Architecture

See [docs/architecture.md](docs/architecture.md) for the component breakdown, data flow,
concurrency model, extension points, and known limitations.

## Class Reference

The sections below are generated per-class API notes; more per-class docs live in
[docs/](docs/).

## PipelineHttpClientFactoryExtensions
The `PipelineHttpClientFactoryExtensions` class provides convenient extension methods for `PipelineHttpClientFactory`, allowing for simplified HTTP client creation, configuration, and data exchange. It includes methods for setting up clients with base addresses, applying custom timeout/retry policies, and executing asynchronous GET and POST requests.

Example usage:
```csharp
using DotNetRealtimePipeline.Integration;
using System.Net.Http;
using System.Text;

// Assume factory is an initialized instance of PipelineHttpClientFactory
var factory = new PipelineHttpClientFactory();

// Create a client with a specific base address
var client = factory.CreateClientWithBaseAddress("https://api.example.com");

// Create a configured service client with retry and compression
var serviceClient = factory.CreateConfiguredServiceClient(
    serviceName: "MyService",
    timeout: TimeSpan.FromSeconds(30),
    maxRetries: 5,
    useCompression: true
);

// Execute a GET request
string result = await factory.GetStringAsync("https://api.example.com/data");

// Execute a POST request with JSON
var content = new StringContent("{\"key\":\"value\"}", Encoding.UTF8, "application/json");
string postResult = await factory.PostJsonAsync("https://api.example.com/ingest", content);
```

## BackpressureMetricsCollectorTests
The `BackpressureMetricsCollectorTests` class provides unit tests for the `BackpressureMetricsCollector` class, verifying its ability to track and report backpressure metrics across pipeline stages. It includes tests for handling unknown stages, recording manual events, aggregating metrics, and resetting collected data.

Example usage:
```csharp
using DotNetRealtimePipeline.Tests.Unit;

var tests = new BackpressureMetricsCollectorTests();

// Test unknown stage behavior
tests.GetStageMetrics_UnknownStage_ReturnsNull();

// Test activation event recording
tests.RecordManualEvent_Activation_IncrementsActivationCount();
tests.RecordManualEvent_TwoActivations_CountIsTwo();

// Test snapshot aggregation
tests.GetSnapshot_WithNoEvents_ReturnsEmptySnapshot();
tests.GetSnapshot_AggregatesAcrossStages();

// Test event retrieval
tests.GetRecentEvents_ReturnsUpToRequestedCount();
tests.GetStageEvents_ReturnsOnlyEventsForThatStage();

// Test reset functionality
tests.Reset_ClearsAllMetricsAndEvents();

// Test integration with backpressure activation
tests.Poll_AfterBackpressureActivated_RecordsActivationEvent();
```

## BackpressureServiceTests
The `BackpressureServiceTests` class provides unit tests for the `BackpressureService` class, verifying its ability to manage backpressure across pipeline stages. It includes tests for creating contexts, adding to buffers, applying backpressure, and removing from buffers. 

Example usage:
```csharp
using DotNetRealtimePipeline.Services;
using DotNetRealtimePipeline.Domain.Enums;

var service = new BackpressureService();

// Create a context
service.CreateContext("TestStage", 1000);

// Add to buffer
var result = service.TryAddToBuffer("TestStage", 500);

// Apply backpressure with block strategy
var response = service.ApplyBackpressureAsync(
    "TestStage",
    BackpressureStrategy.Block,
    timeoutMs: 1000
).Result;

// Remove from buffer
service.RemoveFromBuffer("TestStage", 200);
```

## PipelineVisualizerTests
The `PipelineVisualizerTests` class provides unit tests for the `PipelineVisualizer` class, verifying its ability to visualize pipeline stages and their relationships. It includes tests for building nodes, rendering pipeline visualizations, and computing health labels for pipeline nodes. 

Example usage:
```csharp
var visualizer = new PipelineVisualizerTests();
visualizer.BuildNodes_WithValidConfig_ReturnsOneNodePerStage();
visualizer.BuildNodes_EdgesAreLinkedSequentially();
visualizer.Render_ContainsPipelineName();
visualizer.Render_ContainsAllStageNames();
visualizer.RenderCompact_ContainsSeparators();
visualizer.PipelineVisualizationNode_ComputeHealthLabel_BackpressuredIsCritical();
visualizer.PipelineVisualizationNode_ComputeHealthLabel_HighBufferIsWarning();
visualizer.PipelineVisualizationNode_ComputeHealthLabel_NormalIsHealthy();
```

## PipelineBenchmarks
The `PipelineBenchmarks` class provides performance benchmarks for the dotnet-realtime-pipeline library. It measures throughput and memory allocation for critical pipeline operations.

To use `PipelineBenchmarks`, create an instance and call the `Setup` method to initialize the benchmark environment. Run individual benchmarks using their respective methods.

Example usage:
```csharp
using DotNetRealtimePipeline.Benchmarks;

var benchmarks = new PipelineBenchmarks();
benchmarks.Setup();

await benchmarks.IngestSingleDataPoint();
await benchmarks.ProcessBatch(100);
benchmarks.ProcessDataPointsThroughWindowing(1000);
await benchmarks.GenerateHealthReport();
benchmarks.BackpressureBufferOperations();
await benchmarks.EndToEndThroughput();
await benchmarks.MemoryAllocationBenchmark();

benchmarks.Cleanup();
```

## EventSubscriberBaseExtensions
The `EventSubscriberBaseExtensions` class provides utility extension methods for working with event subscribers in the real-time pipeline. It offers safe unsubscription handling, subscriber identification, metrics collection, and status monitoring capabilities. These methods enable consistent management of different subscriber types while providing type-specific metrics and health indicators.

Example usage:
```csharp
using DotNetRealtimePipeline.Events;
using DotNetRealtimePipeline.Domain.Models;

// Create a processing completion subscriber
var processingSubscriber = new ProcessingCompletionSubscriber("DataProcessingStage");

// Safe unsubscription (handles already unsubscribed subscribers gracefully)
processingSubscriber.SafeUnsubscribe();

// Get subscriber type name for logging
string subscriberType = processingSubscriber.GetSubscriberTypeName();
Console.WriteLine($"Subscriber type: {subscriberType}");

// Access metrics from different subscriber types
var backpressureSubscriber = new BackpressureAlertSubscriber("BufferMonitor");
int backpressureEvents = backpressureSubscriber.GetBackpressureEventCount();

var metricsSubscriber = new MetricsAggregationSubscriber("PerformanceTracker");
double avgProcessingTime = metricsSubscriber.GetAverageProcessingTime();
int metricsCount = metricsSubscriber.GetMetricsCount();

var errorSubscriber = new ErrorAlertSubscriber("ErrorHandler");
int errorCount = errorSubscriber.GetErrorCount();

// Check if subscriber is in critical state
double successRate = processingSubscriber.GetSuccessRatePercent();
bool isCritical = processingSubscriber.IsInCriticalState();

// Get formatted status string for monitoring
string statusString = processingSubscriber.GetStatusString();
Console.WriteLine($"Status: {statusString}");

// Convert collection to read-only
var subscribers = new List<EventSubscriberBase> { processingSubscriber, backpressureSubscriber };
IReadOnlyList<EventSubscriberBase> readOnlySubscribers = subscribers.AsReadOnly();
```

## PipelineEventPublisherExtensions
The `PipelineEventPublisherExtensions` class provides a set of extension methods for pipeline event publishers, simplifying the process of publishing various pipeline events such as data ingestion, processing completion, and error notifications. It also includes utility methods to monitor subscriber status and inspect active subscription counts across the pipeline.

Example usage:
```csharp
using DotNetRealtimePipeline.Events;
using DotNetRealtimePipeline.Domain.Models;

// Assuming 'publisher' is an initialized instance of PipelineEventPublisher
var dataPoint = new DataPoint { /* ... */ };

// Check subscriber status
if (PipelineEventPublisherExtensions.HasSubscribers(publisher, nameof(DataIngestedEvent)))
{
    var counts = publisher.GetAllSubscriberCounts();
    Console.WriteLine($"Subscribers for DataIngested: {counts[nameof(DataIngestedEvent)]}");
}

// Publish events
await publisher.PublishDataIngestedAsync(dataPoint, new Dictionary<string, object> { { "key", "value" } });
await publisher.PublishProcessingCompletedAsync("123", success: true, stageName: "ProcessingStage");
await publisher.PublishBackpressureDetectedAsync("ProcessingStage", 50, 100, true);
await publisher.PublishPipelineErrorAsync("IngestionOperation", new Exception("Something went wrong"));

// Batch processing
await publisher.PublishDataIngestedBatchAsync(new[] { dataPoint });
```

## ApiEndpointHandlerExtensions
The `ApiEndpointHandlerExtensions` class provides helper methods to streamline the creation of structured API responses, including standard successful and error results. It also simplifies the construction of paginated responses and allows for the seamless inclusion of batch processing statistics in the API output.

Example usage:
```csharp
using DotNetRealtimePipeline.API;

// Successful response
var okResponse = ApiEndpointHandlerExtensions.Ok(new { Id = 1, Name = "Success" });

// Error response
var errorResponse = ApiEndpointHandlerExtensions.Error<object>("Failed to process");

// Paginated response
var items = new List<string> { "item1", "item2", "item3" };
var paginated = items.ToPaginatedResponse(page: 1, pageSize: 10, totalCount: 3);

// Accessing pagination metadata
int totalPages = paginated.Data.TotalPages;
int currentPage = paginated.Data.Page;
```

## ApiEndpointHandlerValidation
The `ApiEndpointHandlerValidation` static class provides a set of extension methods for validating common API-related objects within the pipeline, such as `ApiEndpointHandler.ApiResponse<T>`, `BatchIngestResult`, and `PipelineStatusInfo`. It allows for concise validation of these objects using `Validate` to retrieve errors, `IsValid` to check status, or `EnsureValid` to throw an exception upon invalid state.

Example usage:
```csharp
using DotNetRealtimePipeline.API;

// Example using PipelineStatusInfo
var status = new PipelineStatusInfo {
    PipelineName = "MyPipeline",
    Version = "v1.0.0",
    TotalProcessed = 100,
    TotalFailed = 0,
    Pending = 0,
    HealthStatus = "Healthy"
};

// Check if the object is valid
if (status.IsValid())
{
    // Process the status
}
else
{
    // Retrieve validation errors
    var errors = status.Validate();
    Console.WriteLine($"Validation failed: {string.Join(", ", errors)}");
}

// Or, ensure validity by throwing an exception if invalid
status.EnsureValid();
```

## DeadLetterQueueExtensions
The `DeadLetterQueueExtensions` class provides extension methods for working with dead-letter entries in the pipeline. It includes methods for processing failed entries for retry, finding entries by various criteria, and generating comprehensive reports of dead-letter queue state.

Example usage:
```csharp
using DotNetRealtimePipeline.DeadLetter;
using System;
using System.Threading.Tasks;

// Assume queue is an initialized instance of DeadLetterQueue
var queue = new DeadLetterQueue();

// Process entries for retry (up to 50 entries)
var result = await queue.ProcessForRetryAsync(
    maxCount: 50,
    processEntry: async entry => {
        // Your retry logic here
        try {
            // Attempt to reprocess the data point
            await ProcessDataPointAsync(entry.DataPoint);
            return true; // Success
        } catch {
            return false; // Requeue for another attempt
        }
    }
);

Console.WriteLine($"Processed {result.TotalProcessed} entries: " +
            $"{result.SuccessfullyProcessed} succeeded, " +
            $"{result.FailedProcessing} failed");

// Find entries by failure stage
var failedStageEntries = await queue.FindByStageAsync("DataProcessingStage", maxCount: 100);

// Find entries matching a custom predicate
var recentFailures = await queue.FindAsync(
    entry => entry.EnqueuedAt > DateTime.UtcNow.AddHours(-1),
    maxCount: 50
);

// Generate a detailed report
string report = await queue.GetReportAsync(includeDetails: true);
Console.WriteLine(report);

// Access processing result properties
int totalProcessed = result.TotalProcessed;
int successful = result.SuccessfullyProcessed;
int failed = result.FailedProcessing;
IReadOnlyList<DeadLetterEntry> processedEntries = result.EntriesProcessed;
```

## WebhookHandlerExtensions
The `WebhookHandlerExtensions` class provides convenient extension methods for `WebhookHandler` to simplify webhook subscription management, event dispatching, and subscription inspection. It includes methods for subscribing to specific event types, unsubscribing, checking subscription status, managing failed subscriptions, and retrieving subscription statistics.

Example usage:
```csharp
using DotNetRealtimePipeline.Integration;
using System;
using System.Threading.Tasks;

// Assume handler is an initialized instance of WebhookHandler
var handler = new WebhookHandler();

// Subscribe to a single event type
var webhookUrl = "https://api.example.com/webhooks/events";
handler.SubscribeTo(webhookUrl, WebhookEventType.DataIngested);

// Subscribe to multiple event types with a secret for verification
var eventTypes = new[] { WebhookEventType.DataIngested, WebhookEventType.ProcessingCompleted };
handler.SubscribeTo(webhookUrl, eventTypes, "my-secret-key");

// Check if there are active subscriptions for an event type
bool hasSubscribers = handler.HasSubscriptions(WebhookEventType.DataIngested);
Console.WriteLine($"Active subscriptions: {hasSubscribers}");

// Get subscription count for an event type
int subscriptionCount = handler.GetSubscriptionCount(WebhookEventType.DataIngested);
Console.WriteLine($"Subscription count: {subscriptionCount}");

// Get all subscriptions for a specific event type
var subscriptions = handler.GetSubscriptionsFor(WebhookEventType.DataIngested);
foreach (var subscription in subscriptions)
{
    Console.WriteLine($"Subscription: {subscription.Url}, Active: {subscription.IsActive}");
}

// Send a webhook event to all subscribers
await handler.SendWebhookEventAsync(WebhookEventType.DataIngested, new { Message = "Data ingestion completed", Timestamp = DateTime.UtcNow });

// Unsubscribe from all subscriptions matching a URL
bool unsubscribed = handler.UnsubscribeFrom(webhookUrl);
Console.WriteLine($"Unsubscribed: {unsubscribed}");

// Disable subscriptions that have failed too many times
int disabledCount = handler.DisableFailedSubscriptions(failureThreshold: 3);
Console.WriteLine($"Disabled {disabledCount} failed subscriptions");

// Get statistics about subscriptions
var oldestActive = handler.GetOldestActiveSubscription();
var mostRecentDelivery = handler.GetMostRecentDelivery();
```

## WebhookHandlerValidation
The `WebhookHandlerValidation` static class provides validation extension methods for webhook-related components, including `WebhookHandler`, `WebhookSubscription`, and `WebhookPayload`. It allows for concise validation of these components using `Validate` to retrieve errors, `IsValid` to check status, or `EnsureValid` to throw an exception upon invalid state.

Example usage:
```csharp
using DotNetRealtimePipeline.Integration;

// Example using WebhookSubscription
var subscription = new WebhookSubscription {
    Id = Guid.NewGuid().ToString(),
    Url = "https://api.example.com/webhook",
    EventTypes = 1, // Example enum value
    CreatedAt = DateTime.UtcNow
};

// Check if the object is valid
if (subscription.IsValid())
{
    // Process the subscription
}
else
{
    // Retrieve validation errors
    var errors = subscription.Validate();
    Console.WriteLine($"Validation failed: {string.Join(", ", errors)}");
}

// Or, ensure validity by throwing an exception if invalid
subscription.EnsureValid();
```
