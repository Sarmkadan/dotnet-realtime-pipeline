using System;
using System.Collections.Generic;
using DotNetRealtimePipeline.Domain.Models;
using FluentAssertions;
using Xunit;

namespace DotNetRealtimePipeline.Tests.Unit;

/// <summary>
/// Extension methods for <see cref="MetricAggregationTests"/> to simplify common test scenarios and operations.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649:FileHeaderShouldMatchTypeName", Justification = "Extension class naming convention")]
public static class MetricAggregationTestsExtensions
{
    /// <summary>
    /// Creates a standard metric for testing throughput calculations.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="itemsPerSecond">The expected items per second throughput.</param>
    /// <returns>A configured <see cref="MetricAggregation"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
    public static MetricAggregation CreateThroughputMetric(this MetricAggregationTests tests, double itemsPerSecond)
    {
        ArgumentNullException.ThrowIfNull(tests);

        // 500 items over a 5-second window → 100 items/s
        return new MetricAggregation(1, 0, 5_000, "STANDARD")
        {
            TotalItemsProcessed = 500,
            TotalItemsFailed = 0,
            TotalItemsSkipped = 0,
            TotalBackpressureMs = 0
        };
    }

    /// <summary>
    /// Creates a metric with zero duration window for testing edge cases.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <returns>A configured <see cref="MetricAggregation"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
    public static MetricAggregation CreateZeroDurationMetric(this MetricAggregationTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        return new MetricAggregation(1, 10_000, 10_000, "STANDARD")
        {
            TotalItemsProcessed = 100,
            TotalItemsFailed = 0,
            TotalItemsSkipped = 0,
            TotalBackpressureMs = 0
        };
    }

    /// <summary>
    /// Creates a metric with failed items for testing error rate calculations.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="processedCount">Total items processed.</param>
    /// <param name="failedCount">Total items failed.</param>
    /// <returns>A configured <see cref="MetricAggregation"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
    public static MetricAggregation CreateErrorRateMetric(this MetricAggregationTests tests, long processedCount, long failedCount)
    {
        ArgumentNullException.ThrowIfNull(tests);

        return new MetricAggregation(1, 0, 10_000, "STANDARD")
        {
            TotalItemsProcessed = processedCount,
            TotalItemsFailed = failedCount,
            TotalItemsSkipped = 0,
            TotalBackpressureMs = 0
        };
    }

    /// <summary>
    /// Creates a metric with no items recorded for testing success rate calculations.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <returns>A configured <see cref="MetricAggregation"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
    public static MetricAggregation CreateEmptyMetric(this MetricAggregationTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        return new MetricAggregation(1, 0, 10_000, "STANDARD")
        {
            TotalItemsProcessed = 0,
            TotalItemsFailed = 0,
            TotalItemsSkipped = 0,
            TotalBackpressureMs = 0
        };
    }

    /// <summary>
    /// Creates a metric with error rate exceeding 5% for testing unhealthy detection.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <returns>A configured <see cref="MetricAggregation"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
    public static MetricAggregation CreateUnhealthyMetric(this MetricAggregationTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        // 10 failures out of 100 total → 10% error rate
        return new MetricAggregation(1, 0, 10_000, "STANDARD")
        {
            TotalItemsProcessed = 90,
            TotalItemsFailed = 10,
            TotalItemsSkipped = 0,
            TotalBackpressureMs = 0
        };
    }

    /// <summary>
    /// Creates a metric with processing time samples for testing average calculation.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="samples">Processing time samples in milliseconds.</param>
    /// <returns>A configured <see cref="MetricAggregation"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
    public static MetricAggregation CreateProcessingTimeMetric(this MetricAggregationTests tests, params double[] samples)
    {
        ArgumentNullException.ThrowIfNull(tests);

        var metric = new MetricAggregation(1, 0, 10_000, "STANDARD")
        {
            TotalItemsProcessed = 100,
            TotalItemsFailed = 0,
            TotalItemsSkipped = 0,
            TotalBackpressureMs = 0
        };

        metric.ComputeAverageProcessingTime(new List<double>(samples));
        return metric;
    }

    /// <summary>
    /// Creates a metric with known backpressure duration for testing backpressure ratio.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="backpressureMs">Duration of backpressure in milliseconds.</param>
    /// <returns>A configured <see cref="MetricAggregation"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
    public static MetricAggregation CreateBackpressureMetric(this MetricAggregationTests tests, long backpressureMs)
    {
        ArgumentNullException.ThrowIfNull(tests);

        // 2000ms backpressure out of a 10000ms window → 20%
        return new MetricAggregation(1, 0, 10_000, "STANDARD")
        {
            TotalItemsProcessed = 100,
            TotalItemsFailed = 0,
            TotalItemsSkipped = 0,
            TotalBackpressureMs = backpressureMs
        };
    }

    /// <summary>
    /// Asserts that the throughput calculation returns the expected items per second value.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="metric">The metric to test.</param>
    /// <param name="expectedItemsPerSecond">The expected throughput in items per second.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> or <paramref name="metric"/> is <see langword="null"/>.</exception>
    public static void ShouldCalculateThroughput(this MetricAggregationTests tests, MetricAggregation metric, double expectedItemsPerSecond)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(metric);

