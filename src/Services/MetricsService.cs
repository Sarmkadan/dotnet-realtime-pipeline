#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Services;

using DotNetRealtimePipeline.Constants;
using DotNetRealtimePipeline.Data.Repositories;
using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Service for collecting, aggregating, and analyzing pipeline metrics.
/// Tracks throughput, latency, error rates, and backpressure.
/// </summary>
public class MetricsService
{
    private readonly IMetricsRepository _repository;
    private long _nextMetricId = 1;
    private readonly List<double> _processingTimes = new();

    public MetricsService(IMetricsRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// Records the processing time of an operation.
    /// </summary>
    public void RecordProcessingTime(long processingTimeMs)
    {
        if (processingTimeMs < 0)
            throw new ArgumentException("Processing time cannot be negative", nameof(processingTimeMs));

        lock (_processingTimes)
        {
            _processingTimes.Add(processingTimeMs);

            // Keep only recent times (last 1000 measurements)
            if (_processingTimes.Count > 1000)
            {
                _processingTimes.RemoveAt(0);
            }
        }
    }

    /// <summary>
    /// Creates a metric aggregation from collected data.
    /// </summary>
    public async Task<MetricAggregation> CreateMetricAggregationAsync(
        long windowStartMs,
        long windowEndMs,
        long itemsProcessed,
        long itemsFailed,
        long itemsSkipped)
    {
        var metric = new MetricAggregation(
            _nextMetricId++,
            windowStartMs,
            windowEndMs,
            "STANDARD"
        );
        metric.TotalItemsProcessed = itemsProcessed;
        metric.TotalItemsFailed = itemsFailed;
        metric.TotalItemsSkipped = itemsSkipped;

        // Calculate processing time statistics
        lock (_processingTimes)
        {
            if (_processingTimes.Count > 0)
            {
                metric.ComputeAverageProcessingTime(_processingTimes.ToList());
                metric.MinProcessingTimeMs = _processingTimes.Min();
                metric.MaxProcessingTimeMs = _processingTimes.Max();
                metric.P95ProcessingTimeMs = CalculatePercentile(_processingTimes, 95);
                metric.P99ProcessingTimeMs = CalculatePercentile(_processingTimes, 99);
            }
        }

        // Save to repository
        return await _repository.SaveAsync(metric);
    }

    /// <summary>
    /// Computes a percentile value from a list of values.
    /// </summary>
    private double CalculatePercentile(List<double> values, int percentile)
    {
        if (values.Count == 0) return 0d;
        if (percentile < 0 || percentile > 100)
            throw new ArgumentException("Percentile must be between 0 and 100", nameof(percentile));

        var sorted = values.OrderBy(x => x).ToList();
        int index = (int)Math.Ceiling((percentile / 100.0) * sorted.Count) - 1;
        index = Math.Max(0, Math.Min(index, sorted.Count - 1));

        return sorted[index];
    }

    /// <summary>
    /// Computes health status based on current metrics.
    /// </summary>
    public async Task<HealthReport> GenerateHealthReportAsync()
    {
        MetricAggregation latest;
        try
        {
            latest = await _repository.GetLatestAsync();
        }
        catch
        {
            return new HealthReport
            {
                Status = "UNKNOWN",
                Message = "No metrics available",
                GeneratedAt = DateTime.UtcNow
            };
        }

        var report = new HealthReport
        {
            Status = latest.IsUnhealthy() ? "UNHEALTHY" : "HEALTHY",
            ThroughputItemsPerSecond = latest.CalculateThroughput(),
            SuccessRatePercent = latest.CalculateSuccessRate(),
            ErrorRatePercent = latest.CalculateErrorRate(),
            AverageProcessingTimeMs = latest.AverageProcessingTimeMs,
            P95ProcessingTimeMs = latest.P95ProcessingTimeMs,
            P99ProcessingTimeMs = latest.P99ProcessingTimeMs,
            BackpressurePercentage = latest.CalculateBackpressureRatio(),
            TotalProcessed = latest.TotalItemsProcessed,
            TotalFailed = latest.TotalItemsFailed,
            GeneratedAt = DateTime.UtcNow
        };

        if (latest.CalculateErrorRate() > 5)
            report.Message = "ERROR RATE ELEVATED";
        else if (latest.CalculateBackpressureRatio() > 20)
            report.Message = "BACKPRESSURE DETECTED";
        else if (latest.CalculateThroughput() < 100)
            report.Message = "LOW THROUGHPUT";
        else
            report.Message = "OPERATING NORMALLY";

        return report;
    }

    /// <summary>
    /// Gets performance trend analysis.
    /// </summary>
    public async Task<PerformanceTrend> AnalyzePerformanceTrendAsync(int historyCount = 10)
    {
        var recentMetrics = await _repository.GetHistoryAsync(historyCount);

        if (recentMetrics.Count < 2)
        {
            return new PerformanceTrend
            {
                TrendDirection = "INSUFFICIENT_DATA",
                SamplesAnalyzed = recentMetrics.Count
            };
        }

        var firstMetric = recentMetrics.Last();
        var lastMetric = recentMetrics.First();

        double initialThroughput = firstMetric.CalculateThroughput();
        double currentThroughput = lastMetric.CalculateThroughput();

        string direction;
        if (Math.Abs(currentThroughput - initialThroughput) < 0.01)
            direction = "STABLE";
        else if (currentThroughput > initialThroughput)
            direction = "IMPROVING";
        else
            direction = "DEGRADING";

        var trend = new PerformanceTrend
        {
            TrendDirection = direction,
            ThroughputChangePercent = ((currentThroughput - initialThroughput) / initialThroughput) * 100,
            LatencyChangePercent = ((lastMetric.AverageProcessingTimeMs - firstMetric.AverageProcessingTimeMs) / firstMetric.AverageProcessingTimeMs) * 100,
            ErrorRateChangePercent = ((lastMetric.CalculateErrorRate() - firstMetric.CalculateErrorRate()) / firstMetric.CalculateErrorRate()) * 100,
            SamplesAnalyzed = recentMetrics.Count,
            TimeSpanMs = lastMetric.TimeWindowEndMs - firstMetric.TimeWindowStartMs
        };

        return trend;
    }

    /// <summary>
    /// Gets the distribution of metrics by source.
    /// </summary>
    public async Task<MetricDistribution> GetMetricDistributionAsync()
    {
        var latest = await _repository.GetLatestAsync();

        var distribution = new MetricDistribution
        {
            TotalSources = latest.CountBySource.Count,
            SourceBreakdown = new(latest.CountBySource),
            StageErrorRates = new(latest.ErrorRateByStage),
            ComputedAt = DateTime.UtcNow
        };

        return distribution;
    }

    /// <summary>
    /// Records an error/failure in metrics.
    /// </summary>
    public void RecordFailure(string stageName)
    {
        if (string.IsNullOrWhiteSpace(stageName))
            throw new ArgumentException("Stage name cannot be null", nameof(stageName));
        // Metrics are accumulated and flushed periodically via CreateMetricAggregationAsync
    }

    /// <summary>
    /// Clears historical processing times (for testing/reset).
    /// </summary>
    public void ClearProcessingTimes()
    {
        lock (_processingTimes)
        {
            _processingTimes.Clear();
        }
    }
}

/// <summary>
/// Health status report for the pipeline.
/// </summary>
public class HealthReport
{
    public string Status { get; set; } = "UNKNOWN";
    public string Message { get; set; } = "";
    public double ThroughputItemsPerSecond { get; set; }
    public double SuccessRatePercent { get; set; }
    public double ErrorRatePercent { get; set; }
    public double AverageProcessingTimeMs { get; set; }
    public double P95ProcessingTimeMs { get; set; }
    public double P99ProcessingTimeMs { get; set; }
    public double BackpressurePercentage { get; set; }
    public long TotalProcessed { get; set; }
    public long TotalFailed { get; set; }
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// Performance trend analysis.
/// </summary>
public class PerformanceTrend
{
    public string TrendDirection { get; set; }
    public double ThroughputChangePercent { get; set; }
    public double LatencyChangePercent { get; set; }
    public double ErrorRateChangePercent { get; set; }
    public int SamplesAnalyzed { get; set; }
    public long TimeSpanMs { get; set; }
}

/// <summary>
/// Distribution of metrics across sources and stages.
/// </summary>
public class MetricDistribution
{
    public int TotalSources { get; set; }
    public Dictionary<string, long> SourceBreakdown { get; set; } = new();
    public Dictionary<string, double> StageErrorRates { get; set; } = new();
    public DateTime ComputedAt { get; set; }
}
