# Documentation Index

Index of every page in `docs/`, grouped by area. Each entry links to its page and gives a one-line summary taken from the page's opening text.

## Getting started & guides

- [getting-started.md](getting-started.md) — Setting up and running a first real-time pipeline
- [DEVELOPMENT.md](DEVELOPMENT.md) — Development environment setup and contribution steps
- [EXAMPLES.md](EXAMPLES.md) — Walkthrough of all example programs in the project
- [architecture.md](architecture.md) — Detailed technical overview of the architecture
- [api-reference.md](api-reference.md) — Complete reference for the public API
- [faq.md](faq.md) — Frequently asked questions
- [deployment.md](deployment.md) — Production deployment guide
- [docker-guide.md](docker-guide.md) — Docker usage, environment variables and runtime setup
- [SECURITY.md](SECURITY.md) — Security guidelines for production deployments
- [PERFORMANCE.md](PERFORMANCE.md) — Performance tuning strategies and benchmarking
- [TESTING.md](TESTING.md) — Unit, integration and performance testing guidance
- [backpressure.md](backpressure.md) — What backpressure is and how to choose a strategy
- [RetryPolicy.md](RetryPolicy.md) — Retry policy for transient failures before items are dead-lettered
- [ServiceCollectionExtensions.md](ServiceCollectionExtensions.md) — DI composition root (`AddPipelineServices`) and service registration
- [PipelineConstants.md](PipelineConstants.md) — Reference table of pipeline constants grouped by category
- [MIGRATION_v2.md](MIGRATION_v2.md) — Breaking changes and steps for upgrading from v1.x to v2.0
- [migration-guide-v2.md](migration-guide-v2.md) — Short v2.0 migration notes (default port changed from 5000 to 8080); overlaps with `MIGRATION_v2.md`

## Domain models

- [DataPoint.md](DataPoint.md) — A single telemetry measurement: numeric value, timestamp and metadata
- [PipelineConfig.md](PipelineConfig.md) — Declarative specification of a pipeline: identity, buffering and stages
- [PipelineConfigurationBuilder.md](PipelineConfigurationBuilder.md) — Fluent builder for constructing `PipelineConfig` instances
- [ProcessingResult.md](ProcessingResult.md) — Outcome of a single processing stage
- [StreamEvent.md](StreamEvent.md) — Payload unit carried through pipeline stages, with its own lifecycle tracking
- [WindowEvent.md](WindowEvent.md) — Time-bounded aggregation window that collects `DataPoint` instances
- [ITimestampExtractor.md](ITimestampExtractor.md) — Contract for obtaining event-time timestamps from a `DataPoint` for windowing
- [ScalingDecision.md](ScalingDecision.md) — A single autoscaling determination for a pipeline stage
- [DeadLetterEntry.md](DeadLetterEntry.md) — A failed item stored in the dead-letter queue for inspection
- [MetricAggregation.md](MetricAggregation.md) — Snapshot of aggregated pipeline metrics over a time window
- [BackpressureContext.md](BackpressureContext.md) — Runtime state and metrics of a backpressure-aware pipeline stage
- [BackpressureEvent.md](BackpressureEvent.md) — Snapshot of backpressure diagnostics collected from a stage
- [BackpressureMetricsModels.md](BackpressureMetricsModels.md) — Types for individual backpressure transitions and per-stage and pipeline-wide measurements
- [PipelineVisualizationNode.md](PipelineVisualizationNode.md) — DTO representing one pipeline stage for visualization
- [PipelineEnums.md](PipelineEnums.md) — Enums defined in `PipelineEnums.cs`
- [PipelineException.md](PipelineException.md) — Base exception for pipeline processing errors, with a structured `ErrorCode`

## Services

- [PipelineOrchestrator.md](PipelineOrchestrator.md) — Coordinates the lifecycle and data flow of a pipeline
- [PipelineInitializer.md](PipelineInitializer.md) — Constructs, starts, retries and stops a pipeline
- [PipelineStateManager.md](PipelineStateManager.md) — Explicit pipeline state transitions with history
- [PipelineVisualizer.md](PipelineVisualizer.md) — Generates node-based representations of pipeline structure
- [DataProcessingService.md](DataProcessingService.md) — Processes and analyzes data points in real time
- [WindowingService.md](WindowingService.md) — Time-based and count-based window management over streaming data
- [QueryService.md](QueryService.md) — Temporal and statistical queries over pipeline data
- [MetricsService.md](MetricsService.md) — Real-time monitoring of throughput, processing latency and error rates
- [HealthCheckService.md](HealthCheckService.md) — Health checks for pipeline components and system-wide health status
- [BackpressureService.md](BackpressureService.md) — Controls data flow to prevent buffer overflows
- [BackgroundProcessingWorker.md](BackgroundProcessingWorker.md) — Long-lived worker that manages background processing tasks
- [DynamicScalingService.md](DynamicScalingService.md) — Evaluates and manages the scaling state of processing stages
- [DynamicScalingWorker.md](DynamicScalingWorker.md) — Background service that adjusts concurrency based on workload pressure
- [DeadLetterQueue.md](DeadLetterQueue.md) — Durable holding area for messages that failed processing
- [IDeadLetterQueue.md](IDeadLetterQueue.md) — Abstraction that lets dead-letter queue implementations be swapped
- [ExportService.md](ExportService.md) — Exports data points, results and metrics from a pipeline

