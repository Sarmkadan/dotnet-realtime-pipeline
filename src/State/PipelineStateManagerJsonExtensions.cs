#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.State;

using System;
using System.Text.Json;

/// <summary>
/// Provides JSON serialization extensions for <see cref="PipelineStateManager"/> instances.
/// </summary>
public static class PipelineStateManagerJsonExtensions
{
    /// <summary>
    /// Serializes the state history of the specified <see cref="PipelineStateManager"/> to indented JSON.
    /// </summary>
    /// <param name="manager">The pipeline state manager whose state history to serialize.</param>
    /// <returns>An indented JSON string representing the state history.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="manager"/> is null.</exception>
    public static string ToHistoryJson(this PipelineStateManager manager)
    {
        ArgumentNullException.ThrowIfNull(manager);

        return JsonSerializer.Serialize(
            manager.GetStateHistory(),
            new JsonSerializerOptions { WriteIndented = true });
    }
}
