#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Services;

using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Manages pipeline processing checkpoints so that a restarted pipeline
/// can resume from the last committed position rather than reprocessing
/// from the beginning.
/// </summary>
public sealed class CheckpointManager : IDisposable
{
    private readonly ConcurrentDictionary<string, CheckpointEntry> _checkpoints = new();
    private readonly string? _persistencePath;
    private readonly SemaphoreSlim _fileLock = new(1, 1);
    private volatile bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckpointManager"/> class.
    /// When <paramref name="persistencePath"/> is provided, checkpoints are
    /// durably persisted to disk on every commit.
    /// </summary>
    /// <param name="persistencePath">
    /// Optional file path for durable checkpoint storage.
    /// Pass <see langword="null"/> for in-memory-only checkpoints.
    /// </param>
    public CheckpointManager(string? persistencePath = null)
    {
        _persistencePath = persistencePath;
    }

    /// <summary>
    /// Commits a checkpoint for the given stage, recording the offset and timestamp.
    /// If a persistence path was configured, the full checkpoint state is flushed to disk.
    /// </summary>
    /// <param name="stageName">Name of the pipeline stage.</param>
    /// <param name="offset">The offset (e.g. sequence number or timestamp) to checkpoint.</param>
    /// <param name="cancellationToken">Token to cancel the persistence I/O.</param>
    /// <returns>A task representing the asynchronous commit operation.</returns>
    /// <exception cref="ArgumentException"><paramref name="stageName"/> is null or empty.</exception>
    public async Task CommitAsync(string stageName, long offset, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(stageName);
        ObjectDisposedException.ThrowIf(_disposed, this);

        var entry = new CheckpointEntry
        {
            StageName = stageName,
            Offset = offset,
            CommittedAtUtc = DateTimeOffset.UtcNow
        };

        _checkpoints[stageName] = entry;

        if (_persistencePath is not null)
            await FlushToDiskAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves the last committed offset for a stage.
    /// </summary>
    /// <param name="stageName">Name of the pipeline stage.</param>
    /// <returns>The last committed offset, or -1 if no checkpoint exists for the stage.</returns>
    /// <exception cref="ArgumentException"><paramref name="stageName"/> is null or empty.</exception>
    public long GetLastOffset(string stageName)
    {
        ArgumentException.ThrowIfNullOrEmpty(stageName);
        return _checkpoints.TryGetValue(stageName, out var entry) ? entry.Offset : -1L;
    }

    /// <summary>
    /// Returns the full checkpoint entry for a stage, or <see langword="null"/>
    /// if no checkpoint has been committed.
    /// </summary>
    /// <param name="stageName">Name of the pipeline stage.</param>
    /// <returns>The checkpoint entry if found; otherwise <see langword="null"/>.</returns>
    public CheckpointEntry? GetCheckpoint(string stageName)
    {
        ArgumentException.ThrowIfNullOrEmpty(stageName);
        return _checkpoints.GetValueOrDefault(stageName);
    }

    /// <summary>
    /// Removes the checkpoint for a given stage, effectively forcing a
    /// full reprocess on the next pipeline start.
    /// </summary>
    /// <param name="stageName">Name of the pipeline stage.</param>
    /// <returns><see langword="true"/> if a checkpoint was removed; <see langword="false"/> if none existed.</returns>
    public bool Reset(string stageName)
    {
        ArgumentException.ThrowIfNullOrEmpty(stageName);
        return _checkpoints.TryRemove(stageName, out _);
    }

    /// <summary>
    /// Loads previously persisted checkpoints from disk.
    /// Existing in-memory entries for stages present in the file are overwritten.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the read.</param>
    /// <returns>A task representing the asynchronous load.</returns>
    /// <exception cref="InvalidOperationException">No persistence path was configured.</exception>
    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (_persistencePath is null)
            throw new InvalidOperationException("No persistence path configured.");

        if (!File.Exists(_persistencePath))
            return;

        await _fileLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await using var stream = File.OpenRead(_persistencePath);
            var entries = await JsonSerializer.DeserializeAsync<CheckpointEntry[]>(stream, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (entries is not null)
            {
                foreach (var entry in entries)
                    _checkpoints[entry.StageName] = entry;
            }
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private async Task FlushToDiskAsync(CancellationToken ct)
    {
        await _fileLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var entries = _checkpoints.Values;
            var tmpPath = _persistencePath + ".tmp";
            await using (var stream = File.Create(tmpPath))
            {
                await JsonSerializer.SerializeAsync(stream, entries, cancellationToken: ct).ConfigureAwait(false);
            }
            File.Move(tmpPath, _persistencePath!, overwrite: true);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _fileLock.Dispose();
    }
}

/// <summary>
/// Represents a single checkpoint entry for a pipeline stage.
/// </summary>
public sealed class CheckpointEntry
{
    /// <summary>Gets or sets the pipeline stage name.</summary>
    public string StageName { get; set; } = "";

    /// <summary>Gets or sets the committed offset (sequence number or timestamp).</summary>
    public long Offset { get; set; }

    /// <summary>Gets or sets the UTC time when this checkpoint was committed.</summary>
    public DateTimeOffset CommittedAtUtc { get; set; }
}
