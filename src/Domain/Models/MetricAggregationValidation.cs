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
/// Provides validation helpers for <see cref="MetricAggregation"/> instances.
/// </summary>
/// <remarks>
/// This static class contains extension methods for validating <see cref="MetricAggregation"/> objects.
/// It validates all properties including MetricId, time windows, MetricType, processing metrics,
/// percentile relationships, backpressure metrics, ComputedAt timestamp, and dictionary values.
/// All methods throw <see cref="ArgumentNullException"/> for null inputs and provide detailed error messages
/// for validation failures.
/// </remarks>
public static class MetricAggregationValidation
{
    /// <summary>
    /// Validates the specified <see cref="MetricAggregation"/> instance.
    /// </summary>
    /// <param name="value">The metric aggregation to validate.</param>
    /// <returns>A list of validation error messages; empty if the metric aggregation is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this MetricAggregation value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate MetricId
        if (value.MetricId <= 0)
        {
            errors.Add($"MetricId must be a positive integer, but was {value.MetricId}.");
        }

        // Validate time window
        if (value.TimeWindowStartMs < 0)
        {
            errors.Add($"TimeWindowStartMs must be non-negative, but was {value.TimeWindowStartMs}.");
        }

        if (value.TimeWindowEndMs < 0)
        {
            errors.Add($"TimeWindowEndMs must be non-negative, but was {value.TimeWindowEndMs}.");
        }

        if (value.TimeWindowEndMs < value.TimeWindowStartMs)
        {
            errors.Add(
                $"TimeWindowEndMs ({value.TimeWindowEndMs}) must be greater than or equal to TimeWindowStartMs ({value.TimeWindowStartMs}).");
        }

        // Validate MetricType
        if (string.IsNullOrWhiteSpace(value.MetricType))
        {
            errors.Add("MetricType cannot be null or whitespace.");
        }
        else if (value.MetricType.Length > 100)
        {
            errors.Add($"MetricType length must be 100 characters or less, but was {value.MetricType.Length}.");
        }

        // Validate processing metrics
        if (value.TotalItemsProcessed < 0)
        {
            errors.Add($"TotalItemsProcessed must be non-negative, but was {value.TotalItemsProcessed}.");
        }

        if (value.TotalItemsFailed < 0)
        {
            errors.Add($"TotalItemsFailed must be non-negative, but was {value.TotalItemsFailed}.");
        }

        if (value.TotalItemsSkipped < 0)
        {
            errors.Add($"TotalItemsSkipped must be non-negative, but was {value.TotalItemsSkipped}.");
        }

        // Validate processing time metrics
        if (value.AverageProcessingTimeMs < 0)
        {
            errors.Add($"AverageProcessingTimeMs must be non-negative, but was {value.AverageProcessingTimeMs}.");
        }

        if (value.MinProcessingTimeMs < 0)
        {
            errors.Add($"MinProcessingTimeMs must be non-negative, but was {value.MinProcessingTimeMs}.");
        }

        if (value.MaxProcessingTimeMs < 0)
        {
            errors.Add($"MaxProcessingTimeMs must be non-negative, but was {value.MaxProcessingTimeMs}.");
        }

        if (value.P95ProcessingTimeMs < 0)
        {
            errors.Add($"P95ProcessingTimeMs must be non-negative, but was {value.P95ProcessingTimeMs}.");
        }

        if (value.P99ProcessingTimeMs < 0)
        {
            errors.Add($"P99ProcessingTimeMs must be non-negative, but was {value.P99ProcessingTimeMs}.");
        }

        // Validate percentile relationships
        if (value.P95ProcessingTimeMs > 0 && value.MaxProcessingTimeMs > 0 && value.P95ProcessingTimeMs > value.MaxProcessingTimeMs)
        {
            errors.Add(
                $"P95ProcessingTimeMs ({value.P95ProcessingTimeMs}) must be less than or equal to MaxProcessingTimeMs ({value.MaxProcessingTimeMs}).");
        }

        if (value.P99ProcessingTimeMs > 0 && value.MaxProcessingTimeMs > 0 && value.P99ProcessingTimeMs > value.MaxProcessingTimeMs)
        {
            errors.Add(
                $"P99ProcessingTimeMs ({value.P99ProcessingTimeMs}) must be less than or equal to MaxProcessingTimeMs ({value.MaxProcessingTimeMs}).");
        }

        if (value.P99ProcessingTimeMs > 0 && value.P95ProcessingTimeMs > 0 && value.P99ProcessingTimeMs < value.P95ProcessingTimeMs)
        {
            errors.Add(
                $"P99ProcessingTimeMs ({value.P99ProcessingTimeMs}) must be greater than or equal to P95ProcessingTimeMs ({value.P95ProcessingTimeMs}).");
        }

        // Validate backpressure metrics
        if (value.BackpressureEvents < 0)
        {
            errors.Add($"BackpressureEvents must be non-negative, but was {value.BackpressureEvents}.");
        }

        if (value.TotalBackpressureMs < 0)
        {
            errors.Add($"TotalBackpressureMs must be non-negative, but was {value.TotalBackpressureMs}.");
        }

        // Validate ComputedAt
        if (value.ComputedAt == default)
        {
            errors.Add("ComputedAt must be set to a valid DateTime, but was default(DateTime).");
        }
        else if (value.ComputedAt.Kind != DateTimeKind.Utc)
        {
            errors.Add("ComputedAt must be in UTC timezone.");
        }

        // Validate CountBySource dictionary
        if (value.CountBySource is null)
        {
            errors.Add("CountBySource dictionary cannot be null.");
        }
        else
        {
            foreach (var kvp in value.CountBySource)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    errors.Add("CountBySource contains an entry with null or whitespace key.");
                    break;
                }

                if (kvp.Value < 0)
                {
                    errors.Add(
                        $"CountBySource[{kvp.Key}] must be non-negative, but was {kvp.Value}.");
                }
            }
        }

        // Validate ErrorRateByStage dictionary
        if (value.ErrorRateByStage is null)
        {
            errors.Add("ErrorRateByStage dictionary cannot be null.");
        }
        else
        {
            foreach (var kvp in value.ErrorRateByStage)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    errors.Add("ErrorRateByStage contains an entry with null or whitespace key.");
                    break;
                }

                if (kvp.Value < 0 || kvp.Value > 100)
                {
                    errors.Add(
                        $"ErrorRateByStage[{kvp.Key}] must be between 0 and 100, but was {kvp.Value:F2}.");
                }
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="MetricAggregation"/> is valid.
    /// </summary>
    /// <param name="value">The metric aggregation to check.</param>
    /// <returns><see langword="true"/> if the metric aggregation is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this MetricAggregation value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="MetricAggregation"/> is valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="value">The metric aggregation to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid.</exception>
 /// <remarks>
 /// This method validates all properties including MetricId, time windows, MetricType, processing metrics,
 /// percentile relationships, backpressure metrics, ComputedAt timestamp, and dictionary values.
 /// If validation fails, an <see cref="ArgumentException"/> is thrown with detailed error messages.
 /// </remarks>
    public static void EnsureValid(this MetricAggregation value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"MetricAggregation is invalid. Validation failed with {errors.Count} error(s):{Environment.NewLine}- " +
            string.Join($"{Environment.NewLine}- ", errors));
    }
}