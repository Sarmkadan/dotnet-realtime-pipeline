#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Domain.Models;

using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Provides validation helpers for <see cref="PipelineConfig"/> instances.
/// </summary>
public static class PipelineConfigValidation
{
    /// <summary>
    /// Validates the specified <see cref="PipelineConfig"/> instance.
    /// </summary>
    /// <param name="value">The configuration to validate.</param>
    /// <returns>A list of human-readable validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> ValidateConfig(this PipelineConfig? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Required string properties
        if (string.IsNullOrWhiteSpace(value.PipelineName))
        {
            errors.Add($"PipelineName must be a non-empty string.");
        }

        if (string.IsNullOrWhiteSpace(value.Version))
        {
            errors.Add($"Version must be a non-empty string.");
        }

        // WindowType validation
        if (string.IsNullOrWhiteSpace(value.WindowType))
        {
            errors.Add("WindowType must be a non-empty string.");
        }
        else
        {
            var normalizedWindowType = value.WindowType.Trim().ToUpperInvariant();
            if (normalizedWindowType != "TUMBLING" && normalizedWindowType != "SLIDING" && normalizedWindowType != "SESSION")
            {
                errors.Add($"WindowType must be one of: TUMBLING, SLIDING, SESSION (got '{value.WindowType}').");
            }
        }

        // Positive numeric properties
        if (value.ConfigId <= 0)
        {
            errors.Add($"ConfigId must be a positive integer (got {value.ConfigId}).");
        }

        if (value.MaxBufferSize <= 0)
        {
            errors.Add($"MaxBufferSize must be a positive integer (got {value.MaxBufferSize}).");
        }

        if (value.BufferFlushIntervalMs <= 0)
        {
            errors.Add($"BufferFlushIntervalMs must be a positive integer (got {value.BufferFlushIntervalMs}).");
        }

        if (value.MaxConcurrentConsumers <= 0)
        {
            errors.Add($"MaxConcurrentConsumers must be a positive integer (got {value.MaxConcurrentConsumers}).");
        }

        if (value.WindowSizeMs <= 0)
        {
            errors.Add($"WindowSizeMs must be a positive integer (got {value.WindowSizeMs}).");
        }

        if (value.WindowSlideMs <= 0)
        {
            errors.Add($"WindowSlideMs must be a positive integer (got {value.WindowSlideMs}).");
        }

        // Retry configuration
        if (value.MaxRetries < 0)
        {
            errors.Add($"MaxRetries must be a non-negative integer (got {value.MaxRetries}).");
        }

        if (value.RetryDelayMs < 0)
        {
            errors.Add($"RetryDelayMs must be a non-negative integer (got {value.RetryDelayMs}).");
        }

        if (value.ProcessingTimeoutMs <= 0)
        {
            errors.Add($"ProcessingTimeoutMs must be a positive integer (got {value.ProcessingTimeoutMs}).");
        }

        // Threshold validations (0-100 range)
        if (value.BackpressureTriggerThreshold < 0 || value.BackpressureTriggerThreshold > 100)
        {
            errors.Add($"BackpressureTriggerThreshold must be between 0 and 100 (got {value.BackpressureTriggerThreshold.ToString(CultureInfo.InvariantCulture)}).");
        }

        if (value.MinDataQualityThreshold < 0 || value.MinDataQualityThreshold > 100)
        {
            errors.Add($"MinDataQualityThreshold must be between 0 and 100 (got {value.MinDataQualityThreshold}).");
        }

        // Boolean flag validations
        if (value.ValidateOnIngestion && value.MinDataQualityThreshold <= 0)
        {
            errors.Add("MinDataQualityThreshold must be greater than 0 when ValidateOnIngestion is enabled.");
        }

        if (!value.EnableMetricsCollection && value.BackpressureTriggerThreshold >= 100)
        {
            errors.Add("BackpressureTriggerThreshold should typically be less than 100 when metrics collection is disabled.");
        }

        // DateTime validation
        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt must be set to a valid DateTime.");
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("CreatedAt cannot be in the future.");
        }

        if (value.LastModifiedAt == default)
        {
            errors.Add("LastModifiedAt must be set to a valid DateTime.");
        }
        else if (value.LastModifiedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("LastModifiedAt cannot be in the future.");
        }
        else if (value.LastModifiedAt < value.CreatedAt)
        {
            errors.Add("LastModifiedAt cannot be earlier than CreatedAt.");
        }

        // Windowing consistency check
        if (value.WindowSizeMs > 0 && value.WindowSlideMs > 0)
        {
            if (value.WindowSlideMs > value.WindowSizeMs)
            {
                errors.Add("WindowSlideMs must be less than or equal to WindowSizeMs for proper windowing behavior.");
            }
        }
        else if (value.WindowSizeMs <= 0)
        {
            errors.Add("WindowSizeMs must be a positive value for windowing to be configured.");
        }
        else if (value.WindowSlideMs <= 0)
        {
            errors.Add("WindowSlideMs must be a positive value for windowing to be configured.");
        }

        // Stages collection
        if (value.Stages is null)
        {
            errors.Add("Stages collection must not be null.");
        }
        else
        {
            if (value.Stages.Count == 0)
            {
                errors.Add("Stages collection must contain at least one stage.");
            }

            foreach (var stage in value.Stages)
            {
                if (stage is null)
                {
                    errors.Add("Stages collection contains a null element.");
                    break;
                }

                if (string.IsNullOrWhiteSpace(stage.StageName))
                {
                    errors.Add("StageName in Stages collection must be a non-empty string.");
                }

                if (string.IsNullOrWhiteSpace(stage.StageType))
                {
                    errors.Add("StageType in Stages collection must be a non-empty string.");
                }

                if (stage.ExecutionOrder < 0)
                {
                    errors.Add($"ExecutionOrder in Stages collection must be a non-negative integer (got {stage.ExecutionOrder}).");
                }
            }
        }

        // CustomSettings collection
        if (value.CustomSettings is null)
        {
            errors.Add("CustomSettings collection must not be null.");
        }
        else
        {
            foreach (var kvp in value.CustomSettings)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    errors.Add("CustomSettings collection contains an entry with a null or empty key.");
                    break;
                }

                if (kvp.Value is null)
                {
                    errors.Add($"CustomSettings collection contains a null value for key '{kvp.Key}'.");
                    break;
                }
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="PipelineConfig"/> is valid.
    /// </summary>
    /// <param name="value">The configuration to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this PipelineConfig? value) => value.ValidateConfig().Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="PipelineConfig"/> is valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="value">The configuration to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the configuration is invalid.</exception>
    public static void EnsureValid(this PipelineConfig? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.ValidateConfig();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Pipeline configuration is invalid:{Environment.NewLine}" +
                string.Join(Environment.NewLine, errors));
        }
    }
}