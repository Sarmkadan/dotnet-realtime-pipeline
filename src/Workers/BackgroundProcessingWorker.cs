#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Workers;

using DotNetRealtimePipeline.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Background worker for continuous pipeline processing.
/// Manages long-running data processing tasks with graceful shutdown.
/// </summary>
public sealed class BackgroundProcessingWorker : IDisposable
{
    private readonly PipelineOrchestrator _orchestrator;
    private readonly ILogger<BackgroundProcessingWorker> _logger;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private Task _workerTask;
    private volatile bool _isRunning;

    public BackgroundProcessingWorker(
        PipelineOrchestrator orchestrator,
        ILogger<BackgroundProcessingWorker> logger)
    {
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cancellationTokenSource = new CancellationTokenSource();
    }

    /// <summary>
    /// Starts the background worker.
    /// </summary>
    public void Start()
    {
        if (_isRunning)
        {
            _logger.LogWarning("Background worker is already running");
            return;
        }

        _isRunning = true;
        _logger.LogInformation("Starting background processing worker");

        _workerTask = ProcessAsync(_cancellationTokenSource.Token);
    }

    /// <summary>
    /// Stops the background worker gracefully.
    /// </summary>
    public async Task StopAsync()
    {
        if (!_isRunning)
        {
            _logger.LogWarning("Background worker is not running");
            return;
        }

        _logger.LogInformation("Stopping background processing worker");
        _cancellationTokenSource.Cancel();

        if (_workerTask is not null)
        {
            try
            {
                await _workerTask;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Background worker stopped");
            }
        }

        _isRunning = false;
    }

    /// <summary>
    /// Gets the current running state.
    /// </summary>
    public bool IsRunning => _isRunning;

    /// <summary>
    /// Performs background processing.
    /// </summary>
    private async Task ProcessAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Get current pipeline status
                    var status = _orchestrator.GetStatus();

                    if (status.PendingItemsInQueue > 0)
                    {
                        _logger.LogDebug("Background worker: Processing {Count} pending items",
                            status.PendingItemsInQueue);
                    }

                    // Allow other tasks to run
                    await Task.Delay(100, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in background processing loop");
                    await Task.Delay(1000, cancellationToken);
                }
            }
        }
        finally
        {
            _isRunning = false;
        }
    }

    public void Dispose()
    {
        if (_isRunning)
        {
            _cancellationTokenSource.Cancel();

            try
            {
                _workerTask?.GetAwaiter().GetResult();
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown.
            }

            _isRunning = false;
        }

        _cancellationTokenSource.Dispose();
    }
}

/// <summary>
/// Background worker for periodic metrics aggregation.
/// </summary>
public sealed class MetricsAggregationWorker : IDisposable
{
    private readonly MetricsService _metricsService;
    private readonly ILogger<MetricsAggregationWorker> _logger;
    private readonly int _intervalMs;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private Task _workerTask;
    private volatile bool _isRunning;

    public MetricsAggregationWorker(
        MetricsService metricsService,
        ILogger<MetricsAggregationWorker> logger,
        int intervalMs = 5000)
    {
        _metricsService = metricsService ?? throw new ArgumentNullException(nameof(metricsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _intervalMs = intervalMs;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    /// <summary>
    /// Starts the metrics aggregation worker.
    /// </summary>
    public void Start()
    {
        if (_isRunning)
        {
            _logger.LogWarning("Metrics aggregation worker is already running");
            return;
        }

        _isRunning = true;
        _logger.LogInformation("Starting metrics aggregation worker (interval: {Interval}ms)", _intervalMs);

        _workerTask = AggregateAsync(_cancellationTokenSource.Token);
    }

    /// <summary>
    /// Stops the metrics aggregation worker.
    /// </summary>
    public async Task StopAsync()
    {
        if (!_isRunning)
        {
            _logger.LogWarning("Metrics aggregation worker is not running");
            return;
        }

        _logger.LogInformation("Stopping metrics aggregation worker");
        _cancellationTokenSource.Cancel();

        if (_workerTask is not null)
        {
            try
            {
                await _workerTask;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Metrics aggregation worker stopped");
            }
        }

        _isRunning = false;
    }

    public bool IsRunning => _isRunning;

    /// <summary>
    /// Performs periodic metrics aggregation.
    /// </summary>
    private async Task AggregateAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var report = await _metricsService.GenerateHealthReportAsync();

                    _logger.LogInformation(
                        "Metrics aggregated - status: {Status}, processed: {Processed}, failed: {Failed}, throughput: {Throughput:F2}/s, avg: {Avg:F2}ms, p95: {P95:F2}ms",
                        report.Status,
                        report.TotalProcessed,
                        report.TotalFailed,
                        report.ThroughputItemsPerSecond,
                        report.AverageProcessingTimeMs,
                        report.P95ProcessingTimeMs);

                    await Task.Delay(_intervalMs, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during metrics aggregation");
                }
            }
        }
        finally
        {
            _isRunning = false;
        }
    }

