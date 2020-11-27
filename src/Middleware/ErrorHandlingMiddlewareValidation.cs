#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Middleware;

using System;
using System.Collections.Generic;

/// <summary>
/// Provides validation extensions for <see cref="ErrorHandlingMiddleware"/> instances.
/// </summary>
public static class ErrorHandlingMiddlewareValidation
{
    /// <summary>
    /// Validates the specified <see cref="ErrorHandlingMiddleware"/> instance.
    /// </summary>
    /// <param name="value">The <see cref="ErrorHandlingMiddleware"/> instance to validate.</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this ErrorHandlingMiddleware value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // ErrorHandlingMiddleware has no public properties to validate
        // The validation is primarily for the ErrorResponse objects returned by its methods

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ErrorHandlingMiddleware"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="ErrorHandlingMiddleware"/> instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this ErrorHandlingMiddleware value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ErrorHandlingMiddleware"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="ErrorHandlingMiddleware"/> instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is not valid.</exception>
    public static void EnsureValid(this ErrorHandlingMiddleware value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"ErrorHandlingMiddleware is not valid. Errors:{Environment.NewLine}- {
                    string.Join(
                        $"\n- ",
                        errors
                    )
                }",
                nameof(value)
            );
        }
    }
}