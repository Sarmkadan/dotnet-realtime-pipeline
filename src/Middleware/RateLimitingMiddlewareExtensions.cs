#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Middleware;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Extension methods for <see cref="RateLimitingMiddleware"/> to provide additional rate limiting functionality.
/// </summary>
public static class RateLimitingMiddlewareExtensions
{
    /// <summary>
    /// Checks if the rate limit has been exceeded for a given identifier.
    /// </summary>
    /// <param name="middleware">The rate limiting middleware instance.</param>
    /// <param name="identifier">The identifier to check.</param>
    /// <returns>True if the rate limit has been exceeded, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="middleware"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="identifier"/> is null or empty.</exception>
    public static bool IsRateLimitExceeded(this RateLimitingMiddleware middleware, string identifier)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        ArgumentException.ThrowIfNullOrEmpty(identifier);

        var status = middleware.GetStatus(identifier);
        return status.AvailableTokens <= 0;
    }

    /// <summary>
    /// Gets the rate limit status for all identifiers that are within a specified threshold of being rate limited.
    /// </summary>
    /// <param name="middleware">The rate limiting middleware instance.</param>
    /// <param name="threshold">The threshold (in tokens) below which to consider an identifier as being rate limited.
    /// Must be a non-negative value.</param>
    /// <returns>A dictionary of rate limit statuses for identifiers that are within the threshold.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="middleware"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="threshold"/> is negative.</exception>
    public static IReadOnlyDictionary<string, RateLimitStatus> GetStatusesNearLimit(this RateLimitingMiddleware middleware, int threshold)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        ArgumentOutOfRangeException.ThrowIfNegative(threshold);

        return middleware.GetAllStatuses()
            .Where(kvp => kvp.Value.AvailableTokens <= threshold)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}
