#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetRealtimePipeline.Data.Repositories;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Metrics;
using DotNetRealtimePipeline.Services;
using DotNetRealtimePipeline.Visualization;
using Xunit;

namespace DotNetRealtimePipeline.Tests.Unit;

public sealed class PipelineVisualizerTests
{
    private static PipelineConfig BuildConfig()
    {
        var config = new PipelineConfig(1, "TestPipeline", "1.0.0");
        config.AddStage(new PipelineStageDef("Ingestion", "SOURCE"));
        config.AddStage(new PipelineStageDef("Transform", "TRANSFORM"));
        config.AddStage(new PipelineStageDef("Output", "SINK"));
        return config;
    }

    private static PipelineVisualizer BuildVisualizer()
    {
        var backpressure = new BackpressureService();
        var metrics = new MetricsService(new InMemoryMetricsRepository(), new ThroughputCounter());
        return new PipelineVisualizer(backpressure, metrics);
    }

    [Fact]
    public void BuildNodes_WithValidConfig_ReturnsOneNodePerStage()
    {
        // Arrange
        var visualizer = BuildVisualizer();
        var config = BuildConfig();

        // Act
        var nodes = visualizer.BuildNodes(config);

        // Assert
        Assert.Equal(3, nodes.Count);
        Assert.Equal("Ingestion", nodes[0].StageName);
        Assert.Equal("Transform", nodes[1].StageName);
        Assert.Equal("Output", nodes[2].StageName);
    }

    [Fact]
    public void BuildNodes_EdgesAreLinkedSequentially()
    {
        var visualizer = BuildVisualizer();
        var config = BuildConfig();

        var nodes = visualizer.BuildNodes(config);

        // First stage points to second
        Assert.Single(nodes[0].DownstreamStages);
        Assert.Equal("Transform", nodes[0].DownstreamStages[0]);

        // Last stage has no downstream
        Assert.Empty(nodes[2].DownstreamStages);
    }

    [Fact]
    public void Render_ContainsPipelineName()
    {
        var visualizer = BuildVisualizer();
        var config = BuildConfig();

        var output = visualizer.Render(config);

        Assert.Contains("TestPipeline", output);
    }

    [Fact]
    public void Render_ContainsAllStageNames()
    {
        var visualizer = BuildVisualizer();
        var config = BuildConfig();

        var output = visualizer.Render(config);

        Assert.Contains("Ingestion", output);
        Assert.Contains("Transform", output);
        Assert.Contains("Output", output);
    }

    [Fact]
    public void RenderCompact_ContainsSeparators()
    {
        var visualizer = BuildVisualizer();
        var config = BuildConfig();

        var compact = visualizer.RenderCompact(config);

        // Expect at least two " -> " separators for three stages
        Assert.Equal(2, CountOccurrences(compact, "->"));
    }

    [Fact]
    public void PipelineVisualizationNode_ComputeHealthLabel_BackpressuredIsCritical()
    {
        var node = new PipelineVisualizationNode { IsBackpressured = true, BufferFillPercent = 50 };
        Assert.Equal("CRITICAL", node.ComputeHealthLabel());
    }

    [Fact]
    public void PipelineVisualizationNode_ComputeHealthLabel_HighBufferIsWarning()
    {
        var node = new PipelineVisualizationNode { IsBackpressured = false, BufferFillPercent = 80 };
        Assert.Equal("WARNING", node.ComputeHealthLabel());
    }

    [Fact]
    public void PipelineVisualizationNode_ComputeHealthLabel_NormalIsHealthy()
    {
        var node = new PipelineVisualizationNode { IsBackpressured = false, BufferFillPercent = 30 };
        Assert.Equal("HEALTHY", node.ComputeHealthLabel());
    }

    private static int CountOccurrences(string text, string pattern)
    {
        int count = 0;
        int index = 0;
        while ((index = text.IndexOf(pattern, index, StringComparison.Ordinal)) != -1)
        {
            count++;
            index += pattern.Length;
        }
        return count;
    }
}
