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
/// </remarks>
public interface IRawPipelineAccess
{
    /// <summary>
    /// Returns the underlying <see cref="PipeReader"/> for the pipeline's read side.
    /// Callers can pipe this directly into <c>Socket.ReceiveAsync</c> or
    /// <c>NetworkStream.CopyToAsync</c> without an intermediate copy.
    /// </summary>
    PipeReader AsPipeReader();

    /// <summary>
    /// Returns the underlying <see cref="PipeWriter"/> for the pipeline's write side.
    /// Callers can write raw bytes (e.g. from a network buffer) directly into the
    /// pipeline without an intermediate copy.
    /// </summary>
    PipeWriter AsPipeWriter();
}
