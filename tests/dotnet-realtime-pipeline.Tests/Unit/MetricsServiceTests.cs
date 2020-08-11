#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetRealtimePipeline.Data.Repositories;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace DotNetRealtimePipeline.Tests.Unit;

public class MetricsServiceTests
{
    private readonly Mock<IMetricsRepository> _repoMock = new();
    private readonly MetricsService _service;

    public MetricsServiceTests()
    {
        _service = new MetricsService(_repoMock.Object);
    }

    [Fact]
    public void RecordProcessingTime_WithNegativeValue_ThrowsArgumentException()
    {
        // Act
        Action act = () => _service.RecordProcessingTime(-1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*negative*");
    }

    [Fact]
    public void RecordProcessingTime_WithZero_DoesNotThrow()
    {
        // Zero elapsed time is valid (sub-millisecond operations)
        Action act = () => _service.RecordProcessingTime(0);

        act.Should().NotThrow();
    }

    [Fact]
    public async Task CreateMetricAggregationAsync_WithValidArguments_DelegatesToRepository()
    {
        // Arrange
        var stored = new MetricAggregation(1, 0, 5_000, "STANDARD")
        {
            TotalItemsProcessed = 200
        };
        _repoMock.Setup(r => r.SaveAsync(It.IsAny<MetricAggregation>()))
                 .ReturnsAsync(stored);

        // Act
        var result = await _service.CreateMetricAggregationAsync(
            windowStartMs: 0, windowEndMs: 5_000,
            itemsProcessed: 200, itemsFailed: 0, itemsSkipped: 0);

        // Assert
        result.Should().NotBeNull();
        result.TotalItemsProcessed.Should().Be(200);
        _repoMock.Verify(r => r.SaveAsync(It.IsAny<MetricAggregation>()), Times.Once);
    }

    [Fact]
    public async Task GenerateHealthReportAsync_WhenRepositoryThrows_ReturnsUnknownStatus()
    {
        // Arrange
        _repoMock.Setup(r => r.GetLatestAsync())
                 .ThrowsAsync(new InvalidOperationException("no data"));

        // Act
        var report = await _service.GenerateHealthReportAsync();

        // Assert
        report.Status.Should().Be("UNKNOWN");
        report.Message.Should().Be("No metrics available");
    }

    [Fact]
    public async Task GenerateHealthReportAsync_WithHealthyMetrics_ReturnsHealthyStatus()
    {
        // Arrange: 1 000 items in 10 s = 100 items/s, 0.1 % error rate, no backpressure
        var healthy = new MetricAggregation(1, 0, 10_000, "STANDARD")
        {
            TotalItemsProcessed = 1000,
            TotalItemsFailed = 1,
            TotalItemsSkipped = 0,
            TotalBackpressureMs = 0
        };
        _repoMock.Setup(r => r.GetLatestAsync()).ReturnsAsync(healthy);

        // Act
        var report = await _service.GenerateHealthReportAsync();

        // Assert
        report.Status.Should().Be("HEALTHY");
        report.Message.Should().Be("OPERATING NORMALLY");
    }

    [Fact]
    public async Task AnalyzePerformanceTrendAsync_WithFewerThanTwoSamples_ReturnsInsufficientData()
    {
        // Arrange
        _repoMock.Setup(r => r.GetHistoryAsync(It.IsAny<int>()))
                 .ReturnsAsync(new List<MetricAggregation>());

        // Act
        var trend = await _service.AnalyzePerformanceTrendAsync(historyCount: 10);

        // Assert
        trend.TrendDirection.Should().Be("INSUFFICIENT_DATA");
        trend.SamplesAnalyzed.Should().Be(0);
    }

    [Fact]
    public void RecordFailure_WithNullStageName_ThrowsArgumentException()
    {
        // Act
        Action act = () => _service.RecordFailure(null!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WithNullRepository_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => _ = new MetricsService(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("repository");
    }
}
