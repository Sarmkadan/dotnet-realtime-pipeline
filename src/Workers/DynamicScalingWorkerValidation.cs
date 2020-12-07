#nullable enable

namespace DotNetRealtimePipeline.Workers;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;


/// <summary>
/// Validation helpers for <see cref="DynamicScalingWorker"/> instances.
/// </summary>
public static class DynamicScalingWorkerValidation
{
    /// <summary>
    /// Validates the specified <see cref="DynamicScalingWorker"/> instance.
    /// </summary>
    /// <param name="value">The worker instance to validate.</param>
    /// <returns>A list of human-readable validation errors; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this DynamicScalingWorker value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate required dependencies using reflection to access private fields
        var scalingServiceField = typeof(DynamicScalingWorker).GetField(
            "_scalingService", BindingFlags.NonPublic | BindingFlags.Instance);
        var loggerField = typeof(DynamicScalingWorker).GetField(
            "_logger", BindingFlags.NonPublic | BindingFlags.Instance);
        var intervalMsField = typeof(DynamicScalingWorker).GetField(
            "_intervalMs", BindingFlags.NonPublic | BindingFlags.Instance);

        if (scalingServiceField == null || loggerField == null || intervalMsField == null)
        {
            errors.Add("Could not access internal fields of DynamicScalingWorker for validation.");
            return errors.AsReadOnly();
        }

        var scalingService = scalingServiceField.GetValue(value);
        var logger = loggerField.GetValue(value);
        var intervalMs = intervalMsField.GetValue(value) as int?;

        if (scalingService == null)
        {
            errors.Add("DynamicScalingWorker._scalingService cannot be null.");
        }

        if (logger == null)
        {
            errors.Add("DynamicScalingWorker._logger cannot be null.");
        }

        if (intervalMs.HasValue)
        {
            if (intervalMs.Value < 500)
            {
                errors.Add(
                    $"DynamicScalingWorker._intervalMs must be at least 500 (got {intervalMs.Value.ToString(CultureInfo.InvariantCulture)}).");
            }
        }
        else
        {
            errors.Add("DynamicScalingWorker._intervalMs cannot be null.");
        }

        // Validate worker state
        var isRunningField = typeof(DynamicScalingWorker).GetField(
            "_isRunning", BindingFlags.NonPublic | BindingFlags.Instance);
        var isRunning = isRunningField?.GetValue(value) as bool?;

        if (isRunning.HasValue && isRunning.Value && value.IsRunning != true)
        {
            errors.Add("DynamicScalingWorker state inconsistency: _isRunning field does not match IsRunning property.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="DynamicScalingWorker"/> instance is valid.
    /// </summary>
    /// <param name="value">The worker instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this DynamicScalingWorker value)
    {
        return value?.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="DynamicScalingWorker"/> instance is valid.
    /// </summary>
    /// <param name="value">The worker instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is not valid, containing a list of validation errors.</exception>
    public static void EnsureValid(this DynamicScalingWorker value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"DynamicScalingWorker validation failed with {problems.Count} problem(s):{Environment.NewLine}- ".Replace("- ", string.Empty) +
            string.Join(Environment.NewLine + "- ", problems),
            nameof(value));
    }
}