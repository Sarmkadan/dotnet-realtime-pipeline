#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotNetRealtimePipeline.Utilities;

using System;
using System.Text.Json;
using DotNetRealtimePipeline.Domain.Models;

/// <summary>
/// Extension methods providing convenient JSON serialization helpers for pipeline data types.
/// </summary>
public static class SerializationHelperJsonExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes a <see cref="DataPoint"/> to a JSON string.
    /// </summary>
    /// <param name="dataPoint">The data point to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the data point.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dataPoint"/> is null.</exception>
    public static string ToJson(this DataPoint dataPoint, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(dataPoint);

        var options = indented
            ? new JsonSerializerOptions(JsonOptions) { WriteIndented = true }
            : JsonOptions;

        return JsonSerializer.Serialize(dataPoint, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="DataPoint"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A deserialized <see cref="DataPoint"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is malformed or cannot be deserialized.</exception>
    public static DataPoint FromJsonToDataPoint(this string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<DataPoint>(json, JsonOptions)
            ?? throw new JsonException("Deserialization returned null for DataPoint");
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="DataPoint"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="dataPoint">Receives the deserialized value if successful; otherwise, null.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJsonToDataPoint(this string json, out DataPoint? dataPoint)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            dataPoint = JsonSerializer.Deserialize<DataPoint>(json, JsonOptions);
            return dataPoint is not null;
        }
        catch (JsonException)
        {
            dataPoint = null;
            return false;
        }
    }

    /// <summary>
    /// Serializes a collection of <see cref="DataPoint"/> to a JSON array string.
    /// </summary>
    /// <param name="dataPoints">The data points to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON array string representation of the data points.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dataPoints"/> is null.</exception>
    public static string ToJson(this System.Collections.Generic.IReadOnlyList<DataPoint> dataPoints, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(dataPoints);

        var options = indented
            ? new JsonSerializerOptions(JsonOptions) { WriteIndented = true }
            : JsonOptions;

        return JsonSerializer.Serialize(dataPoints, options);
    }

    /// <summary>
    /// Serializes a <see cref="ProcessingResult"/> to a JSON string.
    /// </summary>
    /// <param name="result">The processing result to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the processing result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static string ToJson(this ProcessingResult result, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(result);

        var options = indented
            ? new JsonSerializerOptions(JsonOptions) { WriteIndented = true }
            : JsonOptions;

        return JsonSerializer.Serialize(result, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="ProcessingResult"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A deserialized <see cref="ProcessingResult"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is malformed or cannot be deserialized.</exception>
    public static ProcessingResult FromJsonToProcessingResult(this string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<ProcessingResult>(json, JsonOptions)
            ?? throw new JsonException("Deserialization returned null for ProcessingResult");
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="ProcessingResult"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="result">Receives the deserialized value if successful; otherwise, null.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJsonToProcessingResult(this string json, out ProcessingResult? result)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            result = JsonSerializer.Deserialize<ProcessingResult>(json, JsonOptions);
            return result is not null;
        }
        catch (JsonException)
        {
            result = null;
            return false;
        }
    }

    /// <summary>
    /// Serializes a collection of <see cref="ProcessingResult"/> to a JSON array string.
    /// </summary>
    /// <param name="results">The processing results to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON array string representation of the processing results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="results"/> is null.</exception>
    public static string ToJson(this System.Collections.Generic.IReadOnlyList<ProcessingResult> results, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(results);

        var options = indented
            ? new JsonSerializerOptions(JsonOptions) { WriteIndented = true }
            : JsonOptions;

        return JsonSerializer.Serialize(results, options);
    }

    /// <summary>
    /// Serializes a <see cref="MetricAggregation"/> to a JSON string.
    /// </summary>
    /// <param name="metrics">The metric aggregation to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the metric aggregation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is null.</exception>
    public static string ToJson(this MetricAggregation metrics, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(metrics);

        var options = indented
            ? new JsonSerializerOptions(JsonOptions) { WriteIndented = true }
            : JsonOptions;

        return JsonSerializer.Serialize(metrics, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="MetricAggregation"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A deserialized <see cref="MetricAggregation"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is malformed or cannot be deserialized.</exception>
    public static MetricAggregation FromJsonToMetricAggregation(this string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<MetricAggregation>(json, JsonOptions)
            ?? throw new JsonException("Deserialization returned null for MetricAggregation");
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="MetricAggregation"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="metrics">Receives the deserialized value if successful; otherwise, null.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJsonToMetricAggregation(this string json, out MetricAggregation? metrics)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            metrics = JsonSerializer.Deserialize<MetricAggregation>(json, JsonOptions);
            return metrics is not null;
        }
        catch (JsonException)
        {
            metrics = null;
            return false;
        }
    }

    /// <summary>
    /// Serializes a collection of <see cref="MetricAggregation"/> to a JSON array string.
    /// </summary>
    /// <param name="metrics">The metric aggregations to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON array string representation of the metric aggregations.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is null.</exception>
    public static string ToJson(this System.Collections.Generic.IReadOnlyList<MetricAggregation> metrics, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(metrics);

        var options = indented
            ? new JsonSerializerOptions(JsonOptions) { WriteIndented = true }
            : JsonOptions;

        return JsonSerializer.Serialize(metrics, options);
    }
}