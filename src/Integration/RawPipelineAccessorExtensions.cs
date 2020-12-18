#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Integration;

using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Extension methods for <see cref="RawPipelineAccessor"/> that provide convenient
/// operations for working with pipe readers and writers.
/// </summary>
public static class RawPipelineAccessorExtensions
{
    /// <summary>
    /// Copies all available data from the pipe reader to the specified buffer writer.
    /// </summary>
    /// <param name="accessor">The pipeline accessor.</param>
    /// <param name="writer">The buffer writer to copy data to.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the copy operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="accessor"/> or <paramref name="writer"/> is null.</exception>
    public static async ValueTask CopyToAsync(
        this RawPipelineAccessor accessor,
        IBufferWriter<byte> writer,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(accessor);
        ArgumentNullException.ThrowIfNull(writer);

        var reader = accessor.AsPipeReader();
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
    /// <param name="accessor">The pipeline accessor.</param>
    /// <param name="stream">The stream to copy data to.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the copy operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="accessor"/> or <paramref name="stream"/> is null.</exception>
    public static async ValueTask CopyToStreamAsync(
        this RawPipelineAccessor accessor,
        System.IO.Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(accessor);
        ArgumentNullException.ThrowIfNull(stream);

        var reader = accessor.AsPipeReader();
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
    /// <param name="accessor">The pipeline accessor.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A byte array containing all available data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="accessor"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the pipe is disposed.</exception>
    public static async ValueTask<byte[]> ReadAllAsync(
        this RawPipelineAccessor accessor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(accessor);

        using var ms = new System.IO.MemoryStream();
        await accessor.CopyToStreamAsync(ms, cancellationToken).ConfigureAwait(false);
        return ms.ToArray();
    }

    /// <summary>
    /// Writes data to the pipe writer and flushes it immediately.
    /// </summary>
    /// <param name="accessor">The pipeline accessor.</param>
    /// <param name="data">The data to write.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the write operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="accessor"/> or <paramref name="data"/> is null.</exception>
    public static async ValueTask WriteAsync(
        this RawPipelineAccessor accessor,
        ReadOnlyMemory<byte> data,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(accessor);

        var writer = accessor.AsPipeWriter();
        writer.Write(data.Span);
        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
    }
}