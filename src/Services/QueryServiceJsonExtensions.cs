#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Services;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="QueryService"/>.
/// </summary>
/// <remarks>
/// This static class contains extension methods for serializing <see cref="QueryService"/> instances
/// to JSON strings using camelCase property naming policy by default.
/// </remarks>
public static class QueryServiceJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
    };

    /// <summary>
    /// Serializes a <see cref="QueryService"/> instance to JSON.
    /// </summary>
    /// <param name="value">The <see cref="QueryService"/> instance to serialize.</param>
    /// <param name="indented"><see langword="true"/> to format the JSON with indentation; otherwise, produces compact JSON.</param>
    /// <returns>A JSON string representation of the <see cref="QueryService"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this QueryService value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = new JsonSerializerOptions(_jsonSerializerOptions)
        {
            WriteIndented = indented
        };

        return JsonSerializer.Serialize(value, options);
    }

}