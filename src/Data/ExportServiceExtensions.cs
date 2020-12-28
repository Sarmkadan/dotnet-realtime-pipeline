#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotNetRealtimePipeline.Data;

using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Formatters;
using DotNetRealtimePipeline.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Extension methods for <see cref="ExportService"/> that provide additional functionality
/// for exporting, validating, and managing export operations.
/// </summary>
public static class ExportServiceExtensions
{
    /// <summary>
    /// Validates that the export service can write to the specified output directory.
    /// Creates the directory if it doesn't exist.
    /// </summary>
    /// <param name="exportService">The export service instance.</param>
    /// <param name="outputPath">The output file path.</param>
    /// <returns>True if the directory is accessible; otherwise false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exportService"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="outputPath"/> is null or empty.</exception>
    public static bool ValidateOutputDirectory(this ExportService exportService, string outputPath)
    {
        ArgumentNullException.ThrowIfNull(exportService);
        ArgumentException.ThrowIfNullOrEmpty(outputPath);

        try
        {
            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Test write access
            var testFile = Path.Combine(directory ?? string.Empty, $"test_{Guid.NewGuid()}.tmp");
            File.WriteAllText(testFile, "test");
            File.Delete(testFile);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Exports data points with automatic retry logic for transient failures.
    /// </summary>
    /// <param name="exportService">The export service instance.</param>
    /// <param name="dataPoints">The data points to export.</param>
    /// <param name="outputPath">The output file path.</param>
    /// <param name="format">The output format.</param>
    /// <param name="maxRetries">Maximum number of retry attempts (default: 3).</param>
    /// <returns>The export result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exportService"/> or <paramref name="dataPoints"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="outputPath"/> is null or empty.</exception>
    public static async Task<ExportResult> ExportWithRetryAsync(
        this ExportService exportService,
        List<DataPoint> dataPoints,
        string outputPath,
        OutputFormat format,
        int maxRetries = 3)
    {
        ArgumentNullException.ThrowIfNull(exportService);
        ArgumentNullException.ThrowIfNull(dataPoints);
        ArgumentException.ThrowIfNullOrEmpty(outputPath);

        ExportResult result = null!;
        int retryCount = 0;
        Exception? lastException = null;

        while (retryCount <= maxRetries)
        {
            try
            {
                result = await exportService.ExportDataPointsAsync(dataPoints, outputPath, format);

                if (result.Success)
                {
                    return result;
                }

                lastException = new InvalidOperationException(result.ErrorMessage ?? "Unknown export error");
            }
            catch (Exception ex) when (retryCount < maxRetries)
            {
                lastException = ex;
                retryCount++;

                // Wait with exponential backoff
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount)));
            }
        }

        throw new InvalidOperationException(
            $"Export failed after {maxRetries} retries. Last error: {lastException?.Message}",
            lastException);
    }

    /// <summary>
    /// Exports data points to a stream instead of a file.
    /// </summary>
    /// <param name="exportService">The export service instance.</param>
    /// <param name="dataPoints">The data points to export.</param>
    /// <param name="stream">The output stream.</param>
    /// <param name="format">The output format.</param>
    /// <returns>The export result with stream information.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="exportService"/>,
    /// <paramref name="dataPoints"/>, or <paramref name="stream"/> is null.
    /// </exception>
    public static async Task<ExportResult> ExportToStreamAsync(
        this ExportService exportService,
        List<DataPoint> dataPoints,
        Stream stream,
        OutputFormat format)
    {
        ArgumentNullException.ThrowIfNull(exportService);
        ArgumentNullException.ThrowIfNull(dataPoints);
        ArgumentNullException.ThrowIfNull(stream);

        var result = new ExportResult { StartTime = DateTime.UtcNow };

        try
        {
            var formatter = OutputFormatterFactory.Create(format);
            var content = await formatter.FormatAsync(dataPoints);

            using var writer = new StreamWriter(stream, leaveOpen: true);
            await writer.WriteAsync(content);
            await writer.FlushAsync();

            result.Success = true;
            result.RecordCount = dataPoints.Count;
            result.FileSizeBytes = content.Length;
            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            throw;
        }
        finally
        {
            result.EndTime = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Gets the estimated file size in human-readable format before performing the export.
    /// </summary>
    /// <param name="exportService">The export service instance.</param>
    /// <param name="dataPoints">The data points to estimate size for.</param>
    /// <param name="format">The output format.</param>
    /// <returns>A formatted string with the estimated file size.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="exportService"/> or <paramref name="dataPoints"/> is null.
    /// </exception>
    public static string EstimateFileSize(
        this ExportService exportService,
        List<DataPoint> dataPoints,
        OutputFormat format)
    {
        ArgumentNullException.ThrowIfNull(exportService);
        ArgumentNullException.ThrowIfNull(dataPoints);

        var formatter = OutputFormatterFactory.Create(format);
        var sampleContent = formatter.FormatAsync(dataPoints).Result;
        var sizeBytes = sampleContent.Length;

        return PathHelper.FormatFileSize(sizeBytes);
    }

    /// <summary>
    /// Exports data points with additional metadata in the filename.
    /// </summary>
    /// <param name="exportService">The export service instance.</param>
    /// <param name="dataPoints">The data points to export.</param>
    /// <param name="baseOutputPath">The base output file path (without metadata).</param>
    /// <param name="format">The output format.</param>
    /// <param name="timestamp">Optional timestamp to include in filename.</param>
    /// <param name="includeRecordCount">Whether to include record count in filename.</param>
    /// <returns>The export result.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="exportService"/> or <paramref name="dataPoints"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException"><paramref name="baseOutputPath"/> is null or empty.</exception>
    public static async Task<ExportResult> ExportWithMetadataAsync(
        this ExportService exportService,
        List<DataPoint> dataPoints,
        string baseOutputPath,
        OutputFormat format,
        DateTime? timestamp = null,
        bool includeRecordCount = true)
    {
        ArgumentNullException.ThrowIfNull(exportService);
        ArgumentNullException.ThrowIfNull(dataPoints);
        ArgumentException.ThrowIfNullOrEmpty(baseOutputPath);

        var timestampValue = timestamp ?? DateTime.UtcNow;
        var recordCountSuffix = includeRecordCount ? $"_{dataPoints.Count}" : string.Empty;
        var extension = GetFileExtension(format);
        var filename = $"export_{timestampValue:yyyyMMdd_HHmmss}{recordCountSuffix}.{extension}";
        var outputPath = Path.Combine(Path.GetDirectoryName(baseOutputPath) ?? string.Empty, filename);

        return await exportService.ExportDataPointsAsync(dataPoints, outputPath, format);
    }

    private static string GetFileExtension(OutputFormat format) => format switch
    {
        OutputFormat.Json => "json",
        OutputFormat.Csv => "csv",
        OutputFormat.Html => "html",
        OutputFormat.Table => "txt",
        _ => "txt"
    };
}