#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

namespace DotNetRealtimePipeline.Utilities;

using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Provides validation helpers for <see cref="PathHelper"/> instances.
/// </summary>
public static class PathHelperValidation
{
    /// <summary>
    /// Validates a <see cref="PathHelper"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The path helper to validate.</param>
    /// <returns>A list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this PathHelper value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate that the OriginalPath is not null or empty
        if (string.IsNullOrWhiteSpace(value.OriginalPath))
        {
            problems.Add("OriginalPath cannot be null or whitespace.");
        }

        // Validate that the NormalizedPath is a valid absolute path
        if (string.IsNullOrWhiteSpace(value.NormalizedPath))
        {
            problems.Add("NormalizedPath cannot be null or whitespace.");
        }
        else if (!Path.IsPathFullyQualified(value.NormalizedPath))
        {
            problems.Add("NormalizedPath must be an absolute path.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="PathHelper"/> instance is valid.
    /// </summary>
    /// <param name="value">The path helper to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this PathHelper value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="PathHelper"/> instance is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The path helper to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of validation problems.</exception>
    public static void EnsureValid(this PathHelper value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"PathHelper instance is not valid. Problems:\n  - {string.Join("\n  - ", problems)}",
                nameof(value));
        }
    }
}