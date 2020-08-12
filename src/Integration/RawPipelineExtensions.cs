#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Integration;

using System;
using System.IO.Pipelines;

/// <summary>
/// Extension methods that make zero-copy pipeline access available on any object
/// that implements <see cref="IRawPipelineAccess"/>.
/// </summary>
public static class RawPipelineExtensions
{
    /// <summary>
    /// Returns the raw <see cref="PipeReader"/> for <paramref name="source"/>.
    /// Throws <see cref="NotSupportedException"/> if the object does not implement
    /// <see cref="IRawPipelineAccess"/>, providing a clear error rather than a
    /// confusing null-reference.
    /// </summary>
    public static PipeReader GetPipeReader(this object source)
    {
        if (source is IRawPipelineAccess raw)
            return raw.AsPipeReader();

        throw new NotSupportedException(
            $"{source.GetType().Name} does not implement {nameof(IRawPipelineAccess)}. " +
            "Wrap it with RawPipelineAccessor or implement the interface to enable zero-copy access.");
    }

    /// <summary>
    /// Returns the raw <see cref="PipeWriter"/> for <paramref name="source"/>.
    /// Throws <see cref="NotSupportedException"/> if the object does not implement
    /// <see cref="IRawPipelineAccess"/>.
    /// </summary>
    public static PipeWriter GetPipeWriter(this object source)
    {
        if (source is IRawPipelineAccess raw)
            return raw.AsPipeWriter();

        throw new NotSupportedException(
            $"{source.GetType().Name} does not implement {nameof(IRawPipelineAccess)}. " +
            "Wrap it with RawPipelineAccessor or implement the interface to enable zero-copy access.");
    }
}
