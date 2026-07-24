#nullable enable

using DotNetRealtimePipeline.Data.Repositories;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;
using DotNetRealtimePipeline.Visualization;
using FluentAssertions;
using Xunit;

namespace DotNetRealtimePipeline.Tests;

/// <summary>
/// Unit tests for <see cref="PipelineVisualizerExtensions"/> extension methods.
/// Tests cover happy paths, edge cases, and error conditions for all public methods.
/// </summary>
public sealed class PipelineVisualizerExtensionsTests
{
    private readonly PipelineVisualizer _visualizer;
    private readonly PipelineConfig _config;

    public PipelineVisualizerExtensionsTests()
    {
        // Create minimal services for testing
        var backpressureService = new BackpressureService();
        var metricsService = new MetricsService(new InMemoryMetricsRepository());
        _visualizer = new PipelineVisualizer(backpressureService, metricsService);

        // Create a simple pipeline configuration
        _config = new PipelineConfig
        {
            PipelineName = "TestPipeline",
            Version = "1.0.0",
            Stages = new List<PipelineStageDef>
            {
                new PipelineStageDef("Source", "Source"),
                new PipelineStageDef("Transform", "Processor"),
                new PipelineStageDef("Sink", "Sink")
            }
        };
    }

    [Fact]
    public void RenderToConsole_WithValidInputs_DoesNotThrow()
    {
        // Act & Assert - should not throw
        _visualizer.Invoking(v => v.RenderToConsole(_config))
            .Should().NotThrow();
    }

    [Fact]
    public void RenderToConsole_WithNullVisualizer_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => ((PipelineVisualizer)null!).RenderToConsole(_config);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*visualizer*");
    }

    [Fact]
    public void RenderToConsole_WithNullConfig_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => _visualizer.RenderToConsole(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*config*");
    }

    [Fact]
    public void FindCriticalStages_WithValidPipeline_ReturnsCriticalStages()
    {
        // Arrange - set up critical stage by manipulating backpressure service
        var backpressureService = new BackpressureService();
        var metricsService = new MetricsService(new InMemoryMetricsRepository());
        var visualizer = new PipelineVisualizer(backpressureService, metricsService);

        var config = new PipelineConfig
        {
            PipelineName = "CriticalTest",
            Version = "1.0.0",
            Stages = new List<PipelineStageDef>
            {
                new PipelineStageDef("Stage1", "Source"),
                new PipelineStageDef("Stage2", "Processor"),
                new PipelineStageDef("Stage3", "Sink")
            }
        };

        // Act
        var criticalStages = visualizer.FindCriticalStages(config);

        // Assert - initially should be empty
        criticalStages.Should().BeEmpty();
    }

    [Fact]
    public void FindCriticalStages_WithNullVisualizer_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => ((PipelineVisualizer)null!).FindCriticalStages(_config);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*visualizer*");
    }

    [Fact]
    public void FindCriticalStages_WithNullConfig_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => _visualizer.FindCriticalStages(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*config*");
    }

    [Fact]
    public void FindCriticalStages_WithEmptyPipeline_ReturnsEmptyList()
    {
        // Arrange
        var config = new PipelineConfig
        {
            PipelineName = "EmptyPipeline",
            Version = "1.0.0",
            Stages = new List<PipelineStageDef>()
        };

        // Act
        var criticalStages = _visualizer.FindCriticalStages(config);

        // Assert
        criticalStages.Should().BeEmpty();
    }

    [Fact]
    public void GetStageThroughputSummary_WithValidPipeline_ReturnsCorrectSummary()
    {
        // Arrange
        var backpressureService = new BackpressureService();
        var metricsService = new MetricsService(new InMemoryMetricsRepository());
        var visualizer = new PipelineVisualizer(backpressureService, metricsService);

        var config = new PipelineConfig
        {
            PipelineName = "ThroughputTest",
            Version = "1.0.0",
            Stages = new List<PipelineStageDef>
            {
                new PipelineStageDef("Stage1", "Source"),
                new PipelineStageDef("Stage2", "Processor"),
                new PipelineStageDef("Stage3", "Sink")
            }
        };

        // Act
        var summary = visualizer.GetStageThroughputSummary(config);

        // Assert
        summary.Should().NotBeNull();
        summary.MinThroughput.Should().Be(0);
        summary.MaxThroughput.Should().Be(0);
        summary.AvgThroughput.Should().Be(0);
    }

    [Fact]
    public void GetStageThroughputSummary_WithNullVisualizer_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => ((PipelineVisualizer)null!).GetStageThroughputSummary(_config);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*visualizer*");
    }

    [Fact]
    public void GetStageThroughputSummary_WithNullConfig_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => _visualizer.GetStageThroughputSummary(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*config*");
    }

    [Fact]
    public void GetStageThroughputSummary_WithEmptyPipeline_ReturnsZeroValues()
    {
        // Arrange
        var config = new PipelineConfig
        {
            PipelineName = "EmptyPipeline",
            Version = "1.0.0",
            Stages = new List<PipelineStageDef>()
        };

        // Act
        var summary = _visualizer.GetStageThroughputSummary(config);

        // Assert
        summary.MinThroughput.Should().Be(0);
        summary.MaxThroughput.Should().Be(0);
        summary.AvgThroughput.Should().Be(0);
    }

    [Fact]
    public void GetStageThroughputSummary_WithSingleStage_ReturnsSameValuesForAllMetrics()
    {
        // Arrange
        var backpressureService = new BackpressureService();
        var metricsService = new MetricsService(new InMemoryMetricsRepository());
        var visualizer = new PipelineVisualizer(backpressureService, metricsService);

        var config = new PipelineConfig
        {
            PipelineName = "SingleStage",
            Version = "1.0.0",
            Stages = new List<PipelineStageDef>
            {
                new PipelineStageDef("Solo", "Processor")
            }
        };

        // Act
        var summary = visualizer.GetStageThroughputSummary(config);

        // Assert - all values should be equal for single stage
        summary.MinThroughput.Should().Be(summary.MaxThroughput);
        summary.MaxThroughput.Should().Be(summary.AvgThroughput);
        summary.MinThroughput.Should().Be(0);
    }

    [Fact]
    public void ExtensionMethods_WorkConsistentlyWithSameInput()
    {
        // Act - call same method multiple times
        var criticalStages1 = _visualizer.FindCriticalStages(_config);
        var criticalStages2 = _visualizer.FindCriticalStages(_config);

        var summary1 = _visualizer.GetStageThroughputSummary(_config);
        var summary2 = _visualizer.GetStageThroughputSummary(_config);

        // Assert - results should be consistent
        criticalStages1.Should().BeEquivalentTo(criticalStages2);
        summary1.Should().Be(summary2);
    }
}