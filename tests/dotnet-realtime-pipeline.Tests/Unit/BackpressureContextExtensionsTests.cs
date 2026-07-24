namespace DotNetRealtimePipeline.Tests.Unit;

using Xunit;
using System;
using System.Collections.Generic;
using DotNetRealtimePipeline.Domain.Models;

public class BackpressureContextExtensionsTests
{
    private BackpressureContext CreateContext()
    {
        return new BackpressureContext(1, "test-stage", 1000);
    }

    [Fact]
    public void EstimateTimeToCapacity_ValidConsumption_ReturnsEstimatedTime()
    {
        var context = CreateContext();
        context.BufferSize = 500; // 500 remaining

        // 100 items/sec -> 500 / 100 = 5 sec = 5000ms
        long time = context.EstimateTimeToCapacity(100);
        Assert.Equal(5000, time);
    }

    [Fact]
    public void EstimateTimeToCapacity_EmptyBuffer_ReturnsZero()
    {
        var context = CreateContext();
        context.BufferSize = 1000; // At capacity

        long time = context.EstimateTimeToCapacity(100);
        Assert.Equal(0, time);
    }

    [Fact]
    public void IsCriticallyFull_ThresholdsReached_ReturnsTrue()
    {
        var context = CreateContext();
        context.BufferSize = 950; // 95% full

        Assert.True(context.IsCriticallyFull(percentageThreshold: 90));
        Assert.True(context.IsCriticallyFull(absoluteThreshold: 900));
    }

    [Fact]
    public void GetBackpressureDurationFormatted_ValidTime_ReturnsFormattedString()
    {
        var context = CreateContext();
        context.TotalBackpressureTimeMs = 3661000; // 1 hour, 1 min, 1 sec

        string formatted = context.GetBackpressureDurationFormatted();
        Assert.Equal("01:01:01", formatted);
    }

    [Fact]
    public void RecordBackpressureEvent_ValidInput_RecordsMetrics()
    {
        var context = CreateContext();
        var metadata = new Dictionary<string, string> { { "key", "10" } };

        context.RecordBackpressureEvent("TestEvent", metadata);

        Assert.True(context.IsBackpressured);
        Assert.True(context.BufferMetrics.ContainsKey("BackpressureEvent_TestEvent"));
        Assert.True(context.BufferMetrics.ContainsKey("EventMeta_TestEvent_key"));
        Assert.Equal(10, context.BufferMetrics["EventMeta_TestEvent_key"]);
    }

    [Fact]
    public void SafeRemoveFromBuffer_ValidAmount_RemovesCorrectly()
    {
        var context = CreateContext();
        context.BufferSize = 500;

        long removed = context.SafeRemoveFromBuffer(200);
        
        Assert.Equal(200, removed);
        Assert.Equal(300, context.BufferSize);
    }

    [Fact]
    public void HasSufficientCapacityForBatch_EnoughCapacity_ReturnsTrue()
    {
        var context = CreateContext();
        context.BufferSize = 100;
        // Capacity 1000. 20% is 200. Used 100. Remaining: 900.
        // Need to check for: context.BufferSize + batchSize <= context.MaxBufferCapacity * (requiredCapacityPercent / 100d)
        // 100 + 50 <= 1000 * 0.2 = 200
        // 150 <= 200 -> True
        
        Assert.True(context.HasSufficientCapacityForBatch(50, 20));
    }

    [Fact]
    public void HasSufficientCapacityForBatch_NotEnoughCapacity_ReturnsFalse()
    {
        var context = CreateContext();
        context.BufferSize = 100;
        
        // 100 + 200 <= 1000 * 0.2 = 200
        // 300 <= 200 -> False
        Assert.False(context.HasSufficientCapacityForBatch(200, 20));
    }

    [Fact]
    public void EstimateTimeToCapacity_NullContext_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ((BackpressureContext)null!).EstimateTimeToCapacity(100));
    }
}
