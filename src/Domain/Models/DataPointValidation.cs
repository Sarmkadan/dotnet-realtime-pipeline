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
/// Provides validation helpers for <see cref="DataPoint"/> instances.
/// </summary>
public static class DataPointValidation
{
    /// <summary>
    /// Validates a data point and returns a list of human-readable validation errors.
    /// </summary>
    /// <param name="value">The data point to validate.</param>
    /// <returns>An empty list if valid, otherwise a list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this DataPoint? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (value.Id <= 0)
        {
            errors.Add($"DataPoint.Id must be positive, but was {value.Id}.");
        }

        // Validate Timestamp
        if (value.Timestamp <= 0)
        {
            errors.Add($"DataPoint.Timestamp must be positive, but was {value.Timestamp}.");
        }

        // Validate Value (double can be any value, but NaN and Infinity are invalid)
        if (double.IsNaN(value.Value))
        {
            errors.Add("DataPoint.Value cannot be NaN.");
        }
        else if (double.IsInfinity(value.Value))
        {
            errors.Add("DataPoint.Value cannot be infinite.");
        }

        // Validate Source
        if (string.IsNullOrWhiteSpace(value.Source))
        {
            errors.Add("DataPoint.Source cannot be null or whitespace.");
        }

        // Validate Quality (0-100 range)
        if (value.Quality < 0 || value.Quality > 100)
        {
            errors.Add($"DataPoint.Quality must be between 0 and 100, but was {value.Quality}.");
        }

        // Validate CreatedAt (should not be default DateTime)
        if (value.CreatedAt == default)
        {
            errors.Add("DataPoint.CreatedAt cannot be default(DateTime).");
        }

        // Validate Metadata (should not be null)
        if (value.Metadata is null)
        {
            errors.Add("DataPoint.Metadata cannot be null.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified data point is valid.
    /// </summary>
    /// <param name="value">The data point to check.</param>
    /// <returns><see langword="true"/> if the data point is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this DataPoint? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the data point is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The data point to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the data point is invalid, containing the validation errors.</exception>
    public static void EnsureValid(this DataPoint? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"DataPoint is invalid. Validation errors: {string.Join(" ", errors)}");
        }
    }
}