        double throughput = metric.CalculateThroughput();
        throughput.Should().BeApproximately(expectedItemsPerSecond, 0.001);
    }

    /// <summary>
    /// Asserts that the throughput calculation returns zero when the duration window is zero.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="metric">The metric to test.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> or <paramref name="metric"/> is <see langword="null"/>.</exception>
    public static void ShouldReturnZeroThroughputForZeroDuration(this MetricAggregationTests tests, MetricAggregation metric)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(metric);

        double throughput = metric.CalculateThroughput();
        throughput.Should().Be(0);
    }

    /// <summary>
    /// Asserts that the error rate calculation returns the expected percentage.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="metric">The metric to test.</param>
    /// <param name="expectedErrorRate">The expected error rate as a percentage (0-100).</param>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> or <paramref name="metric"/> is <see langword="null"/>.</exception>
    public static void ShouldCalculateErrorRate(this MetricAggregationTests tests, MetricAggregation metric, double expectedErrorRate)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(metric);

        double errorRate = metric.CalculateErrorRate();
        errorRate.Should().BeApproximately(expectedErrorRate, 0.001);
    }

    /// <summary>
    /// Asserts that the success rate returns 100% when no items are recorded.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="metric">The metric to test.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> or <paramref name="metric"/> is <see langword="null"/>.</exception>
    public static void ShouldReturnHundredPercentSuccessRateWhenNoItems(this MetricAggregationTests tests, MetricAggregation metric)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(metric);

        double successRate = metric.CalculateSuccessRate();
        successRate.Should().Be(100);
    }

    /// <summary>
    /// Asserts that the unhealthy detection returns true when error rate exceeds 5%.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="metric">The metric to test.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> or <paramref name="metric"/> is <see langword="null"/>.</exception>
    public static void ShouldDetectUnhealthyWhenErrorRateExceedsFivePercent(this MetricAggregationTests tests, MetricAggregation metric)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(metric);

        bool unhealthy = metric.IsUnhealthy();
        unhealthy.Should().BeTrue();
    }

    /// <summary>
    /// Asserts that the average processing time is calculated correctly from valid samples.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="metric">The metric to test.</param>
    /// <param name="expectedAverage">The expected average processing time in milliseconds.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> or <paramref name="metric"/> is <see langword="null"/>.</exception>
    public static void ShouldCalculateCorrectAverageProcessingTime(this MetricAggregationTests tests, MetricAggregation metric, double expectedAverage)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(metric);

        double average = metric.AverageProcessingTimeMs;
        average.Should().BeApproximately(expectedAverage, 0.001);
    }

    /// <summary>
    /// Asserts that the average processing time is set to zero when the sample list is empty.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="metric">The metric to test.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> or <paramref name="metric"/> is <see langword="null"/>.</exception>
    public static void ShouldSetAverageToZeroForEmptySamples(this MetricAggregationTests tests, MetricAggregation metric)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(metric);

        double average = metric.AverageProcessingTimeMs;
        average.Should().Be(0);
    }

    /// <summary>
    /// Asserts that the backpressure ratio is calculated correctly from known backpressure duration.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="metric">The metric to test.</param>
    /// <param name="expectedRatio">The expected backpressure ratio (0-1).</param>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> or <paramref name="metric"/> is <see langword="null"/>.</exception>
    public static void ShouldCalculateCorrectBackpressureRatio(this MetricAggregationTests tests, MetricAggregation metric, double expectedRatio)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(metric);

        double ratio = metric.CalculateBackpressureRatio();
        ratio.Should().BeApproximately(expectedRatio, 0.001);
    }

    /// <summary>
    /// Asserts that the result value is within an acceptable range.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="value">The value to check.</param>
    /// <param name="minimum">The minimum acceptable value.</param>
    /// <param name="maximum">The maximum acceptable value.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
    public static void ShouldBeWithinRange(this MetricAggregationTests tests, double value, double minimum, double maximum)
    {
        ArgumentNullException.ThrowIfNull(tests);

        value.Should().BeInRange(minimum, maximum);
    }

    /// <summary>
    /// Asserts that the result value is greater than or equal to the expected minimum.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="value">The value to check.</param>
    /// <param name="minimum">The minimum acceptable value.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
    public static void ShouldBeAtLeast(this MetricAggregationTests tests, double value, double minimum)
    {
        ArgumentNullException.ThrowIfNull(tests);

        value.Should().BeGreaterThanOrEqualTo(minimum);
    }

    /// <summary>
    /// Asserts that the result value is less than or equal to the expected maximum.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="value">The value to check.</param>
    /// <param name="maximum">The maximum acceptable value.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
    public static void ShouldBeAtMost(this MetricAggregationTests tests, double value, double maximum)
    {
        ArgumentNullException.ThrowIfNull(tests);

        value.Should().BeLessThanOrEqualTo(maximum);
    }
}
