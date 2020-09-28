#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Caching;

using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Extension methods for <see cref="CacheService{TKey, TValue}"/> providing additional functionality
/// for common cache operations, batch operations, and time-based operations.
/// </summary>
public static class CacheServiceExtensions
{
    /// <summary>
    /// Attempts to get a value from the cache. If the value is not found or has expired,
    /// calls the value factory to create the value, adds it to the cache, and returns it.
    /// </summary>
    /// <typeparam name="TKey">The type of the cache key.</typeparam>
    /// <typeparam name="TValue">The type of the cache value.</typeparam>
    /// <param name="cache">The cache service instance.</param>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="valueFactory">The function to create the value if it doesn't exist or has expired.</param>
    /// <returns>The value from the cache or created by the factory.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="cache"/> or <paramref name="key"/> or <paramref name="valueFactory"/> is null.</exception>
    public static TValue GetOrAdd<TKey, TValue>(
        this CacheService<TKey, TValue> cache,
        TKey key,
        Func<TKey, TValue> valueFactory) where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(valueFactory);

        if (cache.TryGetValue(key, out var value))
        {
            return value;
        }

        value = valueFactory(key);
        cache.Set(key, value);
        return value;
    }

    /// <summary>
    /// Attempts to get a value from the cache with a specific TTL. If the value is not found or has expired,
    /// calls the value factory to create the value, adds it to the cache with the specified TTL, and returns it.
    /// </summary>
    /// <typeparam name="TKey">The type of the cache key.</typeparam>
    /// <typeparam name="TValue">The type of the cache value.</typeparam>
    /// <param name="cache">The cache service instance.</param>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="ttl">The time-to-live for the cached value if created.</param>
    /// <param name="valueFactory">The function to create the value if it doesn't exist or has expired.</param>
    /// <returns>The value from the cache or created by the factory.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="cache"/> or <paramref name="key"/> or <paramref name="valueFactory"/> is null.</exception>
    public static TValue GetOrAdd<TKey, TValue>(
        this CacheService<TKey, TValue> cache,
        TKey key,
        TimeSpan ttl,
        Func<TKey, TValue> valueFactory) where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(valueFactory);

        if (cache.TryGetValue(key, out var value))
        {
            return value;
        }

        value = valueFactory(key);
        cache.Set(key, value, ttl);
        return value;
    }

    /// <summary>
    /// Attempts to remove multiple keys from the cache in a single operation.
    /// </summary>
    /// <typeparam name="TKey">The type of the cache key.</typeparam>
    /// <typeparam name="TValue">The type of the cache value.</typeparam>
    /// <param name="cache">The cache service instance.</param>
    /// <param name="keys">The collection of keys to remove.</param>
    /// <returns>The number of keys successfully removed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="cache"/> or <paramref name="keys"/> is null.</exception>
    public static int RemoveRange<TKey, TValue>(
        this CacheService<TKey, TValue> cache,
        IEnumerable<TKey> keys) where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(keys);

        var count = 0;
        foreach (var key in keys)
        {
            if (cache.TryRemove(key))
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Attempts to get multiple values from the cache in a single operation.
    /// </summary>
    /// <typeparam name="TKey">The type of the cache key.</typeparam>
    /// <typeparam name="TValue">The type of the cache value.</typeparam>
    /// <param name="cache">The cache service instance.</param>
    /// <param name="keys">The collection of keys to retrieve.</param>
    /// <returns>A dictionary mapping keys to their values (missing keys are not included).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="cache"/> or <paramref name="keys"/> is null.</exception>
    public static IReadOnlyDictionary<TKey, TValue> GetRange<TKey, TValue>(
        this CacheService<TKey, TValue> cache,
        IEnumerable<TKey> keys) where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(keys);

        var result = new Dictionary<TKey, TValue>();
        foreach (var key in keys)
        {
            if (cache.TryGetValue(key, out var value))
            {
                result[key] = value;
            }
        }

        return result.AsReadOnly();
    }

    /// <summary>
    /// Sets multiple key-value pairs in the cache with default TTL.
    /// </summary>
    /// <typeparam name="TKey">The type of the cache key.</typeparam>
    /// <typeparam name="TValue">The type of the cache value.</typeparam>
    /// <param name="cache">The cache service instance.</param>
    /// <param name="items">The collection of key-value pairs to set.</param>
    /// <exception cref="ArgumentNullException"><paramref name="cache"/> or <paramref name="items"/> is null.</exception>
    public static void SetRange<TKey, TValue>(
        this CacheService<TKey, TValue> cache,
        IEnumerable<KeyValuePair<TKey, TValue>> items) where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(items);

        foreach (var item in items)
        {
            cache.Set(item.Key, item.Value);
        }
    }

    /// <summary>
    /// Sets multiple key-value pairs in the cache with a specific TTL.
    /// </summary>
    /// <typeparam name="TKey">The type of the cache key.</typeparam>
    /// <typeparam name="TValue">The type of the cache value.</typeparam>
    /// <param name="cache">The cache service instance.</param>
    /// <param name="items">The collection of key-value pairs to set.</param>
    /// <param name="ttl">The time-to-live for all values.</param>
    /// <exception cref="ArgumentNullException"><paramref name="cache"/> or <paramref name="items"/> is null.</exception>
    public static void SetRange<TKey, TValue>(
        this CacheService<TKey, TValue> cache,
        IEnumerable<KeyValuePair<TKey, TValue>> items,
        TimeSpan ttl) where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(items);

        foreach (var item in items)
        {
            cache.Set(item.Key, item.Value, ttl);
        }
    }

    /// <summary>
    /// Gets cache statistics formatted as a performance counter string.
    /// </summary>
    /// <typeparam name="TKey">The type of the cache key.</typeparam>
    /// <typeparam name="TValue">The type of the cache value.</typeparam>
    /// <param name="cache">The cache service instance.</param>
    /// <returns>A formatted string suitable for performance monitoring systems.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="cache"/> is null.</exception>
    public static string ToPerformanceCounterString<TKey, TValue>(
        this CacheService<TKey, TValue> cache) where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(cache);

        var stats = cache.GetStatistics();
        return string.Create(
            CultureInfo.InvariantCulture,
            $"Cache.Hits={stats.TotalHits}|Misses={stats.TotalMisses}|HitRate={stats.HitRate:F2}%|Size={stats.CurrentSize}|Capacity={stats.MaxCapacity}|Utilization={stats.UtilizationPercent:F1}%");
    }

    /// <summary>
    /// Attempts to get a value from the cache. If the value is not found or has expired,
    /// returns the default value for the type instead of throwing.
    /// </summary>
    /// <typeparam name="TKey">The type of the cache key.</typeparam>
    /// <typeparam name="TValue">The type of the cache value.</typeparam>
    /// <param name="cache">The cache service instance.</param>
    /// <param name="key">The key of the value to get.</param>
    /// <returns>The value from the cache or default(TValue) if not found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="cache"/> or <paramref name="key"/> is null.</exception>
    public static TValue? GetValueOrDefault<TKey, TValue>(
        this CacheService<TKey, TValue> cache,
        TKey key) where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(key);

        cache.TryGetValue(key, out var value);
        return value;
    }

    /// <summary>
    /// Gets the current cache utilization percentage as a value between 0 and 1.
    /// </summary>
    /// <typeparam name="TKey">The type of the cache key.</typeparam>
    /// <typeparam name="TValue">The type of the cache value.</typeparam>
    /// <param name="cache">The cache service instance.</param>
    /// <returns>A value between 0.0 and 1.0 representing cache utilization.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="cache"/> is null.</exception>
    public static double GetUtilizationRatio<TKey, TValue>(
        this CacheService<TKey, TValue> cache) where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(cache);

        var stats = cache.GetStatistics();
        return stats.MaxCapacity > 0 ? stats.UtilizationPercent / 100.0 : 0.0;
    }
}