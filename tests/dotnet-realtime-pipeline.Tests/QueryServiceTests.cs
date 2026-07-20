#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Tests for QueryService to ensure data querying and analysis functionality works correctly
// =============================================================================

namespace DotNetRealtimePipeline.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetRealtimePipeline.Data.Repositories;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;
using DotNetRealtimePipeline.Utilities;
using FluentAssertions;
using Moq;
using Xunit;

public sealed class QueryServiceTests
{
    private readonly Mock<IDataPointRepository> _dataPointRepositoryMock = new();
    private readonly Mock<IMetricsRepository> _metricsRepositoryMock = new();
    private readonly QueryService _queryService;

    public QueryServiceTests()
    {
        _queryService = new QueryService(
            _dataPointRepositoryMock.Object,
            _metricsRepositoryMock.Object
        );
    }

    [Fact]
    public async Task SearchDataPointsAsync_WithSourceFilter_ReturnsFilteredDataPoints()
    {
        // Arrange
        var expectedDataPoints = new List<DataPoint>
        {
            new DataPoint(1, 1000, 42.5, "source1") { Quality = 95 },
            new DataPoint(2, 1001, 43.1, "source1") { Quality = 90 },
            new DataPoint(3, 1002, 41.8, "source2") { Quality = 85 }
        };

        _dataPointRepositoryMock
            .Setup(x => x.GetBySourceAsync("source1"))
            .ReturnsAsync(expectedDataPoints.Where(dp => dp.Source == "source1").ToList());

        // Act
        var result = await _queryService.SearchDataPointsAsync(source: "source1");

        // Assert
        result.Should().BeEquivalentTo(expectedDataPoints.Where(dp => dp.Source == "source1"));
        _dataPointRepositoryMock.Verify(x => x.GetBySourceAsync("source1"), Times.Once);
    }

    [Fact]
    public async Task SearchDataPointsAsync_WithTimeRangeFilter_ReturnsTimeFilteredDataPoints()
    {
        // Arrange
        var startTime = 1000L;
        var endTime = 2000L;
        var expectedDataPoints = new List<DataPoint>
        {
            new DataPoint(1, 1500, 42.5, "source1") { Quality = 95 },
            new DataPoint(2, 1600, 43.1, "source1") { Quality = 90 },
            new DataPoint(3, 2500, 41.8, "source2") { Quality = 85 }
        };

        _dataPointRepositoryMock
            .Setup(x => x.GetByTimeRangeAsync(startTime, endTime))
            .ReturnsAsync(expectedDataPoints.Where(dp => dp.Timestamp >= startTime && dp.Timestamp <= endTime).ToList());

        // Act
        var result = await _queryService.SearchDataPointsAsync(startTime: startTime, endTime: endTime);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(dp => dp.Timestamp.Should().BeInRange(startTime, endTime));
        _dataPointRepositoryMock.Verify(x => x.GetByTimeRangeAsync(startTime, endTime), Times.Once);
    }

    [Fact]
    public async Task SearchDataPointsAsync_WithMinQualityFilter_ReturnsQualityFilteredDataPoints()
    {
        // Arrange
        var minQuality = 90;
        var allDataPoints = new List<DataPoint>
        {
            new DataPoint(1, 1000, 42.5, "source1") { Quality = 95 },
            new DataPoint(2, 1001, 43.1, "source1") { Quality = 85 },
            new DataPoint(3, 1002, 41.8, "source2") { Quality = 90 }
        };

        _dataPointRepositoryMock
            .Setup(x => x.GetPagedAsync(1, 1000))
            .ReturnsAsync(allDataPoints);

        _dataPointRepositoryMock
            .Setup(x => x.GetByQualityThresholdAsync(minQuality))
            .ReturnsAsync(allDataPoints.Where(dp => dp.Quality >= minQuality).ToList());

        // Act
        var result = await _queryService.SearchDataPointsAsync(minQuality: minQuality);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(dp => dp.Quality.Should().BeGreaterThanOrEqualTo(minQuality));
        _dataPointRepositoryMock.Verify(x => x.GetByQualityThresholdAsync(minQuality), Times.Once);
    }

