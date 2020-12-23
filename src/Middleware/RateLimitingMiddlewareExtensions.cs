#nullable enable
// =============================================================================
// Author: [Your Name]
// =============================================================================

namespace DotNetRealtimePipeline.Middleware;

using System;
using System.Collections.Generic;

/// <summary>
/// Extension methods for RateLimitingMiddleware.
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
    /// <param name="threshold">The threshold (in tokens) below which to consider an identifier as being rate limited.</param>
    /// <returns>A dictionary of rate limit statuses for identifiers that are within the threshold.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="middleware"/> is null.</exception>
    public static IReadOnlyDictionary<string, RateLimitStatus> GetStatusesNearLimit(this RateLimitingMiddleware middleware, int threshold)
    {
        ArgumentNullException.ThrowIfNull(middleware);

        var allStatuses = middleware.GetAllStatuses();
        var nearLimitStatuses = new Dictionary<string, RateLimitStatus>();

        foreach (var kvp in allStatuses)
        {
            if (kvp.Value.AvailableTokens <= threshold)
            {
                nearLimitStatuses[kvp.Key] = kvp.Value;
            }
        }

        return nearLimitStatuses;
    }
}
