# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.2] - 2026-05-20
### Fixed
- Fix backpressure not propagating to source when intermediate stage is full
- Added regression test for the fix

## [2.0.1]
### Security
- Added input validation and length limits
- Added request timeout configuration
- Added security policy and vulnerability reporting

## [2.0.0] - 2026-03-19

### Added
- Multi-stage Dockerfile with named build stages (`build`, `final`) and non-root user
- Docker Compose stack with Prometheus, Grafana, and PostgreSQL services
- `HEALTHCHECK` instruction in Dockerfile (wget-based, 30s interval)
- Migration guide (`docs/MIGRATION_v2.md`) covering all breaking changes

### Changed
- **BREAKING:** Default application port changed from 5000 to 8080
- Dockerfile now sets `ASPNETCORE_URLS=http://+:8080` and `UseAppHost=false`
- Docker Compose `resources` block moved under `deploy` per v3 spec
- Updated all health check endpoints to port 8080
- Bumped version labels and package version to 2.0.0

## [1.0.0] - 2025-09-22

### Added
- Real-time data processing pipeline framework for .NET 10
- Four window types: Tumbling, Sliding, Session, Global
- Backpressure management with three strategies: Block, Throttle, Drop
- Comprehensive metrics collection and health reporting
- Thread-safe in-memory repositories (`InMemoryDataPointRepository`, `InMemoryMetricsRepository`)
- Data point validation and quality assessment (0–100 score)
- Time-window aggregation with full statistical output (sum, avg, min, max, stddev, percentiles)
- Configurable service registration via `IServiceCollection` extension methods
- Error handling and retry logic in `DataProcessingService`
- REST API handler and webhook support
- CLI command executor with argument parser
- Plugin/extension system for custom processing stages
- Event publishing and subscriber system
- `PipelineConfigurationBuilder` fluent API
- Full documentation and usage examples
- Docker and Docker Compose support

### Changed
- Stabilised all public APIs ahead of v1.0 release
- Improved buffer flush reliability under concurrent load
- Tightened error messages with structured context

### Fixed
- Race condition in `PipelineStateManager` during shutdown
- Incorrect percentile calculation for windows with fewer than 4 data points
- Backpressure threshold not triggering at exact configured value

## [0.9.0] - 2025-08-25

### Added
- `DynamicScalingService` and `DynamicScalingWorker` for automatic consumer scaling
- `PerformanceHelper` utilities for high-resolution timing
- `StatisticsHelper` with Z-score outlier detection and IQR method
- Native AOT publish profile

### Changed
- `BackgroundProcessingWorker` now honours cancellation tokens consistently
- Reduced lock contention in `InMemoryMetricsRepository` by switching to `ReaderWriterLockSlim`

### Performance
- Batch ingestion throughput increased to ~85,000 events/sec with 16 consumers
- P99 end-to-end latency reduced to under 8 ms

## [0.8.0] - 2025-08-04

### Added
- Integration test project (`tests/Integration/PipelineIntegrationTests.cs`)
- Additional unit tests for `BackpressureService`, `DataPointRepository`, `WindowingService`
- Code coverage configuration via `coverlet`
- `ValidationHelper` with comprehensive data-point rules

### Fixed
- `WindowingService` returning stale window references after flush
- Memory growth in `MetricsService` when `MetricsHistorySize` was unbounded

## [0.7.0] - 2025-07-14

### Added
- `QueryService` with time-range search, aggregate statistics, and trend analysis
- `ExportService` supporting JSON and CSV output formats
- `CompressionHelper` for payload size reduction
- `SerializationHelper` wrapping `System.Text.Json`
- `RetryHelper` with exponential back-off

### Changed
- Repository interfaces extended with `GetByTimeRangeAsync` and `GetBySourceAsync`

## [0.6.0] - 2025-06-23

### Added
- `MetricsExporter` for Prometheus-compatible output
- `ExternalDataSource` integration wrapper with HTTP polling
- `WebhookHandler` for inbound push integrations
- `HttpClientFactory` with retry and timeout policies
- `MonitoringHealthCheckService` reporting pipeline sub-system status

### Changed
- `MetricsService` now exposes moving-average trend direction

## [0.5.0] - 2025-06-02

### Added
- `PipelineEventPublisher` and `EventSubscriber` for internal pub/sub
- `CacheService` for short-lived query result caching
- `RateLimitingMiddleware` and `LoggingMiddleware`
- `ErrorHandlingMiddleware` with structured exception responses
- `OutputFormatter` supporting plain text and JSON rendering

### Fixed
- Duplicate data points accepted when IDs collided across sources

## [0.4.0] - 2025-05-12

### Added
- `BackpressureService` with Block, Throttle, and Drop strategies
- `BackpressureContext` and `ScalingDecision` domain models
- Configurable pressure thresholds via `PipelineConfig.BackpressureThreshold`
- `BackgroundProcessingWorker` for continuous buffer draining

### Changed
- `PipelineOrchestrator` now reports backpressure events through the event system

## [0.3.0] - 2025-04-21

### Added
- `WindowingService` with Tumbling and Sliding window assignment
- Session window support with configurable idle timeout
- Global window for unbounded aggregation
- `WindowEvent` and `WindowStatistics` domain models
- `MetricsService` with throughput and latency tracking

### Changed
- `DataProcessingService` now routes validated points into windowing stage

## [0.2.0] - 2025-03-31

### Added
- `DataProcessingService` with single-item and batch processing
- `PipelineOrchestrator` lifecycle management (`StartAsync` / `StopAsync`)
- `InMemoryDataPointRepository` and `InMemoryMetricsRepository`
- `PipelineConfig` with buffer, windowing, and quality settings
- `PipelineConstants` centralising magic values
- `DateTimeExtensions` and `PathHelper` utilities
- `ServiceCollectionExtensions.AddPipelineServices()` convenience method

### Fixed
- `NullReferenceException` when ingesting a data point before `StartAsync`

## [0.1.0] - 2025-03-10

### Added
- Initial project scaffolding targeting .NET 10
- Core domain models: `DataPoint`, `ProcessingResult`, `MetricAggregation`, `StreamEvent`
- Domain enums: `WindowType`, `BackpressureStrategy`, `HealthStatus`
- `PipelineException` hierarchy for typed error handling
- Solution structure: `src/`, `tests/`, `docs/`, `examples/`
- `.editorconfig`, `.gitignore`, `Makefile`, GitHub Actions build workflow
- README and documentation framework