    [Fact]
    public async Task SearchDataPointsAsync_WithCombinedFilters_AppliesAllFilters()
    {
        // Arrange
        var startTime = 1000L;
        var endTime = 2000L;
        var minQuality = 90;
        var source = "source1";
        var allDataPoints = new List<DataPoint>
        {
            new DataPoint(1, 1500, 42.5, source) { Quality = 95 },
            new DataPoint(2, 1501, 43.1, source) { Quality = 85 },
            new DataPoint(3, 1600, 41.8, "source2") { Quality = 90 }
        };

        _dataPointRepositoryMock
            .Setup(x => x.GetBySourceAsync(source))
            .ReturnsAsync(allDataPoints.Where(dp => dp.Source == source).ToList());

        // Act
        var result = await _queryService.SearchDataPointsAsync(
            startTime: startTime,
            endTime: endTime,
            source: source,
            minQuality: minQuality
        );

        // Assert
        result.Should().HaveCount(1);
        var dataPoint = result[0];
        dataPoint.Source.Should().Be(source);
        dataPoint.Quality.Should().BeGreaterThanOrEqualTo(minQuality);
        dataPoint.Timestamp.Should().BeInRange(startTime, endTime);
    }

    [Fact]
    public async Task SearchDataPointsAsync_WithNoFilters_ReturnsAllDataPoints()
    {
        // Arrange
        var allDataPoints = new List<DataPoint>
        {
            new DataPoint(1, 1000, 42.5, "source1"),
            new DataPoint(2, 1001, 43.1, "source1"),
            new DataPoint(3, 1002, 41.8, "source2")
        };

        _dataPointRepositoryMock
            .Setup(x => x.GetPagedAsync(1, 1000))
            .ReturnsAsync(allDataPoints);

        // Act
        var result = await _queryService.SearchDataPointsAsync();

        // Assert
        result.Should().BeEquivalentTo(allDataPoints);
        _dataPointRepositoryMock.Verify(x => x.GetPagedAsync(1, 1000), Times.Once);
    }

