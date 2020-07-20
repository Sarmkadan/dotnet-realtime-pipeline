// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Middleware;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

/// <summary>
/// Middleware for rate limiting and throttling operations.
/// Supports token bucket algorithm for flexible rate control.
/// </summary>
public class RateLimitingMiddleware
{
    private readonly ConcurrentDictionary<string, RateLimitBucket> _buckets = new();
    private readonly int _tokensPerSecond;
    private readonly int _maxBurstSize;

    public RateLimitingMiddleware(int tokensPerSecond = 1000, int maxBurstSize = 5000)
    {
        _tokensPerSecond = tokensPerSecond;
        _maxBurstSize = maxBurstSize;
    }

    /// <summary>
    /// Checks if an operation is allowed under rate limits.
    /// </summary>
    public bool TryAcquire(string identifier, int tokensRequired = 1)
    {
        var bucket = _buckets.GetOrAdd(identifier, _ => new RateLimitBucket(_tokensPerSecond, _maxBurstSize));
        return bucket.TryConsume(tokensRequired);
    }

    /// <summary>
    /// Gets the current rate limit status for an identifier.
    /// </summary>
    public RateLimitStatus GetStatus(string identifier)
    {
        if (_buckets.TryGetValue(identifier, out var bucket))
        {
            return new RateLimitStatus
            {
                AvailableTokens = bucket.AvailableTokens,
                Capacity = bucket.Capacity,
                ResetTime = bucket.NextRefillTime
            };
        }

        return new RateLimitStatus { AvailableTokens = _tokensPerSecond, Capacity = _maxBurstSize };
    }

    /// <summary>
    /// Resets rate limits for an identifier.
    /// </summary>
    public void Reset(string identifier)
    {
        _buckets.TryRemove(identifier, out _);
    }

    /// <summary>
    /// Gets all rate limit statuses.
    /// </summary>
    public Dictionary<string, RateLimitStatus> GetAllStatuses()
    {
        var result = new Dictionary<string, RateLimitStatus>();

        foreach (var kvp in _buckets)
        {
            result[kvp.Key] = new RateLimitStatus
            {
                AvailableTokens = kvp.Value.AvailableTokens,
                Capacity = kvp.Value.Capacity,
                ResetTime = kvp.Value.NextRefillTime
            };
        }

        return result;
    }
}

/// <summary>
/// Represents the rate limit status for tracking and debugging.
/// </summary>
public class RateLimitStatus
{
    public int AvailableTokens { get; set; }
    public int Capacity { get; set; }
    public DateTime ResetTime { get; set; }
    public bool IsLimited => AvailableTokens < Capacity / 2;
}

/// <summary>
/// Token bucket implementation for rate limiting.
/// </summary>
internal class RateLimitBucket
{
    private long _availableTokens;
    private readonly int _capacity;
    private readonly int _refillTokensPerSecond;
    private DateTime _lastRefillTime = DateTime.UtcNow;
    private readonly object _lockObject = new();

    public int AvailableTokens
    {
        get
        {
            RefillTokens();
            return (int)Math.Min(_availableTokens, _capacity);
        }
    }

    public int Capacity => _capacity;
    public DateTime NextRefillTime { get; private set; }

    public RateLimitBucket(int refillTokensPerSecond, int capacity)
    {
        _refillTokensPerSecond = refillTokensPerSecond;
        _capacity = capacity;
        _availableTokens = capacity;
    }

    /// <summary>
    /// Attempts to consume tokens from the bucket.
    /// </summary>
    public bool TryConsume(int tokensRequired)
    {
        lock (_lockObject)
        {
            RefillTokens();

            if (_availableTokens >= tokensRequired)
            {
                _availableTokens -= tokensRequired;
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Refills the bucket based on elapsed time.
    /// </summary>
    private void RefillTokens()
    {
        var now = DateTime.UtcNow;
        var elapsedSeconds = (now - _lastRefillTime).TotalSeconds;

        if (elapsedSeconds > 0)
        {
            var tokensToAdd = (long)(_refillTokensPerSecond * elapsedSeconds);
            _availableTokens = Math.Min(_availableTokens + tokensToAdd, _capacity);
            _lastRefillTime = now;
            NextRefillTime = now.AddSeconds(1);
        }
    }
}

/// <summary>
/// Middleware for per-stage rate limiting within the pipeline.
/// </summary>
public class StageRateLimitingMiddleware
{
    private readonly Dictionary<string, RateLimitingMiddleware> _stageLimits = new();

    /// <summary>
    /// Registers rate limits for a pipeline stage.
    /// </summary>
    public void RegisterStageLimit(string stageName, int itemsPerSecond, int burstSize)
    {
        _stageLimits[stageName] = new RateLimitingMiddleware(itemsPerSecond, burstSize);
    }

    /// <summary>
    /// Checks if a stage allows new items.
    /// </summary>
    public bool CanProcessInStage(string stageName, int itemCount = 1)
    {
        if (!_stageLimits.TryGetValue(stageName, out var limiter))
        {
            return true; // No limit registered, allow
        }

        return limiter.TryAcquire(stageName, itemCount);
    }

    /// <summary>
    /// Gets rate limit status for all stages.
    /// </summary>
    public Dictionary<string, RateLimitStatus> GetStageLimitStatuses()
    {
        var result = new Dictionary<string, RateLimitStatus>();

        foreach (var kvp in _stageLimits)
        {
            result[kvp.Key] = kvp.Value.GetStatus(kvp.Key);
        }

        return result;
    }
}
