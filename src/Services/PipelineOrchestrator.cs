#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Services;

using DotNetRealtimePipeline.Constants;
using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Orchestrates the entire data processing pipeline.
/// Coordinates multiple services and manages the flow of data through stages.
/// </summary>
public sealed class PipelineOrchestrator
{
    private readonly DataProcessingService _processingService;
    private readonly WindowingService _windowingService;
    private readonly MetricsService _metricsService;
    private readonly BackpressureService _backpressureService;
    private readonly PipelineConfig _config;

    private bool _isRunning;
    private readonly Queue<DataPoint> _incomingDataQueue = new();
    private long _totalProcessed;
    private long _totalFailed;

    public PipelineOrchestrator(
        DataProcessingService processingService,
        WindowingService windowingService,
        MetricsService metricsService,
        BackpressureService backpressureService,
        PipelineConfig config)
    {
        _processingService = processingService ?? throw new ArgumentNullException(nameof(processingService));
        _windowingService = windowingService ?? throw new ArgumentNullException(nameof(windowingService));
        _metricsService = metricsService ?? throw new ArgumentNullException(nameof(metricsService));
        _backpressureService = backpressureService ?? throw new ArgumentNullException(nameof(backpressureService));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Starts the pipeline orchestrator.
    /// </summary>
    public async Task StartAsync()
    {
        if (_isRunning) return;

        _isRunning = true;

        // Initialize backpressure contexts for all stages
        foreach (var stage in _config.Stages)
        {
            _backpressureService.CreateContext(stage.StageName, _config.MaxBufferSize);
        }

        // Initialize backpressure context for Windowing stage
        _backpressureService.CreateContext(PipelineConstants.StageName_Windowing, _config.MaxBufferSize);

        // Start processing loop
        _ = ProcessingLoopAsync();

        await Task.CompletedTask;
    }

    /// <summary>
    /// Stops the pipeline orchestrator.
    /// </summary>
    public async Task StopAsync()
    {
        _isRunning = false;
        await Task.Delay(500); // Allow processing loop to finish
    }

    /// <summary>
    /// Ingests a data point into the pipeline.
    /// </summary>
    public async Task<bool> IngestDataPointAsync(DataPoint dataPoint)
    {
        if (dataPoint is null) throw new ArgumentNullException(nameof(dataPoint));
        if (!_isRunning) throw new InvalidOperationException("Pipeline is not running");

        // Check backpressure on ingestion stage
        bool canAdd = _backpressureService.TryAddToBuffer(
            PipelineConstants.StageName_Ingestion,
            1
        );

        if (!canAdd)
        {
            // Apply backpressure
            await _backpressureService.ApplyBackpressureAsync(
                PipelineConstants.StageName_Ingestion,
                Domain.Enums.BackpressureStrategy.Block,
                100
            );
            return false;
        }

        lock (_incomingDataQueue)
        {
            _incomingDataQueue.Enqueue(dataPoint);
        }

        return true;
    }

    /// <summary>
    /// Gets the current pipeline status.
    /// </summary>
    public PipelineStatus GetStatus()
    {
        return new PipelineStatus
        {
            IsRunning = _isRunning,
            TotalDataPointsProcessed = _totalProcessed,
            TotalDataPointsFailed = _totalFailed,
            PendingItemsInQueue = _incomingDataQueue.Count,
            ConfigurationName = _config.PipelineName,
            ConfigurationVersion = _config.Version,
            BackpressureStatus = _backpressureService.GetSystemStatus(),
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Gets the current health report.
    /// </summary>
    public async Task<HealthReport> GetHealthReportAsync()
    {
        return await _metricsService.GenerateHealthReportAsync();
    }

    /// <summary>
    /// Gets performance trend analysis.
    /// </summary>
    public async Task<PerformanceTrend> GetPerformanceTrendAsync()
    {
        return await _metricsService.AnalyzePerformanceTrendAsync();
    }

    // Private processing loop

    private async Task ProcessingLoopAsync()
    {
        var processingTimer = new Stopwatch();
        List<DataPoint> batch = new();
        int batchSize = 100;

        while (_isRunning)
        {
            try
            {
                // Collect batch of data points
                lock (_incomingDataQueue)
                {
                    while (_incomingDataQueue.Count > 0 && batch.Count < batchSize)
                    {
                        batch.Add(_incomingDataQueue.Dequeue());
                    }
                }

                if (batch.Count == 0)
                {
                    await Task.Delay(100);
                    continue;
                }

                // Process batch
                processingTimer.Restart();
                var results = await _processingService.ProcessBatchAsync(batch);
                processingTimer.Stop();

                // Record metrics
                foreach (var result in results)
                {
                    _metricsService.RecordProcessingTime(result.ProcessingTimeMs);

                    if (result.Success)
                        Interlocked.Increment(ref _totalProcessed);
                    else
                        Interlocked.Increment(ref _totalFailed);
                }

                // Window the processed data
                var dataPoints = new List<DataPoint>();
                foreach (var result in results)
                {
                    if (result.Success && result.GetOutput("DataPointId") is long id)
                    {
                        // In a real implementation, retrieve the data point
                        dataPoints.Add(new DataPoint(id, (long)result.GetOutput("Timestamp")!, (double)0, (string)result.GetOutput("Source")!));
                    }
                }

                if (dataPoints.Count > 0)
                {
                    try
                    {
                        // Check window buffer capacity before adding data points
                        bool canAddToWindow = _backpressureService.TryAddToBuffer(
                            PipelineConstants.StageName_Windowing,
                            dataPoints.Count
                        );

                        if (!canAddToWindow)
                        {
                            // Apply backpressure on windowing stage and propagate to source
                            await _backpressureService.ApplyBackpressureAsync(
                                PipelineConstants.StageName_Windowing,
                                Domain.Enums.BackpressureStrategy.Block,
                                100
                            );
                        }

                        var windows = _windowingService.AssignDataPointsToWindows(dataPoints);
                        foreach (var window in windows)
                        {
                            if (_windowingService.IsWindowComplete(window))
                            {
                                var emission = _windowingService.EmitWindow(window);
                                // Output window results
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Windowing error: {ex.Message}");
                    }
                }

                // Remove from ingestion buffer
                _backpressureService.RemoveFromBuffer(
                    PipelineConstants.StageName_Ingestion,
                    batch.Count
                );

                batch.Clear();
            }
            catch (Exception ex)
            {
                // Log error and continue
                System.Diagnostics.Debug.WriteLine($"Processing loop error: {ex.Message}");
                await Task.Delay(100);
            }
        }
    }
}

/// <summary>
/// Current status of the pipeline.
/// </summary>
public sealed class PipelineStatus
{
    public bool IsRunning { get; set; }
    public long TotalDataPointsProcessed { get; set; }
    public long TotalDataPointsFailed { get; set; }
    public int PendingItemsInQueue { get; set; }
    public string ConfigurationName { get; set; } = "";
    public string ConfigurationVersion { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public BackpressureSystemStatus BackpressureStatus { get; set; } = new();

    public string GetSummary()
    {
        return $"Pipeline[Running={IsRunning}, Processed={TotalDataPointsProcessed}, " +
               $"Failed={TotalDataPointsFailed}, Pending={PendingItemsInQueue}, " +
               $"Health={BackpressureStatus.GetHealthStatus()}]";
    }
}
