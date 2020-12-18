#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using DotNetRealtimePipeline.Data.Repositories;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace DotNetRealtimePipeline.Tests.Unit;

public sealed class QueryServiceTests
{
    private readonly Mock<IDataPointRepository> _mockDataPointRepository;
    private readonly Mock<IMetricsRepository> _mockMetricsRepository;
    private readonly QueryService _service;

    public QueryServiceTests()
    {
        _mockDataPointRepository = new Mock<IDataPointRepository>();
        _mockMetricsRepository = new Mock<IMetricsRepository>();
        _service = new QueryService(_mockDataPointRepository.Object, _mockMetricsRepository.Object);
    }

    [Fact]
    public void Constructor_WithNullDataPointRepository_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new QueryService(null!, _mockMetricsRepository.Object));
    }

    [Fact]
    public void Constructor_WithNullMetricsRepository_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new QueryService(_mockDataPointRepository.Object, null!));
    }

    [Fact]
    public async Task SearchDataPointsAsync_WithSourceSpecified_ShouldCallGetBySourceAsync()
    {
        // Arrange
        var expectedPoints = new List<DataPoint>
        {
            new DataPoint(1, DateTime.UtcNow.Ticks, 10.0, "Sensor1")
        };
        _mockDataPointRepository.Setup(r => r.GetBySourceAsync("Sensor1"))
            .ReturnsAsync(expectedPoints);

        // Act
        var result = await _service.SearchDataPointsAsync(source: "Sensor1");

        // Assert
        result.Should().BeEquivalentTo(expectedPoints);
        _mockDataPointRepository.Verify(r => r.GetBySourceAsync("Sensor1"), Times.Once);
    }

    [Fact]
    public async Task SearchDataPointsAsync_WithTimeRangeSpecified_ShouldCallGetByTimeRangeAsync()
    {
        // Arrange
        var expectedPoints = new List<DataPoint>
        {
            new DataPoint(1, 10000000, 10.0, "Sensor1")
        };
        _mockDataPointRepository.Setup(r => r.GetByTimeRangeAsync(10000000L, 20000000L))
            .ReturnsAsync(expectedPoints);

        // Act
        var result = await _service.SearchDataPointsAsync(startTime: 10000000L, endTime: 20000000L);

        // Assert
        result.Should().BeEquivalentTo(expectedPoints);
        _mockDataPointRepository.Verify(r => r.GetByTimeRangeAsync(10000000L, 20000000L), Times.Once);
    }

    [Fact]
    public async Task SearchDataPointsAsync_WithQualityThresholdSpecified_ShouldCallGetByQualityThresholdAsync()
    {
        // Arrange
        var expectedPoints = new List<DataPoint>
        {
            new DataPoint(1, DateTime.UtcNow.Ticks, 10.0, "Sensor1") { Quality = 80 }
        };
        _mockDataPointRepository.Setup(r => r.GetByQualityThresholdAsync(50))
            .ReturnsAsync(expectedPoints);

        // Act
        var result = await _service.SearchDataPointsAsync(minQuality: 50);

        // Assert
        result.Should().BeEquivalentTo(expectedPoints);
        _mockDataPointRepository.Verify(r => r.GetByQualityThresholdAsync(50), Times.Once);
    }

    [Fact]
    public async Task SearchDataPointsAsync_WithNoParameters_ShouldCallGetPagedAsync()
    {
        // Arrange
        var expectedPoints = new List<DataPoint>
        {
            new DataPoint(1, DateTime.UtcNow.Ticks, 10.0, "Sensor1")
        };
        _mockDataPointRepository.Setup(r => r.GetPagedAsync(1, 1000))
            .ReturnsAsync(expectedPoints);

        // Act
        var result = await _service.SearchDataPointsAsync();

        // Assert
        result.Should().BeEquivalentTo(expectedPoints);
        _mockDataPointRepository.Verify(r => r.GetPagedAsync(1, 1000), Times.Once);
    }

    [Fact]
    public async Task SearchDataPointsAsync_WithSourceAndQuality_ShouldApplyBothFilters()
    {
        // Arrange
        var highQualityPoint = new DataPoint(1, DateTime.UtcNow.Ticks, 10.0, "Sensor1") { Quality = 80 };
        var lowQualityPoint = new DataPoint(2, DateTime.UtcNow.Ticks, 20.0, "Sensor1") { Quality = 30 };
        var allPoints = new List<DataPoint> { highQualityPoint, lowQualityPoint };

        _mockDataPointRepository.Setup(r => r.GetBySourceAsync("Sensor1"))
            .ReturnsAsync(allPoints);

        // Act
        var result = await _service.SearchDataPointsAsync(source: "Sensor1", minQuality: 50);

        // Assert
        result.Should().ContainSingle();
        result.First().Should().BeEquivalentTo(highQualityPoint);
        result.First().Quality.Should().BeGreaterOrEqualTo(50);
    }

    [Fact]
    public async Task SearchDataPointsAsync_WithTimeRangeAndQuality_ShouldApplyBothFilters()
    {
        // Arrange
        var highQualityPoint = new DataPoint(1, 15000000, 10.0, "Sensor1") { Quality = 80 };
        var lowQualityPoint = new DataPoint(2, 15000000, 20.0, "Sensor2") { Quality = 30 };
        var allPoints = new List<DataPoint> { highQualityPoint, lowQualityPoint };

        _mockDataPointRepository.Setup(r => r.GetByTimeRangeAsync(10000000L, 20000000L))
            .ReturnsAsync(allPoints);

        // Act
        var result = await _service.SearchDataPointsAsync(startTime: 10000000L, endTime: 20000000L, minQuality: 50);

        // Assert
        result.Should().ContainSingle();
        result.First().Should().BeEquivalentTo(highQualityPoint);
        result.First().Timestamp.Should().BeInRange(10000000L, 20000000L);
        result.First().Quality.Should().BeGreaterOrEqualTo(50);
    }

    [Fact]
    public async Task GetAggregateStatisticsAsync_WithNoDataPoints_ShouldReturnEmptyStatistics()
    {
        // Arrange
        _mockDataPointRepository.Setup(r => r.GetByTimeRangeAsync(10000000L, 20000000L))
            .ReturnsAsync(new List<DataPoint>());

        // Act
        var result = await _service.GetAggregateStatisticsAsync(10000000L, 20000000L);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(0);
        result.StartMs.Should().Be(10000000L);
        result.EndMs.Should().Be(20000000L);
    }

    [Fact]
    public async Task GetAggregateStatisticsAsync_WithDataPoints_ShouldCalculateCorrectStatistics()
    {
        // Arrange
        var points = new List<DataPoint>
        {
            new DataPoint(1, 10000000, 10.0, "Sensor1"),
            new DataPoint(2, 15000000, 20.0, "Sensor1"),
            new DataPoint(3, 18000000, 30.0, "Sensor2")
        };
        _mockDataPointRepository.Setup(r => r.GetByTimeRangeAsync(10000000L, 20000000L))
            .ReturnsAsync(points);

        // Act
        var result = await _service.GetAggregateStatisticsAsync(10000000L, 20000000L);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(3);
        result.Sum.Should().Be(60.0); // 10 + 20 + 30
        result.Average.Should().Be(20.0); // 60 / 3
        result.Min.Should().Be(10.0);
        result.Max.Should().Be(30.0);
        result.UniqueSourceCount.Should().Be(2); // Sensor1 and Sensor2
    }

    [Fact]
    public async Task GetAggregateStatisticsAsync_ShouldCalculateStandardDeviationAndMedian()
    {
        // Arrange
        var points = new List<DataPoint>
        {
            new DataPoint(1, 10000000, 10.0, "Sensor1"),
            new DataPoint(2, 15000000, 20.0, "Sensor1"),
            new DataPoint(3, 18000000, 30.0, "Sensor1"),
            new DataPoint(4, 19000000, 40.0, "Sensor1")
        };
        _mockDataPointRepository.Setup(r => r.GetByTimeRangeAsync(10000000L, 20000000L))
            .ReturnsAsync(points);

        // Act
        var result = await _service.GetAggregateStatisticsAsync(10000000L, 20000000L);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(4);
        result.Sum.Should().Be(100.0);
        result.Average.Should().Be(25.0);
        result.Min.Should().Be(10.0);
        result.Max.Should().Be(40.0);
        result.Median.Should().Be(25.0); // Average of 20 and 30
        result.StdDev.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAggregateStatisticsAsync_ShouldCalculatePercentiles()
    {
        // Arrange
        var points = new List<DataPoint>();
        for (int i = 1; i <= 10; i++)
        {
            points.Add(new DataPoint(i, 10000000L + i * 1000000, i * 10.0, "Sensor1"));
        }
        _mockDataPointRepository.Setup(r => r.GetByTimeRangeAsync(10000000L, 20000000L))
            .ReturnsAsync(points);

        // Act
        var result = await _service.GetAggregateStatisticsAsync(10000000L, 20000000L);

        // Assert
        result.Should().NotBeNull();
        result.P95.Should().BeGreaterThan(45.0); // 9.5th value in sorted list
        result.P99.Should().BeGreaterThan(90.0); // 9.9th value in sorted list
    }

    [Fact]
    public async Task GetAggregateStatisticsAsync_ShouldCalculateAverageQuality()
    {
        // Arrange
        var points = new List<DataPoint>
        {
            new DataPoint(1, 10000000, 10.0, "Sensor1") { Quality = 80 },
            new DataPoint(2, 15000000, 20.0, "Sensor1") { Quality = 60 },
            new DataPoint(3, 18000000, 30.0, "Sensor1") { Quality = 100 }
        };
        _mockDataPointRepository.Setup(r => r.GetByTimeRangeAsync(10000000L, 20000000L))
            .ReturnsAsync(points);

        // Act
        var result = await _service.GetAggregateStatisticsAsync(10000000L, 20000000L);

        // Assert
        result.Should().NotBeNull();
        result.AverageQuality.Should().Be(80.0); // (80 + 60 + 100) / 3
    }

    [Fact]
    public async Task AnalyzeTrendsAsync_WithInsufficientData_ShouldReturnInsufficientDataStatus()
    {
        // Arrange
        var points = new List<DataPoint>
        {
            new DataPoint(1, 10000000, 10.0, "Sensor1")
        };
        _mockDataPointRepository.Setup(r => r.GetByTimeRangeAsync(10000000L, 20000000L))
            .ReturnsAsync(points);

        // Act
        var result = await _service.AnalyzeTrendsAsync(10000000L, 20000000L, 5000000L);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("INSUFFICIENT_DATA");
    }

    [Fact]
    public async Task AnalyzeTrendsAsync_WithInsufficientIntervals_ShouldReturnInsufficientIntervalsStatus()
    {
        // Arrange
        var points = new List<DataPoint>
        {
            new DataPoint(1, 10000000, 10.0, "Sensor1"),
            new DataPoint(2, 11000000, 20.0, "Sensor1")
        };
        _mockDataPointRepository.Setup(r => r.GetByTimeRangeAsync(10000000L, 20000000L))
            .ReturnsAsync(points);

        // Act
        var result = await _service.AnalyzeTrendsAsync(10000000L, 20000000L, 6000000L); // Large interval = 1 interval

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("INSUFFICIENT_INTERVALS");
    }

    [Fact]
    public async Task AnalyzeTrendsAsync_WithIncreasingTrend_ShouldReturnIncreasingDirection()
    {
        // Arrange
        var points = new List<DataPoint>
        {
            new DataPoint(1, 10000000, 10.0, "Sensor1"),
            new DataPoint(2, 12000000, 20.0, "Sensor1"),
            new DataPoint(3, 14000000, 30.0, "Sensor1"),
            new DataPoint(4, 16000000, 40.0, "Sensor1"),
            new DataPoint(5, 18000000, 50.0, "Sensor1")
        };
        _mockDataPointRepository.Setup(r => r.GetByTimeRangeAsync(10000000L, 20000000L))
            .ReturnsAsync(points);

        // Act
        var result = await _service.AnalyzeTrendsAsync(10000000L, 20000000L, 2000000L);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("SUCCESS");
        result.Direction.Should().Be("INCREASING");
        result.ChangePercent.Should().BePositive();
    }

    [Fact]
    public async Task AnalyzeTrendsAsync_WithDecreasingTrend_ShouldReturnDecreasingDirection()
    {
        // Arrange
        var points = new List<DataPoint>
        {
            new DataPoint(1, 10000000, 50.0, "Sensor1"),
            new DataPoint(2, 12000000, 40.0, "Sensor1"),
            new DataPoint(3, 14000000, 30.0, "Sensor1"),
            new DataPoint(4, 16000000, 20.0, "Sensor1"),
            new DataPoint(5, 18000000, 10.0, "Sensor1")
        };
        _mockDataPointRepository.Setup(r => r.GetByTimeRangeAsync(10000000L, 20000000L))
            .ReturnsAsync(points);

        // Act
        var result = await _service.AnalyzeTrendsAsync(10000000L, 20000000L, 2000000L);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("SUCCESS");
        result.Direction.Should().Be("DECREASING");
        result.ChangePercent.Should().BeNegative();
    }

    [Fact]
    public async Task AnalyzeTrendsAsync_WithStableTrend_ShouldReturnStableDirection()
    {
        // Arrange
        var points = new List<DataPoint>
        {
            new DataPoint(1, 10000000, 25.0, "Sensor1"),
            new DataPoint(2, 12000000, 26.0, "Sensor1"),
            new DataPoint(3, 14000000, 24.0, "Sensor1"),
            new DataPoint(4, 16000000, 25.0, "Sensor1"),
            new DataPoint(5, 18000000, 25.0, "Sensor1")
        };
        _mockDataPointRepository.Setup(r => r.GetByTimeRangeAsync(10000000L, 20000000L))
            .ReturnsAsync(points);

        // Act
        var result = await _service.AnalyzeTrendsAsync(10000000L, 20000000L, 2000000L);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("SUCCESS");
        result.Direction.Should().Be("STABLE");
        result.ChangePercent.Should().BeCloseTo(0, 1); // Close to zero
    }

    [Fact]
    public async Task DecomposeTimeSeriesAsync_WithInsufficientData_ShouldReturnInsufficientDataStatus()
    {
        // Arrange
        var points = new List<DataPoint>
        {
            new DataPoint(1, 10000000, 10.0, "Sensor1"),
            new DataPoint(2, 11000000, 20.0, "Sensor1")
        };
        _mockDataPointRepository.Setup(r => r.GetByTimeRangeAsync(10000000L, 20000000L))
            .ReturnsAsync(points);

        // Act
        var result = await _service.DecomposeTimeSeriesAsync(10000000L, 20000000L, 5);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("INSUFFICIENT_DATA");
    }

    [Fact]
    public async Task DecomposeTimeSeriesAsync_WithSufficientData_ShouldReturnSuccessStatus()
    {
        // Arrange
        var points = new List<DataPoint>();
        for (int i = 1; i <= 10; i++)
        {
            points.Add(new DataPoint(i, 10000000L + i * 1000000, i * 10.0, "Sensor1"));
        }
        _mockDataPointRepository.Setup(r => r.GetByTimeRangeAsync(10000000L, 20000000L))
            .ReturnsAsync(points);

        // Act
        var result = await _service.DecomposeTimeSeriesAsync(10000000L, 20000000L, 3);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("SUCCESS");
        result.OriginalCount.Should().Be(10);
        result.TrendPoints.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetRecentMetricsAsync_ShouldDelegateToMetricsRepository()
    {
        // Arrange
        var expectedMetrics = new List<MetricAggregation>
        {
            new MetricAggregation(1, 10000000, 20000000, "TEST")
        };
        _mockMetricsRepository.Setup(r => r.GetHistoryAsync(10))
            .ReturnsAsync(expectedMetrics);

        // Act
        var result = await _service.GetRecentMetricsAsync(10);

        // Assert
        result.Should().BeEquivalentTo(expectedMetrics);
        _mockMetricsRepository.Verify(r => r.GetHistoryAsync(10), Times.Once);
    }

    [Fact]
    public async Task GetDataPointCountAsync_ShouldDelegateToDataPointRepository()
    {
        // Arrange
        _mockDataPointRepository.Setup(r => r.CountAsync())
            .ReturnsAsync(12345L);

        // Act
        var result = await _service.GetDataPointCountAsync();

        // Assert
        result.Should().Be(12345L);
        _mockDataPointRepository.Verify(r => r.CountAsync(), Times.Once);
    }

    [Fact]
    public void DataAggregateStatistics_ShouldHaveCorrectProperties()
    {
        // Arrange
        var stats = new QueryService.DataAggregateStatistics
        {
            StartMs = 10000000L,
            EndMs = 20000000L,
            Count = 100,
            Sum = 5000.0,
            Average = 50.0,
            Min = 10.0,
            Max = 100.0,
            StdDev = 25.0,
            Median = 45.0,
            P95 = 90.0,
            P99 = 95.0,
            UniqueSourceCount = 5,
            AverageQuality = 85.0
        };

        // Assert
        stats.StartMs.Should().Be(10000000L);
        stats.EndMs.Should().Be(20000000L);
        stats.Count.Should().Be(100);
        stats.Sum.Should().Be(5000.0);
        stats.Average.Should().Be(50.0);
        stats.Min.Should().Be(10.0);
        stats.Max.Should().Be(100.0);
        stats.StdDev.Should().Be(25.0);
        stats.Median.Should().Be(45.0);
        stats.P95.Should().Be(90.0);
        stats.P99.Should().Be(95.0);
        stats.UniqueSourceCount.Should().Be(5);
        stats.AverageQuality.Should().Be(85.0);
    }

    [Fact]
    public void TrendAnalysis_ShouldHaveCorrectProperties()
    {
        // Arrange
        var trend = new QueryService.TrendAnalysis
        {
            Status = "SUCCESS",
            Direction = "INCREASING",
            ChangePercent = 25.5,
            IntervalCount = 10,
            TimeSpanMs = 10000000L,
            Volatility = 5.2
        };

        // Assert
        trend.Status.Should().Be("SUCCESS");
        trend.Direction.Should().Be("INCREASING");
        trend.ChangePercent.Should().Be(25.5);
        trend.IntervalCount.Should().Be(10);
        trend.TimeSpanMs.Should().Be(10000000L);
        trend.Volatility.Should().Be(5.2);
    }

    [Fact]
    public void TimeSeriesDecomposition_ShouldHaveCorrectProperties()
    {
        // Arrange
        var decomposition = new QueryService.TimeSeriesDecomposition
        {
            Status = "SUCCESS",
            OriginalCount = 100,
            TrendPoints = 95,
            SeasonalityStrength = 0.8,
            TrendStrength = 0.7
        };

        // Assert
        decomposition.Status.Should().Be("SUCCESS");
        decomposition.OriginalCount.Should().Be(100);
        decomposition.TrendPoints.Should().Be(95);
        decomposition.SeasonalityStrength.Should().Be(0.8);
        decomposition.TrendStrength.Should().Be(0.7);
    }
}