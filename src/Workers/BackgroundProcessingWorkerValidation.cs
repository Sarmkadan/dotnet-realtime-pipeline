#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Workers;

using System;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// Provides validation helpers for <see cref="BackgroundProcessingWorker"/> and related worker types.
/// </summary>
public static class BackgroundProcessingWorkerValidation
{
    /// <summary>
    /// Validates a <see cref="BackgroundProcessingWorker"/> instance.
    /// </summary>
    /// <param name="value">The worker instance to validate.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this BackgroundProcessingWorker value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether a <see cref="BackgroundProcessingWorker"/> instance is valid.
    /// </summary>
    /// <param name="value">The worker instance to check.</param>
    /// <returns>True if valid; otherwise false.</returns>
    public static bool IsValid(this BackgroundProcessingWorker value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="BackgroundProcessingWorker"/> instance is valid.
    /// </summary>
    /// <param name="value">The worker instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid.</exception>
    public static void EnsureValid(this BackgroundProcessingWorker value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"BackgroundProcessingWorker is not valid. Errors: {string.Join(", ", errors)}");
        }
    }

    /// <summary>
    /// Validates a <see cref="MetricsAggregationWorker"/> instance.
    /// </summary>
    /// <param name="value">The worker instance to validate.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if reflection fails to access private field.</exception>
    public static IReadOnlyList<string> Validate(this MetricsAggregationWorker value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate intervalMs is positive
        if (value.GetIntervalMs() <= 0)
        {
            errors.Add("Interval must be greater than 0 milliseconds.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="MetricsAggregationWorker"/> instance is valid.
    /// </summary>
    /// <param name="value">The worker instance to check.</param>
    /// <returns>True if valid; otherwise false.</returns>
    public static bool IsValid(this MetricsAggregationWorker value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="MetricsAggregationWorker"/> instance is valid.
    /// </summary>
    /// <param name="value">The worker instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid.</exception>
    public static void EnsureValid(this MetricsAggregationWorker value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"MetricsAggregationWorker is not valid. Errors: {string.Join(", ", errors)}");
        }
    }

    /// <summary>
    /// Validates a <see cref="HealthCheckWorker"/> instance.
    /// </summary>
    /// <param name="value">The worker instance to validate.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if reflection fails to access private field.</exception>
    public static IReadOnlyList<string> Validate(this HealthCheckWorker value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate intervalMs is positive
        if (value.GetIntervalMs() <= 0)
        {
            errors.Add("Interval must be greater than 0 milliseconds.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="HealthCheckWorker"/> instance is valid.
    /// </summary>
    /// <param name="value">The worker instance to check.</param>
    /// <returns>True if valid; otherwise false.</returns>
    public static bool IsValid(this HealthCheckWorker value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="HealthCheckWorker"/> instance is valid.
    /// </summary>
    /// <param name="value">The worker instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid.</exception>
    public static void EnsureValid(this HealthCheckWorker value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"HealthCheckWorker is not valid. Errors: {string.Join(", ", errors)}");
        }
    }

    /// <summary>
    /// Validates a <see cref="WorkerCoordinator"/> instance.
    /// </summary>
    /// <param name="value">The coordinator instance to validate.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this WorkerCoordinator value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether a <see cref="WorkerCoordinator"/> instance is valid.
    /// </summary>
    /// <param name="value">The coordinator instance to check.</param>
    /// <returns>True if valid; otherwise false.</returns>
    public static bool IsValid(this WorkerCoordinator value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="WorkerCoordinator"/> instance is valid.
    /// </summary>
    /// <param name="value">The coordinator instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid.</exception>
    public static void EnsureValid(this WorkerCoordinator value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"WorkerCoordinator is not valid. Errors: {string.Join(", ", errors)}");
        }
    }

    // Helper methods to access private fields for validation
    private static int GetIntervalMs(this MetricsAggregationWorker worker)
    {
        const string fieldName = "_intervalMs";
        var field = typeof(MetricsAggregationWorker).GetField(
            fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"Field '{fieldName}' not found on {typeof(MetricsAggregationWorker).Name}.");

        return (int)field.GetValue(worker)!;
    }

    private static int GetIntervalMs(this HealthCheckWorker worker)
    {
        const string fieldName = "_intervalMs";
        var field = typeof(HealthCheckWorker).GetField(
            fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"Field '{fieldName}' not found on {typeof(HealthCheckWorker).Name}.");

        return (int)field.GetValue(worker)!;
    }
}