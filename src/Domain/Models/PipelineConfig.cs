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
public class PipelineConfig
{
    public long ConfigId { get; set; }
    public string PipelineName { get; set; } = "";
    public string Version { get; set; } = "";

    // Buffer configuration
    public long MaxBufferSize { get; set; } = 10000;
    public long BufferFlushIntervalMs { get; set; } = 1000;
    public int MaxConcurrentConsumers { get; set; } = 4;

    // Windowing configuration
    public long WindowSizeMs { get; set; } = 5000;
    public long WindowSlideMs { get; set; } = 1000;
    public string WindowType { get; set; } = "TUMBLING";

    // Performance settings
    public int MaxRetries { get; set; } = 3;
    public long RetryDelayMs { get; set; } = 100;
    public long ProcessingTimeoutMs { get; set; } = 30000;
    public double BackpressureTriggerThreshold { get; set; } = 80.0;

    // Quality settings
    public int MinDataQualityThreshold { get; set; } = 70;
    public bool ValidateOnIngestion { get; set; } = true;
    public bool EnableMetricsCollection { get; set; } = true;

    // Stage configuration
    public List<PipelineStageDef> Stages { get; set; } = new();
    public Dictionary<string, object> CustomSettings { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
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
        if (stage == null) throw new ArgumentNullException(nameof(stage));
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
public class PipelineStageDef
{
    public string StageName { get; set; } = "";
    public string StageType { get; set; } = "";
    public int ExecutionOrder { get; set; }
    public bool Enabled { get; set; } = true;
    public Dictionary<string, object> Parameters { get; set; } = new();

    public PipelineStageDef(string stageName, string stageType)
    {
        StageName = stageName ?? throw new ArgumentNullException(nameof(stageName));
        StageType = stageType ?? throw new ArgumentNullException(nameof(stageType));
    }

    /// <summary>
    /// Adds a parameter to this stage definition.
    /// </summary>
    public void SetParameter(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key cannot be null", nameof(key));
        Parameters[key] = value ?? throw new ArgumentNullException(nameof(value));
    }
}
