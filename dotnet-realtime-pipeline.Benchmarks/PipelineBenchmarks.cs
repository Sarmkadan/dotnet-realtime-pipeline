using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Events;
using DotNetRealtimePipeline.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace DotNetRealtimePipeline.Benchmarks;

/// <summary>
/// Performance benchmarks for the dotnet-realtime-pipeline library.
/// Measures throughput and memory allocation for critical pipeline operations.
/// </summary>
[MemoryDiagnoser]
[HideColumns("Job", "RatioSD", "AllocRatio")]
[RankColumn]
public class PipelineBenchmarks
{
    private ServiceProvider? _serviceProvider;
    private PipelineOrchestrator? _orchestrator;
    private DataProcessingService? _processingService;
    private WindowingService? _windowingService;
    private MetricsService? _metricsService;
    private BackpressureService? _backpressureService;
    private PipelineConfig? _config;

    // Metrics aggregation subscriber for lock-free benchmarking
    private MetricsAggregationSubscriber? _metricsAggregationSubscriber;
    private Mock<ILogger<MetricsAggregationSubscriber>>? _mockLogger;
    private ServiceProvider? _serviceProvider;
    private PipelineOrchestrator? _orchestrator;
    private DataProcessingService? _processingService;
    private WindowingService? _windowingService;
    private MetricsService? _metricsService;
    private BackpressureService? _backpressureService;
    private PipelineConfig? _config;

    [GlobalSetup]
    public void Setup()
    {
        // Create a fresh service provider for each benchmark
        var services = new ServiceCollection();

        // Configure pipeline with high-performance settings
        _config = new PipelineConfig
        {
            PipelineName = "BenchmarkPipeline",
            Version = "1.0.0",
            MaxBufferSize = 100_000,
            MaxConcurrentConsumers = Environment.ProcessorCount,
            BufferFlushIntervalMs = 250,
            WindowSizeMs = 5_000,
            WindowSlideMs = 1_000,
            WindowType = Domain.Enums.WindowType.TUMBLING,
            BackpressureStrategy = Domain.Enums.BackpressureStrategy.Block,
            BackpressureThreshold = 0.9m,
            EnableMetrics = true,
            MetricsHistorySize = 1_000,
            ValidateOnIngestion = false,
            MinDataQualityThreshold = 0.0m,
            EnableQualityAnalysis = false
        };

        services.AddSingleton(_config);

        // Add required services
        services.AddSingleton<DataProcessingService>();
        services.AddSingleton<WindowingService>();
        services.AddSingleton<MetricsService>();
        services.AddSingleton<BackpressureService>();
        services.AddSingleton<PipelineOrchestrator>();

        // Use in-memory repository for benchmarks
        services.AddSingleton<Domain.Repositories.IDataPointRepository,
            Data.Repositories.InMemoryDataPointRepository>();
        services.AddSingleton<Domain.Repositories.IMetricsRepository,
            Data.Repositories.InMemoryMetricsRepository>();

        _serviceProvider = services.BuildServiceProvider();

        _processingService = _serviceProvider.GetRequiredService<DataProcessingService>();
        _windowingService = _serviceProvider.GetRequiredService<WindowingService>();
        _metricsService = _serviceProvider.GetRequiredService<MetricsService>();
        _backpressureService = _serviceProvider.GetRequiredService<BackpressureService>();
        _orchestrator = _serviceProvider.GetRequiredService<PipelineOrchestrator>();

        // Start the orchestrator
        _orchestrator.StartAsync().GetAwaiter().GetResult();
    }

    [GlobalSetup(Target = nameof(MetricsAggregationHotPath))]
    public void SetupMetricsAggregation()
    {
        var loggerFactory = new Mock<ILoggerFactory>();
        var mockLogger = new Mock<ILogger<MetricsAggregationSubscriber>>();
        loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

        _mockLogger = mockLogger;

        var publisher = new PipelineEventPublisher(mockLogger.Object);
        _metricsAggregationSubscriber = new MetricsAggregationSubscriber(publisher, mockLogger.Object);
        _metricsAggregationSubscriber.Subscribe();
    }

