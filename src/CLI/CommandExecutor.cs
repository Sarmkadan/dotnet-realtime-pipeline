// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.CLI;

using DotNetRealtimePipeline.Services;
using DotNetRealtimePipeline.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Executes parsed commands with context-aware handling.
/// Bridges CLI layer with service layer, managing dependencies and error handling.
/// </summary>
public class CommandExecutor
{
    private readonly PipelineOrchestrator _orchestrator;
    private readonly ILogger<CommandExecutor> _logger;

    public CommandExecutor(PipelineOrchestrator orchestrator, ILogger<CommandExecutor> logger)
    {
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes a parsed command with error handling and logging.
    /// </summary>
    public async Task<int> ExecuteAsync(ParsedCommand command)
    {
        if (command == null)
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
                ["pipeline_name"] = status.PipelineName,
                ["version"] = status.Version,
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

public class JsonDataLoader : IDataLoader
{
    public async Task<List<DataPoint>> LoadAsync(string filePath)
    {
        // Placeholder: would use System.Text.Json in production
        await Task.Delay(10);
        return new List<DataPoint>();
    }
}

public class JsonDataExporter : IDataExporter
{
    public async Task ExportAsync(List<DataPoint> data, string outputPath)
    {
        // Placeholder: would serialize to JSON in production
        await Task.Delay(10);
    }
}

public class CsvDataLoader : IDataLoader
{
    public async Task<List<DataPoint>> LoadAsync(string filePath)
    {
        await Task.Delay(10);
        return new List<DataPoint>();
    }
}

public class CsvDataExporter : IDataExporter
{
    public async Task ExportAsync(List<DataPoint> data, string outputPath)
    {
        await Task.Delay(10);
    }
}

public class XmlDataExporter : IDataExporter
{
    public async Task ExportAsync(List<DataPoint> data, string outputPath)
    {
        await Task.Delay(10);
    }
}
