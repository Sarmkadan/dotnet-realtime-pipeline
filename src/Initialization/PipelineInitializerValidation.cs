#nullable enable

namespace DotNetRealtimePipeline.Initialization;

using System;
using System.Collections.Generic;
using System.Linq;
using DotNetRealtimePipeline.State;

/// <summary>
/// Provides validation helpers for <see cref="PipelineInitializer"/> instances.
/// </summary>
public static class PipelineInitializerValidation
{
    /// <summary>
    /// Validates the specified <see cref="PipelineInitializer"/> instance.
    /// </summary>
    /// <param name="value">The pipeline initializer to validate.</param>
    /// <returns>A list of validation errors; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this PipelineInitializer value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate service provider
        if (value.GetServiceProvider() is null)
        {
            errors.Add("ServiceProvider is null.");
        }

        // Validate logger
        if (value.GetLogger() is null)
        {
            errors.Add("Logger is null.");
        }

        // Validate state manager
        var stateManager = value.GetStateManager();
        if (stateManager is null)
        {
            errors.Add("StateManager is null.");
        }
        else
        {
            // Validate state manager itself
            var stateManagerErrors = stateManager.Validate();
            if (stateManagerErrors.Count > 0)
            {
                errors.AddRange(stateManagerErrors.Select(e => $"StateManager validation failed: {e}"));
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="PipelineInitializer"/> instance is valid.
    /// </summary>
    /// <param name="value">The pipeline initializer to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this PipelineInitializer value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="PipelineInitializer"/> instance is valid.
    /// </summary>
    /// <param name="value">The pipeline initializer to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of validation errors.</exception>
    public static void EnsureValid(this PipelineInitializer value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                "PipelineInitializer validation failed. Errors:\n" + string.Join("\n", errors),
                nameof(value));
        }
    }
}