    [GlobalCleanup(Target = nameof(MetricsAggregationHotPath))]
    public void CleanupMetricsAggregation()
    {
        _metricsAggregationSubscriber = null;
        _mockLogger = null;
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _serviceProvider?.Dispose();
    }

    /// <summary>
    /// Benchmark: Single data point ingestion through PipelineOrchestrator
    /// Measures the end-to-end latency and throughput for individual data point ingestion.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Ingestion")]
    public async Task IngestSingleDataPoint()
    {
        var dataPoint = new DataPoint(
            id: 1,
            timestamp: DateTime.UtcNow.Ticks,
            value: 42.5,
            source: "BenchmarkSensor"
        );

        await _orchestrator!.IngestDataPointAsync(dataPoint);
    }

    /// <summary>
    /// Benchmark: Batch data point processing through DataProcessingService
    /// Measures throughput for batch processing with varying batch sizes.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Processing")]
    [Arguments(100)]
    [Arguments(1_000)]
    [Arguments(10_000)]
    public async Task ProcessBatch(int batchSize)
    {
        var dataPoints = new List<DataPoint>();
        var now = DateTime.UtcNow.Ticks;

        for (int i = 0; i < batchSize; i++)
        {
            dataPoints.Add(new DataPoint(
                id: i,
                timestamp: now + i * 100,
                value: i * 1.5,
                source: "BenchmarkSensor"
            ));
        }

        await _processingService!.ProcessBatchAsync(dataPoints);
    }

    /// <summary>
    /// Benchmark: Window assignment and statistics calculation
    /// Measures the performance of windowing operations with varying data sizes.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Windowing")]
    [Arguments(100)]
    [Arguments(1_000)]
    [Arguments(10_000)]
    public void ProcessDataPointsThroughWindowing(int dataPointCount)
    {
        var dataPoints = new List<DataPoint>();
        var now = DateTime.UtcNow.Ticks;

        for (int i = 0; i < dataPointCount; i++)
        {
            dataPoints.Add(new DataPoint(
                id: i,
                timestamp: now + i * 100,
                value: i * 1.5,
                source: "BenchmarkSensor"
            ));
        }

        _windowingService!.ProcessDataPoints(dataPoints);
    }

    /// <summary>
    /// Benchmark: Health report generation
    /// Measures the overhead of generating comprehensive health reports.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Monitoring")]
    public async Task GenerateHealthReport()
    {
        await _metricsService!.GenerateHealthReportAsync();
    }

    /// <summary>
    /// Benchmark: Backpressure management operations
    /// Measures the performance of buffer management and backpressure strategies.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Backpressure")]
    public void BackpressureBufferOperations()
    {
        // Test buffer capacity checks
        _backpressureService!.TryAddToBuffer("TestStage", 100);

        // Test buffer removal
        _backpressureService!.RemoveFromBuffer("TestStage", 50);

        // Test system status
        _backpressureService!.GetSystemStatus();
    }

    /// <summary>
    /// Benchmark: End-to-end pipeline throughput
    /// Measures the maximum sustainable throughput of the entire pipeline.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Throughput")]
    public async Task EndToEndThroughput()
    {
        const int totalItems = 50_000;
        const int batchSize = 1_000;
        var dataPoints = new List<DataPoint>();
        var now = DateTime.UtcNow.Ticks;

        // Pre-generate data points
        for (int i = 0; i < totalItems; i++)
        {
            dataPoints.Add(new DataPoint(
                id: i,
                timestamp: now + i * 100,
                value: i * 1.5,
                source: "ThroughputSensor"
            ));
        }

        // Ingest in batches
        for (int i = 0; i < totalItems; i += batchSize)
        {
            var batch = dataPoints.Skip(i).Take(batchSize).ToList();
            foreach (var point in batch)
            {
                await _orchestrator!.IngestDataPointAsync(point);
            }
        }

        // Allow processing to complete
        await Task.Delay(500);
    }

