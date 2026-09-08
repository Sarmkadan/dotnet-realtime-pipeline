#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Configuration;

using System;
using System.Text.Json;
using EventServiceConfiguration = global::WorkerOptions;

/// <summary>
/// Provides JSON serialization helpers for <see cref="EventServiceConfiguration"/> instances.
/// </summary>
public static class EventServiceConfigurationJsonExtensions
{
    /// <summary>
    /// Serializes an <see cref="EventServiceConfiguration"/> instance to indented JSON.
    /// </summary>
    /// <param name="config">The event service configuration to serialize.</param>
    /// <returns>An indented JSON representation of the event service configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is null.</exception>
    public static string ToJson(this EventServiceConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(config);

        return JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
    }
}