## Events

- [PipelineEventPublisher.md](PipelineEventPublisher.md) — Central hub that broadcasts pipeline lifecycle events
- [EventSubscriber.md](EventSubscriber.md) — Subscriber-side building blocks of the event system
- [EventSubscriberBase.md](EventSubscriberBase.md) — Base class for event subscribers, covering the subscription lifecycle
- [EventServiceConfiguration.md](EventServiceConfiguration.md) — Methods and properties for configuring event services
- [SubscriberChannel.md](SubscriberChannel.md) — Internal per-subscriber bounded channel that dispatches events
- [SubscriberOptions.md](SubscriberOptions.md) — Public options for subscriber queue size and behavior

## Integration

- [ExternalDataSource.md](ExternalDataSource.md) — Common async contract and HTTP implementation for external data sources
- [IExternalDataSource.md](IExternalDataSource.md) — Standardized interface for interacting with external data sources
- [HttpClientFactory.md](HttpClientFactory.md) — `PipelineHttpClientFactory` and its named-client builder
- [PipelineHttpClientFactory.md](PipelineHttpClientFactory.md) — Creation, configuration and lifecycle of pre-configured `HttpClient` instances
- [MetricsExporter.md](MetricsExporter.md) — Exporters that send `MetricAggregation` snapshots to external systems
- [IMetricsExporter.md](IMetricsExporter.md) — Contract for exporting metrics to monitoring systems
- [IDataPointRepository.md](IDataPointRepository.md) — Async persistence contract for `DataPoint` instances
- [InMemoryDataPointRepository.md](InMemoryDataPointRepository.md) — Thread-safe in-memory implementation of `IDataPointRepository`
- [IMetricsRepository.md](IMetricsRepository.md) — Async persistence and query contract for `MetricAggregation` records
- [InMemoryMetricsRepository.md](InMemoryMetricsRepository.md) — In-memory implementation of `IMetricsRepository`
- [IOutputFormatter.md](IOutputFormatter.md) — Contract for formatting output values into strings
- [OutputFormatter.md](OutputFormatter.md) — JSON, CSV, plain-text table and HTML output formatters
- [IRawPipelineAccess.md](IRawPipelineAccess.md) — Opt-in contract that exposes the underlying `PipeReader` and `PipeWriter`
- [RawPipelineAccessor.md](RawPipelineAccessor.md) — Lightweight wrapper around a duplex pipe with direct reader/writer access

## API, CLI, middleware & plugins

- [ApiEndpointHandler.md](ApiEndpointHandler.md) — Structured interface exposing pipeline operations as endpoints
- [RestApiHandler.md](RestApiHandler.md) — REST API handlers defined in `RestApiHandler.cs`
- [WebhookHandler.md](WebhookHandler.md) — Lifecycle of a webhook subscription
- [CommandExecutor.md](CommandExecutor.md) — Async facade for executing pipeline commands, ingesting and querying data, and exporting results
- [CommandLineParser.md](CommandLineParser.md) — Parsing and validation of command-line arguments for pipeline commands
- [ErrorHandlingMiddleware.md](ErrorHandlingMiddleware.md) — Error-handling middleware page (file currently holds an unfinished generation prompt, not finished docs)
- [LoggingMiddleware.md](LoggingMiddleware.md) — Instruments ingestion, processing and backpressure events with structured logging
- [RateLimitingMiddleware.md](RateLimitingMiddleware.md) — Token-bucket rate limiting with configurable capacity and refill interval
- [IPipelinePlugin.md](IPipelinePlugin.md) — Contract for plugins, including lifecycle management
- [ExtensionSystem.md](ExtensionSystem.md) — Plugin and hook APIs defined in `ExtensionSystem.cs`
- [PluginManager.md](PluginManager.md) — `PluginManager`, `PipelinePluginBase` and `LoggingPlugin`

## Metrics & health

- [IPipelineMetrics.md](IPipelineMetrics.md) — Contract for recording event counts and reading throughput in events per second
- [ThroughputCounter.md](ThroughputCounter.md) — Thread-safe counter that measures event processing rate
- [BackpressureMetricsCollector.md](BackpressureMetricsCollector.md) — Tracks backpressure events and buffer utilization for each stage

