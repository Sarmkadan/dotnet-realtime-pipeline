#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Domain.Models;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents aggregated metrics for monitoring pipeline performance.
/// Tracks throughput, latency, error rates, and backpressure indicators.
/// </summary>
public sealed class MetricAggregation
{
    /// <summary>
    /// Gets or sets the unique identifier for this metric aggregation.
    /// </summary>
    public long MetricId { get; set; }

    /// <summary>
    /// Gets or sets the start of the time window in milliseconds (Unix timestamp).
    /// </summary>
    public long TimeWindowStartMs { get; set; }

    /// <summary>
    /// Gets or sets the end of the time window in milliseconds (Unix timestamp).
    /// </summary>
    public long TimeWindowEndMs { get; set; }

    /// <summary>
    /// Gets or sets the type of metric aggregation (e.g., "hourly", "daily").
    /// </summary>
    public string MetricType { get; set; } = "";

    /// <summary>
    /// Gets or sets the total number of items successfully processed in this window.
    /// </summary>
    public long TotalItemsProcessed { get; set; }

    /// <summary>
    /// Gets or sets the total number of items that failed processing in this window.
    /// </summary>
    public long TotalItemsFailed { get; set; }

    /// <summary>
    /// Gets or sets the total number of items skipped in this window.
    /// </summary>
    public long TotalItemsSkipped { get; set; }

    /// <summary>
    /// Gets or sets the average processing time in milliseconds.
    /// </summary>
    public double AverageProcessingTimeMs { get; set; }

    /// <summary>
    /// Gets or sets the minimum processing time in milliseconds.
    /// </summary>
    public double MinProcessingTimeMs { get; set; }

    /// <summary>
    /// Gets or sets the maximum processing time in milliseconds.
    /// </summary>
    public double MaxProcessingTimeMs { get; set; }

    /// <summary>
    /// Gets or sets the 95th percentile processing time in milliseconds.
    /// </summary>
    public double P95ProcessingTimeMs { get; set; }

    /// <summary>
    /// Gets or sets the 99th percentile processing time in milliseconds.
    /// </summary>
    public double P99ProcessingTimeMs { get; set; }

    /// <summary>
    /// Gets or sets the number of backpressure events triggered in this window.
    /// </summary>
    public long BackpressureEvents { get; set; }

    /// <summary>
    /// Gets or sets the total time spent in backpressure state in milliseconds.
    /// </summary>
    public long TotalBackpressureMs { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when these metrics were computed.
    /// </summary>
    public DateTime ComputedAt { get; set; }

    /// <summary>
    /// Gets or sets the count of items processed by source as key-value pairs.
    /// </summary>
    public Dictionary<string, long> CountBySource { get; set; } = new();

    /// <summary>
    /// Gets or sets the error rate by pipeline stage as key-value pairs.
    /// </summary>
    public Dictionary<string, double> ErrorRateByStage { get; set; } = new();

    public MetricAggregation()
    {
    }

    public MetricAggregation(long metricId, long startMs, long endMs, string metricType)
    {
        MetricId = metricId;
        TimeWindowStartMs = startMs;
        TimeWindowEndMs = endMs;
        MetricType = metricType ?? throw new ArgumentNullException(nameof(metricType));
        ComputedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Calculates the throughput (items/second) for this window.
    /// </summary>
    public double CalculateThroughput()
    {
        long windowDurationMs = TimeWindowEndMs - TimeWindowStartMs;
        if (windowDurationMs <= 0) return 0d;

        double windowDurationSeconds = windowDurationMs / 1000d;
        return TotalItemsProcessed / windowDurationSeconds;
    }

    /// <summary>
    /// Calculates the success rate as a percentage.
    /// </summary>
    public double CalculateSuccessRate()
    {
        long total = TotalItemsProcessed + TotalItemsFailed + TotalItemsSkipped;
        if (total == 0) return 100d;

        return (TotalItemsProcessed / (double)total) * 100d;
    }

    /// <summary>
    /// Calculates the error rate as a percentage.
    /// </summary>
    public double CalculateErrorRate()
    {
        long total = TotalItemsProcessed + TotalItemsFailed + TotalItemsSkipped;
        if (total == 0) return 0d;

        return (TotalItemsFailed / (double)total) * 100d;
    }

    /// <summary>
    /// Calculates the backpressure ratio (time spent backpressured / total time).
    /// </summary>
    public double CalculateBackpressureRatio()
    {
        long windowDurationMs = TimeWindowEndMs - TimeWindowStartMs;
        if (windowDurationMs <= 0) return 0d;

        return (TotalBackpressureMs / (double)windowDurationMs) * 100d;
    }

    /// <summary>
    /// Records a metric for a specific source.
    /// </summary>
    public void RecordSourceMetric(string source, long count)
    {
        if (string.IsNullOrWhiteSpace(source)) throw new ArgumentException("Source cannot be null", nameof(source));

        if (CountBySource.ContainsKey(source))
            CountBySource[source] += count;
        else
            CountBySource[source] = count;
    }

    /// <summary>
    /// Records the error rate for a specific pipeline stage.
    /// </summary>
    public void RecordStageErrorRate(string stageName, double errorRate)
    {
        if (string.IsNullOrWhiteSpace(stageName)) throw new ArgumentException("Stage name cannot be null", nameof(stageName));
        if (errorRate < 0 || errorRate > 100) throw new ArgumentException("Error rate must be between 0 and 100", nameof(errorRate));

        ErrorRateByStage[stageName] = errorRate;
    }

    /// <summary>
    /// Calculates the average processing time across all items.
    /// </summary>
    public void ComputeAverageProcessingTime(List<double> processingTimes)
    {
        if (processingTimes is null || processingTimes.Count == 0)
        {
            AverageProcessingTimeMs = 0;
            return;
        }

        double sum = 0;
        foreach (var time in processingTimes)
            sum += time;

        AverageProcessingTimeMs = sum / processingTimes.Count;
    }

    /// <summary>
    /// Gets a summary of metrics for reporting.
    /// </summary>
    public string GetSummary()
    {
        return $"MetricAggregation[Type={MetricType}, Throughput={CalculateThroughput():F2} items/s, " +
               $"SuccessRate={CalculateSuccessRate():F2}%, AvgLatency={AverageProcessingTimeMs:F2}ms, " +
               $"P95={P95ProcessingTimeMs:F2}ms, Backpressure={CalculateBackpressureRatio():F2}%]";
    }

    /// <summary>
    /// Determines if this metric indicates unhealthy pipeline state.
    /// </summary>
    public bool IsUnhealthy()
    {
        double errorRate = CalculateErrorRate();
        double successRate = CalculateSuccessRate();
        double backpressureRatio = CalculateBackpressureRatio();

        return errorRate > 5 || successRate < 95 || backpressureRatio > 20;
    }
}
