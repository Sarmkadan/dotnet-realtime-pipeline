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

/// <summary>
/// Unit tests for <see cref="MetricsService"/> that verify metrics recording, aggregation, and health reporting functionality.
/// Tests cover edge cases, error handling, and integration with <see cref="IMetricsRepository"/> mocks.
/// </summary>
public sealed class MetricsServiceTests
{
	private readonly Mock<IMetricsRepository> _repoMock = new();
	private readonly MetricsService _service;

	/// <summary>
	/// Initializes a new instance of the <see cref="MetricsServiceTests"/> class.
	/// </summary>
	public MetricsServiceTests()
	{
		_service = new MetricsService(_repoMock.Object);
	}

	[Fact]
	/// <summary>
	/// Tests that <see cref="MetricsService.RecordProcessingTime"/> throws <see cref="ArgumentException"/> when given a negative processing time value.
	/// </summary>
	public void RecordProcessingTime_WithNegativeValue_ThrowsArgumentException()
	{
		// Act
		Action act = () => _service.RecordProcessingTime(-1);

		// Assert
		act.Should().Throw<ArgumentException>()
			.WithMessage("*negative*");
	}

	[Fact]
	/// <summary>
	/// Tests that <see cref="MetricsService.RecordProcessingTime"/> accepts zero as a valid processing time value.
	/// Zero indicates sub-millisecond operations that complete faster than the timer resolution.
	/// </summary>
	public void RecordProcessingTime_WithZero_DoesNotThrow()
	{
		// Zero elapsed time is valid (sub-millisecond operations)
		Action act = () => _service.RecordProcessingTime(0);

		act.Should().NotThrow();
	}

	[Fact]
	/// <summary>
	/// Tests that <see cref="MetricsService.CreateMetricAggregationAsync"/> properly delegates to the repository
	/// when creating a new metric aggregation with valid parameters.
	/// </summary>
	/// <param name="windowStartMs">The start timestamp of the aggregation window in milliseconds.</param>
	/// <param name="windowEndMs">The end timestamp of the aggregation window in milliseconds.</param>
	/// <param name="itemsProcessed">The number of items successfully processed in the window.</param>
	/// <returns>A <see cref="MetricAggregation"/> object with the stored values.</returns>
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
	/// <summary>
	/// Tests that <see cref="MetricsService.GenerateHealthReportAsync"/> returns UNKNOWN status
	/// when the repository throws an exception during data retrieval.
	/// </summary>
	/// <returns>A health report with UNKNOWN status indicating no data availability.</returns>
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
	/// <summary>
	/// Tests that <see cref="MetricsService.GenerateHealthReportAsync"/> returns HEALTHY status
	/// when metrics indicate normal operating conditions.
	/// </summary>
	/// <remarks>
	/// Healthy criteria: 1000 items processed in 10 seconds, 0.1% error rate,
	/// no backpressure, and throughput above minimum threshold.
	/// </remarks>
	/// <returns>A health report with HEALTHY status indicating normal operation.</returns>
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
	/// <summary>
	/// Tests that <see cref="MetricsService.AnalyzePerformanceTrendAsync"/> returns INSUFFICIENT_DATA status
	/// when fewer than two samples are available for trend analysis.
	/// </summary>
	/// <param name="historyCount">The number of historical samples to request from the repository.</param>
	/// <returns>A trend analysis result indicating insufficient data for meaningful analysis.</returns>
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
	/// <summary>
	/// Tests that <see cref="MetricsService.RecordFailure"/> throws <see cref="ArgumentException"/> when provided with a null stage name.
	/// </summary>
	public void RecordFailure_WithNullStageName_ThrowsArgumentException()
	{
		// Act
		Action act = () => _service.RecordFailure(null!);

		// Assert
		act.Should().Throw<ArgumentException>();
	}

	[Fact]
	/// <summary>
	/// Tests that the <see cref="MetricsService"/> constructor throws <see cref="ArgumentNullException"/>
	/// when a null repository is provided.
	/// </summary>
	public void Constructor_WithNullRepository_ThrowsArgumentNullException()
	{
		// Act
		Action act = () => _ = new MetricsService(null!);

		// Assert
		act.Should().Throw<ArgumentNullException>()
			.WithParameterName("repository");
	}
}
