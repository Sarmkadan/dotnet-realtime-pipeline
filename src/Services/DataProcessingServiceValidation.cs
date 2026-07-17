#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Services;

using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;

/// <summary>
/// Provides validation helpers for <see cref="DataProcessingService"/> instances.
/// Validates the service's configuration and internal state.
/// </summary>
public static class DataProcessingServiceValidation
{
    /// <summary>
    /// Validates a <see cref="DataProcessingService"/> instance and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The service instance to validate.</param>
    /// <returns>A read-only list of validation error messages; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this DataProcessingService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate the service's internal state by checking its configuration
        // Since DataProcessingService is sealed with private readonly fields,
        // we validate by attempting to access its configuration
        try
        {
            // Access the service's configuration to validate it's properly initialized
            // The service constructor ensures _config is not null, but we validate its state
            var config = value.GetType()
                .GetField("_config", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(value) as PipelineConfig;

            if (config is null)
            {
                errors.Add("Service configuration is null");
            }
            else if (!config.Validate())
            {
                errors.Add("Service configuration is invalid");
            }
        }
        catch
        {
            errors.Add("Service dependencies (repository or configuration) are invalid or inaccessible");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="DataProcessingService"/> instance is valid.
    /// </summary>
    /// <param name="value">The service to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this DataProcessingService value)
    {
        return value is not null && value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="DataProcessingService"/> instance is valid, throwing an <see cref="ArgumentException"/>
    /// with detailed validation messages if it is not.
    /// </summary>
    /// <param name="value">The service to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, containing a list of validation problems.</exception>
    public static void EnsureValid(this DataProcessingService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"DataProcessingService is invalid. Validation errors:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}",
            nameof(value)
        );
    }
}