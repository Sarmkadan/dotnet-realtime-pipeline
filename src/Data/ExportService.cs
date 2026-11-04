#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Data;

using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Formatters;
using DotNetRealtimePipeline.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Service for exporting pipeline data in various formats.
/// Handles data serialization, file writing, and batch processing.
/// </summary>
public sealed class ExportService
{
    private readonly IOutputFormatter _formatter;
    private readonly ILogger<ExportService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExportService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for diagnostic output.</param>
    public ExportService(ILogger<ExportService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _formatter = new JsonOutputFormatter();
    }

    /// <summary>
    /// Exports data points to a file asynchronously.
    /// </summary>
    /// <param name="dataPoints">The list of data points to export.</param>
    /// <param name="outputPath">The path to the output file.</param>
    /// <param name="format">The output format to use.</param>
    /// <returns>A task that represents the asynchronous export operation.</returns>
    public async Task<ExportResult> ExportDataPointsAsync(
        List<DataPoint> dataPoints,
        string outputPath,
        OutputFormat format)
    {
        ArgumentNullException.ThrowIfNull(dataPoints);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        var result = new ExportResult { StartTime = DateTime.UtcNow };

        try
        {
            _logger.LogInformation("Exporting {Count} data points to {Path} ({Format})",
                dataPoints.Count, outputPath, format);

            // Ensure directory exists
            var directory = System.IO.Path.GetDirectoryName(outputPath);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            // Format the data
            var formatter = OutputFormatterFactory.Create(format);
            var content = await formatter.FormatAsync(dataPoints);

            // Write to file
            await System.IO.File.WriteAllTextAsync(outputPath, content);

            result.Success = true;
            result.OutputPath = outputPath;
            result.RecordCount = dataPoints.Count;
            result.FileSizeBytes = new System.IO.FileInfo(outputPath).Length;

            _logger.LogInformation("Export completed: {Records} records, {Size} bytes",
                result.RecordCount, result.FileSizeBytes);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Export failed");
            result.Success = false;
            result.ErrorMessage = ex.Message;
            return result;
        }
        finally
        {
            result.EndTime = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Exports processing results to a file.
    /// </summary>
    /// <param name="results">The list of processing results to export.</param>
    /// <param name="outputPath">The path to the output file.</param>
    /// <returns>A task that represents the asynchronous export operation.</returns>
    public async Task<ExportResult> ExportResultsAsync(
        List<ProcessingResult> results,
        string outputPath)
    {
        ArgumentNullException.ThrowIfNull(results);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        var result = new ExportResult { StartTime = DateTime.UtcNow };

        try
        {
            _logger.LogInformation("Exporting {Count} processing results to {Path}",
                results.Count, outputPath);

            var content = SerializationHelper.SerializeResults(results);
            await System.IO.File.WriteAllTextAsync(outputPath, content);

            result.Success = true;
            result.OutputPath = outputPath;
            result.RecordCount = results.Count;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Results export failed");
            result.Success = false;
            result.ErrorMessage = ex.Message;
            return result;
        }
        finally
        {
            result.EndTime = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Exports metrics to a file.
    /// </summary>
    /// <param name="metrics">The metrics to export.</param>
    /// <param name="outputPath">The path to the output file.</param>
    /// <returns>A task that represents the asynchronous export operation.</returns>
    public async Task<ExportResult> ExportMetricsAsync(
        MetricAggregation metrics,
        string outputPath)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        var result = new ExportResult { StartTime = DateTime.UtcNow };

        try
        {
            _logger.LogInformation("Exporting metrics to {Path}", outputPath);

            var content = SerializationHelper.SerializeMetrics(metrics);
            await System.IO.File.WriteAllTextAsync(outputPath, content);

            result.Success = true;
            result.OutputPath = outputPath;
            result.RecordCount = 1;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Metrics export failed");
            result.Success = false;
            result.ErrorMessage = ex.Message;
            return result;
        }
        finally
        {
            result.EndTime = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Exports data to multiple formats simultaneously.
    /// </summary>
    /// <param name="dataPoints">The list of data points to export.</param>
    /// <param name="outputDirectory">The directory to save the output files.</param>
    /// <param name="formats">The output formats to use.</param>
    /// <returns>A task that represents the asynchronous export operation.</returns>
    public async Task<List<ExportResult>> ExportMultiFormatAsync(
        List<DataPoint> dataPoints,
        string outputDirectory,
        params OutputFormat[] formats)
    {
        ArgumentNullException.ThrowIfNull(dataPoints);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        ArgumentNullException.ThrowIfNull(formats);

        var results = new List<ExportResult>();

        foreach (var format in formats)
        {
            var filename = $"export_{DateTime.Now:yyyyMMdd_HHmmss}.{GetFileExtension(format)}";
            var path = System.IO.Path.Combine(outputDirectory, filename);

            var result = await ExportDataPointsAsync(dataPoints, path, format);
            results.Add(result);
        }

        return results;
    }

    /// <summary>
    /// Gets the file extension for a format.
    /// </summary>
    /// <param name="format">The output format.</param>
    /// <returns>The file extension for the specified format.</returns>
    private static string GetFileExtension(OutputFormat format)
    {
        return format switch
        {
            OutputFormat.Json => "json",
            OutputFormat.Csv => "csv",
            OutputFormat.Html => "html",
            OutputFormat.Table => "txt",
            _ => "txt"
        };
    }
}

/// <summary>
/// Result of an export operation.
/// </summary>
public sealed class ExportResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the export operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the path to the output file.
    /// </summary>
    public string OutputPath { get; set; }

    /// <summary>
    /// Gets or sets the number of records exported.
    /// </summary>
    public int RecordCount { get; set; }

    /// <summary>
    /// Gets or sets the size of the output file in bytes.
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// Gets or sets the error message if the export operation failed.
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the start time of the export operation.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Gets or sets the end time of the export operation.
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Gets the duration of the export operation.
    /// </summary>
    public TimeSpan Duration => EndTime - StartTime;

    /// <summary>
    /// Returns a string representation of the export result.
    /// </summary>
    /// <returns>A string representation of the export result.</returns>
    public override string ToString()
    {
        if (Success)
        {
            return $"Export successful: {RecordCount} records, {PathHelper.FormatFileSize(FileSizeBytes)}, {Duration.TotalSeconds:F2}s";
        }

        return $"Export failed: {ErrorMessage}";
    }
}

/// <summary>
/// Batch export processor for large datasets.
/// </summary>
public sealed class BatchExportProcessor
{
    private readonly ExportService _exportService;
    private readonly ILogger<BatchExportProcessor> _logger;
    private const int DefaultBatchSize = 10000;

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchExportProcessor"/> class.
    /// </summary>
    /// <param name="exportService">The export service to use for exporting data.</param>
    /// <param name="logger">The logger instance for diagnostic output.</param>
    public BatchExportProcessor(ExportService exportService, ILogger<BatchExportProcessor> logger)
    {
        _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Exports large datasets in batches to manage memory.
    /// </summary>
    /// <param name="dataFetcher">The function to fetch data in batches.</param>
    /// <param name="totalRecords">The total number of records to export.</param>
    /// <param name="outputDirectory">The directory to save the output files.</param>
    /// <param name="format">The output format to use.</param>
    /// <param name="batchSize">The size of each batch.</param>
    /// <returns>A task that represents the asynchronous export operation.</returns>
    public async Task<BatchExportResult> ExportInBatchesAsync(
        Func<int, int, Task<List<DataPoint>>> dataFetcher,
        int totalRecords,
        string outputDirectory,
        OutputFormat format,
        int batchSize = DefaultBatchSize)
    {
        ArgumentNullException.ThrowIfNull(dataFetcher);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        ArgumentNullException.ThrowIfNull(format);

        var result = new BatchExportResult { StartTime = DateTime.UtcNow };

        try
        {
            _logger.LogInformation("Starting batch export: {Total} records in batches of {Size}",
                totalRecords, batchSize);

            var batchFiles = new List<string>();
            int offset = 0;

            while (offset < totalRecords)
            {
                var take = Math.Min(batchSize, totalRecords - offset);
                var batch = await dataFetcher(offset, take);

                if (batch is null || batch.Count == 0)
                    break;

                var filename = $"batch_{offset / batchSize:D4}.{GetFileExtension(format)}";
                var path = System.IO.Path.Combine(outputDirectory, filename);

                var exportResult = await _exportService.ExportDataPointsAsync(batch, path, format);

                if (exportResult.Success)
                {
                    batchFiles.Add(path);
                    result.ExportedRecords += exportResult.RecordCount;
                }

                offset += take;

                _logger.LogInformation("Batch export progress: {Exported}/{Total} records",
                    result.ExportedRecords, totalRecords);
            }

            result.Success = true;
            result.BatchFiles = batchFiles;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Batch export failed");
            result.Success = false;
            result.ErrorMessage = ex.Message;
            return result;
        }
        finally
        {
            result.EndTime = DateTime.UtcNow;
        }
    }

    private static string GetFileExtension(OutputFormat format)
    {
        return format switch
        {
            OutputFormat.Json => "json",
            OutputFormat.Csv => "csv",
            OutputFormat.Html => "html",
            OutputFormat.Table => "txt",
            _ => "txt"
        };
    }
}

/// <summary>
/// Result of a batch export operation.
/// </summary>
public sealed class BatchExportResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the batch export operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the number of records exported.
    /// </summary>
    public int ExportedRecords { get; set; }

    /// <summary>
    /// Gets or sets the list of batch files created.
    /// </summary>
    public List<string> BatchFiles { get; set; } = new();

    /// <summary>
    /// Gets or sets the error message if the batch export operation failed.
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the start time of the batch export operation.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Gets or sets the end time of the batch export operation.
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Gets the duration of the batch export operation.
    /// </summary>
    public TimeSpan Duration => EndTime - StartTime;

    /// <summary>
    /// Returns a string representation of the batch export result.
    /// </summary>
    /// <returns>A string representation of the batch export result.</returns>
    public override string ToString()
    {
        if (Success)
        {
            return $"Batch export successful: {ExportedRecords} records in {BatchFiles.Count} files, {Duration.TotalSeconds:F2}s";
        }

        return $"Batch export failed: {ErrorMessage}";
    }
}
