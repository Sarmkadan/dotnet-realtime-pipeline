#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Services;

using DotNetRealtimePipeline.Constants;
using DotNetRealtimePipeline.Data.Repositories;
using DotNetRealtimePipeline.Domain.Exceptions;
using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Core service for processing data points through the pipeline.
/// Handles validation, transformation, and routing.
/// </summary>
public sealed class DataProcessingService
{
    private readonly IDataPointRepository _repository;
    private readonly PipelineConfig _config;
    private int _nextResultId = 1;

    public DataProcessingService(IDataPointRepository repository, PipelineConfig config)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Processes a single data point through the pipeline stages.
    /// </summary>
    /// <param name="dataPoint">The <see cref="DataPoint"/> to be processed.</param>
    /// <returns>A task that represents the asynchronous operation, returning a <see cref="ProcessingResult"/>.</returns>
    public async Task<ProcessingResult> ProcessDataPointAsync(DataPoint dataPoint)
    {
        if (dataPoint is null) throw new ArgumentNullException(nameof(dataPoint));

        var stopwatch = Stopwatch.StartNew();
        var result = new ProcessingResult(
            _nextResultId++,
            false,
            PipelineConstants.StageName_Ingestion
        );

        try
        {
            // Stage 1: Validation
            if (_config.ValidateOnIngestion && !dataPoint.Validate())
            {
                throw new InvalidDataPointException("Data point failed validation", dataPoint);
            }

            // Stage 2: Quality check
            if (dataPoint.Quality < _config.MinDataQualityThreshold)
            {
                result.AddOutput("QualityCheck", "FAILED");
                result.MarkFailure("Data point quality below threshold");
                return result;
            }

            // Stage 3: Persistence
            var savedPoint = await _repository.CreateAsync(dataPoint);
            result.AddOutput("DataPointId", savedPoint.Id);
            result.AddOutput("Source", savedPoint.Source);
            result.AddOutput("Timestamp", savedPoint.Timestamp);

            // Stage 4: Mark as successful
            result.MarkSuccess();
            result.StageName = PipelineConstants.StageName_Output;
        }
        catch (InvalidDataPointException ex)
        {
            result.MarkFailure(ex.Message, ex);
            result.ErrorMessage = $"Validation failed: {ex.Message}";
        }
        catch (Exception ex)
        {
            result.MarkFailure($"Unexpected error: {ex.Message}", ex);
        }
        finally
        {
            stopwatch.Stop();
            result.ProcessingTimeMs = stopwatch.ElapsedMilliseconds;
        }

        return result;
    }

    /// <summary>
    /// Processes a batch of data points, applying retry logic on failure.
    /// </summary>
    /// <param name="dataPoints">The list of <see cref="DataPoint"/> to be processed.</param>
    /// <returns>A task that represents the asynchronous operation, returning a list of <see cref="ProcessingResult"/>.</returns>
    public async Task<List<ProcessingResult>> ProcessBatchAsync(List<DataPoint> dataPoints)
    {
        if (dataPoints is null || dataPoints.Count == 0)
            return new();

        var results = new List<ProcessingResult>();

        foreach (var dataPoint in dataPoints)
        {
            ProcessingResult result = null;
            int retryCount = 0;

            while (retryCount <= _config.MaxRetries)
            {
                try
                {
                    result = await ProcessDataPointAsync(dataPoint);

                    if (result.Success) break;

                    retryCount++;
                    if (retryCount <= _config.MaxRetries)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(_config.RetryDelayMs * retryCount));
                        result.IncrementRetryCount();
                    }
                }
                catch (Exception ex)
                {
                    result ??= new ProcessingResult(_nextResultId++, false, PipelineConstants.StageName_Ingestion);
                    result.MarkFailure(ex.Message, ex);
                    result.IncrementRetryCount();

                    if (retryCount >= _config.MaxRetries) break;

                    retryCount++;
                    await Task.Delay(TimeSpan.FromMilliseconds(_config.RetryDelayMs * retryCount));
                }
            }

