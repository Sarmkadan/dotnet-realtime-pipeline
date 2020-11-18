#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotNetRealtimePipeline.Utilities;

using System;
using System.Text.Json;

/// <summary>
/// Provides JSON serialization extensions for the <see cref="ValidationHelper"/> type.
/// </summary>
public static class ValidationHelperJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes a <see cref="ValidationHelper"/> to JSON.
    /// </summary>
    /// <param name="value">The validation helper.</param>
    /// <param name="indented">Whether to indent the JSON.</param>
    /// <returns>JSON string.</returns>
    /// <exception cref="ArgumentNullException">If value is null.</exception>
    public static string ToJson(this ValidationHelper value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a <see cref="ValidationHelper"/> from JSON.
    /// </summary>
    /// <param name="json">JSON string.</param>
    /// <returns>Deserialized validation helper or null.</returns>
    /// <exception cref="JsonException">If JSON is invalid.</exception>
    public static ValidationHelper? FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<ValidationHelper>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a <see cref="ValidationHelper"/> from JSON.
    /// </summary>
    /// <param name="json">JSON string.</param>
    /// <param name="value">Receives the result.</param>
    /// <returns>True if successful.</returns>
    public static bool TryFromJson(string json, out ValidationHelper? value)
    {
        value = null;

        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<ValidationHelper>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}