#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotNetRealtimePipeline.Utilities;

using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Provides validation helpers for SerializationHelper operations and its related types.
/// Validates DataPoint, ProcessingResult, and MetricAggregation objects before serialization.
/// </summary>
public static class SerializationHelperValidation
{
    /// <summary>
    /// Validates a SerializationHelper instance by validating its related objects.
    /// </summary>
    /// <param name="value">The SerializationHelper instance to validate</param>
    /// <returns>A list of human-readable validation problems, or empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> ValidateSerialization(this SerializationHelper? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates a DataPoint instance.
    /// </summary>
    /// <param name="dataPoint">The DataPoint to validate</param>
    /// <returns>A list of human-readable validation problems, or empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="dataPoint"/> is null</exception>
    public static IReadOnlyList<string> ValidateDataPoint(this DataPoint dataPoint)
    {
        ArgumentNullException.ThrowIfNull(dataPoint);

        var problems = new List<string>();

        if (dataPoint.Id <= 0)
        {
            problems.Add($"DataPoint.Id must be positive, got {dataPoint.Id}");
        }

        if (dataPoint.Timestamp <= 0)
        {
            problems.Add($"DataPoint.Timestamp must be positive, got {dataPoint.Timestamp}");
        }

        if (string.IsNullOrWhiteSpace(dataPoint.Source))
        {
            problems.Add("DataPoint.Source cannot be null or whitespace");
        }

        if (dataPoint.Quality is < 0 or > 100)
        {
            problems.Add($"DataPoint.Quality must be between 0 and 100, got {dataPoint.Quality}");
        }

        if (double.IsNaN(dataPoint.Value) || double.IsInfinity(dataPoint.Value))
        {
            problems.Add($"DataPoint.Value must be a valid finite number, got {dataPoint.Value}");
        }

        if (dataPoint.CreatedAt == default)
        {
            problems.Add("DataPoint.CreatedAt cannot be default(DateTime)");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a ProcessingResult instance.
    /// </summary>
    /// <param name="result">The ProcessingResult to validate</param>
    /// <returns>A list of human-readable validation problems, or empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is null</exception>
    public static IReadOnlyList<string> ValidateProcessingResult(this ProcessingResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var problems = new List<string>();

        if (result.ResultId <= 0)
        {
            problems.Add($"ProcessingResult.ResultId must be positive, got {result.ResultId}");
        }

        if (string.IsNullOrWhiteSpace(result.StageName))
        {
            problems.Add("ProcessingResult.StageName cannot be null or whitespace");
        }

        if (!result.Success && string.IsNullOrWhiteSpace(result.ErrorMessage))
        {
            problems.Add("ProcessingResult.ErrorMessage cannot be null or whitespace when Success is false");
        }

        if (result.ProcessingTimeMs < 0)
        {
            problems.Add($"ProcessingResult.ProcessingTimeMs must be non-negative, got {result.ProcessingTimeMs}");
        }

        if (result.ProcessedAt == default)
        {
            problems.Add("ProcessingResult.ProcessedAt cannot be default(DateTime)");
        }

        if (result.RetryCount < 0)
        {
            problems.Add($"ProcessingResult.RetryCount must be non-negative, got {result.RetryCount}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a MetricAggregation instance.
    /// </summary>
    /// <param name="metrics">The MetricAggregation to validate</param>
    /// <returns>A list of human-readable validation problems, or empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="metrics"/> is null</exception>
    public static IReadOnlyList<string> ValidateMetricAggregation(this MetricAggregation metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);

        var problems = new List<string>();

        if (metrics.MetricId <= 0)
        {
            problems.Add($"MetricAggregation.MetricId must be positive, got {metrics.MetricId}");
        }

        if (metrics.TimeWindowStartMs <= 0)
        {
            problems.Add($"MetricAggregation.TimeWindowStartMs must be positive, got {metrics.TimeWindowStartMs}");
        }

        if (metrics.TimeWindowEndMs <= 0)
        {
            problems.Add($"MetricAggregation.TimeWindowEndMs must be positive, got {metrics.TimeWindowEndMs}");
        }

        if (metrics.TimeWindowEndMs < metrics.TimeWindowStartMs)
        {
            problems.Add($"MetricAggregation.TimeWindowEndMs ({metrics.TimeWindowEndMs}) must be >= TimeWindowStartMs ({metrics.TimeWindowStartMs})");
        }

        if (string.IsNullOrWhiteSpace(metrics.MetricType))
        {
            problems.Add("MetricAggregation.MetricType cannot be null or whitespace");
        }

        if (metrics.TotalItemsProcessed < 0)
        {
            problems.Add($"MetricAggregation.TotalItemsProcessed must be non-negative, got {metrics.TotalItemsProcessed}");
        }

        if (metrics.TotalItemsFailed < 0)
        {
            problems.Add($"MetricAggregation.TotalItemsFailed must be non-negative, got {metrics.TotalItemsFailed}");
        }

        if (metrics.TotalItemsSkipped < 0)
        {
            problems.Add($"MetricAggregation.TotalItemsSkipped must be non-negative, got {metrics.TotalItemsSkipped}");
        }

        if (metrics.AverageProcessingTimeMs < 0)
        {
            problems.Add($"MetricAggregation.AverageProcessingTimeMs must be non-negative, got {metrics.AverageProcessingTimeMs}");
        }

        if (metrics.MinProcessingTimeMs < 0)
        {
            problems.Add($"MetricAggregation.MinProcessingTimeMs must be non-negative, got {metrics.MinProcessingTimeMs}");
        }

        if (metrics.MaxProcessingTimeMs < 0)
        {
            problems.Add($"MetricAggregation.MaxProcessingTimeMs must be non-negative, got {metrics.MaxProcessingTimeMs}");
        }

        if (metrics.P95ProcessingTimeMs < 0)
        {
            problems.Add($"MetricAggregation.P95ProcessingTimeMs must be non-negative, got {metrics.P95ProcessingTimeMs}");
        }

        if (metrics.P99ProcessingTimeMs < 0)
        {
            problems.Add($"MetricAggregation.P99ProcessingTimeMs must be non-negative, got {metrics.P99ProcessingTimeMs}");
        }

        if (metrics.BackpressureEvents < 0)
        {
            problems.Add($"MetricAggregation.BackpressureEvents must be non-negative, got {metrics.BackpressureEvents}");
        }

        if (metrics.TotalBackpressureMs < 0)
        {
            problems.Add($"MetricAggregation.TotalBackpressureMs must be non-negative, got {metrics.TotalBackpressureMs}");
        }

        if (metrics.ComputedAt == default)
        {
            problems.Add("MetricAggregation.ComputedAt cannot be default(DateTime)");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a list of DataPoint instances.
    /// </summary>
    /// <param name="dataPoints">The list of DataPoint to validate</param>
    /// <returns>A list of human-readable validation problems, or empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="dataPoints"/> is null</exception>
    public static IReadOnlyList<string> ValidateDataPoints(this List<DataPoint> dataPoints)
    {
        ArgumentNullException.ThrowIfNull(dataPoints);

        var problems = new List<string>();

        if (dataPoints.Count == 0)
        {
            problems.Add("DataPoint list cannot be empty");
        }

        for (int i = 0; i < dataPoints.Count; i++)
        {
            var problemsForItem = dataPoints[i].ValidateDataPoint();
            if (problemsForItem.Count > 0)
            {
                problems.AddRange(problemsForItem.Select(p => $"DataPoint[{i}]: {p}"));
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a list of ProcessingResult instances.
    /// </summary>
    /// <param name="results">The list of ProcessingResult to validate</param>
    /// <returns>A list of human-readable validation problems, or empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="results"/> is null</exception>
    public static IReadOnlyList<string> ValidateProcessingResults(this List<ProcessingResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var problems = new List<string>();

        if (results.Count == 0)
        {
            problems.Add("ProcessingResult list cannot be empty");
        }

        for (int i = 0; i < results.Count; i++)
        {
            var problemsForItem = results[i].ValidateProcessingResult();
            if (problemsForItem.Count > 0)
            {
                problems.AddRange(problemsForItem.Select(p => $"ProcessingResult[{i}]: {p}"));
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a SerializationHelper instance is valid.
    /// </summary>
    /// <param name="value">The SerializationHelper instance to check</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(this SerializationHelper? value) => value?.ValidateSerialization().Count == 0;

    /// <summary>
    /// Checks if a DataPoint instance is valid.
    /// </summary>
    /// <param name="dataPoint">The DataPoint to check</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(this DataPoint dataPoint) => dataPoint.ValidateDataPoint().Count == 0;

    /// <summary>
    /// Checks if a ProcessingResult instance is valid.
    /// </summary>
    /// <param name="result">The ProcessingResult to check</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(this ProcessingResult result) => result.ValidateProcessingResult().Count == 0;

    /// <summary>
    /// Checks if a MetricAggregation instance is valid.
    /// </summary>
    /// <param name="metrics">The MetricAggregation to check</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(this MetricAggregation metrics) => metrics.ValidateMetricAggregation().Count == 0;

    /// <summary>
    /// Ensures that a SerializationHelper instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The SerializationHelper instance to validate</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid</exception>
    public static void EnsureValid(this SerializationHelper? value)
    {
        var problems = value.ValidateSerialization();
        if (problems.Count > 0)
        {
            throw new ArgumentException("SerializationHelper validation failed:\n" + string.Join("\n", problems));
        }
    }

    /// <summary>
    /// Ensures that a DataPoint instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="dataPoint">The DataPoint to validate</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="dataPoint"/> is not valid</exception>
    public static void EnsureValid(this DataPoint dataPoint)
    {
        var problems = dataPoint.ValidateDataPoint();
        if (problems.Count > 0)
        {
            throw new ArgumentException("DataPoint validation failed:\n" + string.Join("\n", problems));
        }
    }

    /// <summary>
    /// Ensures that a ProcessingResult instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="result">The ProcessingResult to validate</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="result"/> is not valid</exception>
    public static void EnsureValid(this ProcessingResult result)
    {
        var problems = result.ValidateProcessingResult();
        if (problems.Count > 0)
        {
            throw new ArgumentException("ProcessingResult validation failed:\n" + string.Join("\n", problems));
        }
    }

    /// <summary>
    /// Ensures that a MetricAggregation instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="metrics">The MetricAggregation to validate</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="metrics"/> is not valid</exception>
    public static void EnsureValid(this MetricAggregation metrics)
    {
        var problems = metrics.ValidateMetricAggregation();
        if (problems.Count > 0)
        {
            throw new ArgumentException("MetricAggregation validation failed:\n" + string.Join("\n", problems));
        }
    }
}