    [Fact]
    public async Task SearchDataPointsAsync_WithEmptyRepository_ReturnsEmptyList()
    {
        // Arrange
        _dataPointRepositoryMock
            .Setup(x => x.GetPagedAsync(1, 1000))
            .ReturnsAsync(new List<DataPoint>());

        // Act
        var result = await _queryService.SearchDataPointsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAggregateStatisticsAsync_WithDataPoints_ReturnsCorrectStatistics()
    {
        // Arrange
        var startMs = 1000L;
        var endMs = 2000L;
        var dataPoints = new List<DataPoint>
        {
            new DataPoint(1, 1500, 10.0, "source1") { Quality = 95 },
            new DataPoint(2, 1600, 20.0, "source1") { Quality = 90 },
            new DataPoint(3, 1700, 30.0, "source2") { Quality = 85 },
            new DataPoint(4, 1800, 40.0, "source1") { Quality = 92 }
        };

        _dataPointRepositoryMock
            .Setup(x => x.GetByTimeRangeAsync(startMs, endMs))
            .ReturnsAsync(dataPoints);

        // Act
        var result = await _queryService.GetAggregateStatisticsAsync(startMs, endMs);

        // Assert
        result.Should().NotBeNull();
        result.StartMs.Should().Be(startMs);
        result.EndMs.Should().Be(endMs);
        result.Count.Should().Be(4);
        result.Sum.Should().Be(100.0);
        result.Average.Should().Be(25.0);
        result.Min.Should().Be(10.0);
        result.Max.Should().Be(40.0);
        result.StdDev.Should().BeApproximately(11.1803, 0.0001);
        result.Median.Should().Be(25.0);
        result.P95.Should().Be(40.0);
        result.P99.Should().Be(40.0);
        result.UniqueSourceCount.Should().Be(2);
        result.AverageQuality.Should().BeApproximately(90.5, 0.0001);
    }

    [Fact]
    public async Task GetAggregateStatisticsAsync_WithEmptyDataPoints_ReturnsEmptyStatistics()
    {
        // Arrange
        var startMs = 1000L;
        var endMs = 2000L;

        _dataPointRepositoryMock
            .Setup(x => x.GetByTimeRangeAsync(startMs, endMs))
            .ReturnsAsync(new List<DataPoint>());

        // Act
        var result = await _queryService.GetAggregateStatisticsAsync(startMs, endMs);

        // Assert
        result.Should().NotBeNull();
        result.StartMs.Should().Be(startMs);
        result.EndMs.Should().Be(endMs);
        result.Count.Should().Be(0);
        result.Sum.Should().Be(0);
        result.Average.Should().Be(0);
        result.Min.Should().Be(0);
        result.Max.Should().Be(0);
        result.StdDev.Should().Be(0);
        result.Median.Should().Be(0);
        result.P95.Should().Be(0);
        result.P99.Should().Be(0);
        result.UniqueSourceCount.Should().Be(0);
        result.AverageQuality.Should().Be(0);
    }

    [Fact]
    public async Task GetAggregateStatisticsAsync_WithSingleDataPoint_ReturnsCorrectStatistics()
    {
        // Arrange
        var startMs = 1000L;
        var endMs = 2000L;
        var dataPoint = new DataPoint(1, 1500, 42.5, "source1") { Quality = 95 };

        _dataPointRepositoryMock
            .Setup(x => x.GetByTimeRangeAsync(startMs, endMs))
            .ReturnsAsync(new List<DataPoint> { dataPoint });

        // Act
        var result = await _queryService.GetAggregateStatisticsAsync(startMs, endMs);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(1);
        result.Sum.Should().Be(42.5);
        result.Average.Should().Be(42.5);
        result.Min.Should().Be(42.5);
        result.Max.Should().Be(42.5);
        result.UniqueSourceCount.Should().Be(1);
        result.AverageQuality.Should().Be(95);
    }

    [Fact]
    public async Task AnalyzeTrendsAsync_WithSufficientData_ReturnsTrendAnalysis()
    {
        // Arrange
        var startMs = 1000L;
        var endMs = 5000L;
        var intervalMs = 1000L;
        var dataPoints = new List<DataPoint>
        {
            new DataPoint(1, 1000, 10.0, "source1"),
            new DataPoint(2, 2000, 20.0, "source1"),
            new DataPoint(3, 3000, 30.0, "source1"),
            new DataPoint(4, 4000, 40.0, "source1")
        };

        _dataPointRepositoryMock
            .Setup(x => x.GetByTimeRangeAsync(startMs, endMs))
            .ReturnsAsync(dataPoints);

        // Act
        var result = await _queryService.AnalyzeTrendsAsync(startMs, endMs, intervalMs);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("SUCCESS");
        result.Direction.Should().Be("INCREASING");
        result.ChangePercent.Should().BeApproximately(133.3333, 0.0001);
        result.IntervalCount.Should().Be(4);
        result.TimeSpanMs.Should().Be(4000);
        result.Volatility.Should().BeApproximately(11.1803, 0.0001);
    }

    [Fact]
    public async Task AnalyzeTrendsAsync_WithInsufficientData_ReturnsInsufficientDataStatus()
    {
        // Arrange
        var startMs = 1000L;
        var endMs = 2000L;
        var intervalMs = 1000L;
        var dataPoints = new List<DataPoint> { new DataPoint(1, 1500, 42.5, "source1") };

        _dataPointRepositoryMock
            .Setup(x => x.GetByTimeRangeAsync(startMs, endMs))
            .ReturnsAsync(dataPoints);

        // Act
        var result = await _queryService.AnalyzeTrendsAsync(startMs, endMs, intervalMs);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("INSUFFICIENT_DATA");
    }

    [Fact]
    public async Task AnalyzeTrendsAsync_WithTwoIntervals_ReturnsStableTrend()
    {
        // Arrange
        var startMs = 1000L;
        var endMs = 3000L;
        var intervalMs = 1000L;
        var dataPoints = new List<DataPoint>
        {
            new DataPoint(1, 1000, 10.0, "source1"),
            new DataPoint(2, 2000, 11.0, "source1"),
            new DataPoint(3, 3000, 10.5, "source1")
        };

        _dataPointRepositoryMock
            .Setup(x => x.GetByTimeRangeAsync(startMs, endMs))
            .ReturnsAsync(dataPoints);

        // Act
        var result = await _queryService.AnalyzeTrendsAsync(startMs, endMs, intervalMs);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("SUCCESS");
        result.Direction.Should().BeOneOf("STABLE", "INCREASING");
        result.ChangePercent.Should().BeLessThan(10.0);
    }

    [Fact]
    public async Task AnalyzeTrendsAsync_WithDecreasingData_ReturnsDecreasingTrend()
    {
        // Arrange
        var startMs = 1000L;
        var endMs = 4000L;
        var intervalMs = 1000L;
        var dataPoints = new List<DataPoint>
        {
            new DataPoint(1, 1000, 40.0, "source1"),
            new DataPoint(2, 2000, 30.0, "source1"),
            new DataPoint(3, 3000, 20.0, "source1"),
            new DataPoint(4, 4000, 10.0, "source1")
        };

        _dataPointRepositoryMock
            .Setup(x => x.GetByTimeRangeAsync(startMs, endMs))
            .ReturnsAsync(dataPoints);

        // Act
        var result = await _queryService.AnalyzeTrendsAsync(startMs, endMs, intervalMs);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("SUCCESS");
        result.Direction.Should().Be("DECREASING");
        result.ChangePercent.Should().BeNegative();
    }

    [Fact]
    public async Task DecomposeTimeSeriesAsync_WithSufficientData_ReturnsDecomposition()
    {
        // Arrange
        var startMs = 1000L;
        var endMs = 10000L;
        var dataPoints = new List<DataPoint>();

        // Create a time series with a clear trend
        for (int i = 0; i < 20; i++)
        {
            dataPoints.Add(new DataPoint(i + 1, (long)(1000 + i * 500), i * 5.0 + 10.0, "source1"));
        }

        _dataPointRepositoryMock
            .Setup(x => x.GetByTimeRangeAsync(startMs, endMs))
            .ReturnsAsync(dataPoints);

        // Act
        var result = await _queryService.DecomposeTimeSeriesAsync(startMs, endMs, movingAverageWindow: 3);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("SUCCESS");
        result.OriginalCount.Should().Be(20);
        result.TrendPoints.Should().BeGreaterThan(0);
        result.SeasonalityStrength.Should().BeGreaterThanOrEqualTo(0);
        result.TrendStrength.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task DecomposeTimeSeriesAsync_WithInsufficientData_ReturnsInsufficientDataStatus()
    {
        // Arrange
        var startMs = 1000L;
        var endMs = 2000L;
        var dataPoints = new List<DataPoint> { new DataPoint(1, 1500, 42.5, "source1") };

        _dataPointRepositoryMock
            .Setup(x => x.GetByTimeRangeAsync(startMs, endMs))
            .ReturnsAsync(dataPoints);

        // Act
        var result = await _queryService.DecomposeTimeSeriesAsync(startMs, endMs, movingAverageWindow: 5);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("INSUFFICIENT_DATA");
    }

    [Fact]
    public async Task GetRecentMetricsAsync_ReturnsMetricsInChronologicalOrder()
    {
        // Arrange
        var expectedMetrics = new List<MetricAggregation>
        {
            new MetricAggregation(1, 1000, 2000, "hourly")
            {
                TotalItemsProcessed = 100,
                TotalItemsFailed = 5,
                AverageProcessingTimeMs = 10.5
            },
            new MetricAggregation(2, 2000, 3000, "hourly")
            {
                TotalItemsProcessed = 150,
                TotalItemsFailed = 2,
                AverageProcessingTimeMs = 12.3
            }
        };

        _metricsRepositoryMock
            .Setup(x => x.GetHistoryAsync(10))
            .ReturnsAsync(expectedMetrics);

        // Act
        var result = await _queryService.GetRecentMetricsAsync(10);

        // Assert
        result.Should().BeEquivalentTo(expectedMetrics);
        _metricsRepositoryMock.Verify(x => x.GetHistoryAsync(10), Times.Once);
    }

    [Fact]
    public async Task GetRecentMetricsAsync_WithDefaultCount_ReturnsDefaultNumberOfMetrics()
    {
        // Arrange
        var expectedMetrics = new List<MetricAggregation>();
        for (int i = 0; i < 5; i++)
        {
            expectedMetrics.Add(new MetricAggregation(i + 1, 1000 + i * 1000, 2000 + i * 1000, "hourly"));
        }

        _metricsRepositoryMock
            .Setup(x => x.GetHistoryAsync(10))
            .ReturnsAsync(expectedMetrics);

        // Act
        var result = await _queryService.GetRecentMetricsAsync();

        // Assert
        result.Should().HaveCount(5);
        _metricsRepositoryMock.Verify(x => x.GetHistoryAsync(10), Times.Once);
    }

    [Fact]
    public async Task GetDataPointCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var expectedCount = 12345L;

        _dataPointRepositoryMock
            .Setup(x => x.CountAsync())
            .ReturnsAsync((int)expectedCount);

        // Act
        var result = await _queryService.GetDataPointCountAsync();

        // Assert
        result.Should().Be(expectedCount);
        _dataPointRepositoryMock.Verify(x => x.CountAsync(), Times.Once);
    }

    [Fact]
    public async Task SearchDataPointsAsync_WithInvalidInput_HandlesGracefully()
    {
        // Arrange
        _dataPointRepositoryMock
            .Setup(x => x.GetPagedAsync(1, 1000))
            .ReturnsAsync(new List<DataPoint>());

        // Act & Assert - should not throw exceptions
        Func<Task> act = async () => await _queryService.SearchDataPointsAsync(
            startTime: null,
            endTime: null,
            source: null,
            minQuality: null
        );

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetAggregateStatisticsAsync_WithInvalidTimeRange_HandlesGracefully()
    {
        // Arrange
        var startMs = 2000L;
        var endMs = 1000L; // Invalid range

        _dataPointRepositoryMock
            .Setup(x => x.GetByTimeRangeAsync(startMs, endMs))
            .ReturnsAsync(new List<DataPoint>());

        // Act & Assert - should not throw exceptions
        Func<Task> act = async () => await _queryService.GetAggregateStatisticsAsync(startMs, endMs);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task AnalyzeTrendsAsync_WithVerySmallInterval_ReturnsInsufficientIntervalsStatus()
    {
        // Arrange
        var startMs = 1000L;
        var endMs = 2000L;
        var intervalMs = 100L; // Very small interval
        var dataPoints = new List<DataPoint>
        {
            new DataPoint(1, 1500, 42.5, "source1"),
            new DataPoint(2, 1600, 43.1, "source1")
        };

        _dataPointRepositoryMock
            .Setup(x => x.GetByTimeRangeAsync(startMs, endMs))
            .ReturnsAsync(dataPoints);

        // Act
        var result = await _queryService.AnalyzeTrendsAsync(startMs, endMs, intervalMs);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().BeOneOf("INSUFFICIENT_INTERVALS", "SUCCESS");
    }
}