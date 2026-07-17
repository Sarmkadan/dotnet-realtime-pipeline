#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

namespace DotNetRealtimePipeline.Caching;

using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Provides validation helpers for <see cref="CacheService{TKey, TValue}"/> instances.
/// </summary>
public static class CacheServiceValidation
{
    /// <summary>
    /// Validates a <see cref="CacheService{TKey, TValue}"/> instance.
    /// </summary>
    /// <typeparam name="TKey">The type of the cache key.</typeparam>
    /// <typeparam name="TValue">The type of the cached value.</typeparam>
    /// <param name="value">The cache service to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate<TKey, TValue>(this CacheService<TKey, TValue> value)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        var statistics = value.GetStatistics();
        ValidateStatistics(statistics, problems);

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="CacheService{TKey, TValue}"/> instance is valid.
    /// </summary>
    /// <typeparam name="TKey">The type of the cache key.</typeparam>
    /// <typeparam name="TValue">The type of the cached value.</typeparam>
    /// <param name="value">The cache service to check.</param>
    /// <returns>True if valid; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid<TKey, TValue>(this CacheService<TKey, TValue> value)
        where TKey : notnull
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="CacheService{TKey, TValue}"/> instance is valid.
    /// </summary>
    /// <typeparam name="TKey">The type of the cache key.</typeparam>
    /// <typeparam name="TValue">The type of the cached value.</typeparam>
    /// <param name="value">The cache service to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid.</exception>
    public static void EnsureValid<TKey, TValue>(this CacheService<TKey, TValue> value)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"CacheService is invalid. Problems:\n{string.Join("\n", problems)}");
        }
    }

    private static void ValidateStatistics(CacheStatistics statistics, List<string> problems)
    {
        ArgumentNullException.ThrowIfNull(problems);

        if (statistics is null)
        {
            problems.Add("GetStatistics() returned null.");
            return;
        }

        if (statistics.TotalHits < 0)
        {
            problems.Add($"Statistics.TotalHits cannot be negative, but was {statistics.TotalHits}.");
        }

        if (statistics.TotalMisses < 0)
        {
            problems.Add($"Statistics.TotalMisses cannot be negative, but was {statistics.TotalMisses}.");
        }

        if (statistics.CurrentSize < 0)
        {
            problems.Add($"Statistics.CurrentSize cannot be negative, but was {statistics.CurrentSize}.");
        }

        if (statistics.MaxCapacity <= 0)
        {
            problems.Add($"Statistics.MaxCapacity must be positive, but was {statistics.MaxCapacity}.");
        }

        if (statistics.CurrentSize > statistics.MaxCapacity)
        {
            problems.Add($"Statistics.CurrentSize ({statistics.CurrentSize}) cannot exceed MaxCapacity ({statistics.MaxCapacity}).");
        }

        if (statistics.UtilizationPercent is < 0 or > 100)
        {
            problems.Add($"Statistics.UtilizationPercent must be between 0 and 100, but was {statistics.UtilizationPercent:F2}%.");
        }

        if (statistics.HitRate is < 0 or > 100)
        {
            problems.Add($"Statistics.HitRate must be between 0 and 100, but was {statistics.HitRate:F2}%.");
        }
    }
}