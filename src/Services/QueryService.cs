// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Services;

using DotNetRealtimePipeline.Data.Repositories;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Service for querying and analyzing processed data in the pipeline.
/// Provides convenient methods for data retrieval and analysis.
/// </summary>
public class QueryService
{
    private readonly IDataPointRepository _dataPointRepository;
    private readonly IMetricsRepository _metricsRepository;

    public QueryService(
        IDataPointRepository dataPointRepository,
        IMetricsRepository metricsRepository)
    {
        _dataPointRepository = dataPointRepository ?? throw new ArgumentNullException(nameof(dataPointRepository));
        _metricsRepository = metricsRepository ?? throw new ArgumentNullException(nameof(metricsRepository));
    }

    /// <summary>
    /// Searches for data points by multiple criteria.
    /// </summary>
    public async Task<List<DataPoint>> SearchDataPointsAsync(
        long? startTime = null,
        long? endTime = null,
        string? source = null,
        int? minQuality = null)
    {
        var results = new List<DataPoint>();

        // Get data by source if specified
        if (!string.IsNullOrWhiteSpace(source))
        {
            results = await _dataPointRepository.GetBySourceAsync(source);
        }
        else if (startTime.HasValue && endTime.HasValue)
        {
            // Get data by time range
            results = await _dataPointRepository.GetByTimeRangeAsync(startTime.Value, endTime.Value);
        }
        else if (minQuality.HasValue)
        {
            // Get data by quality threshold
            results = await _dataPointRepository.GetByQualityThresholdAsync(minQuality.Value);
        }
        else
        {
            // Get all data (paginated)
            results = await _dataPointRepository.GetPagedAsync(1, 1000);
        }

        // Apply additional filters
        if (minQuality.HasValue)
        {
            results = results.Where(dp => dp.Quality >= minQuality.Value).ToList();
        }

        if (startTime.HasValue && endTime.HasValue && string.IsNullOrWhiteSpace(source))
        {
            results = results
                .Where(dp => dp.Timestamp >= startTime.Value && dp.Timestamp <= endTime.Value)
                .ToList();
        }

        return results;
    }

    /// <summary>
    /// Gets aggregated statistics for data points in a time range.
    /// </summary>
    public async Task<DataAggregateStatistics> GetAggregateStatisticsAsync(
        long startMs,
        long endMs)
    {
        var dataPoints = await _dataPointRepository.GetByTimeRangeAsync(startMs, endMs);

        if (dataPoints.Count == 0)
        {
            return new DataAggregateStatistics
            {
                StartMs = startMs,
                EndMs = endMs,
                Count = 0
            };
        }

        var values = dataPoints.Select(dp => dp.Value).ToList();

        return new DataAggregateStatistics
        {
            StartMs = startMs,
            EndMs = endMs,
            Count = dataPoints.Count,
            Sum = values.Sum(),
            Average = values.Average(),
            Min = values.Min(),
            Max = values.Max(),
            StdDev = StatisticsHelper.CalculateStandardDeviation(values),
            Median = StatisticsHelper.CalculateMedian(values),
            P95 = StatisticsHelper.CalculatePercentile(values, 95),
            P99 = StatisticsHelper.CalculatePercentile(values, 99),
            UniqueSourceCount = dataPoints.Select(dp => dp.Source).Distinct().Count(),
            AverageQuality = dataPoints.Average(dp => dp.Quality)
        };
    }

    /// <summary>
    /// Analyzes trends in data over time.
    /// </summary>
    public async Task<TrendAnalysis> AnalyzeTrendsAsync(
        long startMs,
        long endMs,
        long intervalMs)
    {
        var dataPoints = await _dataPointRepository.GetByTimeRangeAsync(startMs, endMs);

        if (dataPoints.Count < 2)
        {
            return new TrendAnalysis { Status = "INSUFFICIENT_DATA" };
        }

        // Group data by intervals
        var intervals = new Dictionary<long, List<DataPoint>>();
        foreach (var dp in dataPoints)
        {
            long intervalKey = (dp.Timestamp / intervalMs) * intervalMs;
            if (!intervals.ContainsKey(intervalKey))
            {
                intervals[intervalKey] = new();
            }
            intervals[intervalKey].Add(dp);
        }

        // Calculate average for each interval
        var intervalAverages = intervals
            .OrderBy(kvp => kvp.Key)
            .Select(kvp => kvp.Value.Average(dp => dp.Value))
            .ToList();

        if (intervalAverages.Count < 2)
        {
            return new TrendAnalysis { Status = "INSUFFICIENT_INTERVALS" };
        }

        // Calculate trend direction
        double firstHalfAvg = intervalAverages.Take(intervalAverages.Count / 2).Average();
        double secondHalfAvg = intervalAverages.Skip(intervalAverages.Count / 2).Average();
        double changePercent = ((secondHalfAvg - firstHalfAvg) / firstHalfAvg) * 100;

        string direction;
        if (Math.Abs(changePercent) < 1)
            direction = "STABLE";
        else if (changePercent > 0)
            direction = "INCREASING";
        else
            direction = "DECREASING";

        return new TrendAnalysis
        {
            Status = "SUCCESS",
            Direction = direction,
            ChangePercent = changePercent,
            IntervalCount = intervals.Count,
            TimeSpanMs = endMs - startMs,
            Volatility = StatisticsHelper.CalculateStandardDeviation(intervalAverages)
        };
    }

