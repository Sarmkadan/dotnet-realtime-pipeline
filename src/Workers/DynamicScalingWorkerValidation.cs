#nullable enable

namespace DotNetRealtimePipeline.Workers;

using System;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// Validation helpers for <see cref="DynamicScalingWorker"/> instances.
/// </summary>
public static class DynamicScalingWorkerValidation
{
    private static readonly BindingFlags PrivateInstanceFlags = BindingFlags.NonPublic | BindingFlags.Instance;
    private static readonly FieldInfo ScalingServiceField = typeof(DynamicScalingWorker).GetField("_scalingService", PrivateInstanceFlags) ?? throw new InvalidOperationException("Could not find _scalingService field in DynamicScalingWorker");
    private static readonly FieldInfo LoggerField = typeof(DynamicScalingWorker).GetField("_logger", PrivateInstanceFlags) ?? throw new InvalidOperationException("Could not find _logger field in DynamicScalingWorker");
    private static readonly FieldInfo IntervalMsField = typeof(DynamicScalingWorker).GetField("_intervalMs", PrivateInstanceFlags) ?? throw new InvalidOperationException("Could not find _intervalMs field in DynamicScalingWorker");
    private static readonly FieldInfo IsRunningField = typeof(DynamicScalingWorker).GetField("_isRunning", PrivateInstanceFlags) ?? throw new InvalidOperationException("Could not find _isRunning field in DynamicScalingWorker");

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
        var scalingService = ScalingServiceField.GetValue(value);
        if (scalingService is null)
        {
            errors.Add("DynamicScalingWorker._scalingService cannot be null.");
        }

        var logger = LoggerField.GetValue(value);
        if (logger is null)
        {
            errors.Add("DynamicScalingWorker._logger cannot be null.");
        }

        var intervalMs = (int)IntervalMsField.GetValue(value)!;
        if (intervalMs < 500)
        {
            errors.Add($"DynamicScalingWorker._intervalMs must be at least 500 (got {intervalMs}).");
        }

        // Validate worker state consistency
        var isRunning = (bool)IsRunningField.GetValue(value)!;
        if (isRunning && !value.IsRunning)
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
    public static bool IsValid(this DynamicScalingWorker value) => value?.Validate().Count == 0;

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
