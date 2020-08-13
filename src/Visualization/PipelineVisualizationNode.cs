#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Visualization;

using System.Collections.Generic;

/// <summary>
/// Represents a single stage node in the pipeline visualization graph.
/// Carries display-time state such as buffer fill, health, and throughput.
/// </summary>
public sealed class PipelineVisualizationNode
{
    /// <summary>Stage name shown in the diagram.</summary>
    public string StageName { get; set; } = "";

    /// <summary>Stage type label (SOURCE, TRANSFORM, SINK, …).</summary>
    public string StageType { get; set; } = "";

    /// <summary>Current buffer fill percentage (0–100).</summary>
    public double BufferFillPercent { get; set; }

    /// <summary>Whether backpressure is currently active for this stage.</summary>
    public bool IsBackpressured { get; set; }

    /// <summary>Throughput in events per second at this stage.</summary>
    public double ThroughputEps { get; set; }

    /// <summary>Number of items dropped at this stage.</summary>
    public long DroppedItems { get; set; }

    /// <summary>Human-readable health label (HEALTHY / WARNING / CRITICAL).</summary>
    public string HealthLabel { get; set; } = "HEALTHY";

    /// <summary>Ordered list of downstream stage names (edges in the DAG).</summary>
    public List<string> DownstreamStages { get; set; } = new();

    /// <summary>
    /// Calculates a health label based on buffer fill and backpressure state.
    /// </summary>
    public string ComputeHealthLabel()
    {
        if (IsBackpressured || BufferFillPercent >= 95) return "CRITICAL";
        if (BufferFillPercent >= 75) return "WARNING";
        return "HEALTHY";
    }

    /// <summary>
    /// Renders a compact single-line representation of this node.
    /// </summary>
    public string ToInlineString()
    {
        var health = ComputeHealthLabel();
        return $"[{StageName} | {health} | buf={BufferFillPercent:F0}% | {ThroughputEps:F1} eps]";
    }
}
