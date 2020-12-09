#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Utilities;

using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Extension methods for validating ValidationHelper instances.
/// </summary>
public static class ValidationHelperValidation
{
    /// <summary>
    /// Validates the ValidationHelper instance for common issues.
    /// </summary>
    /// <param name="value">The ValidationHelper instance to validate.</param>
    /// <returns>A list of human-readable validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static IReadOnlyList<string> Validate(this ValidationHelper value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // ValidationHelper is a static class with only static methods
        // No instance state to validate

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if the ValidationHelper instance is valid.
    /// </summary>
    /// <param name="value">The ValidationHelper instance to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static bool IsValid(this ValidationHelper value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return true; // ValidationHelper has no instance state to validate
    }

    /// <summary>
    /// Ensures that the ValidationHelper instance is valid.
    /// </summary>
    /// <param name="value">The ValidationHelper instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is not valid.</exception>
    public static void EnsureValid(this ValidationHelper value)
    {
        ArgumentNullException.ThrowIfNull(value);

        // ValidationHelper has no instance state to validate
        // No validation needed
    }
}