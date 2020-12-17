#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Domain.Models;

using System;
using System.Collections.Generic;

/// <summary>
/// Configuration model for the entire data processing pipeline.
/// Defines stages, buffers, windowing, and performance parameters.
/// </summary>
public sealed class PipelineConfig
{
    /// <summary>
    /// Gets or sets the unique identifier for this configuration.
    /// </summary>
    public long ConfigId { get; set; }

    /// <summary>
    /// Gets or sets the name of the pipeline.
    /// </summary>
    public string PipelineName { get; set; } = "";

    /// <summary>
    /// Gets or sets the version of the pipeline configuration.
    /// </summary>
    public string Version { get; set; } = "";

    // Buffer configuration

    /// <summary>
    /// Gets or sets the maximum buffer size before backpressure is applied.
    /// </summary>
    public long MaxBufferSize { get; set; } = 10000;

    /// <summary>
    /// Gets or sets the buffer flush interval in milliseconds.
    /// </summary>
    public long BufferFlushIntervalMs { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the maximum number of concurrent consumers for parallel processing.
    /// </summary>
    public int MaxConcurrentConsumers { get; set; } = 4;

    // Windowing configuration

    /// <summary>
    /// Gets or sets the window size in milliseconds.
    /// </summary>
    public long WindowSizeMs { get; set; } = 5000;

    /// <summary>
    /// Gets or sets the window slide interval in milliseconds.
    /// </summary>
    public long WindowSlideMs { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the type of windowing algorithm (TUMBLING, SLIDING, SESSION).
    /// </summary>
    public string WindowType { get; set; } = "TUMBLING";

    // Performance settings

    /// <summary>
    /// Gets or sets the maximum number of retry attempts for failed operations.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Gets or sets the delay between retry attempts in milliseconds.
    /// </summary>
    public long RetryDelayMs { get; set; } = 100;

    /// <summary>
    /// Gets or sets the processing timeout in milliseconds.
    /// </summary>
    public long ProcessingTimeoutMs { get; set; } = 30000;

    /// <summary>
    /// Gets or sets the threshold percentage (0-100) at which backpressure is triggered.
    /// </summary>
    public double BackpressureTriggerThreshold { get; set; } = 80.0;

    // Quality settings

    /// <summary>
    /// Gets or sets the minimum data quality threshold (0-100) for processing.
    /// </summary>
    public int MinDataQualityThreshold { get; set; } = 70;

    /// <summary>
    /// Gets or sets a value indicating whether to validate data on ingestion.
    /// </summary>
    public bool ValidateOnIngestion { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether metrics collection is enabled.
    /// </summary>
    public bool EnableMetricsCollection { get; set; } = true;

    // Stage configuration

    /// <summary>
    /// Gets or sets the list of pipeline stage definitions.
    /// </summary>
    public List<PipelineStageDef> Stages { get; set; } = new();

    /// <summary>
    /// Gets or sets custom configuration settings as key-value pairs.
    /// </summary>
    public Dictionary<string, object> CustomSettings { get; set; } = new();

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last modification timestamp.
    /// </summary>
    public DateTime LastModifiedAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this configuration is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    public PipelineConfig()
    {
    }

    public PipelineConfig(long configId, string pipelineName, string version)
    {
        ConfigId = configId;
        PipelineName = pipelineName ?? throw new ArgumentNullException(nameof(pipelineName));
        Version = version ?? throw new ArgumentNullException(nameof(version));
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Validates the configuration for correctness and consistency.
    /// </summary>
    public bool Validate()
    {
        if (ConfigId <= 0) return false;
        if (string.IsNullOrWhiteSpace(PipelineName)) return false;
        if (string.IsNullOrWhiteSpace(Version)) return false;
        if (MaxBufferSize <= 0) return false;
        if (BufferFlushIntervalMs <= 0) return false;
        if (WindowSizeMs <= 0) return false;
        if (WindowSlideMs <= 0) return false;
        if (MaxConcurrentConsumers <= 0) return false;
        if (MinDataQualityThreshold < 0 || MinDataQualityThreshold > 100) return false;
        if (BackpressureTriggerThreshold < 0 || BackpressureTriggerThreshold > 100) return false;

        return true;
    }

    /// <summary>
    /// Adds a pipeline stage definition.
    /// </summary>
    public void AddStage(PipelineStageDef stage)
    {
        if (stage is null) throw new ArgumentNullException(nameof(stage));
        Stages.Add(stage);
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets a stage by name, returns null if not found.
    /// </summary>
    public PipelineStageDef? GetStageByName(string stageName)
    {
        return Stages.Find(s => s.StageName == stageName);
    }

    /// <summary>
    /// Sets a custom configuration setting.
    /// </summary>
    public void SetCustomSetting(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key cannot be null", nameof(key));
        CustomSettings[key] = value ?? throw new ArgumentNullException(nameof(value));
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets a custom configuration setting.
    /// </summary>
    public object? GetCustomSetting(string key)
    {
        return CustomSettings.TryGetValue(key, out var value) ? value : null;
    }

    /// <summary>
    /// Calculates the expected window count per flush interval.
    /// </summary>
    public int CalculateWindowsPerFlushInterval()
    {
        if (WindowSlideMs <= 0) return 1;
        return (int)Math.Ceiling((double)BufferFlushIntervalMs / WindowSlideMs);
    }

    /// <summary>
    /// Gets a summary of the configuration.
    /// </summary>
    public string GetSummary()
    {
        return $"Pipeline[Name={PipelineName}, Version={Version}, Stages={Stages.Count}, " +
               $"BufferSize={MaxBufferSize}, WindowSize={WindowSizeMs}ms, Active={IsActive}]";
    }

    /// <summary>
    /// Creates a copy of this configuration.
    /// </summary>
    public PipelineConfig Clone(long newConfigId)
    {
        var clone = new PipelineConfig(newConfigId, PipelineName, Version)
        {
            MaxBufferSize = MaxBufferSize,
            BufferFlushIntervalMs = BufferFlushIntervalMs,
            MaxConcurrentConsumers = MaxConcurrentConsumers,
            WindowSizeMs = WindowSizeMs,
            WindowSlideMs = WindowSlideMs,
            WindowType = WindowType,
            MaxRetries = MaxRetries,
            RetryDelayMs = RetryDelayMs,
            ProcessingTimeoutMs = ProcessingTimeoutMs,
            BackpressureTriggerThreshold = BackpressureTriggerThreshold,
            MinDataQualityThreshold = MinDataQualityThreshold,
            ValidateOnIngestion = ValidateOnIngestion,
            EnableMetricsCollection = EnableMetricsCollection,
            IsActive = IsActive,
            Stages = new List<PipelineStageDef>(Stages),
            CustomSettings = new Dictionary<string, object>(CustomSettings)
        };

        return clone;
    }
}

/// <summary>
/// Defines a single stage in the processing pipeline.
/// </summary>
public sealed class PipelineStageDef
{
    /// <summary>
    /// Gets or sets the name of the stage.
    /// </summary>
    public string StageName { get; set; } = "";

    /// <summary>
    /// Gets or sets the type of the stage (e.g., "filter", "transform", "aggregate").
    /// </summary>
    public string StageType { get; set; } = "";

    /// <summary>
    /// Gets or sets the execution order of this stage in the pipeline.
    /// </summary>
    public int ExecutionOrder { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this stage is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the parameters for this stage as key-value pairs.
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PipelineStageDef"/> class.
    /// </summary>
    /// <param name="stageName">The name of the stage.</param>
    /// <param name="stageType">The type of the stage.</param>
    public PipelineStageDef(string stageName, string stageType)
    {
        StageName = stageName ?? throw new ArgumentNullException(nameof(stageName));
        StageType = stageType ?? throw new ArgumentNullException(nameof(stageType));
    }

    /// <summary>
    /// Adds a parameter to this stage definition.
    /// </summary>
    /// <param name="key">The parameter key.</param>
    /// <param name="value">The parameter value.</param>
    public void SetParameter(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key cannot be null", nameof(key));
        Parameters[key] = value ?? throw new ArgumentNullException(nameof(value));
    }
}
