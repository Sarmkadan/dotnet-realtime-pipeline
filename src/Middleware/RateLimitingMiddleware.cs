#nullable enable
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
public sealed class RateLimitingMiddleware
{
    private readonly ConcurrentDictionary<string, RateLimitBucket> _buckets = new();
    private readonly int _tokensPerSecond;
    private readonly int _maxBurstSize;

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitingMiddleware"/> class.
    /// </summary>
    /// <param name="tokensPerSecond">The number of tokens replenished per second.</param>
    /// <param name="maxBurstSize">The maximum number of tokens the limiter can hold.</param>
    public RateLimitingMiddleware(int tokensPerSecond = 1000, int maxBurstSize = 5000)
    {
        _tokensPerSecond = tokensPerSecond;
        _maxBurstSize = maxBurstSize;
    }

    /// <summary>
    /// Returns a string that represents the current rate limiter configuration.
    /// </summary>
    /// <returns>A string representation of the rate limiter.</returns>
    public override string ToString() => $"RateLimitingMiddleware {{ AvailableTokens = {_tokensPerSecond}, Capacity = {_maxBurstSize}, ResetTime = {DateTime.UtcNow} }}";

    /// <summary>
    /// Checks if an operation is allowed under rate limits.
    /// </summary>
    /// <param name="identifier">The identifier whose rate limit bucket is checked.</param>
    /// <param name="tokensRequired">The number of tokens required by the operation.</param>
    /// <returns><see langword="true"/> if the requested tokens are available; otherwise, <see langword="false"/>.</returns>
    public bool TryAcquire(string identifier, int tokensRequired = 1)
    {
        ArgumentException.ThrowIfNullOrEmpty(identifier);
        var bucket = _buckets.GetOrAdd(identifier, _ => new RateLimitBucket(_tokensPerSecond, _maxBurstSize));
        return bucket.TryConsume(tokensRequired);
    }

    /// <summary>
    /// Gets the current rate limit status for an identifier.
    /// </summary>
    /// <param name="identifier">The identifier whose rate limit status is retrieved.</param>
    /// <returns>The current rate limit status for the identifier.</returns>
    public RateLimitStatus GetStatus(string identifier)
    {
        ArgumentException.ThrowIfNullOrEmpty(identifier);
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
    /// <param name="identifier">The identifier whose rate limit bucket is removed.</param>
    public void Reset(string identifier)
    {
        ArgumentException.ThrowIfNullOrEmpty(identifier);
        _buckets.TryRemove(identifier, out _);
    }

    /// <summary>
    /// Gets all rate limit statuses.
    /// </summary>
    /// <returns>A dictionary that maps each identifier to its current rate limit status.</returns>
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
public sealed class RateLimitStatus
{
    /// <summary>
    /// Gets or sets the number of tokens currently available.
    /// </summary>
    public int AvailableTokens { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of tokens that can be held.
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// Gets or sets the time at which the next refill is expected.
    /// </summary>
    public DateTime ResetTime { get; set; }

    /// <summary>
    /// Gets a value indicating whether the available tokens are below half of capacity.
    /// </summary>
    public bool IsLimited => AvailableTokens < Capacity / 2;
}

/// <summary>
/// Token bucket implementation for rate limiting.
/// </summary>
internal sealed class RateLimitBucket
{
    private long _availableTokens;
    private readonly int _capacity;
    private readonly int _refillTokensPerSecond;
    private DateTime _lastRefillTime = DateTime.UtcNow;
    private readonly object _lockObject = new();
    private DateTime _nextRefillTime;

    public int AvailableTokens
    {
        get
        {
            lock (_lockObject)
            {
                RefillTokens();
                return (int)Math.Min(_availableTokens, _capacity);
            }
        }
    }

    public int Capacity => _capacity;
    public DateTime NextRefillTime
    {
        get
        {
            lock (_lockObject)
            {
                RefillTokens();
                return _nextRefillTime;
            }
        }
    }

    public RateLimitBucket(int refillTokensPerSecond, int capacity)
    {
        _refillTokensPerSecond = refillTokensPerSecond;
        _capacity = capacity;
        _availableTokens = capacity;
        _nextRefillTime = DateTime.UtcNow.AddSeconds(1);
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
            _nextRefillTime = now.AddSeconds(1);
        }
    }
}

/// <summary>
/// Middleware for per-stage rate limiting within the pipeline.
/// </summary>
public sealed class StageRateLimitingMiddleware
{
    private readonly Dictionary<string, RateLimitingMiddleware> _stageLimits = new();

    /// <summary>
    /// Registers rate limits for a pipeline stage.
    /// </summary>
    /// <param name="stageName">The name of the pipeline stage.</param>
    /// <param name="itemsPerSecond">The number of items replenished per second.</param>
    /// <param name="burstSize">The maximum number of items allowed in a burst.</param>
    public void RegisterStageLimit(string stageName, int itemsPerSecond, int burstSize)
    {
        ArgumentException.ThrowIfNullOrEmpty(stageName);
        _stageLimits[stageName] = new RateLimitingMiddleware(itemsPerSecond, burstSize);
    }

    /// <summary>
    /// Checks if a stage allows new items.
    /// </summary>
    /// <param name="stageName">The name of the pipeline stage.</param>
    /// <param name="itemCount">The number of items to process.</param>
    /// <returns><see langword="true"/> if the stage can process the items; otherwise, <see langword="false"/>.</returns>
    public bool CanProcessInStage(string stageName, int itemCount = 1)
    {
        ArgumentException.ThrowIfNullOrEmpty(stageName);
        if (!_stageLimits.TryGetValue(stageName, out var limiter))
        {
            return true; // No limit registered, allow
        }

        return limiter.TryAcquire(stageName, itemCount);
    }

    /// <summary>
    /// Gets rate limit status for all stages.
    /// </summary>
    /// <returns>A dictionary that maps each stage name to its current rate limit status.</returns>
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
