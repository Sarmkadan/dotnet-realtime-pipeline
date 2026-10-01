#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Services;

using DotNetRealtimePipeline.Domain.Enums;
using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Applies backpressure strategies (throttle, drop, block) to incoming data
/// based on the current load reported by <see cref="BackpressureService"/>.
/// Unlike <see cref="BackpressureService"/> which tracks state,
/// this class executes the actual flow-control actions.
/// </summary>
public sealed class BackpressureHandler : IDisposable
{
    private readonly BackpressureService _backpressureService;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _stageGates = new();
    private readonly ConcurrentDictionary<string, long> _droppedCounts = new();
    private volatile bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackpressureHandler"/> class.
    /// </summary>
    /// <param name="backpressureService">The service that provides backpressure context per stage.</param>
    public BackpressureHandler(BackpressureService backpressureService)
    {
        _backpressureService = backpressureService ?? throw new ArgumentNullException(nameof(backpressureService));
    }

    /// <summary>
    /// Evaluates the backpressure state of a stage and applies the given strategy.
    /// Returns <see langword="true"/> when the caller may proceed; <see langword="false"/>
    /// when the item was dropped according to the strategy.
    /// </summary>
    /// <param name="stageName">Name of the pipeline stage to evaluate.</param>
    /// <param name="strategy">The backpressure strategy to apply.</param>
    /// <param name="cancellationToken">Token to cancel a blocking wait.</param>
    /// <returns><see langword="true"/> if processing may proceed; <see langword="false"/> if the item was dropped.</returns>
    /// <exception cref="ArgumentException"><paramref name="stageName"/> is null or empty.</exception>
    public async Task<bool> TryAcquireAsync(
        string stageName,
        BackpressureStrategy strategy,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(stageName);
        ObjectDisposedException.ThrowIf(_disposed, this);

        var context = _backpressureService.GetContext(stageName);
        if (context is null)
            return true; // no backpressure tracking - allow through

        if (!context.IsBackpressured)
            return true;

        return strategy switch
        {
            BackpressureStrategy.Block => await BlockUntilCapacityAsync(stageName, context, cancellationToken).ConfigureAwait(false),
            BackpressureStrategy.Throttle => await ThrottleAsync(context, cancellationToken).ConfigureAwait(false),
            BackpressureStrategy.DropNewest => DropAndRecord(stageName),
            BackpressureStrategy.DropOldest => true, // caller handles eviction
            BackpressureStrategy.Queue => true, // enqueue unconditionally
            _ => true
        };
    }

    /// <summary>
    /// Returns the total number of items dropped for a given stage.
    /// </summary>
    /// <param name="stageName">The pipeline stage name.</param>
    /// <returns>Number of dropped items, or 0 if no drops were recorded.</returns>
    public long GetDroppedCount(string stageName)
    {
        ArgumentException.ThrowIfNullOrEmpty(stageName);
        return _droppedCounts.GetValueOrDefault(stageName, 0L);
    }

    /// <summary>
    /// Resets the dropped-item counter for a stage.
    /// </summary>
    /// <param name="stageName">The pipeline stage name.</param>
    public void ResetDroppedCount(string stageName)
    {
        ArgumentException.ThrowIfNullOrEmpty(stageName);
        _droppedCounts.TryRemove(stageName, out _);
    }

    /// <summary>
    /// Registers a concurrency gate for a stage, limiting the number of
    /// concurrent consumers that may process items simultaneously.
    /// </summary>
    /// <param name="stageName">The pipeline stage name.</param>
    /// <param name="maxConcurrency">Maximum allowed concurrent processors.</param>
    /// <exception cref="ArgumentException"><paramref name="maxConcurrency"/> is less than 1.</exception>
    public void RegisterGate(string stageName, int maxConcurrency)
    {
        ArgumentException.ThrowIfNullOrEmpty(stageName);
        if (maxConcurrency < 1)
            throw new ArgumentException("Max concurrency must be >= 1", nameof(maxConcurrency));

        _stageGates[stageName] = new SemaphoreSlim(maxConcurrency, maxConcurrency);
    }

    private async Task<bool> BlockUntilCapacityAsync(
        string stageName,
        BackpressureContext context,
        CancellationToken ct)
    {
        var gate = _stageGates.GetOrAdd(stageName,
            _ => new SemaphoreSlim(context.MaxConcurrentConsumers, context.MaxConcurrentConsumers));

        await gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            // Re-check after acquiring gate
            return !context.IsBackpressured || context.BufferSize < context.MaxBufferCapacity;
        }
        finally
        {
            gate.Release();
        }
    }

    private static async Task<bool> ThrottleAsync(BackpressureContext context, CancellationToken ct)
    {
        double ratio = context.MaxBufferCapacity > 0
            ? (double)context.BufferSize / context.MaxBufferCapacity
            : 0d;

        int delayMs = (int)(ratio * 500);
        if (delayMs > 0)
            await Task.Delay(Math.Min(delayMs, 500), ct).ConfigureAwait(false);

        return true;
    }

    private bool DropAndRecord(string stageName)
    {
        _droppedCounts.AddOrUpdate(stageName, 1L, (_, count) => count + 1);
        return false;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var gate in _stageGates.Values)
            gate.Dispose();

        _stageGates.Clear();
    }
}
