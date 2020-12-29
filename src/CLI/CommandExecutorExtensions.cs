#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotNetRealtimePipeline.CLI;

using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Extension methods for <see cref="CommandExecutor"/> providing convenient fluent APIs
/// for common data operations and pipeline management scenarios.
/// </summary>
public static class CommandExecutorExtensions
{
    /// <summary>
    /// Executes a command and returns the result as a boolean success indicator.
    /// </summary>
    /// <param name="executor">The command executor instance.</param>
    /// <param name="command">The parsed command to execute.</param>
    /// <returns>True if the command executed successfully; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="executor"/> or <paramref name="command"/> is null.</exception>
    public static async Task<bool> ExecuteSuccessfullyAsync(this CommandExecutor executor, ParsedCommand command)
    {
        ArgumentNullException.ThrowIfNull(executor);
        ArgumentNullException.ThrowIfNull(command);

        var exitCode = await executor.ExecuteAsync(command);
        return exitCode == 0;
    }

    /// <summary>
    /// Ingests data points from a file and returns the count of successfully ingested points.
    /// </summary>
    /// <param name="executor">The command executor instance.</param>
    /// <param name="filePath">Path to the data file (JSON or CSV format).</param>
    /// <param name="format">File format: "json" or "csv".</param>
    /// <returns>Number of successfully ingested data points.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="executor"/> or <paramref name="filePath"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is empty or contains only whitespace.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    public static async Task<int> IngestFromFileAsync(this CommandExecutor executor, string filePath, string format = "json")
    {
        ArgumentNullException.ThrowIfNull(executor);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(format);

        var success = await executor.IngestDataAsync(filePath, useBatch: false, format);
        return success ? await executor.CountDataPointsAsync(filePath, format) : 0;
    }

    /// <summary>
    /// Queries data points and returns them as a read-only list for safe consumption.
    /// </summary>
    /// <param name="executor">The command executor instance.</param>
    /// <param name="startMs">Start timestamp in milliseconds since epoch.</param>
    /// <param name="endMs">End timestamp in milliseconds since epoch.</param>
    /// <param name="source">Optional source filter (empty string for all sources).</param>
    /// <param name="minQuality">Minimum quality threshold (0 for no minimum).</param>
    /// <returns>Read-only list of matching data points.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="executor"/> is null.</exception>
    public static async Task<IReadOnlyList<DataPoint>> QueryDataAsync(
        this CommandExecutor executor,
        long startMs,
        long endMs,
        string source = "",
        int minQuality = 0)
    {
        ArgumentNullException.ThrowIfNull(executor);

        var results = await executor.QueryDataAsync(startMs, endMs, source, minQuality);
        return results.AsReadOnly();
    }

    /// <summary>
    /// Gets pipeline status and returns it as a dictionary with strongly-typed values.
    /// </summary>
    /// <param name="executor">The command executor instance.</param>
    /// <returns>Dictionary containing pipeline status information.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="executor"/> is null.</exception>
    public static async Task<Dictionary<string, object>> GetStatusAsync(this CommandExecutor executor)
    {
        ArgumentNullException.ThrowIfNull(executor);

        var status = await executor.GetStatusAsync();
        return status;
    }

    /// <summary>
    /// Counts the number of data points in a file without ingesting them.
    /// </summary>
    /// <param name="executor">The command executor instance.</param>
    /// <param name="filePath">Path to the data file (JSON or CSV format).</param>
    /// <param name="format">File format: "json" or "csv".</param>
    /// <returns>Number of data points in the file.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="executor"/> or <paramref name="filePath"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is empty or contains only whitespace.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    public static async Task<int> CountDataPointsAsync(this CommandExecutor executor, string filePath, string format = "json")
    {
        ArgumentNullException.ThrowIfNull(executor);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(format);

        var loader = FormatFactory.CreateLoader(format);
        var dataPoints = await loader.LoadAsync(filePath);
        return dataPoints.Count;
    }

    /// <summary>
    /// Exports data points to a file and returns the number of exported records.
    /// </summary>
    /// <param name="executor">The command executor instance.</param>
    /// <param name="startMs">Start timestamp in milliseconds since epoch.</param>
    /// <param name="endMs">End timestamp in milliseconds since epoch.</param>
    /// <param name="outputPath">Destination file path.</param>
    /// <param name="format">Export format: "json", "csv", or "xml".</param>
    /// <returns>Number of exported data points.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="executor"/> or <paramref name="outputPath"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="outputPath"/> is empty or contains only whitespace.</exception>
    public static async Task<int> ExportToFileAsync(
        this CommandExecutor executor,
        long startMs,
        long endMs,
        string outputPath,
        string format = "json")
    {
        ArgumentNullException.ThrowIfNull(executor);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(format);

        var success = await executor.ExportDataAsync(startMs, endMs, outputPath, format);
        return success ? await executor.CountExportedDataPointsAsync(outputPath, format) : 0;
    }

    /// <summary>
    /// Counts the number of data points in an exported file.
    /// </summary>
    /// <param name="executor">The command executor instance.</param>
    /// <param name="filePath">Path to the exported data file.</param>
    /// <param name="format">File format: "json", "csv", or "xml".</param>
    /// <returns>Number of data points in the exported file.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="executor"/> or <paramref name="filePath"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is empty or contains only whitespace.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    public static async Task<int> CountExportedDataPointsAsync(this CommandExecutor executor, string filePath, string format = "json")
    {
        ArgumentNullException.ThrowIfNull(executor);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(format);

        var loader = FormatFactory.CreateLoader(format);
        var dataPoints = await loader.LoadAsync(filePath);
        return dataPoints.Count;
    }

    /// <summary>
    /// Gets a summary string representation of the pipeline status.
    /// </summary>
    /// <param name="executor">The command executor instance.</param>
    /// <returns>Formatted status summary string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="executor"/> is null.</exception>
    public static async Task<string> GetStatusSummaryAsync(this CommandExecutor executor)
    {
        ArgumentNullException.ThrowIfNull(executor);

        var status = await executor.GetStatusAsync();
        if (status.Count == 0)
        {
            return "Pipeline status unavailable";
        }

        var sb = new StringBuilder();
        sb.AppendLine("Pipeline Status:");
        sb.AppendLine($"  Name: {status.GetValueOrDefault("pipeline_name", "Unknown")}");
        sb.AppendLine($"  Version: {status.GetValueOrDefault("version", "Unknown")}");
        sb.AppendLine($"  Running: {status.GetValueOrDefault("is_running", false)}");
        sb.AppendLine($"  Processed: {status.GetValueOrDefault("total_processed", 0)}");
        sb.AppendLine($"  Success Rate: {status.GetValueOrDefault("success_rate_percent", 0.0):F2}%");
        sb.AppendLine($"  Throughput: {status.GetValueOrDefault("throughput_items_per_sec", 0):F2} items/sec");
        sb.AppendLine($"  Health: {status.GetValueOrDefault("health_status", "Unknown")}");

        return sb.ToString();
    }
}