// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Domain.Models;

using System;

/// <summary>Indicates the direction of a dynamic scaling adjustment for a pipeline stage.</summary>
public enum ScalingDirection
{
    /// <summary>Consumer count is unchanged.</summary>
    None,

    /// <summary>Consumer count is increased to handle elevated load.</summary>
    Up,

    /// <summary>Consumer count is decreased to reclaim idle resources.</summary>
    Down
}

/// <summary>Describes a single scaling decision applied to a pipeline stage.</summary>
public sealed class ScalingDecision
{
    /// <summary>Name of the pipeline stage this decision targets.</summary>
    public string StageName { get; init; } = string.Empty;

    /// <summary>Direction of the consumer count adjustment.</summary>
    public ScalingDirection Direction { get; init; }

    /// <summary>Human-readable rationale for the decision.</summary>
    public string Reason { get; init; } = string.Empty;

    /// <summary>Maximum consumer limit before the decision was applied.</summary>
    public int FromConsumers { get; init; }

    /// <summary>Maximum consumer limit after the decision is applied.</summary>
    public int ToConsumers { get; init; }

    /// <summary>Buffer fill percentage observed when the decision was evaluated.</summary>
    public double BufferFillPercent { get; init; }

    /// <summary>Backpressure event frequency (events per minute) at decision time.</summary>
    public double BackpressureFrequency { get; init; }

    /// <summary>UTC timestamp when the decision was made.</summary>
    public DateTime DecidedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>Tracks the live scaling state and history for a single pipeline stage.</summary>
public sealed class StageScalingState
{
    /// <summary>Name of the monitored pipeline stage.</summary>
    public string StageName { get; init; } = string.Empty;

    /// <summary>Current maximum consumer count applied to this stage.</summary>
    public int CurrentConsumers { get; set; }

    /// <summary>Most recent scaling decision applied to this stage.</summary>
    public ScalingDecision? LastDecision { get; set; }

    /// <summary>UTC timestamp of the last applied scaling action.</summary>
    public DateTime LastScalingActionAt { get; set; } = DateTime.MinValue;

    /// <summary>Total number of scale-up operations performed on this stage.</summary>
    public int ScaleUpCount { get; set; }

    /// <summary>Total number of scale-down operations performed on this stage.</summary>
    public int ScaleDownCount { get; set; }
}
