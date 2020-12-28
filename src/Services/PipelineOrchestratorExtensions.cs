#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Services;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DotNetRealtimePipeline.Constants;
using DotNetRealtimePipeline.Domain.Models;

/// <summary>
/// Extension methods that provide convenient wrappers around <see cref="PipelineOrchestrator"/>.
/// </summary>
public static class PipelineOrchestratorExtensions
{
    /// <summary>
    /// Returns a human‑readable summary of the current pipeline status.
    /// </summary>
    /// <param name="orchestrator">The orchestrator instance.</param>
    /// <returns>A string containing key status information.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="orchestrator"/> is <c>null</c>.</exception>
    public static string GetStatusSummary(this PipelineOrchestrator orchestrator)
    {
        ArgumentNullException.ThrowIfNull(orchestrator);
        var status = orchestrator.GetStatus();
        return $"Pipeline[Running={status.IsRunning}, Processed={status.TotalDataPointsProcessed}, " +
               $"Failed={status.TotalDataPointsFailed}, Pending={status.PendingItemsInQueue}, " +
               $"Health={status.BackpressureStatus.GetHealthStatus()}, " +
               $"Timestamp={status.Timestamp.ToString("o", CultureInfo.InvariantCulture)}]";
    }

    /// <summary>
    /// Retrieves the latest health report from the pipeline.
    /// </summary>
    /// <param name="orchestrator">The orchestrator instance.</param>
    /// <returns>A task that resolves to the current <see cref="HealthReport"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="orchestrator"/> is <c>null</c>.</exception>
    public static Task<HealthReport> GetHealthReportAsync(this PipelineOrchestrator orchestrator)
    {
        ArgumentNullException.ThrowIfNull(orchestrator);
        return orchestrator.GetHealthReportAsync();
    }

    /// <summary>
    /// Retrieves the current performance trend analysis.
    /// </summary>
    /// <param name="orchestrator">The orchestrator instance.</param>
    /// <returns>A task that resolves to the current <see cref="PerformanceTrend"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="orchestrator"/> is <c>null</c>.</exception>
    public static Task<PerformanceTrend> GetPerformanceTrendAsync(this PipelineOrchestrator orchestrator)
    {
        ArgumentNullException.ThrowIfNull(orchestrator);
        return orchestrator.GetPerformanceTrendAsync();
    }

    /// <summary>
    /// Processes a batch of data points by delegating to <see cref="PipelineOrchestrator.ProcessBatchDataPointsAsync"/>.
    /// </summary>
    /// <param name="orchestrator">The orchestrator instance.</param>
    /// <param name="dataPoints">The data points to process.</param>
    /// <returns>A task that resolves to a <see cref="BatchProcessingResult"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="orchestrator"/> or <paramref name="dataPoints"/> is <c>null</c>.</exception>
    public static Task<BatchProcessingResult> ProcessBatchAsync(
        this PipelineOrchestrator orchestrator,
        IEnumerable<DataPoint> dataPoints)
    {
        ArgumentNullException.ThrowIfNull(orchestrator);
        ArgumentNullException.ThrowIfNull(dataPoints);
        return orchestrator.ProcessBatchDataPointsAsync(dataPoints.ToList());
    }
}
