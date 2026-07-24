#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Integration;

using System.IO.Pipelines;

/// <summary>
/// Opt-in interface for pipeline components that can expose their underlying
/// <see cref="System.IO.Pipelines"/> primitives directly, enabling zero-copy
/// integration with <c>Socket</c>, <c>NetworkStream</c>, ASP.NET Core request
/// bodies, and other <c>PipeReader</c>/<c>PipeWriter</c>-aware APIs.
/// </summary>
/// <remarks>
/// Obtaining a <see cref="PipeReader"/> or <see cref="PipeWriter"/> via this
/// interface bypasses the higher-level <c>IDataStream&lt;T&gt;</c> abstraction
/// and avoids the intermediate buffer copy it introduces.  Use this only when
/// the extra throughput is worth the loss of the higher-level back-pressure and
/// windowing guarantees — see <c>docs/backpressure.md</c> for guidance.
/// <para>
/// <b>Thread-safety contract:</b> <see cref="AsPipeReader"/> and
/// <see cref="AsPipeWriter"/> return the live, shared reader/writer of the
/// underlying pipe — not a snapshot. A <see cref="PipeReader"/> supports at
/// most one concurrent reader and a <see cref="PipeWriter"/> supports at most
/// one concurrent writer; calling <c>ReadAsync</c>/<c>Write</c> from more than
/// one thread on the same side concurrently is undefined behaviour, matching
/// the contract of <see cref="System.IO.Pipelines.Pipe"/> itself. It is safe
/// for one reader thread and one writer thread to operate concurrently on
/// opposite sides of the same pipe. All extension helpers over this interface
/// (see <c>RawPipelineAccessExtensions</c>) inherit this constraint — they do
/// not add their own synchronization.
/// </para>
/// <para>
/// This is deliberately the only extension surface over the reader/writer
/// themselves. The concrete <see cref="RawPipelineAccessor"/> type additionally
/// has <c>RawPipelineAccessorJsonExtensions</c> and
/// <c>RawPipelineAccessorValidation</c> helpers, but those operate solely on the
/// accessor's identity/configuration - never on the live reader or writer - so
/// they carry no thread-safety restriction and are safe to call concurrently
/// with an in-progress read or write.
/// </para>
/// </remarks>
public interface IRawPipelineAccess
{
    /// <summary>
    /// Returns the underlying <see cref="PipeReader"/> for the pipeline's read side.
    /// Callers can pipe this directly into <c>Socket.ReceiveAsync</c> or
    /// <c>NetworkStream.CopyToAsync</c> without an intermediate copy.
    /// </summary>
    /// <remarks>
    /// The returned instance is the pipe's live reader, not a snapshot. Only one
    /// caller may read from it at a time while the pipeline is running.
    /// </remarks>
    PipeReader AsPipeReader();

    /// <summary>
    /// Returns the underlying <see cref="PipeWriter"/> for the pipeline's write side.
    /// Callers can write raw bytes (e.g. from a network buffer) directly into the
    /// pipeline without an intermediate copy.
    /// </summary>
    /// <remarks>
    /// The returned instance is the pipe's live writer, not a snapshot. Only one
    /// caller may write to it at a time while the pipeline is running.
    /// </remarks>
    PipeWriter AsPipeWriter();
}
