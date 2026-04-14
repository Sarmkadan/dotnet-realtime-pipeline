#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Metrics;

using DotNetRealtimePipeline.Constants;
using DotNetRealtimePipeline.Services;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Collects time-series backpressure metrics by polling <see cref="BackpressureService"/>.
/// Records activation/release events, peak buffer levels, dropped item counts, and cumulative
/// duration so operators and dashboards can understand back-pressure behaviour over time.
/// </summary>
public sealed class BackpressureMetricsCollector
{
    private readonly BackpressureService _backpressureService;
    private readonly int _maxEventHistory;

    // Per-stage tracking state
    private readonly Dictionary<string, StageTrackingState> _stageState = new();
    private readonly List<BackpressureEvent> _eventHistory = new();
    private readonly object _lock = new();

    public BackpressureMetricsCollector(BackpressureService backpressureService, int maxEventHistory = 500)
    {
        _backpressureService = backpressureService ?? throw new ArgumentNullException(nameof(backpressureService));
        _maxEventHistory = maxEventHistory > 0
            ? maxEventHistory
            : PipelineConstants.MaxBackpressureEventHistory;
    }

    /// <summary>
    /// Polls the current state of all registered stages and records any transitions
    /// (backpressure activated or released).  Call this on a regular timer to build
    /// a meaningful history.
    /// </summary>
    public void Poll()
    {
        var bufferStatus = _backpressureService.GetBufferStatus();
        var sysStatus = _backpressureService.GetSystemStatus();
        var now = DateTime.UtcNow;

        lock (_lock)
        {
            foreach (var kv in bufferStatus)
            {
                var stageName = kv.Key;
                var ctx = _backpressureService.GetContext(stageName);
                if (ctx is null) continue;

                double fillPercent = ctx.GetBufferFillPercentage();
                bool isCurrentlyBackpressured = ctx.IsBackpressured;
                long dropped = ctx.DroppedItemCount;

                if (!_stageState.TryGetValue(stageName, out var state))
                {
                    state = new StageTrackingState { StageName = stageName };
                    _stageState[stageName] = state;
                }

                // Update peak
                if (fillPercent > state.PeakBufferFillPercent)
                    state.PeakBufferFillPercent = fillPercent;

                state.CurrentBufferFillPercent = fillPercent;
                state.TotalDroppedItems = dropped;

                // Detect activation transition
                if (isCurrentlyBackpressured && !state.WasBackpressured)
                {
                    state.ActivationCount++;
                    state.LastActivationAt = now;
                    state.WasBackpressured = true;
                    RecordEvent(stageName, fillPercent, dropped, isActivation: true, now);
                }

                // Detect release transition
                if (!isCurrentlyBackpressured && state.WasBackpressured)
                {
                    state.WasBackpressured = false;
                    if (state.LastActivationAt.HasValue)
                        state.TotalActiveDurationMs += (long)(now - state.LastActivationAt.Value).TotalMilliseconds;
                    RecordEvent(stageName, fillPercent, dropped, isActivation: false, now);
                }
            }
        }
    }

    /// <summary>
    /// Records a manual backpressure event for a stage without waiting for a poll cycle.
    /// Useful when the caller has observed a state transition externally.
    /// </summary>
    public void RecordManualEvent(string stageName, double bufferFillPercent, long droppedItems, bool isActivation)
    {
        if (string.IsNullOrWhiteSpace(stageName))
            throw new ArgumentException("Stage name cannot be null or empty.", nameof(stageName));

        lock (_lock)
        {
            RecordEvent(stageName, bufferFillPercent, droppedItems, isActivation, DateTime.UtcNow);

            if (!_stageState.TryGetValue(stageName, out var state))
            {
                state = new StageTrackingState { StageName = stageName };
                _stageState[stageName] = state;
            }

            if (isActivation)
            {
                state.ActivationCount++;
                state.LastActivationAt = DateTime.UtcNow;
                state.WasBackpressured = true;
            }
            else
            {
                state.WasBackpressured = false;
            }

            if (bufferFillPercent > state.PeakBufferFillPercent)
                state.PeakBufferFillPercent = bufferFillPercent;

            state.CurrentBufferFillPercent = bufferFillPercent;
            state.TotalDroppedItems = droppedItems;
        }
    }