    public void Dispose()
    {
        if (_isRunning)
        {
            _cancellationTokenSource.Cancel();

            try
            {
                _workerTask?.GetAwaiter().GetResult();
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown.
            }

            _isRunning = false;
        }

        _cancellationTokenSource.Dispose();
    }
}

/// <summary>
/// Background worker for periodic health checks.
/// </summary>
public sealed class HealthCheckWorker : IDisposable
{
    private readonly PipelineOrchestrator _orchestrator;
    private readonly ILogger<HealthCheckWorker> _logger;
    private readonly int _intervalMs;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private Task _workerTask;
    private volatile bool _isRunning;

    public HealthCheckWorker(
        PipelineOrchestrator orchestrator,
        ILogger<HealthCheckWorker> logger,
        int intervalMs = 10000)
    {
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _intervalMs = intervalMs;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    /// <summary>
    /// Starts the health check worker.
    /// </summary>
    public void Start()
    {
        if (_isRunning)
        {
            _logger.LogWarning("Health check worker is already running");
            return;
        }

        _isRunning = true;
        _logger.LogInformation("Starting health check worker (interval: {Interval}ms)", _intervalMs);

        _workerTask = PerformHealthChecksAsync(_cancellationTokenSource.Token);
    }

    /// <summary>
    /// Stops the health check worker.
    /// </summary>
    public async Task StopAsync()
    {
        if (!_isRunning)
        {
            _logger.LogWarning("Health check worker is not running");
            return;
        }

        _logger.LogInformation("Stopping health check worker");
        _cancellationTokenSource.Cancel();

        if (_workerTask is not null)
        {
            try
            {
                await _workerTask;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Health check worker stopped");
            }
        }

        _isRunning = false;
    }

    public bool IsRunning => _isRunning;

    /// <summary>
    /// Performs periodic health checks.
    /// </summary>
    private async Task PerformHealthChecksAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var health = await _orchestrator.GetHealthReportAsync();
                    _logger.LogInformation("Health Check - Status: {Status}, Throughput: {Throughput:F2} items/sec",
                        health.Status, health.ThroughputItemsPerSecond);

                    await Task.Delay(_intervalMs, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during health check");
                }
            }
        }
        finally
        {
            _isRunning = false;
        }
    }

    public void Dispose()
    {
        if (_isRunning)
        {
            _cancellationTokenSource.Cancel();

            try
            {
                _workerTask?.GetAwaiter().GetResult();
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown.
            }

            _isRunning = false;
        }

        _cancellationTokenSource.Dispose();
    }
}

/// <summary>
/// Coordinator for managing multiple background workers.
/// </summary>
public sealed class WorkerCoordinator : IDisposable
{
    private readonly BackgroundProcessingWorker _processingWorker;
    private readonly MetricsAggregationWorker _metricsWorker;
    private readonly HealthCheckWorker _healthCheckWorker;
    private readonly ILogger<WorkerCoordinator> _logger;

    public WorkerCoordinator(
        BackgroundProcessingWorker processingWorker,
        MetricsAggregationWorker metricsWorker,
        HealthCheckWorker healthCheckWorker,
        ILogger<WorkerCoordinator> logger)
    {
        _processingWorker = processingWorker ?? throw new ArgumentNullException(nameof(processingWorker));
        _metricsWorker = metricsWorker ?? throw new ArgumentNullException(nameof(metricsWorker));
        _healthCheckWorker = healthCheckWorker ?? throw new ArgumentNullException(nameof(healthCheckWorker));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Starts all workers.
    /// </summary>
    public void StartAll()
    {
        _logger.LogInformation("Starting all background workers");
        _processingWorker.Start();
        _metricsWorker.Start();
        _healthCheckWorker.Start();
    }

    /// <summary>
    /// Stops all workers gracefully.
    /// </summary>
    public async Task StopAllAsync()
    {
        _logger.LogInformation("Stopping all background workers");
        await Task.WhenAll(
            _processingWorker.StopAsync(),
            _metricsWorker.StopAsync(),
            _healthCheckWorker.StopAsync()
        );
    }

    public void Dispose()
    {
        _processingWorker?.Dispose();
        _metricsWorker?.Dispose();
        _healthCheckWorker?.Dispose();
    }
}
