#nullable enable
using System;
using System.Text.Json;

/// <summary>
/// Provides System.Text.Json serialization helpers for <see cref="PipelineVisualizerTests"/>.
/// </summary>
namespace DotNetRealtimePipeline.Tests.Unit;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="PipelineVisualizerTests"/> instances.
/// </summary>
public static class PipelineVisualizerTestsJsonExtensions
{
    /// <summary>
    /// Cached <see cref="JsonSerializerOptions"/> that uses camel-case property naming and no indentation.
    /// </summary>
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes the <see cref="PipelineVisualizerTests"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The instance to serialize.</param>
    /// <param name="indented">If <c>true</c>, the output JSON is formatted with indentation.</param>
    /// <returns>A JSON representation of <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson(this PipelineVisualizerTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        var opts = indented ? new JsonSerializerOptions(Options) { WriteIndented = true } : Options;
        return JsonSerializer.Serialize(value, opts);
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="PipelineVisualizerTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized <see cref="PipelineVisualizerTests"/> instance, or <c>null</c> if the JSON represents a null value.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <c>null</c> or an empty string.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be mapped to <see cref="PipelineVisualizerTests"/>.</exception>
    public static PipelineVisualizerTests? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<PipelineVisualizerTests>(json, Options);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="PipelineVisualizerTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">
    /// When this method returns, contains the deserialized <see cref="PipelineVisualizerTests"/> instance if the operation succeeded;
    /// otherwise, <c>null</c>.
    /// </param>
    /// <returns><c>true</c> if deserialization succeeded; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <c>null</c> or an empty string.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be mapped to <see cref="PipelineVisualizerTests"/>.</exception>
    public static bool TryFromJson(string json, out PipelineVisualizerTests? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        try
        {
            value = JsonSerializer.Deserialize<PipelineVisualizerTests>(json, Options);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}