    /// <summary>
    /// Benchmark: Memory allocation for data point processing
    /// Measures memory allocations during batch processing operations.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Memory")]
    public async Task MemoryAllocationBenchmark()
    {
        var dataPoints = new List<DataPoint>();

        for (int i = 0; i < 10_000; i++)
        {
            dataPoints.Add(new DataPoint(
                id: i,
                timestamp: DateTime.UtcNow.Ticks,
                value: i * 1.5,
                source: "MemoryTestSensor"
            ));
        }

        await _processingService!.ProcessBatchAsync(dataPoints);
    }

    /// <summary>
    /// Benchmark: Metrics aggregation hot path - lock-free implementation
    /// Measures events/sec and bytes allocated per event for the optimized MetricsAggregationSubscriber.
    /// Target: Zero allocations per event for counter/gauge updates.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("MetricsAggregation")]
    public async Task MetricsAggregationHotPath()
    {
        // Create a metric aggregation similar to what would be published in real usage
        var metrics = new MetricAggregation(
            metricId: 1,
            startMs: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            endMs: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 1000,
            metricType: "throughput"
        );
        metrics.TotalItemsProcessed = 1000;
        metrics.AverageProcessingTimeMs = 42.5;

        // Publish metrics event - this is the hot path we're optimizing
        var eventArgs = new MetricsCollectedEventArgs
        {
            Metrics = metrics,
            Timestamp = DateTime.UtcNow
        };

        // Simulate high-frequency event publishing
        for (int i = 0; i < 10_000; i++)
        {
            await _metricsAggregationSubscriber!.OnMetricsCollectedAsync(eventArgs);
        }

        // Force snapshot to measure final state
        _ = _metricsAggregationSubscriber!.GetAverageProcessingTime();
    }

    /// <summary>
    /// Benchmark: Metrics aggregation allocation audit
    /// Measures bytes allocated per event to verify zero-allocation goal.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("AllocationAudit")]
    [MemoryDiagnoser]
    public async Task MetricsAggregationAllocationAudit()
    {
        // Create fresh subscriber for each iteration to measure allocations
        var loggerFactory = new Mock<ILoggerFactory>();
        var mockLogger = new Mock<ILogger<MetricsAggregationSubscriber>>();
        loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

        var publisher = new PipelineEventPublisher(mockLogger.Object);
        var subscriber = new MetricsAggregationSubscriber(publisher, mockLogger.Object);
        subscriber.Subscribe();

        var metrics = new MetricAggregation(
            metricId: 1,
            startMs: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            endMs: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 1000,
            metricType: "throughput"
        );
        metrics.TotalItemsProcessed = 1000;
        metrics.AverageProcessingTimeMs = 42.5;

        var eventArgs = new MetricsCollectedEventArgs
        {
            Metrics = metrics,
            Timestamp = DateTime.UtcNow
        };

        // Process many events to get stable allocation measurement
        for (int i = 0; i < 100_000; i++)
        {
            await subscriber.OnMetricsCollectedAsync(eventArgs);
        }

        // Get final metrics
        _ = subscriber.GetAverageProcessingTime();
        _ = subscriber.GetMetricsCount();
    }

    /// <summary>
    /// Benchmark: Metrics aggregation snapshot performance
    /// Measures the cost of taking consistent snapshots from striped counters.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("MetricsAggregation")]
    public void MetricsAggregationSnapshot()
    {
        // Trigger snapshot operations
        _ = _metricsAggregationSubscriber!.GetAverageProcessingTime();
        _ = _metricsAggregationSubscriber.GetMetricsCount();
        _metricsAggregationSubscriber.Reset();
    }
}