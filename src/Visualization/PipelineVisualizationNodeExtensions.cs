#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Visualization;

using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Provides extension methods for <see cref="PipelineVisualizationNode"/> to simplify
/// common visualization and analysis operations on pipeline stage nodes.
/// </summary>
public static class PipelineVisualizationNodeExtensions
{
    /// <summary>
    /// Determines if this node is in a critical state based on its health label.
    /// </summary>
    /// <param name="node">The pipeline visualization node to check.</param>
    /// <returns>True if the node's health is CRITICAL; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="node"/> is null.</exception>
    public static bool IsCritical(this PipelineVisualizationNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return string.Equals(node.HealthLabel, "CRITICAL", StringComparison.Ordinal);
    }

    /// <summary>
    /// Determines if this node is in a warning state based on its health label.
    /// </summary>
    /// <param name="node">The pipeline visualization node to check.</param>
    /// <returns>True if the node's health is WARNING; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="node"/> is null.</exception>
    public static bool IsWarning(this PipelineVisualizationNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return string.Equals(node.HealthLabel, "WARNING", StringComparison.Ordinal);
    }

    /// <summary>
    /// Determines if this node is in a healthy state based on its health label.
    /// </summary>
    /// <param name="node">The pipeline visualization node to check.</param>
    /// <returns>True if the node's health is HEALTHY; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="node"/> is null.</exception>
    public static bool IsHealthy(this PipelineVisualizationNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return string.Equals(node.HealthLabel, "HEALTHY", StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets the downstream stages as a read-only list for safe enumeration.
    /// </summary>
    /// <param name="node">The pipeline visualization node.</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> of downstream stage names.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="node"/> is null.</exception>
    public static IReadOnlyList<string> GetDownstreamStages(this PipelineVisualizationNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return node.DownstreamStages.AsReadOnly();
    }

    /// <summary>
    /// Formats the throughput value as a human-readable string with appropriate units.
    /// </summary>
    /// <param name="node">The pipeline visualization node.</param>
    /// <returns>A formatted throughput string (e.g., "1.2K eps", "5.6M eps").</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="node"/> is null.</exception>
    public static string FormatThroughput(this PipelineVisualizationNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return FormatNumber(node.ThroughputEps) + " eps";
    }

    /// <summary>
    /// Formats the buffer fill percentage as a human-readable string.
    /// </summary>
    /// <param name="node">The pipeline visualization node.</param>
    /// <returns>A formatted buffer fill string with percent sign.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="node"/> is null.</exception>
    public static string FormatBufferFill(this PipelineVisualizationNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return node.BufferFillPercent.ToString("F1", CultureInfo.InvariantCulture) + "%";
    }

    /// <summary>
    /// Gets a color code suitable for visualizing this node's health state.
    /// Returns "#FF0000" for CRITICAL, "#FFA500" for WARNING, and "#008000" for HEALTHY.
    /// </summary>
    /// <param name="node">The pipeline visualization node.</param>
    /// <returns>A hex color code string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="node"/> is null.</exception>
    public static string GetHealthColor(this PipelineVisualizationNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return node.HealthLabel switch
        {
            "CRITICAL" => "#FF0000",
            "WARNING" => "#FFA500",
            _ => "#008000"
        };
    }

    /// <summary>
    /// Determines if this node has any downstream connections.
    /// </summary>
    /// <param name="node">The pipeline visualization node.</param>
    /// <returns>True if the node has one or more downstream stages; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="node"/> is null.</exception>
    public static bool HasDownstream(this PipelineVisualizationNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return node.DownstreamStages.Count > 0;
    }

    /// <summary>
    /// Gets a summary string suitable for tooltips or status displays.
    /// </summary>
    /// <param name="node">The pipeline visualization node.</param>
    /// <returns>A formatted summary string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="node"/> is null.</exception>
    public static string GetStatusSummary(this PipelineVisualizationNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return $"{node.StageName} ({node.StageType}) | {node.HealthLabel} | " +
               $"Buffer: {node.FormatBufferFill()} | Throughput: {node.FormatThroughput()} | " +
               $"Dropped: {node.DroppedItems:N0} | Backpressure: {(node.IsBackpressured ? "ACTIVE" : "INACTIVE")}";
    }

    /// <summary>
    /// Formats a number with appropriate SI suffix (K, M, B, etc.).
    /// </summary>
    /// <param name="value">The numeric value to format.</param>
    /// <returns>A formatted string with suffix.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is negative.</exception>
    private static string FormatNumber(double value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);

        if (value >= 1_000_000_000)
        {
            return (value / 1_000_000_000).ToString("F2", CultureInfo.InvariantCulture) + "B";
        }

        if (value >= 1_000_000)
        {
            return (value / 1_000_000).ToString("F2", CultureInfo.InvariantCulture) + "M";
        }

        if (value >= 1_000)
        {
            return (value / 1_000).ToString("F2", CultureInfo.InvariantCulture) + "K";
        }

        return value.ToString("F2", CultureInfo.InvariantCulture);
    }
}