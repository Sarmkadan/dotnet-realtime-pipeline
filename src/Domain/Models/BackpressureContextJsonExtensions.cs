#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Domain.Models;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="BackpressureContext"/>.
/// </summary>
public static class BackpressureContextJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
    };

    /// <summary>
    /// Serializes a <see cref="BackpressureContext"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The backpressure context to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the backpressure context.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this BackpressureContext value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true
            }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="BackpressureContext"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized backpressure context, or null if the JSON is null or empty.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static BackpressureContext? FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<BackpressureContext>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="BackpressureContext"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized backpressure context if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out BackpressureContext? value)
    {
        value = null;

        if (string.IsNullOrEmpty(json))
        {
            return true;
        }

        try
        {
            value = JsonSerializer.Deserialize<BackpressureContext>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Custom JSON converter for <see cref="Queue{T}"/> to ensure proper serialization.
    /// </summary>
    private sealed class QueueJsonConverter : JsonConverter<Queue<long>>
    {
        public override Queue<long> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return new Queue<long>();
            }

            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException($"Expected StartArray, got {reader.TokenType}");
            }

            var queue = new Queue<long>();
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType == JsonTokenType.Number)
                {
                    queue.Enqueue(reader.GetInt64());
                }
                else
                {
                    throw new JsonException($"Expected number, got {reader.TokenType}");
                }
            }

            return queue;
        }

        public override void Write(
            Utf8JsonWriter writer,
            Queue<long> value,
            JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var item in value)
            {
                writer.WriteNumberValue(item);
            }
            writer.WriteEndArray();
        }
    }

    /// <summary>
    /// Custom JSON converter for <see cref="Dictionary{TKey,TValue}"/> to ensure proper serialization.
    /// </summary>
    private sealed class DictionaryJsonConverter : JsonConverter<Dictionary<string, long>>
    {
        public override Dictionary<string, long> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return new Dictionary<string, long>();
            }

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException($"Expected StartObject, got {reader.TokenType}");
            }

            var dictionary = new Dictionary<string, long>(StringComparer.Ordinal);
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propertyName = reader.GetString()!;
                    reader.Read();

                    if (reader.TokenType == JsonTokenType.Number)
                    {
                        dictionary[propertyName] = reader.GetInt64();
                    }
                    else
                    {
                        throw new JsonException($"Expected number for property {propertyName}, got {reader.TokenType}");
                    }
                }
            }

            return dictionary;
        }

        public override void Write(
            Utf8JsonWriter writer,
            Dictionary<string, long> value,
            JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();
            foreach (var kvp in value)
            {
                writer.WriteNumber(kvp.Key, kvp.Value);
            }
            writer.WriteEndObject();
        }
    }

    /// <summary>
    /// Custom JSON converter for <see cref="DateTime"/> to ensure consistent UTC formatting.
    /// </summary>
    private sealed class DateTimeJsonConverter : JsonConverter<DateTime>
    {
        private const string DateFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";

        public override DateTime Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return DateTime.MinValue;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                string dateString = reader.GetString()!;
                if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result))
                {
                    return result;
                }
            }

            throw new JsonException($"Expected string for DateTime, got {reader.TokenType}");
        }

        public override void Write(
            Utf8JsonWriter writer,
            DateTime value,
            JsonSerializerOptions options)
        {
            if (value == DateTime.MinValue || value == default)
            {
                writer.WriteStringValue(string.Empty);
            }
            else
            {
                writer.WriteStringValue(value.ToUniversalTime().ToString(DateFormat, CultureInfo.InvariantCulture));
            }
        }
    }
}