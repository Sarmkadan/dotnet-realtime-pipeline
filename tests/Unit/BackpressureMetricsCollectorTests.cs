#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetRealtimePipeline.Metrics;
using DotNetRealtimePipeline.Services;
using Xunit;

namespace DotNetRealtimePipeline.Tests.Unit;

public sealed class BackpressureMetricsCollectorTests
{
    private static BackpressureService NewService() => new();

    private static BackpressureMetricsCollector NewCollector(BackpressureService svc)
        => new(svc, maxEventHistory: 200);

    // ── GetStageMetrics ──────────────────────────────────────────────────────

    [Fact]
    public void GetStageMetrics_UnknownStage_ReturnsNull()
    {
        var svc = NewService();
        var collector = NewCollector(svc);

        var result = collector.GetStageMetrics("NonExistent");

        Assert.Null(result);
    }

    [Fact]
    public void RecordManualEvent_Activation_IncrementsActivationCount()
    {
        var svc = NewService();
        var collector = NewCollector(svc);

        collector.RecordManualEvent("StageA", 85.0, 0, isActivation: true);
        var metrics = collector.GetStageMetrics("StageA");

        Assert.NotNull(metrics);
        Assert.Equal(1, metrics!.ActivationCount);
        Assert.Equal(85.0, metrics.PeakBufferFillPercent);
    }

    [Fact]
    public void RecordManualEvent_TwoActivations_CountIsTwo()
    {
        var svc = NewService();
        var collector = NewCollector(svc);

        collector.RecordManualEvent("StageB", 80.0, 0, isActivation: true);
        collector.RecordManualEvent("StageB", 90.0, 5, isActivation: true);
        var metrics = collector.GetStageMetrics("StageB");

        Assert.NotNull(metrics);
        Assert.Equal(2, metrics!.ActivationCount);
        Assert.Equal(90.0, metrics.PeakBufferFillPercent);
        Assert.Equal(5, metrics.TotalDroppedItems);
    }

    // ── GetSnapshot ──────────────────────────────────────────────────────────

    [Fact]
    public void GetSnapshot_WithNoEvents_ReturnsEmptySnapshot()
    {
        var svc = NewService();
        var collector = NewCollector(svc);

        var snapshot = collector.GetSnapshot();

        Assert.Empty(snapshot.StageMetrics);
        Assert.Equal(0, snapshot.TotalActivations);
        Assert.Equal(0, snapshot.TotalDroppedItems);
    }

    [Fact]
    public void GetSnapshot_AggregatesAcrossStages()
    {
        var svc = NewService();
        var collector = NewCollector(svc);

        collector.RecordManualEvent("S1", 80, 2, true);
        collector.RecordManualEvent("S2", 95, 8, true);

        var snapshot = collector.GetSnapshot();

        Assert.Equal(2, snapshot.TotalActivations);
        Assert.Equal(10, snapshot.TotalDroppedItems);
        Assert.Equal(2, snapshot.StageMetrics.Count);
    }

    // ── GetRecentEvents ──────────────────────────────────────────────────────

    [Fact]
    public void GetRecentEvents_ReturnsUpToRequestedCount()
    {
        var svc = NewService();
        var collector = NewCollector(svc);

        for (int i = 0; i < 10; i++)
            collector.RecordManualEvent("Stage", i * 5, 0, i % 2 == 0);

        var events = collector.GetRecentEvents(5);

        Assert.Equal(5, events.Count);
    }

    [Fact]
    public void GetStageEvents_ReturnsOnlyEventsForThatStage()
    {
        var svc = NewService();
        var collector = NewCollector(svc);

        collector.RecordManualEvent("Alpha", 80, 0, true);
        collector.RecordManualEvent("Beta", 70, 0, true);
        collector.RecordManualEvent("Alpha", 60, 0, false);

        var alphaEvents = collector.GetStageEvents("Alpha");

        Assert.Equal(2, alphaEvents.Count);
        Assert.All(alphaEvents, e => Assert.Equal("Alpha", e.StageName));
    }

    // ── Reset ────────────────────────────────────────────────────────────────

    [Fact]
    public void Reset_ClearsAllMetricsAndEvents()
    {
        var svc = NewService();
        var collector = NewCollector(svc);

        collector.RecordManualEvent("Stage", 90, 3, true);
        collector.Reset();

        var snapshot = collector.GetSnapshot();
        Assert.Empty(snapshot.StageMetrics);
        Assert.Empty(collector.GetRecentEvents(10));
    }

    // ── Poll integration ─────────────────────────────────────────────────────

    [Fact]
    public void Poll_AfterBackpressureActivated_RecordsActivationEvent()
    {
        var svc = NewService();
        svc.CreateContext("PollStage", 100);

        // Fill buffer to capacity and trigger backpressure
        svc.TryAddToBuffer("PollStage", 100);
        svc.TryAddToBuffer("PollStage", 50); // This triggers activation inside TryAddToBuffer

        var collector = NewCollector(svc);
        collector.Poll();

        var events = collector.GetStageEvents("PollStage");
        Assert.NotEmpty(events);
    }
}
