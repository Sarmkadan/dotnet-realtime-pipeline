#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotNetRealtimePipeline.Configuration;

using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Provides validation helpers for <see cref="PipelineConfigurationBuilder"/> instances.
/// </summary>
public static class PipelineConfigurationBuilderValidation
{
    /// <summary>
    /// Validates a <see cref="PipelineConfigurationBuilder"/> instance and returns any validation problems.
    /// Validates the builder's internal configuration state by examining the underlying <see cref="PipelineConfig"/>.
    /// </summary>
    /// <param name="value">The <see cref="PipelineConfigurationBuilder"/> to validate.</param>
    /// <returns>A read-only list of validation problem descriptions. Empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this PipelineConfigurationBuilder value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();
        var config = value.GetInternalConfig();

        // Validate required fields from the underlying PipelineConfig
        if (string.IsNullOrWhiteSpace(config.PipelineName))
        {
            problems.Add("PipelineName cannot be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(config.Version))
        {
            problems.Add("Version cannot be null or whitespace.");
        }

        // Validate buffer configuration (must be positive)
        if (config.MaxBufferSize <= 0)
        {
            problems.Add(
                $"MaxBufferSize must be a positive integer (got {config.MaxBufferSize.ToString(CultureInfo.InvariantCulture)}).");
        }

        if (config.BufferFlushIntervalMs <= 0)
        {
            problems.Add(
                $"BufferFlushIntervalMs must be a positive integer (got {config.BufferFlushIntervalMs.ToString(CultureInfo.InvariantCulture)}).");
        }

        if (config.MaxConcurrentConsumers <= 0)
        {
            problems.Add(
                $"MaxConcurrentConsumers must be a positive integer (got {config.MaxConcurrentConsumers.ToString(CultureInfo.InvariantCulture)}).");
        }

        // Validate windowing configuration (must be positive)
        if (config.WindowSizeMs <= 0)
        {
            problems.Add(
                $"WindowSizeMs must be a positive integer (got {config.WindowSizeMs.ToString(CultureInfo.InvariantCulture)}).");
        }

        if (config.WindowSlideMs <= 0)
        {
            problems.Add(
                $"WindowSlideMs must be a positive integer (got {config.WindowSlideMs.ToString(CultureInfo.InvariantCulture)}).");
        }

        if (string.IsNullOrWhiteSpace(config.WindowType))
        {
            problems.Add("WindowType cannot be null or whitespace.");
        }

        // Validate performance configuration (must be non-negative)
        if (config.MaxRetries < 0)
        {
            problems.Add(
                $"MaxRetries must be a non-negative integer (got {config.MaxRetries.ToString(CultureInfo.InvariantCulture)}).");
        }

        if (config.RetryDelayMs < 0)
        {
            problems.Add(
                $"RetryDelayMs must be a non-negative integer (got {config.RetryDelayMs.ToString(CultureInfo.InvariantCulture)}).");
        }

        if (config.ProcessingTimeoutMs < 0)
        {
            problems.Add(
                $"ProcessingTimeoutMs must be a non-negative integer (got {config.ProcessingTimeoutMs.ToString(CultureInfo.InvariantCulture)}).");
        }

        if (config.BackpressureTriggerThreshold < 0 || config.BackpressureTriggerThreshold > 100)
        {
            problems.Add(
                $"BackpressureTriggerThreshold must be between 0 and 100 (got {config.BackpressureTriggerThreshold.ToString(CultureInfo.InvariantCulture)}).");
        }

        // Validate quality configuration (must be within valid range)
        if (config.MinDataQualityThreshold < 0 || config.MinDataQualityThreshold > 100)
        {
            problems.Add(
                $"MinDataQualityThreshold must be between 0 and 100 (got {config.MinDataQualityThreshold.ToString(CultureInfo.InvariantCulture)}).");
        }

        // Validate stages (must have at least one stage or default stages will be added)
        if (config.Stages.Count == 0)
        {
            problems.Add("At least one stage must be configured, or default stages will be added during Build().");
        }

        // Validate custom settings keys
        foreach (var key in config.CustomSettings.Keys)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                problems.Add("CustomSettings contains a null or whitespace key.");
                break;
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="PipelineConfigurationBuilder"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="PipelineConfigurationBuilder"/> to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this PipelineConfigurationBuilder value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="PipelineConfigurationBuilder"/> instance is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The <see cref="PipelineConfigurationBuilder"/> to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of validation problems.</exception>
    public static void EnsureValid(this PipelineConfigurationBuilder value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"PipelineConfigurationBuilder validation failed with {problems.Count} problem(s):{Environment.NewLine}- ".Replace("- ", string.Empty) +
            string.Join(Environment.NewLine + "- ", problems),
            nameof(value));
    }

    /// <summary>
    /// Gets the internal PipelineConfig from the builder for validation purposes.
    /// This is a private reflection helper to access the internal _config field.
    /// </summary>
    private static PipelineConfig GetInternalConfig(this PipelineConfigurationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Use reflection to access the private _config field
        var field = typeof(PipelineConfigurationBuilder).GetField("_config",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field == null)
        {
            throw new InvalidOperationException("Could not access internal _config field of PipelineConfigurationBuilder.");
        }

        return (PipelineConfig)field.GetValue(builder)!;
    }
}