            if (result is not null)
            {
                results.Add(result);
            }
        }

        return results;
    }

    /// <summary>
    /// Retrieves and processes data points from a specific time window.
    /// </summary>
    /// <param name="startMs">The start time of the window in milliseconds.</param>
    /// <param name="endMs">The end time of the window in milliseconds.</param>
    /// <returns>A task that represents the asynchronous operation, returning a list of <see cref="DataPoint"/> in the time window.</returns>
    public async Task<List<DataPoint>> GetProcessedDataInWindowAsync(long startMs, long endMs)
    {
        if (startMs > endMs)
            throw new ArgumentException("Start time must be <= end time");

        return await _repository.GetByTimeRangeAsync(startMs, endMs);
    }

    /// <summary>
    /// Analyzes data quality statistics for a batch of points.
    /// </summary>
    /// <param name="dataPoints">The list of <see cref="DataPoint"/> to analyze.</param>
    /// <returns>A <see cref="DataQualityAnalysis"/> object containing the statistics.</returns>
    public DataQualityAnalysis AnalyzeDataQuality(List<DataPoint> dataPoints)
    {
        if (dataPoints is null || dataPoints.Count == 0)
            return new DataQualityAnalysis { TotalPoints = 0, QualityScore = 0 };

        var analysis = new DataQualityAnalysis
        {
            TotalPoints = dataPoints.Count,
            HighQualityCount = dataPoints.Count(p => p.Quality >= _config.MinDataQualityThreshold),
            LowQualityCount = dataPoints.Count(p => p.Quality < _config.MinDataQualityThreshold),
            AverageQuality = dataPoints.Average(p => p.Quality),
            MinQuality = dataPoints.Min(p => p.Quality),
            MaxQuality = dataPoints.Max(p => p.Quality),
            UniqueSourceCount = dataPoints.Select(p => p.Source).Distinct().Count()
        };

        analysis.QualityScore = (int)analysis.AverageQuality;
        analysis.PassRate = (analysis.HighQualityCount / (double)analysis.TotalPoints) * 100;

        return analysis;
    }

    /// <summary>
    /// Filters data points based on quality threshold.
    /// </summary>
    /// <param name="minQuality">The minimum quality score to filter by.</param>
    /// <returns>A task that represents the asynchronous operation, returning a list of <see cref="DataPoint"/>.</returns>
    public async Task<List<DataPoint>> FilterByQualityAsync(int minQuality)
    {
        return await _repository.GetByQualityThresholdAsync(minQuality);
    }

    /// <summary>
    /// Gets statistics about data processing.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation, returning a <see cref="DataProcessingStatistics"/> object.</returns>
    public async Task<DataProcessingStatistics> GetStatisticsAsync()
    {
        int totalCount = await _repository.CountAsync();
        var stats = new DataProcessingStatistics
        {
            TotalDataPoints = totalCount,
            ConfiguredMaxRetries = _config.MaxRetries,
            QualityThreshold = _config.MinDataQualityThreshold,
            ProcessingTimeoutMs = _config.ProcessingTimeoutMs
        };

        return stats;
    }
}

/// <summary>
/// Analysis results for data quality.
/// </summary>
public sealed class DataQualityAnalysis
{
    public int TotalPoints { get; set; }
    public int HighQualityCount { get; set; }
    public int LowQualityCount { get; set; }
    public double AverageQuality { get; set; }
    public int MinQuality { get; set; }
    public int MaxQuality { get; set; }
    public int UniqueSourceCount { get; set; }
    public int QualityScore { get; set; }
    public double PassRate { get; set; }
}

/// <summary>
/// Statistics about data processing.
/// </summary>
public sealed class DataProcessingStatistics
{
    public int TotalDataPoints { get; set; }
    public int ConfiguredMaxRetries { get; set; }
    public int QualityThreshold { get; set; }
    public long ProcessingTimeoutMs { get; set; }
}
