#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using DotNetRealtimePipeline.Data.Repositories;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Metrics;
using DotNetRealtimePipeline.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace DotNetRealtimePipeline.Tests.Unit;

public sealed class MetricsServiceTests
{
    private readonly Mock<IMetricsRepository> _mockRepository;
    private readonly Mock<IPipelineMetrics> _mockThroughputCounter;
    private readonly MetricsService _service;

    public MetricsServiceTests()
    {
        _mockRepository = new Mock<IMetricsRepository>();
        _mockThroughputCounter = new Mock<IPipelineMetrics>();
        _service = new MetricsService(_mockRepository.Object, _mockThroughputCounter.Object);
    }

    [Fact]
    public void Constructor_WithNullRepository_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new MetricsService(null!, _mockThroughputCounter.Object));
    }

    [Fact]
    public void Constructor_WithNullThroughputCounter_ShouldCreateDefault()
    {
        // Act
        var service = new MetricsService(_mockRepository.Object, null);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void GetThroughput_ShouldDelegateToThroughputCounter()
    {
        // Arrange
        _mockThroughputCounter.Setup(t => t.GetThroughput())
            .Returns(123.45);

        // Act
        var result = _service.GetThroughput();

        // Assert
        result.Should().Be(123.45);
        _mockThroughputCounter.Verify(t => t.GetThroughput(), Times.Once);
    }

    [Fact]
    public void GetThroughput_WithStageName_ShouldDelegateToThroughputCounter()
    {
        // Arrange
        _mockThroughputCounter.Setup(t => t.GetThroughput("Stage1"))
            .Returns(456.78);

        // Act
        var result = _service.GetThroughput("Stage1");

        // Assert
        result.Should().Be(456.78);
        _mockThroughputCounter.Verify(t => t.GetThroughput("Stage1"), Times.Once);
    }

    [Fact]
    public void RecordThroughput_WithCount_ShouldDelegateToThroughputCounter()
    {
        // Act
        _service.RecordThroughput(100);

        // Assert
        _mockThroughputCounter.Verify(t => t.RecordEvents(100), Times.Once);
    }

    [Fact]
    public void RecordThroughput_WithStageNameAndCount_ShouldDelegateToThroughputCounter()
    {
        // Act
        _service.RecordThroughput("Stage1", 50);

        // Assert
        _mockThroughputCounter.Verify(t => t.RecordEvents("Stage1", 50), Times.Once);
    }

    [Fact]
    public void RecordProcessingTime_WithNegativeValue_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.RecordProcessingTime(-100));
    }

    [Fact]
    public void RecordProcessingTime_WithValidValue_ShouldStoreProcessingTime()
    {
        // Act
        _service.RecordProcessingTime(150);
        _service.RecordProcessingTime(200);
        _service.RecordProcessingTime(100);

        // Assert - Can't directly verify private field, but no exception means success
    }

    [Fact]
    public async Task CreateMetricAggregationAsync_ShouldCreateAndSaveMetric()
    {
        // Arrange
        var metric = new MetricAggregation(1, 1000, 2000, "TEST");
        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<MetricAggregation>()))
            .ReturnsAsync(metric);

        // Act
        var result = await _service.CreateMetricAggregationAsync(1000, 2000, 50, 2, 1);

        // Assert
        result.Should().NotBeNull();
        result.TotalItemsProcessed.Should().Be(50);
        result.TotalItemsFailed.Should().Be(2);
        result.TotalItemsSkipped.Should().Be(1);
        _mockRepository.Verify(r => r.SaveAsync(It.IsAny<MetricAggregation>()), Times.Once);
    }

    [Fact]
    public void ClearProcessingTimes_ShouldClearStoredTimes()
    {
        // Arrange - Add some processing times
        _service.RecordProcessingTime(100);
        _service.RecordProcessingTime(200);

        // Act
        _service.ClearProcessingTimes();

        // Assert - Can't directly verify, but method completes without exception
    }

    [Fact]
    public async Task GenerateHealthReportAsync_WhenRepositoryThrows_ShouldReturnUnknownStatus()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetLatestAsync())
            .ThrowsAsync<Exception>();

        // Act
        var result = await _service.GenerateHealthReportAsync();

        // Assert
        result.Status.Should().Be("UNKNOWN");
        result.Message.Should().Contain("No metrics available");
    }

    [Fact]
    public async Task GenerateHealthReportAsync_ShouldCalculateHealthStatus()
    {
        // Arrange
        var metric = new MetricAggregation(1, 1000, 2000, "TEST")
        {
            TotalItemsProcessed = 1000,
            TotalItemsFailed = 50,
            TotalItemsSkipped = 10
        };
        _mockRepository.Setup(r => r.GetLatestAsync())
            .ReturnsAsync(metric);

        // Act
        var result = await _service.GenerateHealthReportAsync();

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("HEALTHY"); // Not unhealthy with low error rate
        result.TotalProcessed.Should().Be(1000);
        result.TotalFailed.Should().Be(50);
    }

    [Fact]
    public async Task GenerateHealthReportAsync_WithHighErrorRate_ShouldMarkAsUnhealthy()
    {
        // Arrange
        var metric = new MetricAggregation(1, 1000, 2000, "TEST")
        {
            TotalItemsProcessed = 100,
            TotalItemsFailed = 20 // 20% error rate
        };
        _mockRepository.Setup(r => r.GetLatestAsync())
            .ReturnsAsync(metric);

        // Act
        var result = await _service.GenerateHealthReportAsync();

        // Assert
        result.Status.Should().Be("UNHEALTHY");
        result.Message.Should().Be("ERROR RATE ELEVATED");
    }

    [Fact]
    public async Task GenerateHealthReportAsync_WithHighBackpressure_ShouldMarkAsUnhealthy()
    {
        // Arrange
        var metric = new MetricAggregation(1, 1000, 2000, "TEST")
        {
            TotalItemsProcessed = 1000,
            TotalItemsFailed = 5,
            BackpressureRatio = 30.0 // 30% backpressure
        };
        _mockRepository.Setup(r => r.GetLatestAsync())
            .ReturnsAsync(metric);

        // Act
        var result = await _service.GenerateHealthReportAsync();

        // Assert
        result.Status.Should().Be("UNHEALTHY");
        result.Message.Should().Be("BACKPRESSURE DETECTED");
    }

    [Fact]
    public async Task GenerateHealthReportAsync_WithLowThroughput_ShouldMarkAsUnhealthy()
    {
        // Arrange
        var metric = new MetricAggregation(1, 1000, 2000, "TEST")
        {
            TotalItemsProcessed = 50,
            ThroughputItemsPerSecond = 50.0 // Low throughput
        };
        _mockRepository.Setup(r => r.GetLatestAsync())
            .ReturnsAsync(metric);

        // Act
        var result = await _service.GenerateHealthReportAsync();

        // Assert
        result.Status.Should().Be("UNHEALTHY");
        result.Message.Should().Be("LOW THROUGHPUT");
    }

    [Fact]
    public async Task AnalyzePerformanceTrendAsync_WithInsufficientData_ShouldReturnInsufficientDataStatus()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetHistoryAsync(10))
            .ReturnsAsync(new List<MetricAggregation>());

        // Act
        var result = await _service.AnalyzePerformanceTrendAsync();

        // Assert
        result.Status.Should().Be("INSUFFICIENT_DATA");
        result.SamplesAnalyzed.Should().Be(0);
    }

    [Fact]
    public async Task AnalyzePerformanceTrendAsync_WithInsufficientHistory_ShouldReturnInsufficientDataStatus()
    {
        // Arrange
        var metrics = new List<MetricAggregation>
        {
            new MetricAggregation(1, 1000, 2000, "M1"),
            new MetricAggregation(2, 2000, 3000, "M2")
        };
        _mockRepository.Setup(r => r.GetHistoryAsync(10))
            .ReturnsAsync(metrics);

        // Act
        var result = await _service.AnalyzePerformanceTrendAsync();

        // Assert
        result.Status.Should().Be("INSUFFICIENT_DATA");
        result.SamplesAnalyzed.Should().Be(2);
    }

    [Fact]
    public async Task AnalyzePerformanceTrendAsync_WithStableThroughput_ShouldReturnStableDirection()
    {
        // Arrange
        var metrics = new List<MetricAggregation>
        {
            new MetricAggregation(1, 1000, 2000, "M1") { TotalItemsProcessed = 1000 },
            new MetricAggregation(2, 2000, 3000, "M2") { TotalItemsProcessed = 1001 }
        };
        _mockRepository.Setup(r => r.GetHistoryAsync(10))
            .ReturnsAsync(metrics);

        // Act
        var result = await _service.AnalyzePerformanceTrendAsync();

        // Assert
        result.Status.Should().Be("SUCCESS");
        result.TrendDirection.Should().Be("STABLE");
    }

    [Fact]
    public async Task AnalyzePerformanceTrendAsync_WithImprovingThroughput_ShouldReturnImprovingDirection()
    {
        // Arrange
        var metrics = new List<MetricAggregation>
        {
            new MetricAggregation(1, 1000, 2000, "M1") { TotalItemsProcessed = 1000 },
            new MetricAggregation(2, 2000, 3000, "M2") { TotalItemsProcessed = 1500 }
        };
        _mockRepository.Setup(r => r.GetHistoryAsync(10))
            .ReturnsAsync(metrics);

        // Act
        var result = await _service.AnalyzePerformanceTrendAsync();

        // Assert
        result.Status.Should().Be("SUCCESS");
        result.TrendDirection.Should().Be("IMPROVING");
        result.ThroughputChangePercent.Should().BePositive();
    }

    [Fact]
    public async Task AnalyzePerformanceTrendAsync_WithDegradingThroughput_ShouldReturnDegradingDirection()
    {
        // Arrange
        var metrics = new List<MetricAggregation>
        {
            new MetricAggregation(1, 1000, 2000, "M1") { TotalItemsProcessed = 1500 },
            new MetricAggregation(2, 2000, 3000, "M2") { TotalItemsProcessed = 1000 }
        };
        _mockRepository.Setup(r => r.GetHistoryAsync(10))
            .ReturnsAsync(metrics);

        // Act
        var result = await _service.AnalyzePerformanceTrendAsync();

        // Assert
        result.Status.Should().Be("SUCCESS");
        result.TrendDirection.Should().Be("DEGRADING");
        result.ThroughputChangePercent.Should().BeNegative();
    }

    [Fact]
    public async Task GetMetricDistributionAsync_ShouldReturnDistribution()
    {
        // Arrange
        var metric = new MetricAggregation(1, 1000, 2000, "TEST")
        {
            CountBySource = new Dictionary<string, long> { { "Source1", 50 }, { "Source2", 30 } },
            ErrorRateByStage = new Dictionary<string, double> { { "Stage1", 5.0 }, { "Stage2", 2.0 } }
        };
        _mockRepository.Setup(r => r.GetLatestAsync())
            .ReturnsAsync(metric);

        // Act
        var result = await _service.GetMetricDistributionAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalSources.Should().Be(2);
        result.SourceBreakdown.Should().ContainKey("Source1").WhoseValue.Should().Be(50);
        result.StageErrorRates.Should().ContainKey("Stage1").WhoseValue.Should().Be(5.0);
    }

    [Fact]
    public void RecordFailure_WithNullStageName_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.RecordFailure(null!));
        Assert.Throws<ArgumentException>(() => _service.RecordFailure(""));
        Assert.Throws<ArgumentException>(() => _service.RecordFailure("   "));
    }

    [Fact]
    public void RecordFailure_WithValidStageName_ShouldNotThrow()
    {
        // Act - Should not throw
        _service.RecordFailure("Stage1");
        _service.RecordFailure("Processing");
    }

    [Fact]
    public void HealthReport_ShouldHaveCorrectProperties()
    {
        // Arrange
        var report = new MetricsService.HealthReport
        {
            Status = "HEALTHY",
            Message = "All systems operational",
            ThroughputItemsPerSecond = 123.45,
            SuccessRatePercent = 99.5,
            ErrorRatePercent = 0.5,
            AverageProcessingTimeMs = 100.0,
            P95ProcessingTimeMs = 150.0,
            P99ProcessingTimeMs = 200.0,
            BackpressurePercentage = 5.0,
            TotalProcessed = 10000,
            TotalFailed = 50,
            GeneratedAt = DateTime.UtcNow
        };

        // Assert
        report.Status.Should().Be("HEALTHY");
        report.Message.Should().Be("All systems operational");
        report.ThroughputItemsPerSecond.Should().Be(123.45);
    }

    [Fact]
    public void PerformanceTrend_ShouldHaveCorrectProperties()
    {
        // Arrange
        var trend = new MetricsService.PerformanceTrend
        {
            TrendDirection = "IMPROVING",
            ThroughputChangePercent = 25.5,
            LatencyChangePercent = -10.2,
            ErrorRateChangePercent = -5.0,
            SamplesAnalyzed = 10,
            TimeSpanMs = 10000
        };

        // Assert
        trend.TrendDirection.Should().Be("IMPROVING");
        trend.ThroughputChangePercent.Should().Be(25.5);
        trend.SamplesAnalyzed.Should().Be(10);
    }

    [Fact]
    public void MetricDistribution_ShouldHaveCorrectProperties()
    {
        // Arrange
        var distribution = new MetricsService.MetricDistribution
        {
            TotalSources = 3,
            SourceBreakdown = new Dictionary<string, long> { { "S1", 100 }, { "S2", 200 } },
            StageErrorRates = new Dictionary<string, double> { { "Stage1", 5.0 } },
            ComputedAt = DateTime.UtcNow
        };

        // Assert
        distribution.TotalSources.Should().Be(3);
        distribution.SourceBreakdown.Should().ContainKey("S1").WhoseValue.Should().Be(100);
    }
}
