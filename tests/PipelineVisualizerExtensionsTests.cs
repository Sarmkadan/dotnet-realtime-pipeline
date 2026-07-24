// tests/PipelineVisualizerExtensionsTests.cs

namespace DotNetRealtimePipeline.Tests.Visualization;

using Xunit;
using System;
using System.Collections.Generic;
using DotNetRealtimePipeline.Visualization;
using DotNetRealtimePipeline.Domain.Models;

public class PipelineVisualizerExtensionsTests
{
    [Fact]
    public void RenderToConsole_HappyPath_ThrowsArgumentNullException()
    {
        // Arrange
        var visualizer = new PipelineVisualizer();
        var config = new PipelineConfig();

        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => visualizer.RenderToConsole(null, config));
        Assert.Throws<ArgumentNullException>(() => visualizer.RenderToConsole(visualizer, null));
    }

    [Fact]
    public void FindCriticalStages_HappyPath_ReturnsCriticalStages()
    {
        // Arrange
        var visualizer = new PipelineVisualizer();
        var config = new PipelineConfig();
        var nodes = new List<PipelineVisualizationNode>
        {
            new PipelineVisualizationNode { StageName = "Stage1", HealthLabel = "CRITICAL" },
            new PipelineVisualizationNode { StageName = "Stage2", HealthLabel = "OK" },
            new PipelineVisualizationNode { StageName = "Stage3", HealthLabel = "CRITICAL" }
        };

        // Act
        var criticalStages = visualizer.FindCriticalStages(config, nodes);

        // Assert
        Assert.Equal(2, criticalStages.Count);
        Assert.Equal("Stage1", criticalStages[0].StageName);
        Assert.Equal("CRITICAL", criticalStages[0].HealthStatus);
        Assert.Equal("Stage3", criticalStages[1].StageName);
        Assert.Equal("CRITICAL", criticalStages[1].HealthStatus);
    }

    [Fact]
    public void GetStageThroughputSummary_HappyPath_ReturnsThroughputSummary()
    {
        // Arrange
        var visualizer = new PipelineVisualizer();
        var config = new PipelineConfig();
        var nodes = new List<PipelineVisualizationNode>
        {
            new PipelineVisualizationNode { StageName = "Stage1", ThroughputEps = 10 },
            new PipelineVisualizationNode { StageName = "Stage2", ThroughputEps = 20 },
            new PipelineVisualizationNode { StageName = "Stage3", ThroughputEps = 30 }
        };

        // Act
        var throughputSummary = visualizer.GetStageThroughputSummary(config, nodes);

        // Assert
        Assert.Equal(10, throughputSummary.MinThroughput);
        Assert.Equal(30, throughputSummary.MaxThroughput);
        Assert.Equal(20, throughputSummary.AvgThroughput);
    }

    [Fact]
    public void RenderToConsole_EmptyNodes_ThrowsArgumentNullException()
    {
        // Arrange
        var visualizer = new PipelineVisualizer();
        var config = new PipelineConfig();
        var nodes = new List<PipelineVisualizationNode>();

        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => visualizer.RenderToConsole(visualizer, config, nodes));
    }

    [Fact]
    public void FindCriticalStages_EmptyNodes_ReturnsEmptyList()
    {
        // Arrange
        var visualizer = new PipelineVisualizer();
        var config = new PipelineConfig();
        var nodes = new List<PipelineVisualizationNode>();

        // Act
        var criticalStages = visualizer.FindCriticalStages(config, nodes);

        // Assert
        Assert.Empty(criticalStages);
    }

    [Fact]
    public void GetStageThroughputSummary_EmptyNodes_ReturnsDefaultValues()
    {
        // Arrange
        var visualizer = new PipelineVisualizer();
        var config = new PipelineConfig();
        var nodes = new List<PipelineVisualizationNode>();

        // Act
        var throughputSummary = visualizer.GetStageThroughputSummary(config, nodes);

        // Assert
        Assert.Equal(0, throughputSummary.MinThroughput);
        Assert.Equal(0, throughputSummary.MaxThroughput);
        Assert.Equal(0, throughputSummary.AvgThroughput);
    }
}
