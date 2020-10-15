#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Visualization;
using Xunit;

namespace DotNetRealtimePipeline.Tests.Unit;

/// <summary>
/// Extension methods for <see cref="PipelineVisualizerTests"/> to provide fluent assertions
/// and helper methods for testing pipeline visualization scenarios.
/// </summary>
public static class PipelineVisualizerTestsExtensions
{
    /// <summary>
    /// Creates a pipeline configuration with the specified number of stages.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="stageCount">Number of stages to create.</param>
    /// <param name="pipelineName">Optional pipeline name. Defaults to "TestPipeline".</param>
    /// <returns>A configured <see cref="PipelineConfig"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when stageCount is less than 1.</exception>
    public static PipelineConfig CreatePipelineConfig(this PipelineVisualizerTests test, int stageCount, string? pipelineName = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(stageCount, 1);
        ArgumentNullException.ThrowIfNull(test);

        var config = new PipelineConfig(1, pipelineName ?? "TestPipeline", "1.0.0");
        for (int i = 0; i < stageCount; i++)
        {
            config.AddStage(new PipelineStageDef($"Stage{i + 1}", "TRANSFORM"));
        }
        return config;
    }

    /// <summary>
    /// Creates a pipeline visualization node with the specified health state.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="isBackpressured">Whether the node is backpressured.</param>
    /// <param name="bufferFillPercent">The buffer fill percentage (0-100).</param>
    /// <param name="stageName">Optional stage name. Defaults to "TestStage".</param>
    /// <returns>A configured <see cref="PipelineVisualizationNode"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when bufferFillPercent is outside 0-100 range.</exception>
    public static PipelineVisualizationNode CreateVisualizationNode(
        this PipelineVisualizerTests test,
        bool isBackpressured,
        int bufferFillPercent,
        string? stageName = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(bufferFillPercent, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bufferFillPercent, 100);
        ArgumentNullException.ThrowIfNull(test);

        return new PipelineVisualizationNode
        {
            StageName = stageName ?? "TestStage",
            IsBackpressured = isBackpressured,
            BufferFillPercent = bufferFillPercent
        };
    }

    /// <summary>
    /// Asserts that the node has the expected health label based on its state.
    /// </summary>
    /// <param name="node">The node to test.</param>
    /// <param name="expectedHealth">The expected health label (CRITICAL, WARNING, or HEALTHY).</param>
    /// <exception cref="XunitException">Thrown when the health label doesn't match expected.</exception>
    public static void ShouldHaveHealthLabel(this PipelineVisualizationNode node, string expectedHealth)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentException.ThrowIfNullOrEmpty(expectedHealth);

        var actualHealth = node.ComputeHealthLabel();
        Assert.Equal(expectedHealth, actualHealth);
    }

    /// <summary>
    /// Asserts that the pipeline configuration has the expected number of stages.
    /// </summary>
    /// <param name="config">The pipeline configuration.</param>
    /// <param name="expectedStageCount">The expected number of stages.</param>
    /// <exception cref="XunitException">Thrown when stage count doesn't match expected.</exception>
    public static void ShouldHaveStageCount(this PipelineConfig config, int expectedStageCount)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentOutOfRangeException.ThrowIfLessThan(expectedStageCount, 0);

        Assert.Equal(expectedStageCount, config.Stages.Count);
    }

    /// <summary>
    /// Asserts that the pipeline visualization contains all expected stage names.
    /// </summary>
    /// <param name="output">The visualization output.</param>
    /// <param name="stageNames">The expected stage names to find.</param>
    /// <exception cref="ArgumentNullException">Thrown when output or stageNames is null.</exception>
    /// <exception cref="ArgumentException">Thrown when stageNames is empty.</exception>
    public static void ShouldContainStageNames(this string output, params string[] stageNames)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(stageNames);
        ArgumentException.ThrowIfNullOrEmpty(stageNames);

        foreach (var stageName in stageNames)
        {
            Assert.Contains(stageName, output);
        }
    }

    /// <summary>
    /// Asserts that the pipeline visualization contains the expected number of stage separators.
    /// </summary>
    /// <param name="compactOutput">The compact visualization output.</param>
    /// <param name="expectedSeparatorCount">The expected number of separators.</param>
    /// <exception cref="XunitException">Thrown when separator count doesn't match expected.</exception>
    public static void ShouldContainSeparatorCount(this string compactOutput, int expectedSeparatorCount)
    {
        ArgumentNullException.ThrowIfNull(compactOutput);
        ArgumentOutOfRangeException.ThrowIfLessThan(expectedSeparatorCount, 0);

        int actualCount = CountOccurrences(compactOutput, "->");
        Assert.Equal(expectedSeparatorCount, actualCount);
    }

    /// <summary>
    /// Counts the occurrences of a pattern in a string.
    /// </summary>
    /// <param name="text">The text to search.</param>
    /// <param name="pattern">The pattern to count.</param>
    /// <returns>The number of occurrences.</returns>
    private static int CountOccurrences(string text, string pattern)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(pattern);

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