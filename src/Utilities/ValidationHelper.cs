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
    public static bool IsInTimeRange(DataPoint dataPoint, long startMs, long endMs)
    {
        return dataPoint.Timestamp >= startMs && dataPoint.Timestamp <= endMs;
    }

    /// <summary>
    /// Checks if a value is within acceptable bounds.
    /// </summary>
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
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; } = "";
    public List<long> InvalidIndices { get; set; } = new();

    public string GetSummary()
    {
        if (IsValid) return "Validation passed";
        return $"Validation failed: {ErrorMessage}";
    }
}
