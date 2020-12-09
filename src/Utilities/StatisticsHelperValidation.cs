#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotNetRealtimePipeline.Utilities;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

/// <summary>
/// Provides validation helpers for <see cref="StatisticsHelper"/> instances.
/// </summary>
public static class StatisticsHelperValidation
{
    /// <summary>
    /// Validates a <see cref="StatisticsHelper"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The statistics helper to validate.</param>
    /// <returns>A list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this StatisticsHelper value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Mean
        if (double.IsNaN(value.Mean) || double.IsInfinity(value.Mean))
            problems.Add("Mean is NaN or infinity.");
        else if (value.Mean == 0 && !double.IsNaN(value.Mean))
            problems.Add("Mean is zero, which may indicate empty or invalid data.");

        // Validate Median
        if (double.IsNaN(value.Median) || double.IsInfinity(value.Median))
            problems.Add("Median is NaN or infinity.");
        else if (value.Median == 0 && !double.IsNaN(value.Median))
            problems.Add("Median is zero, which may indicate empty or invalid data.");

        // Validate StandardDeviation
        if (double.IsNaN(value.StandardDeviation) || double.IsInfinity(value.StandardDeviation))
            problems.Add("StandardDeviation is NaN or infinity.");
        else if (value.StandardDeviation < 0)
            problems.Add("StandardDeviation cannot be negative.");
        else if (value.StandardDeviation == 0 && !double.IsNaN(value.StandardDeviation))
            problems.Add("StandardDeviation is zero, which may indicate no variation in data.");

        // Validate CoefficientOfVariation
        if (double.IsNaN(value.CoefficientOfVariation) || double.IsInfinity(value.CoefficientOfVariation))
            problems.Add("CoefficientOfVariation is NaN or infinity.");
        else if (value.CoefficientOfVariation < 0)
            problems.Add("CoefficientOfVariation cannot be negative.");
        else if (value.CoefficientOfVariation == 0 && !double.IsNaN(value.CoefficientOfVariation))
            problems.Add("CoefficientOfVariation is zero, which may indicate no variation in data.");

        // Validate Outliers
        if (value.Outliers is null)
            problems.Add("Outliers collection is null.");
        else
        {
            foreach (var outlier in value.Outliers)
            {
                if (double.IsNaN(outlier) || double.IsInfinity(outlier))
                    problems.Add("An outlier value is NaN or infinity.");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="StatisticsHelper"/> instance is valid.
    /// </summary>
    /// <param name="value">The statistics helper to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this StatisticsHelper value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="StatisticsHelper"/> instance is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The statistics helper to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of validation problems.</exception>
    public static void EnsureValid(this StatisticsHelper value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"StatisticsHelper instance is not valid. Problems: {string.Join(" ", problems)}",
                nameof(value));
        }
    }
}