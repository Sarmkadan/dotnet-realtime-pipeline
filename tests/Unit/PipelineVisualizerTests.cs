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

/// <summary>
/// Test suite for verifying pipeline visualization functionality including node construction, 
/// health status computation, and rendering of pipeline topology.
/// </summary>
public sealed class PipelineVisualizerTests
{
    /// <summary>
    /// Builds a standard test pipeline configuration with three sequential stages: 
    /// "Ingestion" (SOURCE), "Transform" (TRANSFORM), and "Output" (SINK).
    /// </summary>
    /// <returns>Configured PipelineConfig instance</returns>
    private static PipelineConfig BuildConfig()
    {
        var config = new PipelineConfig(1, "TestPipeline", "1.0.0");
        config.AddStage(new PipelineStageDef("Ingestion", "SOURCE"));
        config.AddStage(new PipelineStageDef("Transform", "TRANSFORM"));
        config.AddStage(new PipelineStageDef("Output", "SINK"));
        return config;
    }

    /// <summary>
    /// Creates a PipelineVisualizer instance with default dependencies for testing.
    /// </summary>
    /// <returns>Initialized PipelineVisualizer</returns>
    private static PipelineVisualizer BuildVisualizer()
    {
        var backpressure = new BackpressureService();
        var metrics = new MetricsService(new InMemoryMetricsRepository(), new ThroughputCounter());
        return new PipelineVisualizer(backpressure, metrics);
    }

    /// <summary>
    /// Verifies that BuildNodes() creates one visualization node per pipeline stage.
    /// </summary>
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

    /// <summary>
    /// Verifies that downstream stage relationships are correctly established between sequential stages.
    /// First stage links to second, second to third, and last stage has no downstream.
    /// </summary>
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

    /// <summary>
    /// Verifies that the full pipeline visualization output contains the pipeline name.
    /// </summary>
    [Fact]
    public void Render_ContainsPipelineName()
    {
        var visualizer = BuildVisualizer();
        var config = BuildConfig();

        var output = visualizer.Render(config);

        Assert.Contains("TestPipeline", output);
    }

    /// <summary>
    /// Verifies that the full pipeline visualization output contains all stage names.
    /// </summary>
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

    /// <summary>
    /// Verifies that the compact visualization format contains the expected number of separators.
    /// For 3 stages, expects exactly 2 " -> " separators.
    /// </summary>
    [Fact]
    public void RenderCompact_ContainsSeparators()
    {
        var visualizer = BuildVisualizer();
        var config = BuildConfig();

        var compact = visualizer.RenderCompact(config);

        // Expect at least two " -> " separators for three stages
        Assert.Equal(2, CountOccurrences(compact, "->"));
    }

    /// <summary>
    /// Verifies that a backpressured node with any buffer fill percentage returns "CRITICAL" health status.
    /// </summary>
    [Fact]
    public void PipelineVisualizationNode_ComputeHealthLabel_BackpressuredIsCritical()
    {
        var node = new PipelineVisualizationNode { IsBackpressured = true, BufferFillPercent = 50 };
        Assert.Equal("CRITICAL", node.ComputeHealthLabel());
    }

    /// <summary>
    /// Verifies that a non-backpressured node with high buffer fill (>= 80%) returns "WARNING" health status.
    /// </summary>
    [Fact]
    public void PipelineVisualizationNode_ComputeHealthLabel_HighBufferIsWarning()
    {
        var node = new PipelineVisualizationNode { IsBackpressured = false, BufferFillPercent = 80 };
        Assert.Equal("WARNING", node.ComputeHealthLabel());
    }

    /// <summary>
    /// Verifies that a non-backpressured node with normal buffer fill (< 80%) returns "HEALTHY" status.
    /// </summary>
    [Fact]
    public void PipelineVisualizationNode_ComputeHealthLabel_NormalIsHealthy()
    {
        var node = new PipelineVisualizationNode { IsBackpressured = false, BufferFillPercent = 30 };
        Assert.Equal("HEALTHY", node.ComputeHealthLabel());
    }

    /// <summary>
    /// Counts the number of pattern occurrences in a text string.
    /// </summary>
    /// <param name="text">Text to search in</param>
    /// <param name="pattern">Pattern to find</param>
    /// <returns>Number of occurrences</returns>
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
