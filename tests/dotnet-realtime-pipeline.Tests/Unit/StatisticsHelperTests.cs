// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetRealtimePipeline.Utilities;
using FluentAssertions;

namespace DotNetRealtimePipeline.Tests.Unit;

public class StatisticsHelperTests
{
    // -------------------------------------------------------------------------
    // CalculateMean
    // -------------------------------------------------------------------------

    [Fact]
    public void CalculateMean_NullList_ReturnsZero()
    {
        StatisticsHelper.CalculateMean(null!).Should().Be(0);
    }

    [Fact]
    public void CalculateMean_EmptyList_ReturnsZero()
    {
        StatisticsHelper.CalculateMean(new List<double>()).Should().Be(0);
    }

    [Fact]
    public void CalculateMean_SingleValue_ReturnsThatValue()
    {
        StatisticsHelper.CalculateMean(new List<double> { 42.0 }).Should().Be(42.0);
    }

    [Fact]
    public void CalculateMean_MultipleValues_ReturnsCorrectAverage()
    {
        StatisticsHelper.CalculateMean(new List<double> { 10, 20, 30 }).Should().Be(20.0);
    }

    // -------------------------------------------------------------------------
    // CalculateMedian
    // -------------------------------------------------------------------------

    [Fact]
    public void CalculateMedian_NullList_ReturnsZero()
    {
        StatisticsHelper.CalculateMedian(null!).Should().Be(0);
    }

    [Fact]
    public void CalculateMedian_EmptyList_ReturnsZero()
    {
        StatisticsHelper.CalculateMedian(new List<double>()).Should().Be(0);
    }

    [Fact]
    public void CalculateMedian_OddCount_ReturnsMiddleValue()
    {
        StatisticsHelper.CalculateMedian(new List<double> { 1, 3, 5 }).Should().Be(3.0);
    }

    [Fact]
    public void CalculateMedian_EvenCount_ReturnsAverageOfMiddleTwo()
    {
        StatisticsHelper.CalculateMedian(new List<double> { 1, 2, 3, 4 }).Should().Be(2.5);
    }

    [Fact]
    public void CalculateMedian_UnsortedInput_StillReturnsCorrectMedian()
    {
        StatisticsHelper.CalculateMedian(new List<double> { 5, 1, 3 }).Should().Be(3.0);
    }

    // -------------------------------------------------------------------------
    // CalculateStandardDeviation
    // -------------------------------------------------------------------------

    [Fact]
    public void CalculateStandardDeviation_NullList_ReturnsZero()
    {
        StatisticsHelper.CalculateStandardDeviation(null!).Should().Be(0);
    }

    [Fact]
    public void CalculateStandardDeviation_SingleValue_ReturnsZero()
    {
        StatisticsHelper.CalculateStandardDeviation(new List<double> { 5 }).Should().Be(0);
    }

    [Fact]
    public void CalculateStandardDeviation_IdenticalValues_ReturnsZero()
    {
        StatisticsHelper.CalculateStandardDeviation(new List<double> { 5, 5, 5 }).Should().Be(0);
    }

    [Fact]
    public void CalculateStandardDeviation_VariedValues_ReturnsPositive()
    {
        StatisticsHelper.CalculateStandardDeviation(new List<double> { 1, 5, 10 }).Should().BeGreaterThan(0);
    }

    // -------------------------------------------------------------------------
    // CalculatePercentile
    // -------------------------------------------------------------------------

    [Fact]
    public void CalculatePercentile_NullList_ReturnsZero()
    {
        StatisticsHelper.CalculatePercentile(null!, 50).Should().Be(0);
    }

    [Fact]
    public void CalculatePercentile_EmptyList_ReturnsZero()
    {
        StatisticsHelper.CalculatePercentile(new List<double>(), 50).Should().Be(0);
    }

    [Fact]
    public void CalculatePercentile_InvalidPercentile_ThrowsArgumentException()
    {
        var act = () => StatisticsHelper.CalculatePercentile(new List<double> { 1 }, 101);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CalculatePercentile_NegativePercentile_ThrowsArgumentException()
    {
        var act = () => StatisticsHelper.CalculatePercentile(new List<double> { 1 }, -1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CalculatePercentile_P50_ReturnsMedian()
    {
        var values = new List<double> { 10, 20, 30, 40, 50 };
        var p50 = StatisticsHelper.CalculatePercentile(values, 50);
        p50.Should().BeGreaterThanOrEqualTo(20).And.BeLessThanOrEqualTo(40);
    }

    [Fact]
    public void CalculatePercentile_P99_ReturnsHighValue()
    {
        var values = Enumerable.Range(1, 100).Select(i => (double)i).ToList();
        var p99 = StatisticsHelper.CalculatePercentile(values, 99);
        p99.Should().BeGreaterThanOrEqualTo(99);
    }

    // -------------------------------------------------------------------------
    // CalculateMovingAverage
    // -------------------------------------------------------------------------

    [Fact]
    public void CalculateMovingAverage_NullList_ReturnsEmpty()
    {
        StatisticsHelper.CalculateMovingAverage(null!, 3).Should().BeEmpty();
    }

    [Fact]
    public void CalculateMovingAverage_WindowLargerThanData_ReturnsEmpty()
    {
        StatisticsHelper.CalculateMovingAverage(new List<double> { 1, 2 }, 5).Should().BeEmpty();
    }
}
