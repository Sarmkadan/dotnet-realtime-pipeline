#nullable enable
// =============================================================================
// Author: [Your Name]
// =============================================================================

namespace DotNetRealtimePipeline.Utilities;

using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

/// <summary>
/// Extension methods for SerializationHelper.
/// </summary>
public static class SerializationHelperExtensions
{
    /// <summary>
    /// Serializes a list of ProcessingResult to a file asynchronously.
    /// </summary>
    /// <param name="results">List of ProcessingResult to serialize.</param>
    /// <param name="filePath">Path to the file to write to.</param>
    /// <exception cref="ArgumentNullException">Thrown when results or filePath is null.</exception>
    public static async Task SerializeResultsToFileAsync(this List<ProcessingResult> results, string filePath)
    {
        ArgumentNullException.ThrowIfNull(results);
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        var content = SerializationHelper.SerializeResults(results);
        await System.IO.File.WriteAllTextAsync(filePath, content, System.Text.Encoding.UTF8);
    }

    /// <summary>
    /// Deserializes a file to a list of ProcessingResult asynchronously.
    /// </summary>
    /// <param name="filePath">Path to the file to read from.</param>
    /// <returns>A list of ProcessingResult.</returns>
    /// <exception cref="ArgumentException">Thrown when filePath is null or empty.</exception>
    public static async Task<List<ProcessingResult>> DeserializeResultsFromFileAsync(this string filePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        var content = await System.IO.File.ReadAllTextAsync(filePath, System.Text.Encoding.UTF8);
        return System.Text.Json.JsonSerializer.Deserialize<List<ProcessingResult>>(content, SerializationHelper.JsonOptions)
            ?? new List<ProcessingResult>();
    }

    /// <summary>
    /// Serializes a MetricAggregation to a file asynchronously.
    /// </summary>
    /// <param name="metrics">MetricAggregation to serialize.</param>
    /// <param name="filePath">Path to the file to write to.</param>
    /// <exception cref="ArgumentNullException">Thrown when metrics or filePath is null.</exception>
    public static async Task SerializeMetricsToFileAsync(this MetricAggregation metrics, string filePath)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        var content = SerializationHelper.SerializeMetrics(metrics);
        await System.IO.File.WriteAllTextAsync(filePath, content, System.Text.Encoding.UTF8);
    }
}
