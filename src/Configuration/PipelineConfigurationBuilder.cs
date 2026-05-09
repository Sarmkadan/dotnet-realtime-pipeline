#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Configuration;

using DotNetRealtimePipeline.Constants;
using DotNetRealtimePipeline.Domain.Models;
using System;

/// <summary>
/// Fluent builder for constructing pipeline configurations.
/// Provides a convenient way to set up pipeline parameters.
/// </summary>
public sealed class PipelineConfigurationBuilder
{
    private readonly PipelineConfig _config;

    public PipelineConfigurationBuilder(string pipelineName, string version)
    {
        if (string.IsNullOrWhiteSpace(pipelineName))
            throw new ArgumentException("Pipeline name cannot be null", nameof(pipelineName));
        if (string.IsNullOrWhiteSpace(version))
            throw new ArgumentException("Version cannot be null", nameof(version));

        _config = new PipelineConfig(1, pipelineName, version);
    }

    /// <summary>
    /// Sets the buffer configuration.
    /// </summary>
    public PipelineConfigurationBuilder WithBufferConfiguration(
        long maxBufferSize,
        long flushIntervalMs,
        int maxConcurrentConsumers)
    {
        _config.MaxBufferSize = maxBufferSize;
        _config.BufferFlushIntervalMs = flushIntervalMs;
        _config.MaxConcurrentConsumers = maxConcurrentConsumers;
        return this;
    }

    /// <summary>
    /// Sets the windowing configuration.
    /// </summary>
    public PipelineConfigurationBuilder WithWindowingConfiguration(
        long windowSizeMs,
        long windowSlideMs,
        string windowType)
    {
        _config.WindowSizeMs = windowSizeMs;
        _config.WindowSlideMs = windowSlideMs;
        _config.WindowType = windowType;
        return this;
    }

    /// <summary>
    /// Sets the performance configuration.
    /// </summary>
    public PipelineConfigurationBuilder WithPerformanceConfiguration(
        int maxRetries,
        long retryDelayMs,
        long processingTimeoutMs,
        double backpressureTriggerThreshold)
    {
        _config.MaxRetries = maxRetries;
        _config.RetryDelayMs = retryDelayMs;
        _config.ProcessingTimeoutMs = processingTimeoutMs;
        _config.BackpressureTriggerThreshold = backpressureTriggerThreshold;
        return this;
    }

    /// <summary>
    /// Sets the quality configuration.
    /// </summary>
    public PipelineConfigurationBuilder WithQualityConfiguration(
        int minDataQualityThreshold,
        bool validateOnIngestion,
        bool enableMetricsCollection)
    {
        _config.MinDataQualityThreshold = minDataQualityThreshold;
        _config.ValidateOnIngestion = validateOnIngestion;
        _config.EnableMetricsCollection = enableMetricsCollection;
        return this;
    }

    /// <summary>
    /// Adds a stage to the pipeline.
    /// </summary>
    public PipelineConfigurationBuilder WithStage(string stageName, string stageType)
    {
        _config.AddStage(new PipelineStageDef(stageName, stageType));
        return this;
    }

    /// <summary>
    /// Adds a custom setting.
    /// </summary>
    public PipelineConfigurationBuilder WithCustomSetting(string key, object value)
    {
        _config.SetCustomSetting(key, value);
        return this;
    }

    /// <summary>
    /// Activates the pipeline.
    /// </summary>
    public PipelineConfigurationBuilder Activate()
    {
        _config.IsActive = true;
        return this;
    }

    /// <summary>
    /// Deactivates the pipeline.
    /// </summary>
    public PipelineConfigurationBuilder Deactivate()
    {
        _config.IsActive = false;
        return this;
    }

    /// <summary>
    /// Sets default high-performance configuration.
    /// </summary>
    public PipelineConfigurationBuilder WithHighPerformanceDefaults()
    {
        _config.MaxBufferSize = 100000;
        _config.BufferFlushIntervalMs = 500;
        _config.MaxConcurrentConsumers = 16;
        _config.WindowSizeMs = 1000;
        _config.WindowSlideMs = 500;
        _config.MaxRetries = 2;
        _config.RetryDelayMs = 50;
        return this;
    }

    /// <summary>
    /// Sets default low-latency configuration.
    /// </summary>
    public PipelineConfigurationBuilder WithLowLatencyDefaults()
    {
        _config.MaxBufferSize = 5000;
        _config.BufferFlushIntervalMs = 100;
        _config.MaxConcurrentConsumers = 2;
        _config.WindowSizeMs = 1000;
        _config.WindowSlideMs = 100;
        _config.ProcessingTimeoutMs = 5000;
        return this;
    }

    /// <summary>
    /// Sets default high-reliability configuration.
    /// </summary>
    public PipelineConfigurationBuilder WithHighReliabilityDefaults()
    {
        _config.MaxBufferSize = 50000;
        _config.BufferFlushIntervalMs = 2000;
        _config.MaxConcurrentConsumers = 4;
        _config.MaxRetries = 5;
        _config.RetryDelayMs = 500;
        _config.MinDataQualityThreshold = 85;
        _config.ValidateOnIngestion = true;
        return this;
    }

    /// <summary>
    /// Builds and returns the configured pipeline.
    /// </summary>
    public PipelineConfig Build()
    {
        // Add default stages if none configured
        if (_config.Stages.Count == 0)
        {
            _config.Stages.Add(new PipelineStageDef(PipelineConstants.StageName_Ingestion, "SOURCE"));
            _config.Stages.Add(new PipelineStageDef(PipelineConstants.StageName_Validation, "FILTER"));
            _config.Stages.Add(new PipelineStageDef(PipelineConstants.StageName_Transformation, "TRANSFORM"));
            _config.Stages.Add(new PipelineStageDef(PipelineConstants.StageName_Windowing, "WINDOW"));
            _config.Stages.Add(new PipelineStageDef(PipelineConstants.StageName_Aggregation, "AGGREGATE"));
            _config.Stages.Add(new PipelineStageDef(PipelineConstants.StageName_Output, "SINK"));
        }

        // Validate configuration
        if (!_config.Validate())
            throw new InvalidOperationException("Configuration validation failed");

        return _config;
    }
}
