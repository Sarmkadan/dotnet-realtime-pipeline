#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text.Json;

namespace DotNetRealtimePipeline.Domain.Models;

/// <summary>
/// Provides JSON serialization helpers for <see cref="ScalingDecision"/>.
/// </summary>
public static class ScalingDecisionJsonExtensions
{
    /// <summary>
    /// Cached <see cref="JsonSerializerOptions"/> that uses camel‑case property naming.
    /// </summary>
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        // WriteIndented is set per call.
    };

    /// <summary>
    /// Serialises the <see cref="ScalingDecision"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The instance to serialise.</param>
    /// <param name="indented">If <c>true</c>, the output JSON will be formatted with indentation.</param>
    /// <returns>A JSON representation of <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson(this ScalingDecision value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        // Clone the cached options to avoid mutating the shared instance.
        var options = new JsonSerializerOptions(_options) { WriteIndented = indented };
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserialises a JSON string to a <see cref="ScalingDecision"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized <see cref="ScalingDecision"/>, or <c>null</c> if the JSON represents a null value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <c>null</c>.</exception>
    /// <exception cref="JsonException">The JSON is invalid or does not match the <see cref="ScalingDecision"/> schema.</exception>
    public static ScalingDecision? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return JsonSerializer.Deserialize<ScalingDecision>(json, _options);
    }

    /// <summary>
    /// Attempts to deserialise a JSON string to a <see cref="ScalingDecision"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">
    /// When this method returns, contains the deserialized <see cref="ScalingDecision"/> if the operation succeeded,
    /// or <c>null</c> if it failed.
    /// </param>
    /// <returns><c>true</c> if deserialization succeeded; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <c>null</c>.</exception>
    public static bool TryFromJson(string json, out ScalingDecision? value)
    {
        ArgumentNullException.ThrowIfNull(json);
        try
        {
            value = JsonSerializer.Deserialize<ScalingDecision>(json, _options);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
