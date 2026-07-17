#nullable enable

namespace DotNetRealtimePipeline.Domain.Models;

using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Provides extension methods for <see cref="BackpressureContext"/> to simplify common operations
/// and add domain-specific functionality for pipeline backpressure management.
/// </summary>
/// <remarks>
/// All extension methods validate their inputs and throw appropriate exceptions for null or invalid values.
/// Methods are designed to be thread-safe when called on the same <see cref="BackpressureContext"/> instance.
/// </remarks>
public static class BackpressureContextExtensions
{
    /// <summary>
    /// Calculates the estimated time (in milliseconds) until the buffer reaches capacity
    /// based on the current fill rate and consumption rate.
    /// </summary>
    /// <param name="context">The backpressure context.</param>
    /// <param name="consumptionRatePerSecond">Items consumed per second.</param>
    /// <returns>Estimated milliseconds until capacity, or -1 if buffer is empty or consumption is zero.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="consumptionRatePerSecond"/> is not positive.</exception>
    public static long EstimateTimeToCapacity(
        this BackpressureContext context,
        double consumptionRatePerSecond)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (consumptionRatePerSecond <= 0)
            throw new ArgumentOutOfRangeException(nameof(consumptionRatePerSecond), consumptionRatePerSecond, "Must be positive");

        if (context.MaxBufferCapacity <= 0)
            return -1;

        long remainingCapacity = context.MaxBufferCapacity - context.BufferSize;
        if (remainingCapacity <= 0)
            return 0;

        double fillRatePerSecond = consumptionRatePerSecond;
        double secondsToCapacity = remainingCapacity / fillRatePerSecond;
        return (long)(secondsToCapacity * 1000);
    }

    /// <summary>
    /// Determines if the buffer is critically full based on both percentage and absolute thresholds.
    /// </summary>
    /// <param name="context">The backpressure context.</param>
    /// <param name="percentageThreshold">Percentage threshold (0-100).</param>
    /// <param name="absoluteThreshold">Absolute item threshold.</param>
    /// <returns>True if buffer is critically full; otherwise false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="percentageThreshold"/> is not in range [0, 100].</exception>
    public static bool IsCriticallyFull(
        this BackpressureContext context,
        double percentageThreshold = 90,
        long absoluteThreshold = 0)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (percentageThreshold is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(percentageThreshold), percentageThreshold, "Must be between 0 and 100");

        double fillPercent = context.GetBufferFillPercentage();
        bool isPercentageCritical = fillPercent >= percentageThreshold;
        bool isAbsoluteCritical = context.BufferSize >= absoluteThreshold;

        return isPercentageCritical || isAbsoluteCritical;
    }

    /// <summary>
    /// Gets the backpressure duration in a human-readable format (HH:MM:SS).
    /// </summary>
    /// <param name="context">The backpressure context.</param>
    /// <returns>Formatted duration string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> is null.</exception>
    public static string GetBackpressureDurationFormatted(this BackpressureContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        long totalMs = context.TotalBackpressureTimeMs;
        if (totalMs <= 0)
            return "00:00:00";

        TimeSpan duration = TimeSpan.FromMilliseconds(totalMs);
        return $"{duration.Hours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}";
    }

    /// <summary>
    /// Records a backpressure event with optional metadata.
    /// </summary>
    /// <param name="context">The backpressure context.</param>
    /// <param name="eventType">Type of backpressure event.</param>
    /// <param name="metadata">Optional metadata dictionary.</param>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="eventType"/> is null or empty.</exception>
    public static void RecordBackpressureEvent(
        this BackpressureContext context,
        string eventType,
        Dictionary<string, string>? metadata = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrEmpty(eventType, nameof(eventType));

        context.ActivateBackpressure();
        context.RecordMetric($"BackpressureEvent_{eventType}", 1);

        if (metadata is { Count: > 0 })
        {
            foreach (var kvp in metadata)
            {
                context.RecordMetric($"EventMeta_{eventType}_{kvp.Key}", long.Parse(kvp.Value, CultureInfo.InvariantCulture));
            }
        }
    }

    /// <summary>
    /// Gets a summary of buffer metrics including fill percentage, dropped items, and consumer stats.
    /// </summary>
    /// <param name="context">The backpressure context.</param>
    /// <returns>Formatted metrics summary.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> is null.</exception>
    public static string GetBufferMetricsSummary(this BackpressureContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var metrics = new Dictionary<string, string>
        {
            ["Stage"] = context.PipelineStageName ?? "Unknown",
            ["Fill %"] = context.GetBufferFillPercentage().ToString("F2", CultureInfo.InvariantCulture),
            ["Buffer Size"] = context.BufferSize.ToString(CultureInfo.InvariantCulture),
            ["Capacity"] = context.MaxBufferCapacity.ToString(CultureInfo.InvariantCulture),
            ["Dropped Items"] = context.DroppedItemCount.ToString(CultureInfo.InvariantCulture),
            ["Consumers"] = $"{context.ActiveConsumers}/{context.MaxConcurrentConsumers}",
            ["Status"] = context.GetHealthStatus(),
            ["Created"] = context.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            ["Last Update"] = context.LastUpdatedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
        };

        if (context.TotalBackpressureTimeMs > 0)
        {
            metrics["Backpressure Duration"] = context.GetBackpressureDurationFormatted();
        }

        var lines = new List<string>();
        foreach (var kvp in metrics)
        {
            lines.Add($"{kvp.Key}: {kvp.Value}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Safely removes items from buffer, ensuring we never go below zero.
    /// Returns the actual number of items removed.
    /// </summary>
    /// <param name="context">The backpressure context.</param>
    /// <param name="itemCount">Number of items to remove.</param>
    /// <returns>Actual number of items removed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> is null.</exception>
    public static long SafeRemoveFromBuffer(this BackpressureContext context, long itemCount)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (itemCount <= 0)
            return 0;

        long actualRemoved = Math.Min(itemCount, context.BufferSize);
        context.RemoveFromBuffer(actualRemoved);
        return actualRemoved;
    }

    /// <summary>
    /// Checks if the buffer has sufficient capacity for a batch operation.
    /// </summary>
    /// <param name="context">The backpressure context.</param>
    /// <param name="batchSize">Size of the batch to add.</param>
    /// <param name="requiredCapacityPercent">Minimum required capacity percentage (0-100). Defaults to 20%.</param>
    /// <returns>True if sufficient capacity exists; otherwise false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="requiredCapacityPercent"/> is not in range [0, 100].</exception>
    public static bool HasSufficientCapacityForBatch(
        this BackpressureContext context,
        long batchSize,
        double requiredCapacityPercent = 20)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (requiredCapacityPercent is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(requiredCapacityPercent), requiredCapacityPercent, "Must be between 0 and 100");

        return batchSize <= 0 || context.BufferSize + batchSize <= context.MaxBufferCapacity * (requiredCapacityPercent / 100d);
    }
}
