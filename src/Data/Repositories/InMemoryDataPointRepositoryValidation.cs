#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Data.Repositories;

using System;
using System.Collections.Generic;

/// <summary>
/// Provides validation helpers for <see cref="InMemoryDataPointRepository"/> instances.
/// </summary>
public static class InMemoryDataPointRepositoryValidation
{
    /// <summary>
    /// Validates the repository state and data integrity.
    /// </summary>
    /// <param name="value">The repository instance to validate.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static IReadOnlyList<string> Validate(this InMemoryDataPointRepository value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate internal store integrity
        var store = value.GetInternalStore();

        foreach (var kvp in store)
        {
            var dataPoint = kvp.Value;

            if (dataPoint is null)
            {
                errors.Add($"DataPoint at ID {kvp.Key} is null");
                continue;
            }

            if (dataPoint.Id != kvp.Key)
            {
                errors.Add($"DataPoint ID mismatch: stored key {kvp.Key} does not match data point ID {dataPoint.Id}");
            }

            if (dataPoint.Id <= 0)
            {
                errors.Add($"DataPoint with ID {dataPoint.Id} has invalid ID (must be positive)");
            }

            if (dataPoint.Timestamp <= 0)
            {
                errors.Add($"DataPoint with ID {dataPoint.Id} has invalid Timestamp {dataPoint.Timestamp} (must be positive)");
            }

            if (string.IsNullOrWhiteSpace(dataPoint.Source))
            {
                errors.Add($"DataPoint with ID {dataPoint.Id} has null or empty Source");
            }

            if (dataPoint.Quality < 0 || dataPoint.Quality > 100)
            {
                errors.Add($"DataPoint with ID {dataPoint.Id} has invalid Quality {dataPoint.Quality} (must be between 0 and 100)");
            }

            if (dataPoint.CreatedAt == default)
            {
                errors.Add($"DataPoint with ID {dataPoint.Id} has default CreatedAt value");
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the repository state and data are valid.
    /// </summary>
    /// <param name="value">The repository instance to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static bool IsValid(this InMemoryDataPointRepository value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures the repository state and data are valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The repository instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, with error details.</exception>
    public static void EnsureValid(this InMemoryDataPointRepository value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"InMemoryDataPointRepository validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}