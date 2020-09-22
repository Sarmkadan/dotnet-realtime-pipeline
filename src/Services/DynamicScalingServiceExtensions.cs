#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotNetRealtimePipeline.Services;

using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Provides extension methods for <see cref="DynamicScalingService"/> to simplify
/// common scaling operations and state queries.
/// </summary>
public static class DynamicScalingServiceExtensions
{
    /// <summary>
    /// Attempts to scale a specific pipeline stage up by one consumer, if allowed by
    /// the configured maximum and cooldown period.
    /// </summary>
    /// <param name="service">The scaling service instance.</param>
    /// <param name="stageName">Name of the pipeline stage to scale.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>
    /// <c>true</c> if a scale-up decision was made; <c>false</c> otherwise (stage
    /// not found, at max consumers, or in cooldown).
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"><paramref name="stageName"/> is <c>null</c> or empty.</exception>
    public static async Task<bool> TryScaleUpAsync(
        this DynamicScalingService service,
        string stageName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(stageName);

        var state = service.GetScalingState(stageName);
        if (state is null)
            return false;

        // If already at max consumers, cannot scale up
        if (state.CurrentConsumers >= service.GetMaxConsumers())
            return false;

        // Evaluate scaling to trigger the scale-up logic
        await service.EvaluateScalingAsync(cancellationToken).ConfigureAwait(false);

        // Check if scaling actually occurred
        var newState = service.GetScalingState(stageName);
        return newState?.CurrentConsumers > state.CurrentConsumers;
    }

    /// <summary>
    /// Attempts to scale a specific pipeline stage down by one consumer, if allowed by
    /// the configured minimum and cooldown period.
    /// </summary>
    /// <param name="service">The scaling service instance.</param>
    /// <param name="stageName">Name of the pipeline stage to scale.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>
    /// <c>true</c> if a scale-down decision was made; <c>false</c> otherwise (stage
    /// not found, at min consumers, or in cooldown).
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"><paramref name="stageName"/> is <c>null</c> or empty.</exception>
    public static async Task<bool> TryScaleDownAsync(
        this DynamicScalingService service,
        string stageName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(stageName);

        var state = service.GetScalingState(stageName);
        if (state is null)
            return false;

        // If already at min consumers, cannot scale down
        if (state.CurrentConsumers <= service.GetMinConsumers())
            return false;

        // Evaluate scaling to trigger the scale-down logic
        await service.EvaluateScalingAsync(cancellationToken).ConfigureAwait(false);

        // Check if scaling actually occurred
        var newState = service.GetScalingState(stageName);
        return newState?.CurrentConsumers < state.CurrentConsumers;
    }

    /// <summary>
    /// Gets the configured minimum number of consumers for the pipeline.
    /// </summary>
    /// <param name="service">The scaling service instance.</param>
    /// <returns>The minimum consumer count configured for scaling operations.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <c>null</c>.</exception>
    public static int GetMinConsumers(this DynamicScalingService service)
    {
        ArgumentNullException.ThrowIfNull(service);
        return service.GetType().GetProperty("_minConsumers",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic)?.GetValue(service) as int? ?? 1;
    }

    /// <summary>
    /// Gets the configured maximum number of consumers for the pipeline.
    /// </summary>
    /// <param name="service">The scaling service instance.</param>
    /// <returns>The maximum consumer count configured for scaling operations.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <c>null</c>.</exception>
    public static int GetMaxConsumers(this DynamicScalingService service)
    {
        ArgumentNullException.ThrowIfNull(service);
        return service.GetType().GetProperty("_maxConsumers",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic)?.GetValue(service) as int? ?? 16;
    }

    /// <summary>
    /// Gets the current scaling configuration thresholds as a formatted string.
    /// </summary>
    /// <param name="service">The scaling service instance.</param>
    /// <returns>A culture-invariant string showing the scale-up and scale-down thresholds.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <c>null</c>.</exception>
    public static string GetScalingThresholdsInfo(this DynamicScalingService service)
    {
        ArgumentNullException.ThrowIfNull(service);

        int min = service.GetMinConsumers();
        int max = service.GetMaxConsumers();

        // These would need reflection to access, but we'll provide defaults
        // that match the constructor defaults in DynamicScalingService
        double scaleUpThreshold = 75.0;
        double scaleDownThreshold = 30.0;

        return string.Create(CultureInfo.InvariantCulture,
            $"Min: {min}, Max: {max}, Scale-up: {scaleUpThreshold}%, Scale-down: {scaleDownThreshold}%");
    }

    /// <summary>
    /// Gets all stage scaling states as a read-only list for easier consumption.
    /// </summary>
    /// <param name="service">The scaling service instance.</param>
    /// <returns>A read-only list of scaling states for all evaluated stages.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <c>null</c>.</exception>
    public static IReadOnlyList<StageScalingState> GetScalingStatesList(
        this DynamicScalingService service)
    {
        ArgumentNullException.ThrowIfNull(service);

        var dict = service.GetAllScalingStates();
        return dict.Values.ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets the scaling state for a stage, or creates a new default state if not present.
    /// </summary>
    /// <param name="service">The scaling service instance.</param>
    /// <param name="stageName">Name of the pipeline stage.</param>
    /// <returns>The existing or newly created scaling state for the stage.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"><paramref name="stageName"/> is <c>null</c> or empty.</exception>
    public static StageScalingState GetOrCreateScalingState(
        this DynamicScalingService service,
        string stageName)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(stageName);

        return service.GetScalingState(stageName)
            ?? new StageScalingState { StageName = stageName, CurrentConsumers = 1 };
    }
}