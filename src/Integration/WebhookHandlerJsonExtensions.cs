#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Integration;

using System;
using System.Text.Json;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="WebhookHandler"/>.
/// </summary>
public static class WebhookHandlerJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes the <see cref="WebhookHandler"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The webhook handler to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the webhook handler.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this WebhookHandler value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        return JsonSerializer.Serialize(value, GetJsonSerializerOptions(indented));
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="WebhookHandler"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized webhook handler, or null if the JSON is invalid.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static WebhookHandler? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<WebhookHandler>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="WebhookHandler"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized webhook handler if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out WebhookHandler? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<WebhookHandler>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Gets the appropriate JSON serializer options based on the indented flag.
    /// </summary>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>Configured <see cref="JsonSerializerOptions"/>.</returns>
    private static JsonSerializerOptions GetJsonSerializerOptions(bool indented)
        => indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;
}