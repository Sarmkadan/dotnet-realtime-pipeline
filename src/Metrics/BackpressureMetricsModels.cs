#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Metrics;

using System;
using System.Collections.Generic;

/// <summary>
/// A time-stamped backpressure event recorded by <see cref="BackpressureMetricsCollector"/>.
/// </summary>
public sealed class BackpressureEvent
{
    /// <summary>UTC time the event was captured.</summary>
    public DateTime Timestamp { get; set; }

    /// <summary>Name of the pipeline stage that triggered backpressure.</summary>
    public string StageName { get; set; } = "";

    /// <summary>Buffer fill percentage at the moment of the event (0–100).</summary>
    public double BufferFillPercent { get; set; }

    /// <summary><c>true</c> when backpressure was activated; <c>false</c> when it was released.</summary>
    public bool IsActivation { get; set; }

    /// <summary>Number of items dropped at this stage when the event was captured.</summary>
    public long DroppedItems { get; set; }
}

/// <summary>
/// Per-stage backpressure metrics snapshot.
/// </summary>
public sealed class StageBackpressureMetrics
{
    /// <summary>Stage name.</summary>
    public string StageName { get; set; } = "";

    /// <summary>Number of times backpressure was activated for this stage.</summary>
    public long ActivationCount { get; set; }

    /// <summary>Cumulative milliseconds that backpressure was active.</summary>
    public long TotalActiveDurationMs { get; set; }

    /// <summary>Peak buffer fill percentage observed.</summary>
    public double PeakBufferFillPercent { get; set; }

    /// <summary>Current buffer fill percentage.</summary>
    public double CurrentBufferFillPercent { get; set; }

    /// <summary>Total items dropped at this stage.</summary>
    public long TotalDroppedItems { get; set; }

    /// <summary>UTC timestamp of the last activation event.</summary>
    public DateTime? LastActivationAt { get; set; }
}

/// <summary>
/// Aggregated backpressure metrics across the whole pipeline.
/// </summary>
public sealed class BackpressureMetricsSnapshot
{
    /// <summary>Per-stage metrics.</summary>
    public List<StageBackpressureMetrics> StageMetrics { get; set; } = new();

    /// <summary>Total backpressure activation events across all stages.</summary>
    public long TotalActivations { get; set; }

    /// <summary>Total items dropped across all stages.</summary>
    public long TotalDroppedItems { get; set; }

    /// <summary>Number of stages currently under backpressure.</summary>
    public int ActiveBackpressureStages { get; set; }

    /// <summary>UTC time the snapshot was taken.</summary>
    public DateTime SnapshotAt { get; set; }
}
