#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace DotNetRealtimePipeline.Tests.Unit;

public static class MetricsServiceTestsValidation
{
    /// <summary>
    /// Validates the <see cref="MetricsServiceTests"/> instance for common issues.
    /// </summary>
    /// <param name="value">The test class instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> Validate(this MetricsServiceTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate private fields via public API where possible
        // No public properties to validate directly

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="MetricsServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The test class instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this MetricsServiceTests? value)
    {
        return value?.Validate() is var errors && errors.Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="MetricsServiceTests"/> instance is valid,
    /// throwing an <see cref="ArgumentException"/> with a detailed message if it is not.
    /// </summary>
    /// <param name="value">The test class instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the instance is not valid.</exception>
    public static void EnsureValid(this MetricsServiceTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                message: $"MetricsServiceTests instance is not valid. Problems:\n{string.Join("\n", errors)}",
                paramName: nameof(value));
        }
    }
}
