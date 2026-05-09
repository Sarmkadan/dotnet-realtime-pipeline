#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Caching;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// In-memory cache service with TTL support and eviction policies.
/// Thread-safe implementation with configurable cache behavior and statistics tracking.
/// </summary>
public class CacheService<TKey, TValue> where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, CacheEntry> _cache = new();
    private readonly int _maxCapacity;
    private readonly TimeSpan _defaultTtl;
    private readonly EvictionPolicy _evictionPolicy;
    private long _hits;
    private long _misses;

    public CacheService(int maxCapacity = 1000, TimeSpan? defaultTtl = null, EvictionPolicy policy = EvictionPolicy.LRU)
    {
        _maxCapacity = maxCapacity;
        _defaultTtl = defaultTtl ?? TimeSpan.FromHours(1);
        _evictionPolicy = policy;
    }

    /// <summary>
    /// Gets a value from the cache.
    /// </summary>
    public bool TryGetValue(TKey key, out TValue value)
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.IsExpired)
            {
                _cache.TryRemove(key, out _);
                _misses++;
                value = default;
                return false;
            }

            entry.LastAccessTime = DateTime.UtcNow;
            entry.AccessCount++;
            _hits++;
            value = entry.Value;
            return true;
        }

        _misses++;
        value = default;
        return false;
    }

    /// <summary>
    /// Sets a value in the cache with default TTL.
    /// </summary>
    public void Set(TKey key, TValue value)
    {
        Set(key, value, _defaultTtl);
    }

    /// <summary>
    /// Sets a value in the cache with a specific TTL.
    /// </summary>
    public void Set(TKey key, TValue value, TimeSpan ttl)
    {
        if (_cache.Count >= _maxCapacity && !_cache.ContainsKey(key))
        {
            EvictEntry();
        }

        var entry = new CacheEntry
        {
            Value = value,
            CreatedTime = DateTime.UtcNow,
            LastAccessTime = DateTime.UtcNow,
            ExpirationTime = DateTime.UtcNow.Add(ttl),
            AccessCount = 0
        };

        _cache[key] = entry;
    }

    /// <summary>
    /// Removes an entry from the cache.
    /// </summary>
    public bool TryRemove(TKey key)
    {
        return _cache.TryRemove(key, out _);
    }

    /// <summary>
    /// Clears all entries from the cache.
    /// </summary>
    public void Clear()
    {
        _cache.Clear();
        _hits = 0;
        _misses = 0;
    }

    /// <summary>
    /// Gets cache statistics.
    /// </summary>
    public CacheStatistics GetStatistics()
    {
        var total = _hits + _misses;
        var hitRate = total > 0 ? (_hits * 100.0) / total : 0;

        return new CacheStatistics
        {
            TotalHits = _hits,
            TotalMisses = _misses,
            HitRate = hitRate,
            CurrentSize = _cache.Count,
            MaxCapacity = _maxCapacity,
            UtilizationPercent = (_cache.Count * 100.0) / _maxCapacity
        };
    }

    /// <summary>
    /// Removes all expired entries from the cache.
    /// </summary>
    public int RemoveExpiredEntries()
    {
        var expiredKeys = _cache
            .Where(kvp => kvp.Value.IsExpired)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _cache.TryRemove(key, out _);
        }

        return expiredKeys.Count;
    }

    /// <summary>
    /// Evicts an entry based on the configured policy.
    /// </summary>
    private void EvictEntry()
    {
        switch (_evictionPolicy)
        {
            case EvictionPolicy.LRU:
                EvictLRU();
                break;
            case EvictionPolicy.LFU:
                EvictLFU();
                break;
            case EvictionPolicy.FIFO:
                EvictFIFO();
                break;
            default:
                EvictLRU();
                break;
        }
    }

    private void EvictLRU()
    {
        var lruKey = _cache
            .OrderBy(kvp => kvp.Value.LastAccessTime)
            .FirstOrDefault().Key;

        if (lruKey is not null)
        {
            _cache.TryRemove(lruKey, out _);
        }
    }

    private void EvictLFU()
    {
        var lfuKey = _cache
            .OrderBy(kvp => kvp.Value.AccessCount)
            .FirstOrDefault().Key;

        if (lfuKey is not null)
        {
            _cache.TryRemove(lfuKey, out _);
        }
    }

    private void EvictFIFO()
    {
        var fifoKey = _cache
            .OrderBy(kvp => kvp.Value.CreatedTime)
            .FirstOrDefault().Key;

        if (fifoKey is not null)
        {
            _cache.TryRemove(fifoKey, out _);
        }
    }

    private class CacheEntry
    {
        public TValue Value { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime LastAccessTime { get; set; }
        public DateTime ExpirationTime { get; set; }
        public long AccessCount { get; set; }

        public bool IsExpired => DateTime.UtcNow > ExpirationTime;
    }
}

/// <summary>
/// Cache statistics for monitoring and analysis.
/// </summary>
public class CacheStatistics
{
    public long TotalHits { get; set; }
    public long TotalMisses { get; set; }
    public double HitRate { get; set; }
    public int CurrentSize { get; set; }
    public int MaxCapacity { get; set; }
    public double UtilizationPercent { get; set; }

    public override string ToString()
    {
        return $"Hits: {TotalHits}, Misses: {TotalMisses}, HitRate: {HitRate:F2}%, Size: {CurrentSize}/{MaxCapacity} ({UtilizationPercent:F1}%)";
    }
}

/// <summary>
/// Eviction policy for cache entries.
/// </summary>
public enum EvictionPolicy
{
    LRU,  // Least Recently Used
    LFU,  // Least Frequently Used
    FIFO  // First In, First Out
}

/// <summary>
/// Distributed cache abstraction for external caching systems.
/// </summary>
public interface IDistributedCache
{
    Task<T> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? ttl = null);
    Task RemoveAsync(string key);
    Task<bool> ExistsAsync(string key);
    Task ClearAsync();
}

/// <summary>
/// In-memory implementation of distributed cache.
/// </summary>
public class InMemoryDistributedCache : IDistributedCache
{
    private readonly CacheService<string, object> _cache;

    public InMemoryDistributedCache(int maxCapacity = 5000)
    {
        _cache = new CacheService<string, object>(maxCapacity);
    }

    public async Task<T> GetAsync<T>(string key)
    {
        await Task.CompletedTask;

        if (_cache.TryGetValue(key, out var value) && value is T typed)
        {
            return typed;
        }

        return default;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
    {
        await Task.CompletedTask;
        _cache.Set(key, value, ttl ?? TimeSpan.FromHours(1));
    }

    public async Task RemoveAsync(string key)
    {
        await Task.CompletedTask;
        _cache.TryRemove(key);
    }

    public async Task<bool> ExistsAsync(string key)
    {
        await Task.CompletedTask;
        return _cache.TryGetValue(key, out _);
    }

    public async Task ClearAsync()
    {
        await Task.CompletedTask;
        _cache.Clear();
    }
}
