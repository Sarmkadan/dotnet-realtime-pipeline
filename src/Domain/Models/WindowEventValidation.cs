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
/// Provides validation helpers for <see cref="WindowEvent"/> instances.
/// Ensures window events meet business rules and data integrity constraints.
/// </summary>
public static class WindowEventValidation
{
    /// <summary>
    /// Validates a window event and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The window event to validate.</param>
    /// <returns>A read-only list of validation error messages. Empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this WindowEvent value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate WindowId
        if (value.WindowId <= 0)
        {
            errors.Add(
                $"WindowId must be positive, but was {value.WindowId}.");
        }

        // Validate WindowStartMs and WindowEndMs
        if (value.WindowStartMs <= 0)
        {
            errors.Add(
                $"WindowStartMs must be positive, but was {value.WindowStartMs}.");
        }

        if (value.WindowEndMs <= 0)
        {
            errors.Add(
                $"WindowEndMs must be positive, but was {value.WindowEndMs}.");
        }

        if (value.WindowEndMs <= value.WindowStartMs)
        {
            errors.Add(
                $"WindowEndMs ({value.WindowEndMs}) must be greater than WindowStartMs ({value.WindowStartMs}).");
        }

        // Validate AggregationType
        if (string.IsNullOrWhiteSpace(value.AggregationType))
        {
            errors.Add("AggregationType cannot be null or whitespace.");
        }
        else if (value.AggregationType.Length > 100)
        {
            errors.Add(
                $"AggregationType length must be 100 characters or less, but was {value.AggregationType.Length}.");
        }

        // Validate CreatedAt
        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt must be set to a valid DateTime.");
        }

        // Validate CreatedAtTicks
        if (value.CreatedAtTicks <= 0)
        {
            errors.Add(
                $"CreatedAtTicks must be positive, but was {value.CreatedAtTicks}.");
        }

        // Validate DataPoints collection
        if (value.DataPoints is null)
        {
            errors.Add("DataPoints collection cannot be null.");
        }
        else
        {
            foreach (var dataPoint in value.DataPoints)
            {
                if (dataPoint is null)
                {
                    errors.Add("DataPoints collection contains a null data point.");
                    break;
                }

                // Validate each data point's timestamp falls within window
                if (dataPoint.Timestamp < value.WindowStartMs || dataPoint.Timestamp > value.WindowEndMs)
                {
                    errors.Add(
                        $"DataPoint with Id {dataPoint.Id} has timestamp {dataPoint.Timestamp} " +
                        $"outside window range [{value.WindowStartMs}, {value.WindowEndMs}].");
                }
            }
        }

        // Validate Description if not null
        if (value.Description is not null && value.Description.Length > 500)
        {
            errors.Add(
                $"Description length must be 500 characters or less, but was {value.Description.Length}.");
        }

        // Validate IsComplete flag
        // Note: IsComplete can be false during processing, so we don't enforce it must be true

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a window event is valid.
    /// </summary>
    /// <param name="value">The window event to check.</param>
    /// <returns>True if the window event is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this WindowEvent value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures a window event is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The window event to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the window event is invalid.</exception>
    public static void EnsureValid(this WindowEvent value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"WindowEvent validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", errors)}");
        }
    }

    /// <summary>
    /// Validates that a window event's duration is within acceptable bounds.
    /// </summary>
    /// <param name="value">The window event to validate.</param>
    /// <param name="maxDurationMs">Maximum allowed duration in milliseconds.</param>
    /// <param name="minDurationMs">Minimum allowed duration in milliseconds.</param>
    /// <returns>True if the duration is within bounds; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="maxDurationMs"/> or <paramref name="minDurationMs"/> are negative.</exception>
    public static bool IsDurationValid(
        this WindowEvent value,
        long maxDurationMs = 86400000, // 24 hours in milliseconds
        long minDurationMs = 1) // Minimum 1 millisecond
    {
        ArgumentNullException.ThrowIfNull(value);

        if (maxDurationMs < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxDurationMs),
                "Maximum duration cannot be negative.");
        }

        if (minDurationMs < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minDurationMs),
                "Minimum duration cannot be negative.");
        }

        if (minDurationMs > maxDurationMs)
        {
            throw new ArgumentException(
                "Minimum duration cannot be greater than maximum duration.");
        }

        var duration = value.GetDurationMs();
        return duration >= minDurationMs && duration <= maxDurationMs;
    }

    /// <summary>
    /// Validates that a window event has sufficient data points.
    /// </summary>
    /// <param name="value">The window event to validate.</param>
    /// <param name="minDataPoints">Minimum number of data points required.</param>
    /// <returns>True if the data point count meets the requirement; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="minDataPoints"/> is negative.</exception>
    public static bool HasSufficientDataPoints(
        this WindowEvent value,
        int minDataPoints = 1)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (minDataPoints < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minDataPoints),
                "Minimum data point count cannot be negative.");
        }

        return value.GetDataPointCount() >= minDataPoints;
    }

    /// <summary>
    /// Validates that a window event's aggregation type is supported.
    /// </summary>
    /// <param name="value">The window event to validate.</param>
    /// <param name="supportedTypes">Collection of supported aggregation types.</param>
    /// <returns>True if the aggregation type is supported; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> or <paramref name="supportedTypes"/> is null.</exception>
    public static bool HasSupportedAggregationType(
        this WindowEvent value,
        IReadOnlyCollection<string> supportedTypes)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(supportedTypes);

        if (string.IsNullOrWhiteSpace(value.AggregationType))
        {
            return false;
        }

        return supportedTypes.Contains(value.AggregationType, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validates that a window event's data points meet quality thresholds.
    /// </summary>
    /// <param name="value">The window event to validate.</param>
    /// <param name="qualityThreshold">Minimum quality threshold (0-100).</param>
    /// <returns>True if all data points meet the quality threshold; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="qualityThreshold"/> is outside 0-100 range.</exception>
    public static bool HasQualityDataPoints(
        this WindowEvent value,
        int qualityThreshold = 80)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (qualityThreshold < 0 || qualityThreshold > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(qualityThreshold),
                "Quality threshold must be between 0 and 100.");
        }

        if (value.DataPoints is null || value.DataPoints.Count == 0)
        {
            return false;
        }

        return value.DataPoints.All(dp => dp.Quality >= qualityThreshold);
    }

    /// <summary>
    /// Validates that a window event is complete and ready for processing.
    /// </summary>
    /// <param name="value">The window event to validate.</param>
    /// <returns>True if the window event is complete; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsCompleteAndValid(this WindowEvent value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value.IsComplete && value.IsValid();
    }

    /// <summary>
    /// Validates that a window event's timestamps are reasonable relative to current time.
    /// </summary>
    /// <param name="value">The window event to validate.</param>
    /// <param name="maxFutureMs">Maximum milliseconds in the future allowed for window end.</param>
    /// <returns>True if timestamps are reasonable; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="maxFutureMs"/> is negative.</exception>
    public static bool HasReasonableTimestamps(
        this WindowEvent value,
        long maxFutureMs = 3600000) // 1 hour in milliseconds
    {
        ArgumentNullException.ThrowIfNull(value);

        if (maxFutureMs < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxFutureMs),
                "Maximum future time cannot be negative.");
        }

        var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var windowEndMs = value.WindowEndMs;

        // Window end should not be too far in the future
        return windowEndMs <= nowMs + maxFutureMs;
    }
}