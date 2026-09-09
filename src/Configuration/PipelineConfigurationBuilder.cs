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
    private const long InitialPipelineId = 1;
    private const int NoConfiguredStagesCount = 0;
    private const long DefaultBufferSizeForHighThroughput = 100000;
    private const long DefaultFlushIntervalMsForHighThroughput = 500;
    private const int DefaultConcurrentConsumersForHighThroughput = 16;
    private const long DefaultWindowSizeMsForHighThroughput = PipelineConstants.DefaultWindowSlideMs;
    private const long DefaultWindowSlideMsForHighThroughput = 500;
    private const int DefaultRetriesForHighThroughput = 2;
    private const long DefaultRetryDelayMsForHighThroughput = 50;
    private const long DefaultBufferSizeForLowLatency = PipelineConstants.DefaultWindowSizeMs;
    private const long DefaultFlushIntervalMsForLowLatency = PipelineConstants.DefaultRetryDelayMs;
    private const int DefaultConcurrentConsumersForLowLatency = 2;
    private const long DefaultWindowSizeMsForLowLatency = PipelineConstants.DefaultWindowSlideMs;
    private const long DefaultWindowSlideMsForLowLatency = 100;
    private const long DefaultProcessingTimeoutMsForLowLatency = PipelineConstants.MetricsCollectionIntervalMs;
    private const long DefaultBufferSizeForHighReliability = 50000;
    private const long DefaultFlushIntervalMsForHighReliability = 2000;
    private const int DefaultRetriesForHighReliability = 5;
    private const long DefaultRetryDelayMsForHighReliability = 500;
    private const int DefaultDataQualityThresholdForHighReliability = 85;

    private readonly PipelineConfig _config;

    public PipelineConfigurationBuilder(string pipelineName, string version)
    {
        ArgumentException.ThrowIfNullOrEmpty(pipelineName);
        ArgumentException.ThrowIfNullOrEmpty(version);

        _config = new PipelineConfig(InitialPipelineId, pipelineName, version);
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
        ArgumentException.ThrowIfNullOrEmpty(windowType);
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
        ArgumentException.ThrowIfNullOrEmpty(stageName);
        ArgumentException.ThrowIfNullOrEmpty(stageType);
        _config.AddStage(new PipelineStageDef(stageName, stageType));
        return this;
    }

    /// <summary>
    /// Adds a custom setting.
    /// </summary>
    public PipelineConfigurationBuilder WithCustomSetting(string key, object value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(value);
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
        _config.MaxBufferSize = DefaultBufferSizeForHighThroughput;
        _config.BufferFlushIntervalMs = DefaultFlushIntervalMsForHighThroughput;
        _config.MaxConcurrentConsumers = DefaultConcurrentConsumersForHighThroughput;
        _config.WindowSizeMs = DefaultWindowSizeMsForHighThroughput;
        _config.WindowSlideMs = DefaultWindowSlideMsForHighThroughput;
        _config.MaxRetries = DefaultRetriesForHighThroughput;
        _config.RetryDelayMs = DefaultRetryDelayMsForHighThroughput;
        return this;
    }

    /// <summary>
    /// Sets default low-latency configuration.
    /// </summary>
    public PipelineConfigurationBuilder WithLowLatencyDefaults()
    {
        _config.MaxBufferSize = DefaultBufferSizeForLowLatency;
        _config.BufferFlushIntervalMs = PipelineConstants.DefaultRetryDelayMs;
        _config.MaxConcurrentConsumers = DefaultConcurrentConsumersForLowLatency;
        _config.WindowSizeMs = DefaultWindowSizeMsForLowLatency;
        _config.WindowSlideMs = DefaultWindowSlideMsForLowLatency;
        _config.ProcessingTimeoutMs = PipelineConstants.MetricsCollectionIntervalMs;
        return this;
    }

    /// <summary>
    /// Sets default high-reliability configuration.
    /// </summary>
    public PipelineConfigurationBuilder WithHighReliabilityDefaults()
    {
        _config.MaxBufferSize = DefaultBufferSizeForHighReliability;
        _config.BufferFlushIntervalMs = DefaultFlushIntervalMsForHighReliability;
        _config.MaxConcurrentConsumers = PipelineConstants.DefaultMaxConcurrentConsumers;
        _config.MaxRetries = DefaultRetriesForHighReliability;
        _config.RetryDelayMs = DefaultRetryDelayMsForHighReliability;
        _config.MinDataQualityThreshold = DefaultDataQualityThresholdForHighReliability;
        _config.ValidateOnIngestion = true;
        return this;
    }

    /// <summary>
    /// Builds and returns the configured pipeline.
    /// </summary>
    public PipelineConfig Build()
    {
        // Add default stages if none configured
        if (_config.Stages.Count == NoConfiguredStagesCount)
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
