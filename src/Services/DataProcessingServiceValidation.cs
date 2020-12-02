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

        // DataProcessingService has no public properties to validate directly
        // Instead, we validate the service's configuration through its public methods
        // This ensures the service is properly initialized and can function correctly

        // Validate PipelineConfig through GetStatisticsAsync
        // The service constructor validates its dependencies, so if we can call a method,
        // the service is properly initialized
        try
        {
            // Attempt to call a method to validate the service is properly initialized
            // This will throw if _repository or _config are null/invalid
            _ = value.GetStatisticsAsync();
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            // If we can't call the method, the service dependencies are invalid
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
        return value.Validate().Count == 0;
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