#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Integration;

using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Extension methods for <see cref="IRawPipelineAccess"/> that provide convenient
/// operations for working with the pipe reader and writer it exposes, plus a
/// generic entry point for callers that only hold an untyped reference to a
/// pipeline component.
/// </summary>
/// <remarks>
/// This is the single extension surface over <see cref="IRawPipelineAccess"/>;
/// there is deliberately no parallel set of helpers over any concrete
/// implementation (e.g. <see cref="RawPipelineAccessor"/>) so the two never
/// drift out of sync again. Every method here inherits the thread-safety
/// contract documented on <see cref="IRawPipelineAccess"/>: the reader and
/// writer are live, shared, single-consumer/single-producer objects, not
/// snapshots, so calling these helpers concurrently from multiple threads on
/// the same side of the pipe while the pipeline is running is not supported.
/// </remarks>
public static class RawPipelineAccessExtensions
{
    /// <summary>
    /// Copies all available data from the pipe reader to the specified buffer writer.
    /// </summary>
    /// <param name="access">The raw pipeline access instance.</param>
    /// <param name="writer">The buffer writer to copy data to.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the copy operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="access"/> or <paramref name="writer"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the pipe reader is completed or disposed.</exception>
    public static async ValueTask CopyToAsync(
        this IRawPipelineAccess access,
        IBufferWriter<byte> writer,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(access);
        ArgumentNullException.ThrowIfNull(writer);

        var reader = access.AsPipeReader();
        while (true)
        {
            var result = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
            var buffer = result.Buffer;

            if (result.IsCompleted && buffer.IsEmpty)
            {
                reader.AdvanceTo(buffer.Start, buffer.End);
                break;
            }

            foreach (var memory in buffer)
            {
                writer.Write(memory.Span);
            }

            reader.AdvanceTo(buffer.End);

            if (result.IsCompleted)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Copies all available data from the pipe reader to the specified stream.
    /// </summary>
    /// <param name="access">The raw pipeline access instance.</param>
    /// <param name="stream">The stream to copy data to.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the copy operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="access"/> or <paramref name="stream"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the pipe reader is completed or disposed.</exception>
    public static async ValueTask CopyToStreamAsync(
        this IRawPipelineAccess access,
        System.IO.Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(access);
        ArgumentNullException.ThrowIfNull(stream);

        var reader = access.AsPipeReader();
        while (true)
        {
            var result = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
            var buffer = result.Buffer;

            if (result.IsCompleted && buffer.IsEmpty)
            {
                reader.AdvanceTo(buffer.Start, buffer.End);
                break;
            }

            foreach (var memory in buffer)
            {
                await stream.WriteAsync(memory, cancellationToken).ConfigureAwait(false);
            }

            reader.AdvanceTo(buffer.End);

            if (result.IsCompleted)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Reads all available data from the pipe reader and returns it as a single byte array.
    /// </summary>
    /// <param name="access">The raw pipeline access instance.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A byte array containing all available data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="access"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the pipe is disposed.</exception>
    public static async ValueTask<byte[]> ReadAllAsync(
        this IRawPipelineAccess access,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(access);

        using var ms = new System.IO.MemoryStream();
        await access.CopyToStreamAsync(ms, cancellationToken).ConfigureAwait(false);
        return ms.ToArray();
    }

    /// <summary>
    /// Writes data to the pipe writer and flushes it immediately.
    /// </summary>
    /// <param name="access">The raw pipeline access instance.</param>
    /// <param name="data">The data to write.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the write operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="access"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the pipe writer is completed or disposed.</exception>
    public static async ValueTask WriteAsync(
        this IRawPipelineAccess access,
        ReadOnlyMemory<byte> data,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(access);

        var writer = access.AsPipeWriter();
        writer.Write(data.Span);
        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the raw <see cref="PipeReader"/> for the specified <paramref name="source"/>, for
    /// callers that only hold an untyped reference and want to probe for zero-copy support.
    /// </summary>
    /// <param name="source">The object to probe for <see cref="IRawPipelineAccess"/> support.</param>
    /// <returns>A <see cref="PipeReader"/> instance providing zero-copy access to the pipeline.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException">Thrown when <paramref name="source"/> does not implement <see cref="IRawPipelineAccess"/>.</exception>
    [SuppressMessage("Style", "IDE0270:Use coalesce expression", Justification = "Explicit null check for clarity")]
    public static PipeReader GetPipeReader(this object source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source is IRawPipelineAccess raw
            ? raw.AsPipeReader()
            : throw new NotSupportedException(
                $"{source.GetType().Name} does not implement {nameof(IRawPipelineAccess)}. " +
                "Wrap it with RawPipelineAccessor or implement the interface to enable zero-copy access.");
    }

    /// <summary>
    /// Gets the raw <see cref="PipeWriter"/> for the specified <paramref name="source"/>, for
    /// callers that only hold an untyped reference and want to probe for zero-copy support.
    /// </summary>
    /// <param name="source">The object to probe for <see cref="IRawPipelineAccess"/> support.</param>
    /// <returns>A <see cref="PipeWriter"/> instance providing zero-copy access to the pipeline.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException">Thrown when <paramref name="source"/> does not implement <see cref="IRawPipelineAccess"/>.</exception>
    [SuppressMessage("Style", "IDE0270:Use coalesce expression", Justification = "Explicit null check for clarity")]
    public static PipeWriter GetPipeWriter(this object source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source is IRawPipelineAccess raw
            ? raw.AsPipeWriter()
            : throw new NotSupportedException(
                $"{source.GetType().Name} does not implement {nameof(IRawPipelineAccess)}. " +
                "Wrap it with RawPipelineAccessor or implement the interface to enable zero-copy access.");
    }
}
