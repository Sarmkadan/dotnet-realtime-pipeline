#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Configuration;

using System;
using System.Collections.Generic;

/// <summary>
/// Provides validation helpers for <see cref="WorkerOptions"/> instances.
/// </summary>
public static class EventServiceConfigurationValidation
{
    /// <summary>
    /// Validates a <see cref="WorkerOptions"/> instance and returns any validation problems.
    /// </summary>
    /// <param name="value">The <see cref="WorkerOptions"/> to validate.</param>
    /// <returns>A read-only list of validation problem descriptions. Empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this WorkerOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate interval values (must be positive)
        if (value.MetricsAggregationIntervalMs <= 0)
        {
            problems.Add(
                $"MetricsAggregationIntervalMs must be a positive integer (got {value.MetricsAggregationIntervalMs}).");
        }

        if (value.HealthCheckIntervalMs <= 0)
        {
            problems.Add(
                $"HealthCheckIntervalMs must be a positive integer (got {value.HealthCheckIntervalMs}).");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="WorkerOptions"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="WorkerOptions"/> to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this WorkerOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="WorkerOptions"/> instance is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The <see cref="WorkerOptions"/> to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of validation problems.</exception>
    public static void EnsureValid(this WorkerOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"WorkerOptions validation failed with {problems.Count} problem(s):{Environment.NewLine}- ".Replace("- ", string.Empty) +
            string.Join(Environment.NewLine + "- ", problems),
            nameof(value));
    }
}