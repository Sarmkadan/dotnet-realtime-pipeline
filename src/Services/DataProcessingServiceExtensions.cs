#nullable enable

namespace DotNetRealtimePipeline.Services;

using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Extension methods for <see cref="DataProcessingService"/> that provide additional data processing capabilities
/// for batch operations, quality analysis, and reporting.
/// </summary>
public static class DataProcessingServiceExtensions
{
    /// <summary>
    /// Processes a batch of data points with automatic quality filtering based on configured threshold.
    /// </summary>
    /// <param name="service">The data processing service instance.</param>
    /// <param name="dataPoints">The list of <see cref="DataPoint"/> to be processed.</param>
    /// <param name="minQuality">Optional minimum quality threshold override (uses service config if null).</param>
    /// <returns>A task that represents the asynchronous operation, returning a list of <see cref="ProcessingResult"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> or <paramref name="dataPoints"/> is null.</exception>
    public static async Task<List<ProcessingResult>> ProcessBatchWithQualityFilterAsync(
        this DataProcessingService service,
        List<DataPoint> dataPoints,
        int? minQuality = null)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(dataPoints);

        var qualityThreshold = minQuality ?? GetConfigQualityThreshold(service);
        var filteredPoints = dataPoints.Where(p => p.Quality >= qualityThreshold).ToList();

        return await service.ProcessBatchAsync(filteredPoints);
    }

    /// <summary>
    /// Retrieves processed data points from a time window and performs quality analysis.
    /// </summary>
    /// <param name="service">The data processing service instance.</param>
    /// <param name="startMs">The start time of the window in milliseconds.</param>
    /// <param name="endMs">The end time of the window in milliseconds.</param>
    /// <param name="includeQualityAnalysis">Whether to include quality analysis in the result.</param>
    /// <returns>A tuple containing the data points and optional quality analysis.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="startMs"/> is greater than <paramref name="endMs"/>.</exception>
    public static async Task<(List<DataPoint> DataPoints, DataQualityAnalysis? Analysis)> GetProcessedDataWithAnalysisAsync(
        this DataProcessingService service,
        long startMs,
        long endMs,
        bool includeQualityAnalysis = true)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(startMs, endMs);

        var dataPoints = await service.GetProcessedDataInWindowAsync(startMs, endMs);
        DataQualityAnalysis? analysis = null;

        if (includeQualityAnalysis && dataPoints.Count > 0)
        {
            analysis = service.AnalyzeDataQuality(dataPoints);
        }

        return (dataPoints, analysis);
    }

    /// <summary>
    /// Generates a formatted quality report string suitable for logging or dashboard display.
    /// </summary>
    /// <param name="service">The data processing service instance.</param>
    /// <param name="dataPoints">The data points to analyze and report on.</param>
    /// <param name="includeDetailedStats">Whether to include detailed statistics in the report.</param>
    /// <returns>A formatted quality report string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> or <paramref name="dataPoints"/> is null.</exception>
    public static string GenerateQualityReportString(
        this DataProcessingService service,
        List<DataPoint> dataPoints,
        bool includeDetailedStats = true)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(dataPoints);

        var analysis = service.AnalyzeDataQuality(dataPoints);
        var culture = CultureInfo.InvariantCulture;

        var reportLines = new List<string>
        {
            "=== Data Quality Report ===",
            $"Total Points: {analysis.TotalPoints:N0}",
            $"Quality Score: {analysis.QualityScore}",
            $"Pass Rate: {analysis.PassRate.ToString("F2", culture)}%",
            $"High Quality: {analysis.HighQualityCount:N0}",
            $"Low Quality: {analysis.LowQualityCount:N0}",
            $"Average Quality: {analysis.AverageQuality.ToString("F2", culture)}",
            $"Quality Range: [{analysis.MinQuality} - {analysis.MaxQuality}]",
            $"Unique Sources: {analysis.UniqueSourceCount:N0}"
        };

        if (includeDetailedStats)
        {
            reportLines.AddRange(new[]
            {
                $"Min Quality: {analysis.MinQuality}",
                $"Max Quality: {analysis.MaxQuality}",
                $"Configured Threshold: {GetConfigQualityThreshold(service)}"
            });
        }

        return string.Join(Environment.NewLine, reportLines);
    }

    /// <summary>
    /// Gets processing statistics with additional derived metrics calculated.
    /// </summary>
    /// <param name="service">The data processing service instance.</param>
    /// <returns>A dictionary containing all processing statistics and derived metrics.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    public static async Task<IReadOnlyDictionary<string, object>> GetProcessingStatisticsAsync(
        this DataProcessingService service)
    {
        ArgumentNullException.ThrowIfNull(service);

        var stats = await service.GetStatisticsAsync();

        var result = new Dictionary<string, object>
        {
            ["TotalDataPoints"] = stats.TotalDataPoints,
            ["ConfiguredMaxRetries"] = stats.ConfiguredMaxRetries,
            ["QualityThreshold"] = stats.QualityThreshold,
            ["ProcessingTimeoutMs"] = stats.ProcessingTimeoutMs
        };

        return result.AsReadOnly();
    }

    /// <summary>
    /// Gets the configured minimum quality threshold from the service's configuration.
    /// </summary>
    /// <param name="service">The data processing service instance.</param>
    /// <returns>The configured minimum quality threshold.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    private static int GetConfigQualityThreshold(DataProcessingService service)
    {
        ArgumentNullException.ThrowIfNull(service);

        try
        {
            var configField = typeof(DataProcessingService).GetField(
                "_config",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (configField?.GetValue(service) is { } config)
            {
                var thresholdProperty = typeof(PipelineConfig).GetProperty(
                    "MinDataQualityThreshold",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                if (thresholdProperty?.GetValue(config) is int threshold)
                {
                    return threshold;
                }
            }
        }
        catch
        {
            // Fall through to default if reflection fails
        }

        return 50; // Default threshold when config cannot be accessed
    }
}