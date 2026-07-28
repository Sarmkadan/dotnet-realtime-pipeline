using System;
using System.Collections.Generic;
using Xunit;
using DotNetRealtimePipeline.Domain.Models;

namespace DotNetRealtimePipeline.Tests;

public sealed class StreamEventExtensionsTests
{
    [Fact]
    public void FilterPayload_HappyPath_ReturnsExpectedDictionary()
    {
        // Arrange
        var streamEvent = new StreamEvent
        {
            Payload = new Dictionary<string, object> { ["key1"] = "value1", ["key2"] = "value2" }
        };
        var keys = new[] { "key1" };

        // Act
        var result = streamEvent.FilterPayload(keys);

        // Assert
        Assert.Single(result);
        Assert.Contains(result, x => x.Key == "key1" && x.Value == "value1");
    }

    [Fact]
    public void GetPayload_HappyPath_ReturnsExpectedValue()
    {
        // Arrange
        var streamEvent = new StreamEvent
        {
            Payload = new Dictionary<string, object> { ["key1"] = "value1" }
        };

        // Act
        var result = streamEvent.GetPayload<string>("key1");

        // Assert
        Assert.Equal("value1", result);
    }

    [Fact]
    public void HasBeenProcessedByAnyStage_HappyPath_ReturnsTrue()
    {
        // Arrange
        var streamEvent = new StreamEvent
        {
            ProcessedByStages = new List<string> { "stage1" }
        };
        var stages = new[] { "stage1" };

        // Act
        var result = streamEvent.HasBeenProcessedByAnyStage(stages);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetRemainingStagesCount_HappyPath_ReturnsExpectedCount()
    {
        // Arrange
        var streamEvent = new StreamEvent
        {
            ProcessedByStages = new List<string> { "stage1" }
        };
        var allStages = new[] { "stage1", "stage2" };

        // Act
        var result = streamEvent.GetRemainingStagesCount(allStages);

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void DeepCopy_HappyPath_ReturnsDeepCopy()
    {
        // Arrange
        var streamEvent = new StreamEvent
        {
            EventId = 1,
            DataPointId = 2,
            Timestamp = DateTime.UtcNow,
            EventType = "type",
            Priority = 1,
            SourceSystem = "system",
            CorrelationId = "correlation",
            CausationId = "causation",
            IsRetry = true,
            RetryAttempt = 1,
            LastErrorMessage = "error",
            CreatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow,
            Payload = new Dictionary<string, object> { ["key1"] = "value1" },
            ProcessedByStages = new List<string> { "stage1" }
        };

        // Act
        var result = streamEvent.DeepCopy();

        // Assert
        Assert.NotSame(streamEvent, result);
        Assert.Equal(streamEvent.EventId, result.EventId);
        Assert.Equal(streamEvent.DataPointId, result.DataPointId);
        Assert.Equal(streamEvent.Timestamp, result.Timestamp);
        Assert.Equal(streamEvent.EventType, result.EventType);
        Assert.Equal(streamEvent.Priority, result.Priority);
        Assert.Equal(streamEvent.SourceSystem, result.SourceSystem);
        Assert.Equal(streamEvent.CorrelationId, result.CorrelationId);
        Assert.Equal(streamEvent.CausationId, result.CausationId);
        Assert.Equal(streamEvent.IsRetry, result.IsRetry);
        Assert.Equal(streamEvent.RetryAttempt, result.RetryAttempt);
        Assert.Equal(streamEvent.LastErrorMessage, result.LastErrorMessage);
        Assert.Equal(streamEvent.CreatedAt, result.CreatedAt);
        Assert.Equal(streamEvent.CompletedAt, result.CompletedAt);
        Assert.NotSame(streamEvent.Payload, result.Payload);
        Assert.Equal(streamEvent.Payload, result.Payload);
        Assert.NotSame(streamEvent.ProcessedByStages, result.ProcessedByStages);
        Assert.Equal(streamEvent.ProcessedByStages, result.ProcessedByStages);
    }

    [Fact]
    public void IsStale_HappyPath_ReturnsTrue()
    {
        // Arrange
        var streamEvent = new StreamEvent
        {
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };

        // Act
        var result = streamEvent.IsStale(30 * 60 * 1000); // 30 minutes

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetPriorityString_HappyPath_ReturnsExpectedString()
    {
        // Arrange
        var streamEvent = new StreamEvent
        {
            Priority = 1
        };

        // Act
        var result = streamEvent.GetPriorityString();

        // Assert
        Assert.Equal("Critical", result);
    }

    [Fact]
    public void GetPayloadAsJson_HappyPath_ReturnsExpectedJson()
    {
        // Arrange
        var streamEvent = new StreamEvent
        {
            Payload = new Dictionary<string, object> { ["key1"] = "value1" }
        };

        // Act
        var result = streamEvent.GetPayloadAsJson("key1");

        // Assert
        Assert.Equal("\"value1\"", result);
    }

    [Fact]
    public void HasFailed_HappyPath_ReturnsTrue()
    {
        // Arrange
        var streamEvent = new StreamEvent
        {
            LastErrorMessage = "error"
        };

        // Act
        var result = streamEvent.HasFailed();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetProcessingCompletionPercentage_HappyPath_ReturnsExpectedPercentage()
    {
        // Arrange
        var streamEvent = new StreamEvent
        {
            ProcessedByStages = new List<string> { "stage1" }
        };
        var totalStages = new[] { "stage1", "stage2" };

        // Act
        var result = streamEvent.GetProcessingCompletionPercentage(totalStages.Length);

        // Assert
        Assert.Equal(50, result);
    }
}