## Utilities

- [BatchProcessor.md](BatchProcessor.md) — Partitions input into batches and processes them asynchronously with progress tracking
- [CacheService.md](CacheService.md) — High-performance in-memory cache
- [CompressionHelper.md](CompressionHelper.md) — Gzip compression and decompression helpers
- [DateTimeExtensions.md](DateTimeExtensions.md) — Static date and time helpers for pipeline code
- [PathHelper.md](PathHelper.md) — Static file system path manipulation, validation and discovery helpers
- [PerformanceHelper.md](PerformanceHelper.md) — Measures execution time of code blocks and benchmarks operations
- [PipelineJsonUtilities.md](PipelineJsonUtilities.md) — Shared JSON serializer options and helpers
- [PipelineBenchmarks.md](PipelineBenchmarks.md) — Benchmarking utilities for evaluating pipeline performance
- [RetryHelper.md](RetryHelper.md) — Configurable retry policies for operations that fail on transient errors
- [SerializationHelper.md](SerializationHelper.md) — Serializes and deserializes `DataPoint` objects to and from JSON, CSV and files
- [SlidingWindowAggregator.md](SlidingWindowAggregator.md) — Incremental aggregation of numeric values over a sliding time window
- [StatisticsHelper.md](StatisticsHelper.md) — Descriptive statistics, time-series transforms and outlier detection
- [ValidationHelper.md](ValidationHelper.md) — Centralized validation of data integrity, configuration and processing results

## Tests

- [DataPointTests.md](DataPointTests.md) — Unit tests for the `DataPoint` type
- [DataPointRepositoryTests.md](DataPointRepositoryTests.md) — Tests for the data point repository's add, read and delete behavior
- [DataProcessingServiceTests.md](DataProcessingServiceTests.md) — Test suite for the core data processing logic
- [DataProcessingServiceTestsExtensions.md](DataProcessingServiceTestsExtensions.md) — Test-data factory helpers for `DataProcessingService` tests
- [BackpressureServiceTests.md](BackpressureServiceTests.md) — Tests for the backpressure service
- [BackpressureServiceTestsExtensions.md](BackpressureServiceTestsExtensions.md) — Test helpers for backpressure service tests
- [BackpressureMetricsCollectorTests.md](BackpressureMetricsCollectorTests.md) — Tests for `BackpressureMetricsCollector` metric tracking and aggregation
- [DeadLetterQueueTests.md](DeadLetterQueueTests.md) — Unit tests for the dead-letter queue
- [MetricAggregationTests.md](MetricAggregationTests.md) — Tests for metric aggregation calculations
- [MetricAggregationTestsExtensions.md](MetricAggregationTestsExtensions.md) — Factory and fluent assertion helpers for `MetricAggregation` tests
- [MetricsServiceTests.md](MetricsServiceTests.md) — Test suite for `MetricsService`
- [MetricsServiceTestsExtensions.md](MetricsServiceTestsExtensions.md) — Arrange and assert helpers for `MetricsService` tests
- [PipelineIntegrationTests.md](PipelineIntegrationTests.md) — Integration test suite for the pipeline
- [PipelineVisualizerTests.md](PipelineVisualizerTests.md) — Tests for visualization logic, rendering output and health status
- [PipelineVisualizerTestsExtensions.md](PipelineVisualizerTestsExtensions.md) — Test helpers for pipeline visualizer tests
- [WindowingServiceTests.md](WindowingServiceTests.md) — Tests for `WindowingService` window management

## Extensions & validation companions

### Extension methods

