#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.CLI;

using DotNetRealtimePipeline.Services;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Visualization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;

/// <summary>
/// Executes parsed commands with context-aware handling.
/// Bridges CLI layer with service layer, managing dependencies and error handling.
/// </summary>
public sealed class CommandExecutor
{
    private readonly PipelineOrchestrator _orchestrator;
    private readonly ILogger<CommandExecutor> _logger;
    private readonly PipelineVisualizer _visualizer;

    public CommandExecutor(
        PipelineOrchestrator orchestrator,
        ILogger<CommandExecutor> logger,
        PipelineVisualizer visualizer)
    {
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _visualizer = visualizer ?? throw new ArgumentNullException(nameof(visualizer));
    }

    /// <summary>
    /// Executes a parsed command with error handling and logging.
    /// </summary>
    public async Task<int> ExecuteAsync(ParsedCommand command)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (!command.IsValid)
        {
            _logger.LogError("Invalid command: {Error}", command.ErrorMessage);
            return 1;
        }

        try
        {
            _logger.LogInformation("Executing command: {Verb}", command.Verb);
            return await command.ExecuteAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Command execution failed: {Message}", ex.Message);
            return 1;
        }
    }

    /// <summary>
    /// Ingests data points from a file or batch.
    /// </summary>
    public async Task<bool> IngestDataAsync(string filePath, bool useBatch, string format)
    {
        try
        {
            _logger.LogInformation("Starting data ingestion from: {Path}", filePath);

            var dataPoints = await LoadDataPointsFromFile(filePath, format);
            _logger.LogInformation("Loaded {Count} data points", dataPoints.Count);

            if (useBatch)
            {
                var results = await _orchestrator.ProcessBatchDataPointsAsync(dataPoints);
                _logger.LogInformation("Batch processing complete: {Success} succeeded, {Failed} failed",
                    results.SuccessfulCount, results.FailedCount);
            }
            else
            {
                int successCount = 0;
                foreach (var point in dataPoints)
                {
                    if (await _orchestrator.IngestDataPointAsync(point))
                        successCount++;
                }
                _logger.LogInformation("Ingestion complete: {Success}/{Total} succeeded",
                    successCount, dataPoints.Count);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Data ingestion failed");
            return false;
        }
    }

    /// <summary>
    /// Queries data points within a time range.
    /// </summary>
    public async Task<List<DataPoint>> QueryDataAsync(long startMs, long endMs, string source, int minQuality)
    {
        try
        {
            _logger.LogInformation("Querying data: {Start} to {End}, source={Source}, quality={Quality}",
                startMs, endMs, source, minQuality);

            var queryService = _orchestrator.GetQueryService();
            var results = await queryService.SearchDataPointsAsync(startMs, endMs, source, minQuality);

            _logger.LogInformation("Query returned {Count} results", results.Count);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Data query failed");
            return new List<DataPoint>();
        }
    }

    /// <summary>
    /// Gets current pipeline status.
    /// </summary>
    public async Task<Dictionary<string, object>> GetStatusAsync()
    {
        try
        {
            var status = _orchestrator.GetStatus();
            var health = await _orchestrator.GetHealthReportAsync();

            var result = new Dictionary<string, object>
            {
                ["pipeline_name"] = status.ConfigurationName,
                ["version"] = status.ConfigurationVersion,
                ["is_running"] = status.IsRunning,
                ["total_processed"] = status.TotalDataPointsProcessed,
                ["total_failed"] = status.TotalDataPointsFailed,
                ["pending"] = status.PendingItemsInQueue,
                ["health_status"] = health?.Status.ToString() ?? "Unknown",
                ["throughput_items_per_sec"] = health?.ThroughputItemsPerSecond ?? 0,
                ["success_rate_percent"] = health?.SuccessRatePercent ?? 0,
                ["avg_latency_ms"] = health?.AverageProcessingTimeMs ?? 0
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve status");
            return new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Exports data to a file in specified format.
    /// </summary>
    public async Task<bool> ExportDataAsync(long startMs, long endMs, string outputPath, string format)
    {
        try
        {
            _logger.LogInformation("Exporting data [{Start},{End}] to {Path} ({Format})",
                startMs, endMs, outputPath, format);

            var queryService = _orchestrator.GetQueryService();
            var data = await queryService.SearchDataPointsAsync(startMs, endMs, "", 0);

            var exporter = FormatFactory.CreateExporter(format);
            await exporter.ExportAsync(data, outputPath);

            _logger.LogInformation("Export complete: {Count} records written", data.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Data export failed");
            return false;
        }
    }

    /// <summary>
    /// Loads data points from a file based on format.
    /// </summary>
    private async Task<List<DataPoint>> LoadDataPointsFromFile(string filePath, string format)
    {
        var loader = FormatFactory.CreateLoader(format);
        return await loader.LoadAsync(filePath);
    }

    /// <summary>
    /// Renders an ASCII visualization of the pipeline topology with live runtime metrics.
    /// Pass <paramref name="compact"/> as <c>true</c> for a single-line summary.
    /// </summary>
    public Task<string> VisualizeAsync(PipelineConfig config, bool compact = false)
    {
        try
        {
            _logger.LogInformation("Rendering pipeline visualization (compact={Compact})", compact);
            var output = compact
                ? _visualizer.RenderCompact(config)
                : _visualizer.Render(config);
            return Task.FromResult(output);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pipeline visualization failed");
            return Task.FromResult(string.Empty);
        }
    }
}

/// <summary>
/// Factory for creating format-specific loaders and exporters.
/// </summary>
public static class FormatFactory
{
    public static IDataLoader CreateLoader(string format)
    {
        return format.ToLowerInvariant() switch
        {
            "json" => new JsonDataLoader(),
            "csv" => new CsvDataLoader(),
            _ => throw new InvalidOperationException($"Unsupported format: {format}")
        };
    }

    public static IDataExporter CreateExporter(string format)
    {
        return format.ToLowerInvariant() switch
        {
            "json" => new JsonDataExporter(),
            "csv" => new CsvDataExporter(),
            "xml" => new XmlDataExporter(),
            _ => throw new InvalidOperationException($"Unsupported format: {format}")
        };
    }
}

public interface IDataLoader
{
    Task<List<DataPoint>> LoadAsync(string filePath);
}

public interface IDataExporter
{
    Task ExportAsync(List<DataPoint> data, string outputPath);
}

public sealed class JsonDataLoader : IDataLoader
{
    /// <summary>
    /// Loads data points from a UTF-8 JSON array file.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is null or blank.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    public async Task<List<DataPoint>> LoadAsync(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
            throw new FileNotFoundException("Input file not found", filePath);

        await using var stream = File.OpenRead(filePath);
        var points = await JsonSerializer.DeserializeAsync<List<DataPoint>>(stream, DataFormats.JsonOptions);
        return points ?? [];
    }
}

public sealed class JsonDataExporter : IDataExporter
{
    /// <summary>
    /// Writes the supplied data points to <paramref name="outputPath"/> as an indented JSON array.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="outputPath"/> is null or blank.</exception>
    public async Task ExportAsync(List<DataPoint> data, string outputPath)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        DataFormats.EnsureDirectory(outputPath);

        await using var stream = File.Create(outputPath);
        await JsonSerializer.SerializeAsync(stream, data, DataFormats.JsonOptions);
    }
}

public sealed class CsvDataLoader : IDataLoader
{
    /// <summary>
    /// Loads data points from a CSV file with the header
    /// <c>Id,Timestamp,Value,Source,Quality,Tags</c>.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is null or blank.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    public async Task<List<DataPoint>> LoadAsync(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
            throw new FileNotFoundException("Input file not found", filePath);

        var points = new List<DataPoint>();
        var isHeader = true;

        using var reader = new StreamReader(filePath, Encoding.UTF8);
        while (await reader.ReadLineAsync() is { } line)
        {
            if (isHeader)
            {
                isHeader = false;
                continue;
            }

            if (string.IsNullOrWhiteSpace(line))
                continue;

            var fields = DataFormats.SplitCsvLine(line);
            if (fields.Count < 4)
                continue;

            if (!long.TryParse(fields[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) ||
                !long.TryParse(fields[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var timestamp) ||
                !double.TryParse(fields[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                continue;
            }

            var point = new DataPoint(id, timestamp, value, fields[3]);

            if (fields.Count > 4 &&
                int.TryParse(fields[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out var quality))
            {
                point.Quality = quality;
            }

            if (fields.Count > 5 && !string.IsNullOrEmpty(fields[5]))
                point.Tags = fields[5];

            points.Add(point);
        }

        return points;
    }
}

public sealed class CsvDataExporter : IDataExporter
{
    /// <summary>
    /// Writes the supplied data points to <paramref name="outputPath"/> as CSV.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="outputPath"/> is null or blank.</exception>
    public async Task ExportAsync(List<DataPoint> data, string outputPath)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        DataFormats.EnsureDirectory(outputPath);

        var sb = new StringBuilder();
        sb.AppendLine("Id,Timestamp,Value,Source,Quality,Tags");

        foreach (var point in data)
        {
            sb.Append(point.Id.ToString(CultureInfo.InvariantCulture)).Append(',')
              .Append(point.Timestamp.ToString(CultureInfo.InvariantCulture)).Append(',')
              .Append(point.Value.ToString("R", CultureInfo.InvariantCulture)).Append(',')
              .Append(DataFormats.EscapeCsv(point.Source)).Append(',')
              .Append(point.Quality.ToString(CultureInfo.InvariantCulture)).Append(',')
              .AppendLine(DataFormats.EscapeCsv(point.Tags));
        }

        await File.WriteAllTextAsync(outputPath, sb.ToString(), Encoding.UTF8);
    }
}

public sealed class XmlDataExporter : IDataExporter
{
    /// <summary>
    /// Writes the supplied data points to <paramref name="outputPath"/> as an XML document.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="outputPath"/> is null or blank.</exception>
    public async Task ExportAsync(List<DataPoint> data, string outputPath)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        DataFormats.EnsureDirectory(outputPath);

        var settings = new XmlWriterSettings { Indent = true, Async = true, Encoding = new UTF8Encoding(false) };

        await using var stream = File.Create(outputPath);
        await using var writer = XmlWriter.Create(stream, settings);

        await writer.WriteStartDocumentAsync();
        await writer.WriteStartElementAsync(null, "dataPoints", null);

        foreach (var point in data)
        {
            await writer.WriteStartElementAsync(null, "dataPoint", null);
            await writer.WriteElementStringAsync(null, "id", null, point.Id.ToString(CultureInfo.InvariantCulture));
            await writer.WriteElementStringAsync(null, "timestamp", null, point.Timestamp.ToString(CultureInfo.InvariantCulture));
            await writer.WriteElementStringAsync(null, "value", null, point.Value.ToString("R", CultureInfo.InvariantCulture));
            await writer.WriteElementStringAsync(null, "source", null, point.Source);
            await writer.WriteElementStringAsync(null, "quality", null, point.Quality.ToString(CultureInfo.InvariantCulture));
            await writer.WriteElementStringAsync(null, "tags", null, point.Tags ?? string.Empty);
            await writer.WriteEndElementAsync();
        }

        await writer.WriteEndElementAsync();
        await writer.WriteEndDocumentAsync();
        await writer.FlushAsync();
    }
}

/// <summary>
/// Shared serialization primitives used by the CLI loaders and exporters.
/// </summary>
internal static class DataFormats
{
    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    /// <summary>Creates the parent directory of <paramref name="path"/> when it does not exist.</summary>
    internal static void EnsureDirectory(string path)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
    }

    /// <summary>Quotes a CSV field when it contains a separator, quote or line break.</summary>
    internal static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value.AsSpan().IndexOfAny(",\"\n\r") >= 0
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;
    }

    /// <summary>Splits a single CSV line honouring double-quoted fields and escaped quotes.</summary>
    internal static List<string> SplitCsvLine(string line)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    current.Append(c);
                }
            }
            else if (c == '"')
            {
                inQuotes = true;
            }
            else if (c == ',')
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else if (c != '\r')
            {
                current.Append(c);
            }
        }

        fields.Add(current.ToString());
        return fields;
    }
}
