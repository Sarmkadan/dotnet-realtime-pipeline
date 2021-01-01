#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetRealtimePipeline.Domain.Models;
using FluentAssertions;
using Xunit;

namespace DotNetRealtimePipeline.Tests.Unit;

/// <summary>
/// Unit tests for the <see cref="DotNetRealtimePipeline.Domain.Models.MetricAggregation"/> class.
/// Tests various metric calculation methods including throughput, error rate, success rate,
/// backpressure ratio, and average processing time calculations.
/// </summary>
public sealed class MetricAggregationTests
{
	/// <summary>
	/// Helper method to create a <see cref="DotNetRealtimePipeline.Domain.Models.MetricAggregation"/> instance for testing.
	/// </summary>
	/// <param name="startMs">The start timestamp in milliseconds.</param>
	/// <param name="endMs">The end timestamp in milliseconds.</param>
	/// <param name="processed">The number of items processed.</param>
	/// <param name="failed">The number of items that failed.</param>
	/// <param name="skipped">The number of items skipped (default: 0).</param>
	/// <param name="backpressureMs">The total backpressure duration in milliseconds (default: 0).</param>
	/// <returns>A configured <see cref="DotNetRealtimePipeline.Domain.Models.MetricAggregation"/> instance for testing.</returns>
	private static MetricAggregation BuildMetric(
		long startMs, long endMs,
		long processed, long failed, long skipped = 0,
		long backpressureMs = 0)
	{
		return new MetricAggregation(1, startMs, endMs, "STANDARD")
		{
			TotalItemsProcessed = processed,
			TotalItemsFailed = failed,
			TotalItemsSkipped = skipped,
			TotalBackpressureMs = backpressureMs
		};
	}

	[Fact]
	/// <summary>
	/// Tests that <see cref="DotNetRealtimePipeline.Domain.Models.MetricAggregation.CalculateThroughput"/> correctly calculates throughput
	/// when given a valid time window.
	/// </summary>
	public void CalculateThroughput_WithValidWindow_ReturnsItemsPerSecond()
	{
		// Arrange: 500 items over a 5-second window → 100 items/s
		var metric = BuildMetric(0, 5_000, processed: 500, failed: 0);

		// Act
		double throughput = metric.CalculateThroughput();

		// Assert
		throughput.Should().BeApproximately(100.0, precision: 0.001);
	}

	[Fact]
	/// <summary>
	/// Tests that <see cref="DotNetRealtimePipeline.Domain.Models.MetricAggregation.CalculateThroughput"/> returns zero
	/// when the time window duration is zero.
	/// </summary>
	public void CalculateThroughput_WithZeroDurationWindow_ReturnsZero()
	{
		// Arrange
		var metric = BuildMetric(10_000, 10_000, processed: 100, failed: 0);

		// Act
		double throughput = metric.CalculateThroughput();

		// Assert
		throughput.Should().Be(0d);
	}

	[Fact]
	/// <summary>
	/// Tests that <see cref="DotNetRealtimePipeline.Domain.Models.MetricAggregation.CalculateErrorRate"/> correctly calculates
	/// the error rate percentage based on failed items.
	/// </summary>
	public void CalculateErrorRate_WithFailedItems_ReturnsCorrectPercentage()
	{
		// Arrange: 80 processed, 20 failed → 20 % error rate
		var metric = BuildMetric(0, 10_000, processed: 80, failed: 20);

		// Act
		double errorRate = metric.CalculateErrorRate();

		// Assert
		errorRate.Should().BeApproximately(20.0, precision: 0.001);
	}

	[Fact]
	/// <summary>
	/// Tests that <see cref="DotNetRealtimePipeline.Domain.Models.MetricAggregation.CalculateSuccessRate"/> returns 100%
	/// when no items are recorded (no failures possible).
	/// </summary>
	public void CalculateSuccessRate_WhenNoItemsRecorded_ReturnsHundredPercent()
	{
		// Arrange: empty window
		var metric = BuildMetric(0, 10_000, processed: 0, failed: 0);

		// Act
		double successRate = metric.CalculateSuccessRate();

		// Assert
		successRate.Should().Be(100d);
	}

	[Fact]
	/// <summary>
	/// Tests that <see cref="DotNetRealtimePipeline.Domain.Models.MetricAggregation.IsUnhealthy"/> returns true
	/// when the error rate exceeds the 5% threshold.
	/// </summary>
	public void IsUnhealthy_WhenErrorRateExceedsFivePercent_ReturnsTrue()
	{
		// Arrange: 10 failures out of 100 total → 10 % error rate
		var metric = BuildMetric(0, 10_000, processed: 90, failed: 10);

		// Act
		bool unhealthy = metric.IsUnhealthy();

		// Assert
		unhealthy.Should().BeTrue();
	}

	[Fact]
	/// <summary>
	/// Tests that <see cref="DotNetRealtimePipeline.Domain.Models.MetricAggregation.ComputeAverageProcessingTime"/> correctly
	/// calculates and sets the average processing time from a list of samples.
	/// </summary>
	public void ComputeAverageProcessingTime_WithValidSamples_SetsCorrectAverage()
	{
		// Arrange
		var metric = new MetricAggregation(1, 0, 10_000, "STANDARD");
		var samples = new List<double> { 10.0, 20.0, 30.0 };

		// Act
		metric.ComputeAverageProcessingTime(samples);

		// Assert
		metric.AverageProcessingTimeMs.Should().BeApproximately(20.0, precision: 0.001);
	}

	[Fact]
	/// <summary>
	/// Tests that <see cref="DotNetRealtimePipeline.Domain.Models.MetricAggregation.ComputeAverageProcessingTime"/> sets the average
	/// processing time to zero when given an empty list of samples.
	/// </summary>
	public void ComputeAverageProcessingTime_WithEmptyList_SetsAverageToZero()
	{
		// Arrange
		var metric = new MetricAggregation(1, 0, 10_000, "STANDARD");

		// Act
		metric.ComputeAverageProcessingTime(new List<double>());

		// Assert
		metric.AverageProcessingTimeMs.Should().Be(0d);
	}

	[Fact]
	/// <summary>
	/// Tests that <see cref="DotNetRealtimePipeline.Domain.Models.MetricAggregation.CalculateBackpressureRatio"/> correctly calculates
	/// the ratio of backpressure duration to the total time window.
	/// </summary>
	public void CalculateBackpressureRatio_WithKnownBackpressureDuration_ReturnsCorrectRatio()
	{
		// Arrange: 2 000 ms backpressure out of a 10 000 ms window → 20 %
		var metric = BuildMetric(0, 10_000, processed: 100, failed: 0, backpressureMs: 2_000);

		// Act
		double ratio = metric.CalculateBackpressureRatio();

		// Assert
		ratio.Should().BeApproximately(20.0, precision: 0.001);
	}
}