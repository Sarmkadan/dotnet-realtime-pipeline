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

        // RateLimitingMiddleware validation is implicit - the class is always valid
        // as it uses default values for tokensPerSecond and maxBurstSize when not specified

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a <see cref="RateLimitStatus"/> instance.
    /// </summary>
    /// <param name="status">The rate limit status to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if status is null.</exception>
    public static IReadOnlyList<string> Validate(this RateLimitStatus status)
    {
        ArgumentNullException.ThrowIfNull(status);

        var problems = new List<string>();

        if (status.AvailableTokens < 0)
        {
            problems.Add("AvailableTokens must be non-negative.");
        }

        if (status.Capacity <= 0)
        {
            problems.Add("Capacity must be positive.");
        }

        if (status.ResetTime < DateTime.UtcNow)
        {
            problems.Add("ResetTime must be in the future.");
        }

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

    /// <summary>
    /// Validates a <see cref="RateLimitStatus"/> instance.
    /// </summary>
    /// <param name="status">The rate limit status to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if status is null.</exception>
    public static void EnsureValid(this RateLimitStatus status)
    {
        ArgumentNullException.ThrowIfNull(status);

        var errors = status.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"RateLimitStatus is not valid. Errors:{Environment.NewLine}- {
                string.Join(
                    $"\n- ",
                    errors
                )
            }",
                nameof(status)
            );
        }
    }

    /// <summary>
    /// Validates the TryAcquire parameters.
    /// </summary>
    /// <param name="identifier">The identifier to validate.</param>
    /// <param name="tokensRequired">The number of tokens required.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if identifier is null.</exception>
    /// <exception cref="ArgumentException">Thrown if identifier is empty or tokensRequired is negative.</exception>
    public static IReadOnlyList<string> ValidateParameters(string identifier, int tokensRequired = 1)
    {
        ArgumentException.ThrowIfNullOrEmpty(identifier, nameof(identifier));

        var problems = new List<string>();

        if (tokensRequired <= 0)
        {
            problems.Add("tokensRequired must be positive.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the GetStatus parameters.
    /// </summary>
    /// <param name="identifier">The identifier to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if identifier is null.</exception>
    public static IReadOnlyList<string> ValidateParameters(string identifier)
    {
        ArgumentException.ThrowIfNullOrEmpty(identifier, nameof(identifier));

        return Array.Empty<string>();
    }
}