    /// <summary>
    /// Performs time series decomposition analysis.
    /// </summary>
    public async Task<TimeSeriesDecomposition> DecomposeTimeSeriesAsync(
        long startMs,
        long endMs,
        int movingAverageWindow = 5)
    {
        var dataPoints = await _dataPointRepository.GetByTimeRangeAsync(startMs, endMs);

        if (dataPoints.Count < movingAverageWindow)
        {
            return new TimeSeriesDecomposition { Status = "INSUFFICIENT_DATA" };
        }

        var values = dataPoints.OrderBy(dp => dp.Timestamp).Select(dp => dp.Value).ToList();
        var trend = StatisticsHelper.CalculateMovingAverage(values, movingAverageWindow);

        // Calculate deviations (seasonal/residual)
        var deviations = new List<double>();
        for (int i = 0; i < trend.Count; i++)
        {
            deviations.Add(values[i + (movingAverageWindow / 2)] - trend[i]);
        }

        return new TimeSeriesDecomposition
        {
            Status = "SUCCESS",
            OriginalCount = values.Count,
            TrendPoints = trend.Count,
            SeasonalityStrength = CalculateSeasonalityStrength(deviations),
            TrendStrength = CalculateTrendStrength(values, trend)
        };
    }

    /// <summary>
    /// Gets recently processed metrics.
    /// </summary>
    public async Task<List<MetricAggregation>> GetRecentMetricsAsync(int count = 10)
    {
        return await _metricsRepository.GetHistoryAsync(count);
    }

    /// <summary>
    /// Gets the total count of data points in the repository.
    /// </summary>
    public async Task<long> GetDataPointCountAsync()
    {
        return await _dataPointRepository.CountAsync();
    }

    // Private helper methods

    private double CalculateSeasonalityStrength(List<double> deviations)
    {
        if (deviations.Count == 0) return 0;
        return Math.Abs(StatisticsHelper.CalculateStandardDeviation(deviations));
    }

    private double CalculateTrendStrength(List<double> original, List<double> trend)
    {
        if (trend.Count == 0) return 0;

        var residuals = new List<double>();
        for (int i = 0; i < trend.Count; i++)
        {
            residuals.Add(original[i + (original.Count - trend.Count) / 2] - trend[i]);
        }

        double residualVariance = StatisticsHelper.CalculateStandardDeviation(residuals);
        double originalVariance = StatisticsHelper.CalculateStandardDeviation(original);

        if (originalVariance == 0) return 0;
        return (1 - (residualVariance / originalVariance)) * 100;
    }
}

/// <summary>
/// Aggregated statistics for a data range.
/// </summary>
public class DataAggregateStatistics
{
    public long StartMs { get; set; }
    public long EndMs { get; set; }
    public int Count { get; set; }
    public double Sum { get; set; }
    public double Average { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public double StdDev { get; set; }
    public double Median { get; set; }
    public double P95 { get; set; }
    public double P99 { get; set; }
    public int UniqueSourceCount { get; set; }
    public double AverageQuality { get; set; }
}

/// <summary>
/// Trend analysis results.
/// </summary>
public class TrendAnalysis
{
    public string Status { get; set; } = "UNKNOWN";
    public string Direction { get; set; } = "";
    public double ChangePercent { get; set; }
    public int IntervalCount { get; set; }
    public long TimeSpanMs { get; set; }
    public double Volatility { get; set; }
}

/// <summary>
/// Time series decomposition results.
/// </summary>
public class TimeSeriesDecomposition
{
    public string Status { get; set; } = "UNKNOWN";
    public int OriginalCount { get; set; }
    public int TrendPoints { get; set; }
    public double SeasonalityStrength { get; set; }
    public double TrendStrength { get; set; }
}
