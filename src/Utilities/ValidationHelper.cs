#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Utilities;

using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;

/// <summary>
/// Validation helper methods for pipeline entities.
/// </summary>
public sealed class ValidationHelper
{
    private ValidationHelper() { }

    /// <summary>
    /// Validates a collection of data points.
    /// </summary>
    /// <param name="dataPoints">The data points to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> describing whether the collection is valid.</returns>
    public ValidationResult ValidateDataPoints(List<DataPoint> dataPoints)
    {
        if (dataPoints is null)
            return new ValidationResult { IsValid = false, ErrorMessage = "Data points collection is null" };

        if (dataPoints.Count == 0)
            return new ValidationResult { IsValid = false, ErrorMessage = "Data points collection is empty" };

        var invalidPoints = new List<long>();
        var duplicateIds = new HashSet<long>();

        foreach (var dataPoint in dataPoints)
        {
            if (!dataPoint.Validate())
            {
                invalidPoints.Add(dataPoint.Id);
            }

            if (!duplicateIds.Add(dataPoint.Id))
            {
                invalidPoints.Add(dataPoint.Id);
            }
        }

        if (invalidPoints.Count > 0)
        {
            return new ValidationResult
            {
                IsValid = false,
                ErrorMessage = $"Found {invalidPoints.Count} invalid or duplicate data points",
                InvalidIndices = invalidPoints
            };
        }

        return new ValidationResult { IsValid = true };
    }

    /// <summary>
    /// Validates a pipeline configuration.
    /// </summary>
    /// <param name="config">The pipeline configuration to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> describing whether the configuration is valid.</returns>
    public static ValidationResult ValidatePipelineConfig(PipelineConfig config)
    {
        if (config is null)
            return new ValidationResult { IsValid = false, ErrorMessage = "Configuration is null" };

        if (!config.Validate())
            return new ValidationResult { IsValid = false, ErrorMessage = "Configuration validation failed" };

        if (config.Stages is null || config.Stages.Count == 0)
            return new ValidationResult { IsValid = false, ErrorMessage = "No pipeline stages configured" };

        return new ValidationResult { IsValid = true };
    }

    /// <summary>
    /// Validates processing results.
    /// </summary>
    /// <param name="results">The processing results to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> describing whether all results are valid.</returns>
    public static ValidationResult ValidateProcessingResults(List<ProcessingResult> results)
    {
        if (results is null)
            return new ValidationResult { IsValid = false, ErrorMessage = "Results collection is null" };

        var invalidResults = new List<long>();

        foreach (var result in results)
        {
            if (!result.IsValid())
            {
                invalidResults.Add(result.ResultId);
            }
        }

        if (invalidResults.Count > 0)
        {
            return new ValidationResult
            {
                IsValid = false,
                ErrorMessage = $"Found {invalidResults.Count} invalid results",
                InvalidIndices = invalidResults
            };
        }

        return new ValidationResult { IsValid = true };
    }

    /// <summary>
    /// Validates a window event.
    /// </summary>
    /// <param name="window">The window event to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> describing whether the window event is valid.</returns>
    public static ValidationResult ValidateWindowEvent(WindowEvent window)
    {
        if (window is null)
            return new ValidationResult { IsValid = false, ErrorMessage = "Window is null" };

        if (window.WindowStartMs >= window.WindowEndMs)
            return new ValidationResult { IsValid = false, ErrorMessage = "Window start time must be before end time" };

        if (window.DataPoints is null)
            return new ValidationResult { IsValid = false, ErrorMessage = "Window data points collection is null" };

        return new ValidationResult { IsValid = true };
    }

    /// <summary>
    /// Checks if a data point is within a time range.
    /// </summary>
    /// <param name="dataPoint">The data point whose timestamp is checked.</param>
    /// <param name="startMs">The inclusive start of the range, in milliseconds.</param>
    /// <param name="endMs">The inclusive end of the range, in milliseconds.</param>
    /// <returns><c>true</c> if the data point's timestamp is within the range; otherwise, <c>false</c>.</returns>
    public static bool IsInTimeRange(DataPoint dataPoint, long startMs, long endMs)
    {
        ArgumentNullException.ThrowIfNull(dataPoint);

        return dataPoint.Timestamp >= startMs && dataPoint.Timestamp <= endMs;
    }

    /// <summary>
    /// Checks if a value is within acceptable bounds.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="minValue">The inclusive lower bound.</param>
    /// <param name="maxValue">The inclusive upper bound.</param>
    /// <returns><c>true</c> if the value lies between the bounds (inclusive); otherwise, <c>false</c>.</returns>
    public static bool IsWithinBounds(double value, double minValue, double maxValue)
    {
        return value >= minValue && value <= maxValue;
    }
}

/// <summary>
/// Result of a validation operation.
/// </summary>
public sealed class ValidationResult
{
    /// <summary>
    /// Gets or sets a value indicating whether validation succeeded.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets the description of the validation failure, or an empty string when valid.
    /// </summary>
    public string ErrorMessage { get; set; } = "";

    /// <summary>
    /// Gets or sets the identifiers of the invalid or duplicate items that caused the failure.
    /// </summary>
    public List<long> InvalidIndices { get; set; } = new();

    /// <summary>
    /// Returns a short summary of the validation outcome.
    /// </summary>
    /// <returns>"Validation passed" when valid; otherwise the failure message prefixed with "Validation failed".</returns>
    public string GetSummary()
    {
        if (IsValid) return "Validation passed";
        return $"Validation failed: {ErrorMessage}";
    }

    /// <summary>
    /// Returns a readable representation of the validation result.
    /// </summary>
    /// <returns>"Valid" when valid; otherwise the error message, followed by the invalid indices when present.</returns>
    public override string ToString()
    {
        if (IsValid) return "Valid";

        var result = $"Invalid: {ErrorMessage}";
        return InvalidIndices.Count > 0
            ? $"{result} (Invalid indices ({InvalidIndices.Count}): {string.Join(", ", InvalidIndices)})"
            : result;
    }
}
