// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Utilities;

using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

/// <summary>
/// Helper class for serialization and deserialization of pipeline objects.
/// Supports JSON, CSV, and TSV formats with compression support.
/// </summary>
public class SerializationHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes a data point to JSON string.
    /// </summary>
    public static string ToJson(DataPoint dataPoint)
    {
        return JsonSerializer.Serialize(dataPoint, JsonOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to a data point.
    /// </summary>
    public static DataPoint FromJson(string json)
    {
        return JsonSerializer.Deserialize<DataPoint>(json, JsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize DataPoint");
    }

    /// <summary>
    /// Serializes a collection of data points to JSON array string.
    /// </summary>
    public static string ToJsonArray(List<DataPoint> dataPoints)
    {
        return JsonSerializer.Serialize(dataPoints, JsonOptions);
    }

    /// <summary>
    /// Deserializes a JSON array string to a collection of data points.
    /// </summary>
    public static List<DataPoint> FromJsonArray(string json)
    {
        return JsonSerializer.Deserialize<List<DataPoint>>(json, JsonOptions)
            ?? new List<DataPoint>();
    }

    /// <summary>
    /// Serializes a data point to CSV format.
    /// </summary>
    public static string ToCsv(DataPoint dataPoint, bool includeHeader = true)
    {
        var sb = new StringBuilder();

        if (includeHeader)
        {
            sb.AppendLine("Id,Timestamp,Value,Source,Quality,Tags");
        }

        sb.AppendLine($"{dataPoint.Id},{dataPoint.Timestamp},{dataPoint.Value:F4},{EscapeCsv(dataPoint.Source)},{dataPoint.Quality},{EscapeCsv(dataPoint.Tags)}");

        return sb.ToString();
    }

    /// <summary>
    /// Serializes a collection of data points to CSV format.
    /// </summary>
    public static string ToCsvBatch(List<DataPoint> dataPoints)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Id,Timestamp,Value,Source,Quality,Tags");

        foreach (var point in dataPoints)
        {
            sb.AppendLine($"{point.Id},{point.Timestamp},{point.Value:F4},{EscapeCsv(point.Source)},{point.Quality},{EscapeCsv(point.Tags)}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Serializes processing results to JSON.
    /// </summary>
    public static string SerializeResults(List<ProcessingResult> results)
    {
        return JsonSerializer.Serialize(results, JsonOptions);
    }

    /// <summary>
    /// Serializes metrics aggregation to JSON.
    /// </summary>
    public static string SerializeMetrics(MetricAggregation metrics)
    {
        return JsonSerializer.Serialize(metrics, JsonOptions);
    }

    /// <summary>
    /// Escapes CSV special characters.
    /// </summary>
    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}

/// <summary>
/// Helper for batch serialization with streaming support.
/// </summary>
public class BatchSerializationHelper
{
    /// <summary>
    /// Writes data points to a file asynchronously.
    /// </summary>
    public static async Task WriteToFileAsync(string filePath, List<DataPoint> dataPoints, string format = "json")
    {
        var content = format.ToLowerInvariant() switch
        {
            "csv" => SerializationHelper.ToCsvBatch(dataPoints),
            "json" => SerializationHelper.ToJsonArray(dataPoints),
            _ => throw new InvalidOperationException($"Unsupported format: {format}")
        };

        await System.IO.File.WriteAllTextAsync(filePath, content, Encoding.UTF8);
    }

    /// <summary>
    /// Reads data points from a file asynchronously.
    /// </summary>
    public static async Task<List<DataPoint>> ReadFromFileAsync(string filePath, string format = "json")
    {
        var content = await System.IO.File.ReadAllTextAsync(filePath, Encoding.UTF8);

        return format.ToLowerInvariant() switch
        {
            "json" => SerializationHelper.FromJsonArray(content),
            "csv" => ParseCsvContent(content),
            _ => throw new InvalidOperationException($"Unsupported format: {format}")
        };
    }

    /// <summary>
    /// Parses CSV content into data points.
    /// </summary>
    private static List<DataPoint> ParseCsvContent(string content)
    {
        var dataPoints = new List<DataPoint>();
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Skip header
        for (int i = 1; i < lines.Length; i++)
        {
            var fields = lines[i].Split(',');
            if (fields.Length >= 6)
            {
                try
                {
                    var dataPoint = new DataPoint(
                        id: int.Parse(fields[0]),
                        timestamp: long.Parse(fields[1]),
                        value: double.Parse(fields[2]),
                        source: fields[3]
                    )
                    {
                        Quality = int.Parse(fields[4]),
                        Tags = fields[5]
                    };

                    dataPoints.Add(dataPoint);
                }
                catch
                {
                    // Skip malformed lines
                }
            }
        }

        return dataPoints;
    }
}

/// <summary>
/// Helper for timestamp and date conversion in serialization contexts.
/// </summary>
public class DateTimeSerializationHelper
{
    /// <summary>
    /// Converts a Unix timestamp to ISO 8601 string.
    /// </summary>
    public static string UnixToIso8601(long unixTimeMs)
    {
        var dateTime = UnixTimeStampToDateTime(unixTimeMs);
        return dateTime.ToString("O");
    }

    /// <summary>
    /// Converts an ISO 8601 string to Unix timestamp.
    /// </summary>
    public static long Iso8601ToUnix(string iso8601)
    {
        var dateTime = DateTime.Parse(iso8601, null, System.Globalization.DateTimeStyles.RoundtripKind);
        return (long)(dateTime - DateTime.UnixEpoch).TotalMilliseconds;
    }

    /// <summary>
    /// Converts a Unix millisecond timestamp to DateTime.
    /// </summary>
    private static DateTime UnixTimeStampToDateTime(long unixTimeMs)
    {
        return DateTime.UnixEpoch.AddMilliseconds(unixTimeMs);
    }
}

/// <summary>
/// Helper for object-to-dictionary conversion for flexible serialization.
/// </summary>
public class DictionaryConversionHelper
{
    /// <summary>
    /// Converts a DataPoint to a dictionary.
    /// </summary>
    public static Dictionary<string, object> ToDictionary(DataPoint dataPoint)
    {
        return new Dictionary<string, object>
        {
            ["id"] = dataPoint.Id,
            ["timestamp"] = dataPoint.Timestamp,
            ["value"] = dataPoint.Value,
            ["source"] = dataPoint.Source,
            ["quality"] = dataPoint.Quality,
            ["tags"] = dataPoint.Tags,
            ["metadata"] = dataPoint.GetMetadata()
        };
    }

    /// <summary>
    /// Converts a ProcessingResult to a dictionary.
    /// </summary>
    public static Dictionary<string, object> ToDictionary(ProcessingResult result)
    {
        return new Dictionary<string, object>
        {
            ["result_id"] = result.ResultId,
            ["success"] = result.Success,
            ["error_message"] = result.ErrorMessage,
            ["processing_time_ms"] = result.ProcessingTimeMs,
            ["processed_at"] = result.ProcessedAt,
            ["stage_name"] = result.StageName
        };
    }

    /// <summary>
    /// Converts a MetricAggregation to a dictionary.
    /// </summary>
    public static Dictionary<string, object> ToDictionary(MetricAggregation metrics)
    {
        var dict = new Dictionary<string, object>
        {
            ["computed_at"] = metrics.ComputedAt,
            ["total_items_processed"] = metrics.TotalItemsProcessed,
            ["total_items_failed"] = metrics.TotalItemsFailed,
            ["total_items_skipped"] = metrics.TotalItemsSkipped,
            ["average_processing_time_ms"] = metrics.AverageProcessingTimeMs,
            ["backpressure_events"] = metrics.BackpressureEvents
        };

        return dict;
    }
}
