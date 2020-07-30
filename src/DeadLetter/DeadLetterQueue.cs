#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.DeadLetter;

using DotNetRealtimePipeline.Constants;
using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Thread-safe in-memory implementation of <see cref="IDeadLetterQueue"/>.
/// Stores failed data points, tracks retry attempts, and enforces a configurable
/// capacity limit so the queue does not grow without bound.
/// </summary>
public sealed class DeadLetterQueue : IDeadLetterQueue
{
    private readonly int _maxCapacity;
    private readonly int _defaultMaxRetries;
    private readonly Dictionary<Guid, DeadLetterEntry> _entries = new();
    private readonly object _lock = new();
    private long _totalResolved;

    /// <param name="maxCapacity">
    /// Maximum number of entries the queue will hold at one time.
    /// Once full, the oldest resolved/permanent-failure entries are evicted first;
    /// if none are eligible, new entries are rejected and the caller is notified.
    /// Defaults to <see cref="PipelineConstants.DefaultMaxBufferSize"/> / 10.
    /// </param>
    /// <param name="defaultMaxRetries">
    /// Default retry budget applied to each entry unless overridden.
    /// </param>
    public DeadLetterQueue(int maxCapacity = 1000, int defaultMaxRetries = 3)
    {
        if (maxCapacity <= 0) throw new ArgumentOutOfRangeException(nameof(maxCapacity));
        if (defaultMaxRetries < 0) throw new ArgumentOutOfRangeException(nameof(defaultMaxRetries));

        _maxCapacity = maxCapacity;
        _defaultMaxRetries = defaultMaxRetries;
    }

    /// <inheritdoc/>
    public int Count
    {
        get
        {
            lock (_lock) return _entries.Count;
        }
    }

    /// <inheritdoc/>
    public Task EnqueueAsync(DataPoint dataPoint, string stageName, string failureReason, Exception? exception = null)
    {
        if (dataPoint is null) throw new ArgumentNullException(nameof(dataPoint));
        if (string.IsNullOrWhiteSpace(stageName))
            throw new ArgumentException("Stage name cannot be null or empty.", nameof(stageName));
        if (string.IsNullOrWhiteSpace(failureReason))
            throw new ArgumentException("Failure reason cannot be null or empty.", nameof(failureReason));

        lock (_lock)
        {
            EvictIfNeeded();

            var entry = new DeadLetterEntry
            {
                DataPoint = dataPoint,
                FailureStageName = stageName,
                FailureReason = failureReason,
                MaxRetries = _defaultMaxRetries,
                ExceptionType = exception?.GetType().Name,
                ExceptionMessage = exception?.Message,
                EnqueuedAt = DateTime.UtcNow,
                Status = DeadLetterStatus.Pending
            };

            _entries[entry.EntryId] = entry;
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<DeadLetterEntry>> PeekAsync(int maxCount = 100)
    {
        if (maxCount <= 0) throw new ArgumentOutOfRangeException(nameof(maxCount));

        lock (_lock)
        {
            var result = _entries.Values
                .OrderBy(e => e.EnqueuedAt)
                .Take(maxCount)
                .ToList();

            return Task.FromResult<IReadOnlyList<DeadLetterEntry>>(result.AsReadOnly());
        }
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<DeadLetterEntry>> DequeueForRetryAsync(int maxCount = 10)
    {
        if (maxCount <= 0) throw new ArgumentOutOfRangeException(nameof(maxCount));

        lock (_lock)
        {
            var candidates = _entries.Values
                .Where(e => e.CanRetry)
                .OrderBy(e => e.EnqueuedAt)
                .Take(maxCount)
                .ToList();

            foreach (var entry in candidates)
                entry.BeginRetry();

            return Task.FromResult<IReadOnlyList<DeadLetterEntry>>(candidates.AsReadOnly());
        }
    }

    /// <inheritdoc/>
    public Task AcknowledgeFailureAsync(Guid entryId, string finalReason)
    {
        if (string.IsNullOrWhiteSpace(finalReason))
            throw new ArgumentException("Final reason cannot be null or empty.", nameof(finalReason));

        lock (_lock)
        {
            if (!_entries.TryGetValue(entryId, out var entry))
                return Task.CompletedTask;

            entry.Status = DeadLetterStatus.PermanentFailure;
            entry.ResolutionNote = finalReason;
            entry.ResolvedAt = DateTime.UtcNow;
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task AcknowledgeSuccessAsync(Guid entryId)
    {
        lock (_lock)
        {
            if (!_entries.TryGetValue(entryId, out var entry))
                return Task.CompletedTask;

            entry.Status = DeadLetterStatus.Resolved;
            entry.ResolvedAt = DateTime.UtcNow;
            _entries.Remove(entryId);
            _totalResolved++;
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<DeadLetterQueueStats> GetStatsAsync()
    {
        lock (_lock)
        {
            var stats = new DeadLetterQueueStats
            {
                TotalEntries = _entries.Count,
                PendingEntries = _entries.Values.Count(e => e.Status == DeadLetterStatus.Pending),
                InRetryEntries = _entries.Values.Count(e => e.Status == DeadLetterStatus.InRetry),
                PermanentFailureEntries = _entries.Values.Count(e => e.Status == DeadLetterStatus.PermanentFailure),
                TotalResolved = _totalResolved,
                GeneratedAt = DateTime.UtcNow
            };

            return Task.FromResult(stats);
        }
    }

    // -------------------------------------------------------------------------

    /// <summary>
    /// Evicts resolved and permanently-failed entries when the queue is at capacity.
    /// If still at capacity after eviction, the oldest pending entry is removed to make room.
    /// </summary>
    private void EvictIfNeeded()
    {
        if (_entries.Count < _maxCapacity)
            return;

        // First pass: remove resolved / permanent-failure entries
        var evictable = _entries.Values
            .Where(e => e.Status is DeadLetterStatus.Resolved or DeadLetterStatus.PermanentFailure)
            .Select(e => e.EntryId)
            .ToList();

        foreach (var id in evictable)
            _entries.Remove(id);

        if (_entries.Count < _maxCapacity)
            return;

        // Second pass: evict oldest pending entry
        var oldest = _entries.Values
            .OrderBy(e => e.EnqueuedAt)
            .FirstOrDefault();

        if (oldest is not null)
            _entries.Remove(oldest.EntryId);
    }
}
