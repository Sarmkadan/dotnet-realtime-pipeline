using Xunit;
using DotNetRealtimePipeline.Domain.Models;
using System;

namespace DotNetRealtimePipeline.Tests;

public sealed class ScalingDecisionTests
{
    [Fact]
    public void ScalingDecision_Initialization_SetsPropertiesCorrectly()
    {
        // Arrange
        var decision = new ScalingDecision
        {
            StageName = "Processor",
            Direction = ScalingDirection.Up,
            Reason = "High load detected",
            FromConsumers = 2,
            ToConsumers = 4,
            BufferFillPercent = 85.5,
            BackpressureFrequency = 10.2,
            DecidedAt = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc)
        };

        // Act & Assert
        Assert.Equal("Processor", decision.StageName);
        Assert.Equal(ScalingDirection.Up, decision.Direction);
        Assert.Equal("High load detected", decision.Reason);
        Assert.Equal(2, decision.FromConsumers);
        Assert.Equal(4, decision.ToConsumers);
        Assert.Equal(85.5, decision.BufferFillPercent);
        Assert.Equal(10.2, decision.BackpressureFrequency);
        Assert.Equal(new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc), decision.DecidedAt);
    }

    [Fact]
    public void ScalingDecision_DefaultValues_AreExpected()
    {
        // Arrange
        var decision = new ScalingDecision();

        // Act & Assert
        Assert.Equal(string.Empty, decision.StageName);
        Assert.Equal(ScalingDirection.None, decision.Direction);
        Assert.Equal(string.Empty, decision.Reason);
        Assert.Equal(0, decision.FromConsumers);
        Assert.Equal(0, decision.ToConsumers);
        Assert.Equal(0.0, decision.BufferFillPercent);
        Assert.Equal(0.0, decision.BackpressureFrequency);
        // DecidedAt defaults to DateTime.UtcNow, so we just check it's not default
        Assert.NotEqual(default, decision.DecidedAt);
    }

    [Fact]
    public void StageScalingState_Initialization_SetsPropertiesCorrectly()
    {
        // Arrange
        var state = new StageScalingState
        {
            StageName = "Aggregator",
            CurrentConsumers = 5,
            LastScalingActionAt = DateTime.UtcNow,
            ScaleUpCount = 3,
            ScaleDownCount = 1
        };

        // Act & Assert
        Assert.Equal("Aggregator", state.StageName);
        Assert.Equal(5, state.CurrentConsumers);
        Assert.Equal(3, state.ScaleUpCount);
        Assert.Equal(1, state.ScaleDownCount);
    }

    [Fact]
    public void StageScalingState_LastDecision_CanBeSet()
    {
        // Arrange
        var decision = new ScalingDecision 
        { 
            StageName = "TestStage", 
            Direction = ScalingDirection.Down 
        };
        var state = new StageScalingState();

        // Act
        state.LastDecision = decision;

        // Assert
        Assert.NotNull(state.LastDecision);
        Assert.Equal("TestStage", state.LastDecision.StageName);
        Assert.Equal(ScalingDirection.Down, state.LastDecision.Direction);
    }

    [Fact]
    public void StageScalingState_DefaultValues_AreExpected()
    {
        // Arrange
        var state = new StageScalingState();

        // Act & Assert
        Assert.Equal(string.Empty, state.StageName);
        Assert.Equal(0, state.CurrentConsumers);
        Assert.Null(state.LastDecision);
        Assert.Equal(DateTime.MinValue, state.LastScalingActionAt);
        Assert.Equal(0, state.ScaleUpCount);
        Assert.Equal(0, state.ScaleDownCount);
    }
}
