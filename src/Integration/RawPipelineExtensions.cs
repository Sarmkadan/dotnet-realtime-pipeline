#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Integration;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;

/// <summary>
/// Extension methods that provide zero-copy pipeline access to any object
/// implementing <see cref="IRawPipelineAccess"/>.
/// </summary>
/// <remarks>
/// These extension methods enable efficient, allocation-free access to <see cref="PipeReader"/>
/// and <see cref="PipeWriter"/> instances through the <see cref="IRawPipelineAccess"/> interface,
/// avoiding unnecessary buffering or copying when working with pipeline-capable components.
/// </remarks>
public static class RawPipelineExtensions
{
    /// <summary>
    /// Gets the raw <see cref="PipeReader"/> for the specified <paramref name="source"/>.
    /// </summary>
    /// <param name="source">The object that implements <see cref="IRawPipelineAccess"/>.</param>
    /// <returns>A <see cref="PipeReader"/> instance providing zero-copy access to the pipeline.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException"><paramref name="source"/> does not implement <see cref="IRawPipelineAccess"/>.</exception>
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
    /// Gets the raw <see cref="PipeWriter"/> for the specified <paramref name="source"/>.
    /// </summary>
    /// <param name="source">The object that implements <see cref="IRawPipelineAccess"/>.</param>
    /// <returns>A <see cref="PipeWriter"/> instance providing zero-copy access to the pipeline.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException"><paramref name="source"/> does not implement <see cref="IRawPipelineAccess"/>.</exception>
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