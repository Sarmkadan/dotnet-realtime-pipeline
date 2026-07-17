#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotNetRealtimePipeline.Utilities;

using System;
using System.Collections.Generic;
using DotNetRealtimePipeline.Domain.Models;

/// <summary>
/// Extension methods for validating pipeline entities using the ValidationHelper.
/// </summary>
public static class ValidationHelperValidation
{
    /// <summary>
    /// Validates a collection of data points.
    /// </summary>
    /// <param name="value">The ValidationHelper instance.</param>
    /// <param name="dataPoints">The data points collection to validate.</param>
    /// <returns>A validation result containing any validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value or dataPoints is null.</exception>
    public static ValidationResult Validate(
        this ValidationHelper value,
        List<DataPoint> dataPoints)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(dataPoints);

        return value.ValidateDataPoints(dataPoints);
    }

    /// <summary>
    /// Validates a pipeline configuration.
    /// </summary>
    /// <param name="value">The ValidationHelper instance.</param>
    /// <param name="config">The pipeline configuration to validate.</param>
    /// <returns>A validation result containing any validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value or config is null.</exception>
    public static ValidationResult Validate(
        this ValidationHelper value,
        PipelineConfig config)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(config);

        return ValidationHelper.ValidatePipelineConfig(config);
    }

    /// <summary>
    /// Validates processing results.
    /// </summary>
    /// <param name="value">The ValidationHelper instance.</param>
    /// <param name="results">The processing results to validate.</param>
    /// <returns>A validation result containing any validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value or results is null.</exception>
    public static ValidationResult Validate(
        this ValidationHelper value,
        List<ProcessingResult> results)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(results);

        return ValidationHelper.ValidateProcessingResults(results);
    }

    /// <summary>
    /// Validates a window event.
    /// </summary>
    /// <param name="value">The ValidationHelper instance.</param>
    /// <param name="window">The window event to validate.</param>
    /// <returns>A validation result containing any validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value or window is null.</exception>
    public static ValidationResult Validate(
        this ValidationHelper value,
        WindowEvent window)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(window);

        return ValidationHelper.ValidateWindowEvent(window);
    }

    /// <summary>
    /// Checks if a data point is within a time range.
    /// </summary>
    /// <param name="value">The ValidationHelper instance.</param>
    /// <param name="dataPoint">The data point to check.</param>
    /// <param name="startMs">The start time in milliseconds.</param>
    /// <param name="endMs">The end time in milliseconds.</param>
    /// <returns>True if the data point is within the time range; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value or dataPoint is null.</exception>
    public static bool IsInTimeRange(
        this ValidationHelper value,
        DataPoint dataPoint,
        long startMs,
        long endMs)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(dataPoint);

        return ValidationHelper.IsInTimeRange(dataPoint, startMs, endMs);
    }

    /// <summary>
    /// Checks if a value is within acceptable bounds.
    /// </summary>
    /// <param name="value">The ValidationHelper instance.</param>
    /// <param name="inputValue">The value to check.</param>
    /// <param name="minValue">The minimum acceptable value.</param>
    /// <param name="maxValue">The maximum acceptable value.</param>
    /// <returns>True if the value is within bounds; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static bool IsWithinBounds(
        this ValidationHelper value,
        double inputValue,
        double minValue,
        double maxValue)
    {
        ArgumentNullException.ThrowIfNull(value);

        return ValidationHelper.IsWithinBounds(inputValue, minValue, maxValue);
    }

    /// <summary>
    /// Validates the ValidationHelper instance for common issues.
    /// </summary>
    /// <param name="value">The ValidationHelper instance to validate.</param>
    /// <returns>A list of human-readable validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static IReadOnlyList<string> Validate(this ValidationHelper value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // ValidationHelper is a static class with only static methods
        // No instance state to validate

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if the ValidationHelper instance is valid.
    /// </summary>
    /// <param name="value">The ValidationHelper instance to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static bool IsValid(this ValidationHelper value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return true; // ValidationHelper has no instance state to validate
    }

    /// <summary>
    /// Ensures that the ValidationHelper instance is valid.
    /// </summary>
    /// <param name="value">The ValidationHelper instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is not valid.</exception>
    public static void EnsureValid(this ValidationHelper value)
    {
        ArgumentNullException.ThrowIfNull(value);

        // ValidationHelper has no instance state to validate
        // No validation needed
    }
}