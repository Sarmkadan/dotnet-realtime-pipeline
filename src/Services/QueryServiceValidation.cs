#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Services;

using System;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// Provides validation helpers for <see cref="QueryService"/> instances to ensure
/// constructor dependencies are properly initialized.
/// </summary>
public static class QueryServiceValidation
{
    /// <summary>
    /// Validates a <see cref="QueryService"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The QueryService instance to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this QueryService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate constructor-injected dependencies using reflection
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        var dataPointRepositoryField = typeof(QueryService).GetField(
            "_dataPointRepository", flags);
        var metricsRepositoryField = typeof(QueryService).GetField(
            "_metricsRepository", flags);

        if (dataPointRepositoryField?.GetValue(value) is null)
        {
            errors.Add("DataPointRepository dependency is null.");
        }

        if (metricsRepositoryField?.GetValue(value) is null)
        {
            errors.Add("MetricsRepository dependency is null.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="QueryService"/> instance is valid.
    /// </summary>
    /// <param name="value">The QueryService instance to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this QueryService value)
        => value.Validate().Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="QueryService"/> instance is valid.
    /// </summary>
    /// <param name="value">The QueryService instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing all validation errors.</exception>
    public static void EnsureValid(this QueryService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();

        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"QueryService validation failed with {errors.Count} error(s): {string.Join("; ", errors)}");
    }
}
