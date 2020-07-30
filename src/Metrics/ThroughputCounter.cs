#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Metrics;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// Tracks events-per-second throughput using a sliding time window.
/// Thread-safe; uses a circular bucket approach for O(1) record and query.
/// </summary>
public sealed class ThroughputCounter : IPipelineMetrics
{
    // Each bucket represents one second of events.
    private readonly int _windowSeconds;
    private readonly long[] _globalBuckets;
    private readonly ConcurrentDictionary<string, long[]> _stageBuckets = new();
    private long _lastGlobalSecond;

    /// <param name="windowSeconds">Length of the sliding window in seconds (default 60).</param>
    public ThroughputCounter(int windowSeconds = 60)
    {
        if (windowSeconds <= 0) throw new ArgumentOutOfRangeException(nameof(windowSeconds));

        _windowSeconds = windowSeconds;
        _globalBuckets = new long[windowSeconds];
        _lastGlobalSecond = CurrentSecond();
    }

    /// <inheritdoc/>
    public void RecordEvents(long count)
    {
        if (count <= 0) return;

        long bucket = CurrentSecond() % _windowSeconds;
        ClearStaleGlobalBucket(bucket);
        Interlocked.Add(ref _globalBuckets[bucket], count);
    }

    /// <inheritdoc/>
    public void RecordEvents(string stageName, long count)
    {
        if (string.IsNullOrWhiteSpace(stageName)) throw new ArgumentException("Stage name cannot be empty", nameof(stageName));
        if (count <= 0) return;

        var buckets = _stageBuckets.GetOrAdd(stageName, _ => new long[_windowSeconds]);
        long bucket = CurrentSecond() % _windowSeconds;
        Interlocked.Add(ref buckets[bucket], count);
    }

    /// <inheritdoc/>
    public double GetThroughput()
    {
        long total = 0;
        long now = CurrentSecond();
        for (int i = 0; i < _windowSeconds; i++)
            total += Volatile.Read(ref _globalBuckets[i]);

        return total / (double)_windowSeconds;
    }

    /// <inheritdoc/>
    public double GetThroughput(string stageName)
    {
        if (string.IsNullOrWhiteSpace(stageName)) return 0d;
        if (!_stageBuckets.TryGetValue(stageName, out var buckets)) return 0d;

        long total = 0;
        for (int i = 0; i < _windowSeconds; i++)
            total += Volatile.Read(ref buckets[i]);

        return total / (double)_windowSeconds;
    }

    // Clears a global bucket when it belongs to a previous rotation of the window.
    private void ClearStaleGlobalBucket(long bucketIndex)
    {
        long now = CurrentSecond();
        long last = Volatile.Read(ref _lastGlobalSecond);

        if (now > last)
        {
            // Advance the window: zero out buckets that are now stale.
            long elapsed = Math.Min(now - last, _windowSeconds);
            for (long i = 1; i <= elapsed; i++)
            {
                long idx = (last + i) % _windowSeconds;
                Interlocked.Exchange(ref _globalBuckets[idx], 0);
            }
            Interlocked.Exchange(ref _lastGlobalSecond, now);
        }
    }

    private static long CurrentSecond() =>
        DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}
