#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Middleware;

using System;
using System.Collections.Generic;

/// <summary>
/// Provides validation helpers for <see cref="RateLimitingMiddleware"/> and related classes.
/// </summary>
public static class RateLimitingMiddlewareValidation
{
    /// <summary>
    /// Validates a <see cref="RateLimitingMiddleware"/> instance.
    /// </summary>
    /// <param name="value">The middleware instance to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static IReadOnlyList<string> Validate(this RateLimitingMiddleware value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // RateLimitingMiddleware has no public properties to validate
        // The validation is primarily for the RateLimitStatus objects returned by its methods

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="RateLimitingMiddleware"/> instance is valid.
    /// </summary>
    /// <param name="value">The middleware instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static bool IsValid(this RateLimitingMiddleware value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="RateLimitingMiddleware"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The middleware instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid.</exception>
    public static void EnsureValid(this RateLimitingMiddleware value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"RateLimitingMiddleware is not valid. Errors:{Environment.NewLine}- {
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