// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Domain.Models;

using System;
using System.Collections.Generic;

/// <summary>
/// Manages backpressure state and flow control for the pipeline.
/// Prevents buffer overflow and ensures graceful degradation under load.
/// </summary>
public class BackpressureContext
{
    public long ContextId { get; set; }
    public string PipelineStageName { get; set; } = "";

    public long BufferSize { get; set; }
    public long MaxBufferCapacity { get; set; }
    public bool IsBackpressured { get; set; }
    public long BackpressureStartTimeMs { get; set; }
    public long TotalBackpressureTimeMs { get; set; }

    public int ActiveConsumers { get; set; }
    public int MaxConcurrentConsumers { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }

    public Queue<long> BackpressureEventTimestamps { get; set; } = new();
    public Dictionary<string, long> BufferMetrics { get; set; } = new();

    public BackpressureContext()
    {
    }

    public BackpressureContext(long contextId, string stageName, long maxCapacity)
    {
        ContextId = contextId;
        PipelineStageName = stageName ?? throw new ArgumentNullException(nameof(stageName));
        MaxBufferCapacity = maxCapacity;
        BufferSize = 0;
        IsBackpressured = false;
        CreatedAt = DateTime.UtcNow;
        LastUpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the current buffer fill percentage (0-100).
    /// </summary>
    public double GetBufferFillPercentage()
    {
        if (MaxBufferCapacity <= 0) return 0d;
        return (BufferSize / (double)MaxBufferCapacity) * 100d;
    }

    /// <summary>
    /// Determines if backpressure should be applied based on buffer state.
    /// </summary>
    public bool ShouldApplyBackpressure(double triggerThresholdPercent = 80)
    {
        double fillPercent = GetBufferFillPercentage();
        return fillPercent >= triggerThresholdPercent;
    }

    /// <summary>
    /// Attempts to add items to the buffer and triggers backpressure if needed.
    /// </summary>
    public bool TryAddToBuffer(long itemCount)
    {
        if (itemCount < 0) throw new ArgumentException("Item count cannot be negative", nameof(itemCount));

        long newBufferSize = BufferSize + itemCount;

        if (newBufferSize > MaxBufferCapacity)
        {
            ActivateBackpressure();
            return false;
        }

        BufferSize = newBufferSize;
        LastUpdatedAt = DateTime.UtcNow;
        return true;
    }

    /// <summary>
    /// Removes items from the buffer and may deactivate backpressure.
    /// </summary>
    public void RemoveFromBuffer(long itemCount)
    {
        if (itemCount < 0) throw new ArgumentException("Item count cannot be negative", nameof(itemCount));

        BufferSize = Math.Max(0, BufferSize - itemCount);
        LastUpdatedAt = DateTime.UtcNow;

        double fillPercent = GetBufferFillPercentage();
        if (fillPercent < 60 && IsBackpressured)
        {
            DeactivateBackpressure();
        }
    }

    /// <summary>
    /// Activates backpressure and records the event timestamp.
    /// </summary>
    public void ActivateBackpressure()
    {
        if (!IsBackpressured)
        {
            IsBackpressured = true;
            BackpressureStartTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            BackpressureEventTimestamps.Enqueue(BackpressureStartTimeMs);

            // Keep only last 100 events
            while (BackpressureEventTimestamps.Count > 100)
                BackpressureEventTimestamps.Dequeue();
        }
    }

    /// <summary>
    /// Deactivates backpressure and accumulates backpressure duration.
    /// </summary>
    public void DeactivateBackpressure()
    {
        if (IsBackpressured)
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long duration = now - BackpressureStartTimeMs;
            TotalBackpressureTimeMs += duration;
            IsBackpressured = false;
        }
    }

    /// <summary>
    /// Registers a consumer and checks if max concurrency is reached.
    /// </summary>
    public bool TryRegisterConsumer()
    {
        if (ActiveConsumers >= MaxConcurrentConsumers)
            return false;

        ActiveConsumers++;
        LastUpdatedAt = DateTime.UtcNow;
        return true;
    }

    /// <summary>
    /// Unregisters a consumer and updates metrics.
    /// </summary>
    public void UnregisterConsumer()
    {
        ActiveConsumers = Math.Max(0, ActiveConsumers - 1);
        LastUpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Records a custom buffer metric.
    /// </summary>
    public void RecordMetric(string metricName, long value)
    {
        if (string.IsNullOrWhiteSpace(metricName))
            throw new ArgumentException("Metric name cannot be null", nameof(metricName));

        BufferMetrics[metricName] = value;
    }

    /// <summary>
    /// Gets backpressure event frequency (events per minute).
    /// </summary>
    public double GetBackpressureFrequency()
    {
        if (BackpressureEventTimestamps.Count < 2)
            return 0d;

        long[] timestamps = BackpressureEventTimestamps.ToArray();
        long timeSpanMs = timestamps[^1] - timestamps[0];
        if (timeSpanMs <= 0) return 0d;

        double minutes = timeSpanMs / 60000d;
        return BackpressureEventTimestamps.Count / minutes;
    }

    /// <summary>
    /// Gets a health status string for this backpressure context.
    /// </summary>
    public string GetHealthStatus()
    {
        if (IsBackpressured) return "BACKPRESSURED";
        if (GetBufferFillPercentage() > 75) return "HIGH_LOAD";
        if (GetBufferFillPercentage() > 50) return "MODERATE_LOAD";
        return "HEALTHY";
    }
}
