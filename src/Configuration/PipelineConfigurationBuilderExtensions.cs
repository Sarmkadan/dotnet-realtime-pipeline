#nullable enable
// =============================================================================
// Author: 
// =============================================================================

namespace DotNetRealtimePipeline.Configuration;

using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;

/// <summary>
/// Extension methods for PipelineConfigurationBuilder.
/// </summary>
public static class PipelineConfigurationBuilderExtensions
{
    /// <summary>
    /// Adds multiple stages to the pipeline.
    /// </summary>
    /// <param name="builder">The pipeline configuration builder.</param>
    /// <param name="stages">The stages to add.</param>
    /// <returns>The pipeline configuration builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown if builder or stages is null.</exception>
    public static PipelineConfigurationBuilder WithStages(
        this PipelineConfigurationBuilder builder,
        IEnumerable<(string stageName, string stageType)> stages)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(stages);

        foreach (var (stageName, stageType) in stages)
        {
            builder.WithStage(stageName, stageType);
        }
        return builder;
    }

    /// <summary>
    /// Sets the default buffer configuration for high-throughput pipelines.
    /// </summary>
    /// <param name="builder">The pipeline configuration builder.</param>
    /// <returns>The pipeline configuration builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown if builder is null.</exception>
    public static PipelineConfigurationBuilder WithHighThroughputBufferDefaults(
        this PipelineConfigurationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithBufferConfiguration(
            maxBufferSize: 500000,
            flushIntervalMs: 100,
            maxConcurrentConsumers: 32);
    }

    /// <summary>
    /// Adds a dead-letter queue stage to the pipeline.
    /// </summary>
    /// <param name="builder">The pipeline configuration builder.</param>
    /// <param name="dlqStageName">The name of the dead-letter queue stage.</param>
    /// <param name="dlqStageType">The type of the dead-letter queue stage.</param>
    /// <returns>The pipeline configuration builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown if builder, dlqStageName or dlqStageType is null or empty.</exception>
    public static PipelineConfigurationBuilder WithDeadLetterQueueStage(
        this PipelineConfigurationBuilder builder,
        string dlqStageName,
        string dlqStageType)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrEmpty(dlqStageName, nameof(dlqStageName));
        ArgumentException.ThrowIfNullOrEmpty(dlqStageType, nameof(dlqStageType));

        return builder.WithStage(dlqStageName, dlqStageType);
    }
}
