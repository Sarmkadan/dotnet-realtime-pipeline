#nullable enable

namespace DotNetRealtimePipeline.Tests;

using System;
using System.Collections.Generic;
using DotNetRealtimePipeline.Visualization;
using Xunit;

/// <summary>
/// Unit tests for <see cref="PipelineVisualizationNodeValidation"/> extension methods.
/// Tests all public validation methods: Validate, IsValid, and EnsureValid.
/// </summary>
public sealed class PipelineVisualizationNodeValidationTests
{
    [Fact]
    public void Validate_WithNullNode_ThrowsArgumentNullException()
    {
        PipelineVisualizationNode? node = null;

        var exception = Assert.Throws<ArgumentNullException>(() => node.Validate());
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Validate_WithValidNode_ReturnsEmptyList()
    {
        var node = new PipelineVisualizationNode
        {
            StageName = "Source",
            StageType = "SOURCE",
            BufferFillPercent = 50.0,
            ThroughputEps = 1000.0,
            DroppedItems = 0,
            HealthLabel = "HEALTHY",
            DownstreamStages = new List<string> { "Transform", "Sink" }
        };

        var result = node.Validate();

        Assert.Empty(result);
    }

    [Fact]
    public void Validate_WithValidNode_AllPropertiesValid()
    {
        var node = new PipelineVisualizationNode
        {
            StageName = "Transform",
            StageType = "TRANSFORM",
            BufferFillPercent = 100.0,
            ThroughputEps = 0.0,
            DroppedItems = 0,
            HealthLabel = "CRITICAL",
            DownstreamStages = new List<string>()
        };

        var result = node.Validate();

        Assert.Empty(result);
    }

    [Fact]
    public void Validate_WithEmptyStageName_ReturnsError()
    {
        var node = new PipelineVisualizationNode
        {
            StageName = "",
            StageType = "SOURCE",
            BufferFillPercent = 50.0,
            HealthLabel = "HEALTHY"
        };

        var result = node.Validate();

        Assert.Single(result);
        Assert.Contains("StageName must be a non-empty string.", result);
    }

    [Fact]
    public void Validate_WithNullStageName_ReturnsError()
    {
        var node = new PipelineVisualizationNode
        {
            StageName = null,
            StageType = "SOURCE",
            BufferFillPercent = 50.0,
            HealthLabel = "HEALTHY"
        };

        var result = node.Validate();

        Assert.Single(result);
        Assert.Contains("StageName must be a non-empty string.", result);
    }

    [Fact]
    public void Validate_WithEmptyStageType_ReturnsError()
    {
        var node = new PipelineVisualizationNode
        {
            StageName = "Source",
            StageType = "",
            BufferFillPercent = 50.0,
            HealthLabel = "HEALTHY"
        };

        var result = node.Validate();

        Assert.Single(result);
        Assert.Contains("StageType must be a non-empty string.", result);
    }

    [Fact]
    public void Validate_WithNullStageType_ReturnsError()
    {
        var node = new PipelineVisualizationNode
    {
        StageName = "Source",
        StageType = null,
        BufferFillPercent = 50.0,
        HealthLabel = "HEALTHY"
    };

        var result = node.Validate();

        Assert.Single(result);
        Assert.Contains("StageType must be a non-empty string.", result);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(-0.1)]
    public void Validate_WithNegativeBufferFillPercent_ReturnsError(double bufferFill)
    {
        var node = new PipelineVisualizationNode
        {
            StageName = "Source",
            StageType = "SOURCE",
            BufferFillPercent = bufferFill,
            HealthLabel = "HEALTHY"
        };

        var result = node.Validate();

        Assert.Single(result);
        Assert.Contains("BufferFillPercent must be between 0 and 100 inclusive.", result);
    }

    [Theory]
    [InlineData(101)]
    [InlineData(200)]
    [InlineData(100.1)]
    public void Validate_WithBufferFillPercentOver100_ReturnsError(double bufferFill)
    {
        var node = new PipelineVisualizationNode
        {
            StageName = "Source",
            StageType = "SOURCE",
            BufferFillPercent = bufferFill,
            HealthLabel = "HEALTHY"
        };

        var result = node.Validate();

        Assert.Single(result);
        Assert.Contains("BufferFillPercent must be between 0 and 100 inclusive.", result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    public void Validate_WithValidBufferFillPercent_ReturnsNoErrors(double bufferFill)
    {
        var node = new PipelineVisualizationNode
        {
            StageName = "Source",
            StageType = "SOURCE",
            BufferFillPercent = bufferFill,
            HealthLabel = "HEALTHY"
        };

        var result = node.Validate();

        Assert.Empty(result);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(-0.1)]
    public void Validate_WithNegativeThroughputEps_ReturnsError(double throughput)
    {
        var node = new PipelineVisualizationNode
        {
            StageName = "Source",
            StageType = "SOURCE",
            BufferFillPercent = 50.0,
            ThroughputEps = throughput,
            HealthLabel = "HEALTHY"
        };

        var result = node.Validate();

        Assert.Single(result);
        Assert.Contains("ThroughputEps must be non-negative.", result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1000.5)]
    [InlineData(1000000)]
    public void Validate_WithValidThroughputEps_ReturnsNoErrors(double throughput)
    {
        var node = new PipelineVisualizationNode
        {
            StageName = "Source",
            StageType = "SOURCE",
            BufferFillPercent = 50.0,
            ThroughputEps = throughput,
            HealthLabel = "HEALTHY"
        };

        var result = node.Validate();

        Assert.Empty(result);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_WithNegativeDroppedItems_ReturnsError(long droppedItems)
    {
        var node = new PipelineVisualizationNode
        {
            StageName = "Source",
            StageType = "SOURCE",
            BufferFillPercent = 50.0,
            DroppedItems = droppedItems,
            HealthLabel = "HEALTHY"
        };

        var result = node.Validate();

        Assert.Single(result);
        Assert.Contains("DroppedItems must be non-negative.", result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(1000000)]
    public void Validate_WithValidDroppedItems_ReturnsNoErrors(long droppedItems)
    {
        var node = new PipelineVisualizationNode
        {
            StageName = "Source",
            StageType = "SOURCE",
            BufferFillPercent = 50.0,
            DroppedItems = droppedItems,
            HealthLabel = "HEALTHY"
        };

        var result = node.Validate();

        Assert.Empty(result);
    }

    [Fact]
    public void Validate_WithInvalidHealthLabel_ReturnsError()
    {
        var node = new PipelineVisualizationNode
        {
            StageName = "Source",
            StageType = "SOURCE",
            BufferFillPercent = 50.0,
            HealthLabel = "INVALID"
        };

        var result = node.Validate();

        Assert.Single(result);
        Assert.Contains("HealthLabel must be one of: HEALTHY, WARNING, CRITICAL.", result);
    }

    [Fact]
    public void Validate_WithNullHealthLabel_ReturnsError()
    {
        var node = new PipelineVisualizationNode
        {
            StageName = "Source",
            StageType = "SOURCE",
            BufferFillPercent = 50.0,
            HealthLabel = null
        };

        var result = node.Validate();

        Assert.Single(result);
        Assert.Contains("HealthLabel must be one of: HEALTHY, WARNING, CRITICAL.", result);
    }

    [Fact]
    public void Validate_WithEmptyHealthLabel_ReturnsError()
    {
        var node = new PipelineVisualizationNode
        {
            StageName = "Source",
            StageType = "SOURCE",
            BufferFillPercent = 50.0,
            HealthLabel = ""
        };

        var result = node.Validate();

        Assert.Single(result);
        Assert.Contains("HealthLabel must be one of: HEALTHY, WARNING, CRITICAL.", result);
    }

    [Fact]
    public void Validate_WithCaseInsensitiveHealthLabel_ReturnsNoError()
    {
        var node = new PipelineVisualizationNode
        {
            StageName = "Source",
            StageType = "SOURCE",
            BufferFillPercent = 50.0,
            HealthLabel = "healthy"
        };

        var result = node.Validate();

        Assert.Empty(result);
    }

    [Fact]
    public void Validate_WithNullDownstreamStages_ReturnsError()
    {
        var node = new PipelineVisualizationNode
        {
            StageName = "Source",
            StageType = "SOURCE",
            BufferFillPercent = 50.0,
            HealthLabel = "HEALTHY",
            DownstreamStages = null
        };

        var result = node.Validate();

        Assert.Single(result);
        Assert.Contains("DownstreamStages must not be null.", result);
    }

    [Fact]
    public void Validate_WithEmptyDownstreamStagesList_ReturnsNoErrors()
    {
        var node = new PipelineVisualizationNode
        {
            StageName = "Source",
            StageType = "SOURCE",
            BufferFillPercent = 50.0,
            HealthLabel = "HEALTHY",
            DownstreamStages = new List<string>()
        };

        var result = node.Validate();

        Assert.Empty(result);
    }

    [Fact]
    public void Validate_WithNullItemInDownstreamStages_ReturnsError()
    {
        var node = new PipelineVisualizationNode
        {
            StageName = "Source",
            StageType = "SOURCE",
            BufferFillPercent = 50.0,
            HealthLabel = "HEALTHY",
            DownstreamStages = new List<string> { "Transform", null, "Sink" }
        };

        var result = node.Validate();

        Assert.Equal(1, result.Count);
        Assert.Contains("DownstreamStages[1] must be a non-empty string.", result);
    }

    [Fact]
    public void Validate_WithEmptyItemInDownstreamStages_ReturnsError()
    {
        var node = new PipelineVisualizationNode
        {
            StageName = "Source",
            StageType = "SOURCE",
            BufferFillPercent = 50.0,
            HealthLabel = "HEALTHY",
            DownstreamStages = new List<string> { "Transform", "", "Sink" }
        };

        var result = node.Validate();

        Assert.Single(result);
        Assert.Contains("DownstreamStages[1] must be a non-empty string.", result);
    }

    [Fact]
    public void Validate_WithMultipleErrors_ReturnsAllErrors()
    {
        var node = new PipelineVisualizationNode
        {
            StageName = "",
            StageType = "",
            BufferFillPercent = 150.0,
            ThroughputEps = -100.0,
            DroppedItems = -5,
            HealthLabel = "INVALID",
            DownstreamStages = null
        };

        var result = node.Validate();

        Assert.Equal(7, result.Count);
        Assert.Contains("StageName must be a non-empty string.", result);
        Assert.Contains("StageType must be a non-empty string.", result);
        Assert.Contains("BufferFillPercent must be between 0 and 100 inclusive.", result);
        Assert.Contains("ThroughputEps must be non-negative.", result);
        Assert.Contains("DroppedItems must be non-negative.", result);
        Assert.Contains("HealthLabel must be one of: HEALTHY, WARNING, CRITICAL.", result);
        Assert.Contains("DownstreamStages must not be null.", result);
    }

    [Fact]
    public void IsValid_WithNullNode_ReturnsFalse()
    {
        PipelineVisualizationNode? node = null;

        var result = node.IsValid();

        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithValidNode_ReturnsTrue()
    {
        var node = new PipelineVisualizationNode
        {
            StageName = "Source",
            StageType = "SOURCE",
            BufferFillPercent = 50.0,
            HealthLabel = "HEALTHY"
        };

        var result = node.IsValid();

        Assert.True(result);
    }

    [Fact]
    public void IsValid_WithInvalidNode_ReturnsFalse()
    {
        var node = new PipelineVisualizationNode
        {
            StageName = "",
            StageType = "SOURCE",
            BufferFillPercent = 50.0,
            HealthLabel = "HEALTHY"
        };

        var result = node.IsValid();

        Assert.False(result);
    }

    [Fact]
    public void EnsureValid_WithNullNode_ThrowsArgumentNullException()
    {
        PipelineVisualizationNode? node = null;

        var exception = Assert.Throws<ArgumentNullException>(() => node.EnsureValid());
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void EnsureValid_WithValidNode_DoesNotThrow()
    {
        var node = new PipelineVisualizationNode
        {
            StageName = "Source",
            StageType = "SOURCE",
            BufferFillPercent = 50.0,
            HealthLabel = "HEALTHY"
        };

        var exception = Record.Exception(() => node.EnsureValid());

        Assert.Null(exception);
    }

    [Fact]
    public void EnsureValid_WithInvalidNode_ThrowsArgumentException()
    {
        var node = new PipelineVisualizationNode
        {
            StageName = "",
            StageType = "SOURCE",
            BufferFillPercent = 50.0,
            HealthLabel = "HEALTHY"
        };

        var exception = Assert.Throws<ArgumentException>(() => node.EnsureValid());

        Assert.Equal("value", exception.ParamName);
        Assert.StartsWith("PipelineVisualizationNode is invalid:", exception.Message);
        Assert.Contains("StageName must be a non-empty string.", exception.Message);
    }

    [Fact]
    public void EnsureValid_WithMultipleErrors_ThrowsArgumentExceptionWithAllProblems()
    {
        var node = new PipelineVisualizationNode
        {
            StageName = "",
            StageType = "",
            BufferFillPercent = 150.0,
            ThroughputEps = -100.0,
            DroppedItems = -5,
            HealthLabel = "INVALID",
            DownstreamStages = null
        };

        var exception = Assert.Throws<ArgumentException>(() => node.EnsureValid());

        Assert.Equal("value", exception.ParamName);
        Assert.StartsWith("PipelineVisualizationNode is invalid:", exception.Message);
        Assert.Contains("StageName must be a non-empty string.", exception.Message);
        Assert.Contains("StageType must be a non-empty string.", exception.Message);
        Assert.Contains("BufferFillPercent must be between 0 and 100 inclusive.", exception.Message);
        Assert.Contains("ThroughputEps must be non-negative.", exception.Message);
        Assert.Contains("DroppedItems must be non-negative.", exception.Message);
        Assert.Contains("HealthLabel must be one of: HEALTHY, WARNING, CRITICAL.", exception.Message);
        Assert.Contains("DownstreamStages must not be null.", exception.Message);
    }
}