#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Integration;

using System;
using System.IO.Pipelines;

/// <summary>
/// Default implementation of <see cref="IRawPipelineAccess"/> that wraps a
/// <see cref="System.IO.Pipelines.Pipe"/> and exposes its <see cref="PipeReader"/>
/// and <see cref="PipeWriter"/> ends.
/// </summary>
/// <remarks>
/// Inject or cast to <see cref="IRawPipelineAccess"/> to obtain zero-copy access.
/// The internal <see cref="Pipe"/> is created with the supplied
/// <see cref="PipeOptions"/> so callers can configure pause/resume thresholds,
/// pool size, and scheduler — all of which directly influence back-pressure
/// behaviour when integrating with network streams.
/// <code>
/// // Zero-copy: stream a Socket directly into the pipeline
/// var access = new RawPipelineAccessor();
/// await socket.ReceiveAsync(access.AsPipeWriter(), cancellationToken);
/// </code>
/// </remarks>
public sealed class RawPipelineAccessor : IRawPipelineAccess, IDisposable
{
    private readonly Pipe _pipe;

    /// <summary>
    /// Creates a <see cref="RawPipelineAccessor"/> with default <see cref="PipeOptions"/>.
    /// </summary>
    public RawPipelineAccessor() : this(PipeOptions.Default) { }

    /// <summary>
    /// Creates a <see cref="RawPipelineAccessor"/> with the supplied <paramref name="options"/>.
    /// Use custom options to tune pause/resume thresholds for back-pressure control.
    /// </summary>
    public RawPipelineAccessor(PipeOptions options)
    {
        if (options is null) throw new ArgumentNullException(nameof(options));
        _pipe = new Pipe(options);
    }

    /// <inheritdoc/>
    public PipeReader AsPipeReader() => _pipe.Reader;

    /// <inheritdoc/>
    public PipeWriter AsPipeWriter() => _pipe.Writer;

    /// <summary>
    /// Resets the underlying pipe, clearing any buffered data.
    /// </summary>
    public void Reset() => _pipe.Reset();

    /// <summary>
    /// Disposes the underlying <see cref="Pipe"/> resources.
    /// </summary>
    public void Dispose()
    {
        _pipe.Reader.Complete();
        _pipe.Writer.Complete();
    }
}