    /// <summary>
    /// Returns per-stage metrics for a specific stage, or <c>null</c> if not tracked.
    /// </summary>
    public StageBackpressureMetrics? GetStageMetrics(string stageName)
    {
        if (string.IsNullOrWhiteSpace(stageName))
            throw new ArgumentException("Stage name cannot be null or empty.", nameof(stageName));

        lock (_lock)
        {
            if (!_stageState.TryGetValue(stageName, out var state))
                return null;

            return MapToMetrics(state);
        }
    }

    /// <summary>
    /// Returns a full pipeline-wide snapshot of backpressure metrics.
    /// </summary>
    public BackpressureMetricsSnapshot GetSnapshot()
    {
        lock (_lock)
        {
            var stageMetrics = _stageState.Values.Select(MapToMetrics).ToList();

            return new BackpressureMetricsSnapshot
            {
                StageMetrics = stageMetrics,
                TotalActivations = stageMetrics.Sum(s => s.ActivationCount),
                TotalDroppedItems = stageMetrics.Sum(s => s.TotalDroppedItems),
                ActiveBackpressureStages = stageMetrics.Count(s =>
                    _stageState.TryGetValue(s.StageName, out var st) && st.WasBackpressured),
                SnapshotAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Returns the most recent <paramref name="count"/> backpressure events across all stages.
    /// </summary>
    public IReadOnlyList<BackpressureEvent> GetRecentEvents(int count = 50)
    {
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));

        lock (_lock)
        {
            int skip = Math.Max(0, _eventHistory.Count - count);
            return _eventHistory.Skip(skip).ToList().AsReadOnly();
        }
    }

    /// <summary>
    /// Returns all recorded events for a specific stage.
    /// </summary>
    public IReadOnlyList<BackpressureEvent> GetStageEvents(string stageName)
    {
        if (string.IsNullOrWhiteSpace(stageName))
            throw new ArgumentException("Stage name cannot be null or empty.", nameof(stageName));

        lock (_lock)
        {
            return _eventHistory
                .Where(e => string.Equals(e.StageName, stageName, StringComparison.OrdinalIgnoreCase))
                .ToList()
                .AsReadOnly();
        }
    }

    /// <summary>
    /// Resets all collected metrics and event history.
    /// </summary>
    public void Reset()
    {
        lock (_lock)
        {
            _stageState.Clear();
            _eventHistory.Clear();
        }
    }

    // -------------------------------------------------------------------------

    private void RecordEvent(string stageName, double fillPercent, long dropped, bool isActivation, DateTime ts)
    {
        _eventHistory.Add(new BackpressureEvent
        {
            Timestamp = ts,
            StageName = stageName,
            BufferFillPercent = fillPercent,
            IsActivation = isActivation,
            DroppedItems = dropped
        });

        // Trim to max history
        while (_eventHistory.Count > _maxEventHistory)
            _eventHistory.RemoveAt(0);
    }

    private static StageBackpressureMetrics MapToMetrics(StageTrackingState state)
    {
        return new StageBackpressureMetrics
        {
            StageName = state.StageName,
            ActivationCount = state.ActivationCount,
            TotalActiveDurationMs = state.TotalActiveDurationMs,
            PeakBufferFillPercent = state.PeakBufferFillPercent,
            CurrentBufferFillPercent = state.CurrentBufferFillPercent,
            TotalDroppedItems = state.TotalDroppedItems,
            LastActivationAt = state.LastActivationAt
        };
    }

    // ── Internal tracking state per stage ────────────────────────────────────

    private sealed class StageTrackingState
    {
        public string StageName { get; set; } = "";
        public bool WasBackpressured { get; set; }
        public long ActivationCount { get; set; }
        public long TotalActiveDurationMs { get; set; }
        public double PeakBufferFillPercent { get; set; }
        public double CurrentBufferFillPercent { get; set; }
        public long TotalDroppedItems { get; set; }
        public DateTime? LastActivationAt { get; set; }
    }
}
