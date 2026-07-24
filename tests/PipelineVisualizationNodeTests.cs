#nullable enable
using System.Collections.Generic;
using DotNetRealtimePipeline.Visualization;
using Xunit;

namespace DotNetRealtimePipeline.Tests;

/// <summary>
/// Unit tests for <see cref="PipelineVisualizationNode"/>.
/// Covers property defaults, <c>ComputeHealthLabel</c> and <c>ToInlineString</c>.
/// </summary>
public sealed class PipelineVisualizationNodeTests
{
    [Fact]
    public void DefaultConstructor_InitializesPropertiesWithExpectedDefaults()
    {
        var node = new PipelineVisualizationNode();

        Assert.Equal(string.Empty, node.StageName);
        Assert.Equal(string.Empty, node.StageType);
        Assert.Equal(0.0, node.BufferFillPercent);
        Assert.False(node.IsBackpressured);
        Assert.Equal(0.0, node.ThroughputEps);
        Assert.Equal(0L, node.DroppedItems);
        Assert.Equal("HEALTHY", node.HealthLabel);
        Assert.Empty(node.DownstreamStages);
    }

    [Theory]
    [InlineData(false, 94.9, "HEALTHY")]
    [InlineData(false, 75.0, "WARNING")]
    [InlineData(false, 94.9, "WARNING")]
    [InlineData(false, 95.0, "CRITICAL")]
    [InlineData(true, 0.0, "CRITICAL")]
    [InlineData(false, -5.0, "HEALTHY")] // negative buffer fill should be treated as healthy
    public void ComputeHealthLabel_ReturnsExpectedLabel(bool backpressured, double bufferFill, string expected)
    {
        var node = new PipelineVisualizationNode
        {
            IsBackpressured = backpressured,
            BufferFillPercent = bufferFill
        };

        var actual = node.ComputeHealthLabel();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToInlineString_FormatsStringCorrectly_WithDefaultHealth()
    {
        var node = new PipelineVisualizationNode
        {
            StageName = "Source",
            BufferFillPercent = 12.34,
            ThroughputEps = 1234.567,
            IsBackpressured = false
        };

        // Health should be computed as "HEALTHY"
        var expected = "[Source | HEALTHY | buf=12% | 1234.6 eps]";
        var actual = node.ToInlineString();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToInlineString_UsesComputedHealthLabel_WhenBackpressured()
    {
        var node = new PipelineVisualizationNode
        {
            StageName = "Transform",
            BufferFillPercent = 80.0,
            ThroughputEps = 500.0,
            IsBackpressured = true
        };

        // Health should be "CRITICAL" because backpressure overrides buffer fill
        var expected = "[Transform | CRITICAL | buf=80% | 500.0 eps]";
        var actual = node.ToInlineString();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DownstreamStages_CanBePopulatedAndRead()
    {
        var node = new PipelineVisualizationNode
        {
            StageName = "Aggregator",
            DownstreamStages = new List<string> { "SinkA", "SinkB" }
        };

        Assert.Equal(2, node.DownstreamStages.Count);
        Assert.Contains("SinkA", node.DownstreamStages);
        Assert.Contains("SinkB", node.DownstreamStages);
    }

    [Fact]
    public void ComputeHealthLabel_DoesNotThrow_OnExtremeValues()
    {
        var node = new PipelineVisualizationNode
        {
            BufferFillPercent = double.MaxValue,
            IsBackpressured = false
        };

        // Even with absurd values the method should simply return "CRITICAL"
        var label = node.ComputeHealthLabel();

        Assert.Equal("CRITICAL", label);
    }
}
