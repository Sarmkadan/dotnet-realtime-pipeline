#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Visualization;

using System;
using System.Collections.Generic;
using System.Globalization;
using DotNetRealtimePipeline.Domain.Models;

/// <summary>
/// Provides extension methods for <see cref="PipelineVisualizer"/> to enable common
/// visualization scenarios without duplicating core rendering logic.
/// </summary>
public static class PipelineVisualizerExtensions
{
    /// <summary>
    /// Renders the pipeline visualization directly to the console output.
    /// Useful for quick debugging, logging, or interactive sessions.
    /// </summary>
    /// <param name="visualizer">The pipeline visualizer instance.</param>
    /// <param name="config">The pipeline configuration.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="visualizer"/> or <paramref name="config"/> is null.</exception>
    public static void RenderToConsole(this PipelineVisualizer visualizer, PipelineConfig config)
    {
        ArgumentNullException.ThrowIfNull(visualizer);
        ArgumentNullException.ThrowIfNull(config);

        Console.Write(visualizer.Render(config));
    }

    /// <summary>
    /// Finds all stages in the pipeline that have critical health status.
    /// Returns stages where health is CRITICAL (backpressure active or buffer >= 95%).
    /// </summary>
    /// <param name="visualizer">The pipeline visualizer instance.</param>
    /// <param name="config">The pipeline configuration.</param>
    /// <returns>Read-only list of critical stages with their health status.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="visualizer"/> or <paramref name="config"/> is null.</exception>
    public static IReadOnlyList<(string StageName, string HealthStatus)> FindCriticalStages(
        this PipelineVisualizer visualizer,
        PipelineConfig config)
    {
        ArgumentNullException.ThrowIfNull(visualizer);
        ArgumentNullException.ThrowIfNull(config);

        var nodes = visualizer.BuildNodes(config);
        var criticalStages = new List<(string StageName, string HealthStatus)>(nodes.Count);

        foreach (var node in nodes)
        {
            if (node.HealthLabel is "CRITICAL")
            {
                criticalStages.Add((node.StageName, node.HealthLabel));
            }
        }

        return criticalStages.AsReadOnly();
    }

    /// <summary>
    /// Gets a summary of throughput across all pipeline stages.
    /// Returns minimum, maximum, and average throughput in events per second.
    /// </summary>
    /// <param name="visualizer">The pipeline visualizer instance.</param>
    /// <param name="config">The pipeline configuration.</param>
    /// <returns>Tuple containing min, max, and average throughput across all stages.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="visualizer"/> or <paramref name="config"/> is null.</exception>
    public static (double MinThroughput, double MaxThroughput, double AvgThroughput) GetStageThroughputSummary(
        this PipelineVisualizer visualizer,
        PipelineConfig config)
    {
        ArgumentNullException.ThrowIfNull(visualizer);
        ArgumentNullException.ThrowIfNull(config);

        var nodes = visualizer.BuildNodes(config);
        if (nodes.Count == 0)
        {
            return (0, 0, 0);
        }

        double minThroughput = double.MaxValue;
        double maxThroughput = double.MinValue;
        double totalThroughput = 0;

        foreach (var node in nodes)
        {
            minThroughput = Math.Min(minThroughput, node.ThroughputEps);
            maxThroughput = Math.Max(maxThroughput, node.ThroughputEps);
            totalThroughput += node.ThroughputEps;
        }

        double avgThroughput = totalThroughput / nodes.Count;
        return (minThroughput, maxThroughput, avgThroughput);
    }

}