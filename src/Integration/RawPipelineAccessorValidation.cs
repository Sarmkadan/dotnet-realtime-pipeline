#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotNetRealtimePipeline.Integration;

using System;
using System.Collections.Generic;

/// <summary>
/// Provides validation extensions for <see cref="RawPipelineAccessor"/> instances.
/// </summary>
public static class RawPipelineAccessorValidation
{
    /// <summary>
    /// Validates the specified <see cref="RawPipelineAccessor"/> instance.
    /// </summary>
    /// <param name="value">The <see cref="RawPipelineAccessor"/> to validate.</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this RawPipelineAccessor? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // RawPipelineAccessor itself has no user-configurable properties that can be invalid
        // The underlying Pipe is always created with valid options and managed internally
        // The only validation needed is that the object is not null (handled above)

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="RawPipelineAccessor"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="RawPipelineAccessor"/> to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this RawPipelineAccessor? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="RawPipelineAccessor"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="RawPipelineAccessor"/> to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is not valid.</exception>
    public static void EnsureValid(this RawPipelineAccessor? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"RawPipelineAccessor is not valid. Errors: {string.Join(", ", errors)}");
        }
    }
}