- [ApiEndpointHandlerExtensions.md](ApiEndpointHandlerExtensions.md) — Helpers for building strongly typed API responses
- [BackpressureContextExtensions.md](BackpressureContextExtensions.md) — Helpers for monitoring and reacting to buffer capacity
- [BackpressureServiceExtensions.md](BackpressureServiceExtensions.md) — Extensions for configuring and monitoring backpressure at runtime
- [BatchProcessorExtensions.md](BatchProcessorExtensions.md) — Extensions for batching, processing and analyzing sequences
- [CacheServiceExtensions.md](CacheServiceExtensions.md) — Convenience operations on cache services
- [CommandExecutorExtensions.md](CommandExecutorExtensions.md) — Async helpers for interacting with the command executor
- [DataPointExtensions.md](DataPointExtensions.md) — Fluent updates, metadata access, logging and staleness detection for `DataPoint`
- [DataProcessingServiceExtensions.md](DataProcessingServiceExtensions.md) — Batch quality filtering, quality reporting and processing statistics
- [DeadLetterEntryExtensions.md](DeadLetterEntryExtensions.md) — Helpers for dead-letter entries, including path sanitization
- [DeadLetterQueueExtensions.md](DeadLetterQueueExtensions.md) — Querying, reprocessing and reporting on dead-lettered messages
- [DynamicScalingServiceExtensions.md](DynamicScalingServiceExtensions.md) — Extensions and state-access utilities for dynamic scaling
- [EventSubscriberBaseExtensions.md](EventSubscriberBaseExtensions.md) — Safe unsubscription helpers for subscribers and collections of subscribers
- [ExportServiceExtensions.md](ExportServiceExtensions.md) — Output validation, retry, stream export and size estimation for `ExportService`
- [HealthCheckServiceExtensions.md](HealthCheckServiceExtensions.md) — Registration helpers and focused query methods for `HealthCheckService`
- [InMemoryMetricsRepositoryExtensions.md](InMemoryMetricsRepositoryExtensions.md) — Query helpers for the in-memory metrics repository
- [MetricAggregationExtensions.md](MetricAggregationExtensions.md) — Aggregation, filtering and summary statistics for `MetricAggregation`
- [MetricsServiceExtensions.md](MetricsServiceExtensions.md) — Overloads and reporting helpers for `MetricsService`
- [PerformanceHelperExtensions.md](PerformanceHelperExtensions.md) — Benchmarking helpers for synchronous and asynchronous code
- [PipelineConfigExtensions.md](PipelineConfigExtensions.md) — Querying and retrieving stage definitions from `PipelineConfig`
- [PipelineEventPublisherExtensions.md](PipelineEventPublisherExtensions.md) — Event broadcasting helpers for `PipelineEventPublisher`
- [PipelineHttpClientFactoryExtensions.md](PipelineHttpClientFactoryExtensions.md) — Factory methods for pre-configured `HttpClient` instances
- [PipelineInitializerExtensions.md](PipelineInitializerExtensions.md) — Convenience methods for initializing, starting, retrying and stopping a pipeline
- [PipelineOrchestratorExtensions.md](PipelineOrchestratorExtensions.md) — Reporting and interaction helpers for `PipelineOrchestrator`
- [PipelineStateManagerExtensions.md](PipelineStateManagerExtensions.md) — History queries and formatting helpers for `PipelineStateManager`
- [ProcessingResultExtensions.md](ProcessingResultExtensions.md) — Inspecting, combining, copying and serializing `ProcessingResult`
- [QueryServiceExtensions.md](QueryServiceExtensions.md) — UTC date-range, predicate-based and recent-metric query overloads for `QueryService`
- [SlidingWindowAggregatorExtensions.md](SlidingWindowAggregatorExtensions.md) — Helpers for adding data points to a `SlidingWindowAggregator`
- [StreamEventExtensions.md](StreamEventExtensions.md) — Inspecting, transforming and extracting data from `StreamEvent`
- [WindowEventExtensions.md](WindowEventExtensions.md) — Analysis and formatting helpers for `WindowEvent`
- [WindowingServiceExtensions.md](WindowingServiceExtensions.md) — Lifecycle, state and statistics helpers for `WindowingService`

### Validation

- [ApiEndpointHandlerValidation.md](ApiEndpointHandlerValidation.md) — Validates an instance against endpoint handler rules
- [BackgroundProcessingWorkerValidation.md](BackgroundProcessingWorkerValidation.md) — Validates background worker configuration and state
- [BackpressureMetricsCollectorValidation.md](BackpressureMetricsCollectorValidation.md) — Validates `BackpressureMetricsCollector` configurations
- [BatchProcessorValidation.md](BatchProcessorValidation.md) — Validates batch size, concurrency and timeout settings
- [CompressionHelperValidation.md](CompressionHelperValidation.md) — Validates compression helper inputs and state
- [DateTimeExtensionsValidation.md](DateTimeExtensionsValidation.md) — Validates date and time values used across the pipeline
- [ExportServiceValidation.md](ExportServiceValidation.md) — Validates export service configuration and runtime state
- [HealthCheckServiceValidation.md](HealthCheckServiceValidation.md) — Validates registered health checks against required criteria
- [LoggingMiddlewareValidation.md](LoggingMiddlewareValidation.md) — Validates logging middleware configuration
- [PerformanceHelperValidation.md](PerformanceHelperValidation.md) — Validates performance-related data
- [RateLimitingMiddlewareValidation.md](RateLimitingMiddlewareValidation.md) — Validates rate-limiting middleware parameters and state
- [RetryHelperValidation.md](RetryHelperValidation.md) — Validates retry configuration objects
- [SerializationHelperValidation.md](SerializationHelperValidation.md) — Validates serialized data structures
- [ValidationHelperValidation.md](ValidationHelperValidation.md) — Validates data and enforces validation rules
