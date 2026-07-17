#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

namespace DotNetRealtimePipeline.Services;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Domain.Enums;

/// <summary>
/// Extension methods for <see cref="BackpressureService"/> providing convenient
/// operations for backpressure management and monitoring.
/// </summary>
public static class BackpressureServiceExtensions
{
    /// <summary>
    /// Gets the backpressure context for a stage or creates a new one if it doesn't exist.
    /// </summary>
    /// <param name="service">The backpressure service instance.</param>
    /// <param name="stageName">Name of the pipeline stage.</param>
    /// <param name="maxBufferCapacity">Maximum buffer capacity for the stage.</param>
    /// <returns>The existing or newly created backpressure context.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="stageName"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="stageName"/> is empty or whitespace.</exception>
    public static BackpressureContext GetOrCreateContext(
        this BackpressureService service,
        string stageName,
        long maxBufferCapacity)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(stageName);

        var context = service.GetContext(stageName);
        return context ?? service.CreateContext(stageName, maxBufferCapacity);
    }

    /// <summary>
    /// Safely adds items to a stage's buffer, returning false if the operation would exceed capacity.
    /// This method is thread-safe and handles null checks automatically.
    /// </summary>
    /// <param name="service">The backpressure service instance.</param>
    /// <param name="stageName">Name of the pipeline stage.</param>
    /// <param name="itemCount">Number of items to add to the buffer.</param>
    /// <returns>True if items were added; false if buffer capacity would be exceeded.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="stageName"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="stageName"/> is empty or whitespace.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="itemCount"/> is negative.</exception>
    public static bool SafeAddToBuffer(
        this BackpressureService service,
        string stageName,
        long itemCount)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(stageName);
        ArgumentOutOfRangeException.ThrowIfNegative(itemCount);

        return service.TryAddToBuffer(stageName, itemCount);
    }

    /// <summary>
    /// Gets the current buffer fill percentage for a specific stage.
    /// Returns 0 if the stage doesn't exist or has no capacity.
    /// </summary>
    /// <param name="service">The backpressure service instance.</param>
    /// <param name="stageName">Name of the pipeline stage.</param>
    /// <returns>Buffer fill percentage (0-100), or 0 if stage doesn't exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="stageName"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="stageName"/> is empty or whitespace.</exception>
    public static double GetBufferFillPercentage(
        this BackpressureService service,
        string stageName)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(stageName);

        var context = service.GetContext(stageName);
        return context?.GetBufferFillPercentage() ?? 0d;
    }

    /// <summary>
    /// Checks if backpressure should be applied based on buffer state for a specific stage.
    /// Uses the default threshold of 80% buffer fill.
    /// </summary>
    /// <param name="service">The backpressure service instance.</param>
    /// <param name="stageName">Name of the pipeline stage.</param>
    /// <returns>True if backpressure should be applied; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="stageName"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="stageName"/> is empty or whitespace.</exception>
    public static bool ShouldApplyBackpressure(
        this BackpressureService service,
        string stageName)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(stageName);

        var context = service.GetContext(stageName);
        return context is not null && context.ShouldApplyBackpressure();
    }

    /// <summary>
    /// Gets the dropped item count for a specific stage.
    /// A non-zero value indicates data loss has occurred due to buffer overflow.
    /// </summary>
    /// <param name="service">The backpressure service instance.</param>
    /// <param name="stageName">Name of the pipeline stage.</param>
    /// <returns>Number of items dropped due to buffer overflow.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="stageName"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="stageName"/> is empty or whitespace.</exception>
    public static long GetDroppedItemCount(
        this BackpressureService service,
        string stageName)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(stageName);

        return service.GetDroppedItemCount(stageName);
    }

    /// <summary>
    /// Gets the current buffer status for all registered stages as a formatted string.
    /// </summary>
    /// <param name="service">The backpressure service instance.</param>
    /// <returns>A formatted string showing buffer status for each stage.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    public static string GetBufferStatusReport(
        this BackpressureService service)
    {
        ArgumentNullException.ThrowIfNull(service);

        var status = service.GetBufferStatus();
        var systemStatus = service.GetSystemStatus();

        var report = new System.Text.StringBuilder();
        report.AppendLine("=== Backpressure Buffer Status Report ===");
        report.AppendLine($"Generated: {DateTime.UtcNow:u}");
        report.AppendLine($"Total Stages: {systemStatus.TotalStages}");
        report.AppendLine($"Backpressured Stages: {systemStatus.BackpressuredStages}");
        report.AppendLine($"Average Buffer Fill: {systemStatus.AverageBufferFillPercent:N2}%");
        report.AppendLine($"Total Dropped Items: {systemStatus.TotalDroppedItems}");
        report.AppendLine($"System Health: {systemStatus.GetHealthStatus()}");
        report.AppendLine();

        if (status.Count > 0)
        {
            report.AppendLine("Stage Details:");
            foreach (var kv in status.OrderBy(x => x.Key))
            {
                var context = service.GetContext(kv.Key);
                var fillPercent = context?.GetBufferFillPercentage() ?? 0d;
                var isBackpressured = context?.IsBackpressured ?? false;
                var dropped = service.GetDroppedItemCount(kv.Key);

                report.AppendLine($" {kv.Key,-25} | Fill: {fillPercent,6:N2}% | " +
                    $"Status: {(isBackpressured ? "BACKPRESSURED" : "OK"),-12} | " +
                    $"Dropped: {dropped,8}");
            }
        }
        else
        {
            report.AppendLine("No stages registered.");
        }

        return report.ToString();
    }

    /// <summary>
    /// Gets a consumer slot if available, with optional timeout support.
    /// </summary>
    /// <param name="service">The backpressure service instance.</param>
    /// <param name="stageName">Name of the pipeline stage.</param>
    /// <param name="timeoutMs">Maximum time to wait for a consumer slot (0 = no wait).</param>
    /// <returns>True if a consumer slot was obtained; false if timeout expired or stage not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="stageName"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="stageName"/> is empty or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="timeoutMs"/> is negative.</exception>
    public static async Task<bool> TryRegisterConsumerAsync(
        this BackpressureService service,
        string stageName,
        int timeoutMs = 0)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(stageName);
        ArgumentOutOfRangeException.ThrowIfNegative(timeoutMs);

        if (timeoutMs == 0)
        {
            return service.TryRegisterConsumer(stageName);
        }

        var startTime = DateTime.UtcNow;
        while (DateTime.UtcNow - startTime < TimeSpan.FromMilliseconds(timeoutMs))
        {
            if (service.TryRegisterConsumer(stageName))
            {
                return true;
            }

            await Task.Delay(50);
        }

        return false;
    }

    /// <summary>
    /// Gets the backpressure system status with additional derived metrics.
    /// </summary>
    /// <param name="service">The backpressure service instance.</param>
    /// <returns>A tuple containing the system status and derived metrics.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    public static (BackpressureSystemStatus Status, BackpressureMetrics Metrics) GetEnhancedSystemStatus(
        this BackpressureService service)
    {
        ArgumentNullException.ThrowIfNull(service);

        var status = service.GetSystemStatus();

        var metrics = new BackpressureMetrics
        {
            TotalStages = status.TotalStages,
            BackpressuredStages = status.BackpressuredStages,
            AverageBufferFillPercent = status.AverageBufferFillPercent,
            TotalBackpressureTimeMs = status.TotalBackpressureTimeMs,
            TotalDroppedItems = status.TotalDroppedItems,
            IsSystemBackpressured = status.IsSystemBackpressured,
            HealthStatus = status.GetHealthStatus(),
            Timestamp = status.Timestamp
        };

        // Calculate derived metrics
        if (status.TotalStages > 0)
        {
            metrics.HealthyStages = service.GetBufferStatus()
                .Count(kv => !service.IsBackpressured(kv.Key) &&
                    service.GetBufferFillPercentage(kv.Key) <= 50);

            metrics.WarningStages = service.GetBufferStatus()
                .Count(kv => service.GetBufferFillPercentage(kv.Key) > 50 &&
                    service.GetBufferFillPercentage(kv.Key) <= 75);

            metrics.CriticalStages = service.GetBufferStatus()
                .Count(kv => service.IsBackpressured(kv.Key) ||
                    service.GetBufferFillPercentage(kv.Key) > 75);
        }

        return (status, metrics);
    }

    /// <summary>
    /// Records a custom metric for a specific stage's buffer.
    /// </summary>
    /// <param name="service">The backpressure service instance.</param>
    /// <param name="stageName">Name of the pipeline stage.</param>
    /// <param name="metricName">Name of the metric to record.</param>
    /// <param name="value">Value of the metric.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/>, <paramref name="stageName"/>, or <paramref name="metricName"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="stageName"/> or <paramref name="metricName"/> is empty or whitespace.</exception>
    public static void RecordBufferMetric(
        this BackpressureService service,
        string stageName,
        string metricName,
        long value)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(stageName);
        ArgumentException.ThrowIfNullOrEmpty(metricName);

        var context = service.GetOrCreateContext(stageName, 1000);
        context.RecordMetric(metricName, value);
    }

    /// <summary>
    /// Gets the backpressure frequency (events per minute) for a specific stage.
    /// </summary>
    /// <param name="service">The backpressure service instance.</param>
    /// <param name="stageName">Name of the pipeline stage.</param>
    /// <returns>Backpressure frequency in events per minute, or 0 if insufficient data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="stageName"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="stageName"/> is empty or whitespace.</exception>
    public static double GetBackpressureFrequency(
        this BackpressureService service,
        string stageName)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(stageName);

        var context = service.GetContext(stageName);
        return context?.GetBackpressureFrequency() ?? 0d;
    }
}

/// <summary>
/// Additional metrics derived from backpressure system status.
/// </summary>
public sealed class BackpressureMetrics
{
    public int TotalStages { get; set; }
    public int BackpressuredStages { get; set; }
    public int HealthyStages { get; set; }
    public int WarningStages { get; set; }
    public int CriticalStages { get; set; }
    public double AverageBufferFillPercent { get; set; }
    public long TotalBackpressureTimeMs { get; set; }
    public long TotalDroppedItems { get; set; }
    public bool IsSystemBackpressured { get; set; }
    public string HealthStatus { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}