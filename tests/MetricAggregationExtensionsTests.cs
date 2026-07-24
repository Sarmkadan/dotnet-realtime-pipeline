using System;
using System.Collections.Generic;
using System.Linq;
using DotNetRealtimePipeline.Domain.Models;
using FluentAssertions;
using Xunit;

namespace DotNetRealtimePipeline.Tests;

public class MetricAggregationExtensionsTests
{
    private readonly MetricAggregation _validAggregation;

    public MetricAggregationExtensionsTests()
    {
        _validAggregation = new MetricAggregation
        {
            MetricId = 1,
            MetricType = "hourly",
            TimeWindowStartMs = 1000,
            TimeWindowEndMs = 2000,
            TotalItemsProcessed = 1000,
            TotalItemsFailed = 50,
            TotalItemsSkipped = 25,
            AverageProcessingTimeMs = 150.5,
            MinProcessingTimeMs = 50.0,
            MaxProcessingTimeMs = 500.0,
            P95ProcessingTimeMs = 350.0,
            P99ProcessingTimeMs = 450.0,
            BackpressureEvents = 3,
            TotalBackpressureMs = 150,
            CountBySource = new Dictionary<string, long>
            {
                {"source1", 600},
                {"source2", 300},
                {"source3", 100}
            },
            ErrorRateByStage = new Dictionary<string, double>
            {
                {"stage1", 0.05},
                {"stage2", 0.10},
                {"stage3", 0.0}
            },
            ComputedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public void CalculateSuccessRate_WithValidAggregation_ReturnsCorrectRate()
    {
        // Act
        var result = _validAggregation.CalculateSuccessRate();

        // Assert
        result.Should().BeApproximately(0.95, 0.001);
    }

    [Fact]
    public void CalculateSuccessRate_WithZeroProcessedItems_ReturnsZero()
    {
        // Arrange
        var aggregation = new MetricAggregation
        {
            TotalItemsProcessed = 0,
            TotalItemsFailed = 0,
            TotalItemsSkipped = 0
        };

        // Act
        var result = aggregation.CalculateSuccessRate();

        // Assert
        result.Should().Be(0.0);
    }

    [Fact]
    public void CalculateSuccessRate_WithNullAggregation_ThrowsArgumentNullException()
    {
        // Arrange
        MetricAggregation? aggregation = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => aggregation!.CalculateSuccessRate());
    }

    [Fact]
    public void CalculateCombinedErrorRate_WithValidAggregation_ReturnsAverageErrorRate()
    {
        // Act
        var result = _validAggregation.CalculateCombinedErrorRate();

        // Assert
        result.Should().BeApproximately(0.05, 0.001); // (0.05 + 0.10 + 0.0) / 3
    }

    [Fact]
    public void CalculateCombinedErrorRate_WithNullErrorRateByStage_ReturnsZero()
    {
        // Arrange
        var aggregation = new MetricAggregation
        {
            ErrorRateByStage = null
        };

        // Act
        var result = aggregation.CalculateCombinedErrorRate();

        // Assert
        result.Should().Be(0.0);
    }

    [Fact]
    public void CalculateCombinedErrorRate_WithEmptyErrorRateByStage_ReturnsZero()
    {
        // Arrange
        var aggregation = new MetricAggregation
        {
            ErrorRateByStage = new Dictionary<string, double>()
        };

        // Act
        var result = aggregation.CalculateCombinedErrorRate();

        // Assert
        result.Should().Be(0.0);
    }

    [Fact]
    public void CalculateCombinedErrorRate_WithNullAggregation_ThrowsArgumentNullException()
    {
        // Arrange
        MetricAggregation? aggregation = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => aggregation!.CalculateCombinedErrorRate());
    }

    [Fact]
    public void GetTimeWindowDurationMs_WithValidAggregation_ReturnsCorrectDuration()
    {
        // Act
        var result = _validAggregation.GetTimeWindowDurationMs();

        // Assert
        result.Should().Be(1000); // 2000 - 1000
    }

    [Fact]
    public void GetTimeWindowDurationMs_WithNullAggregation_ThrowsArgumentNullException()
    {
        // Arrange
        MetricAggregation? aggregation = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => aggregation!.GetTimeWindowDurationMs());
    }

    [Fact]
    public void GetTimeWindowDuration_WithValidAggregation_ReturnsCorrectTimeSpan()
    {
        // Act
        var result = _validAggregation.GetTimeWindowDuration();

        // Assert
        result.Should().Be(TimeSpan.FromMilliseconds(1000));
    }

    [Fact]
    public void GetTimeWindowDuration_WithNullAggregation_ThrowsArgumentNullException()
    {
        // Arrange
        MetricAggregation? aggregation = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => aggregation!.GetTimeWindowDuration());
    }

    [Fact]
    public void GetSourceNames_WithValidAggregation_ReturnsSortedSources()
    {
        // Act
        var result = _validAggregation.GetSourceNames();

        // Assert
        result.Should().BeEquivalentTo(new[] {"source1", "source2", "source3"});
        result.Should().BeInAscendingOrder();
    }

    [Fact]
    public void GetSourceNames_WithNullCountBySource_ReturnsEmptyEnumerable()
    {
        // Arrange
        var aggregation = new MetricAggregation
        {
            CountBySource = null
        };

        // Act
        var result = aggregation.GetSourceNames();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetSourceNames_WithEmptyCountBySource_ReturnsEmptyEnumerable()
    {
        // Arrange
        var aggregation = new MetricAggregation
        {
            CountBySource = new Dictionary<string, long>()
        };

        // Act
        var result = aggregation.GetSourceNames();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetSourceNames_WithNullAggregation_ThrowsArgumentNullException()
    {
        // Arrange
        MetricAggregation? aggregation = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => aggregation!.GetSourceNames());
    }

    [Fact]
    public void GetStagesWithErrors_WithValidAggregation_ReturnsStagesWithErrorsOrderedByRateDesc()
    {
        // Act
        var result = _validAggregation.GetStagesWithErrors();

        // Assert
        result.Should().BeEquivalentTo(new[] {"stage2", "stage1"});
        result.Should().BeInDescendingOrder(x => _validAggregation.ErrorRateByStage![x]);
    }

    [Fact]
    public void GetStagesWithErrors_WithNoErrors_ReturnsEmptyEnumerable()
    {
        // Arrange
        var aggregation = new MetricAggregation
        {
            ErrorRateByStage = new Dictionary<string, double>
            {
                {"stage1", 0.0},
                {"stage2", 0.0}
            }
        };

        // Act
        var result = aggregation.GetStagesWithErrors();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetStagesWithErrors_WithNullErrorRateByStage_ReturnsEmptyEnumerable()
    {
        // Arrange
        var aggregation = new MetricAggregation
        {
            ErrorRateByStage = null
        };

        // Act
        var result = aggregation.GetStagesWithErrors();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetStagesWithErrors_WithNullAggregation_ThrowsArgumentNullException()
    {
        // Arrange
        MetricAggregation? aggregation = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => aggregation!.GetStagesWithErrors());
    }

    [Fact]
    public void GetTotalItemsFromSources_WithValidAggregation_ReturnsSumOfAllSources()
    {
        // Act
        var result = _validAggregation.GetTotalItemsFromSources();

        // Assert
        result.Should().Be(1000); // 600 + 300 + 100
    }

    [Fact]
    public void GetTotalItemsFromSources_WithNullCountBySource_ReturnsZero()
    {
        // Arrange
        var aggregation = new MetricAggregation
        {
            CountBySource = null
        };

        // Act
        var result = aggregation.GetTotalItemsFromSources();

        // Assert
        result.Should().Be(0L);
    }

    [Fact]
    public void GetTotalItemsFromSources_WithEmptyCountBySource_ReturnsZero()
    {
        // Arrange
        var aggregation = new MetricAggregation
        {
            CountBySource = new Dictionary<string, long>()
        };

        // Act
        var result = aggregation.GetTotalItemsFromSources();

        // Assert
        result.Should().Be(0L);
    }

    [Fact]
    public void GetTotalItemsFromSources_WithNullAggregation_ThrowsArgumentNullException()
    {
        // Arrange
        MetricAggregation? aggregation = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => aggregation!.GetTotalItemsFromSources());
    }

    [Fact]
    public void GetBackpressurePercentage_WithValidAggregation_ReturnsCorrectPercentage()
    {
        // Act
        var result = _validAggregation.GetBackpressurePercentage();

        // Assert
        result.Should().BeApproximately(15.0, 0.001); // (150 / 1000) * 100
    }

    [Fact]
    public void GetBackpressurePercentage_WithZeroDuration_ReturnsZero()
    {
        // Arrange
        var aggregation = new MetricAggregation
        {
            TimeWindowStartMs = 1000,
            TimeWindowEndMs = 1000,
            TotalBackpressureMs = 150
        };

        // Act
        var result = aggregation.GetBackpressurePercentage();

        // Assert
        result.Should().Be(0.0);
    }

    [Fact]
    public void GetBackpressurePercentage_WithNullAggregation_ThrowsArgumentNullException()
    {
        // Arrange
        MetricAggregation? aggregation = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => aggregation!.GetBackpressurePercentage());
    }

    [Fact]
    public void GetAveragePercentile_WithValidAggregation_ReturnsAverageOfP95AndP99()
    {
        // Act
        var result = _validAggregation.GetAveragePercentile();

        // Assert
        result.Should().BeApproximately(400.0, 0.001); // (350.0 + 450.0) / 2
    }

    [Fact]
    public void GetAveragePercentile_WithNullAggregation_ThrowsArgumentNullException()
    {
        // Arrange
        MetricAggregation? aggregation = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => aggregation!.GetAveragePercentile());
    }

    [Fact]
    public void Combine_WithValidAggregations_ReturnsCombinedAggregation()
    {
        // Arrange
        var aggregation1 = new MetricAggregation
        {
            MetricId = 1,
            MetricType = "hourly",
            TimeWindowStartMs = 1000,
            TimeWindowEndMs = 2000,
            TotalItemsProcessed = 1000,
            TotalItemsFailed = 50,
            TotalItemsSkipped = 25,
            AverageProcessingTimeMs = 150.5,
            MinProcessingTimeMs = 50.0,
            MaxProcessingTimeMs = 500.0,
            P95ProcessingTimeMs = 350.0,
            P99ProcessingTimeMs = 450.0,
            BackpressureEvents = 3,
            TotalBackpressureMs = 150,
            CountBySource = new Dictionary<string, long> { {"source1", 600} },
            ErrorRateByStage = new Dictionary<string, double> { {"stage1", 0.05} }
        };

        var aggregation2 = new MetricAggregation
        {
            MetricId = 2,
            MetricType = "hourly",
            TimeWindowStartMs = 1000,
            TimeWindowEndMs = 3000,
            TotalItemsProcessed = 2000,
            TotalItemsFailed = 100,
            TotalItemsSkipped = 50,
            AverageProcessingTimeMs = 200.0,
            MinProcessingTimeMs = 60.0,
            MaxProcessingTimeMs = 600.0,
            P95ProcessingTimeMs = 400.0,
            P99ProcessingTimeMs = 500.0,
            BackpressureEvents = 5,
            TotalBackpressureMs = 250,
            CountBySource = new Dictionary<string, long> { {"source2", 1000} },
            ErrorRateByStage = new Dictionary<string, double> { {"stage2", 0.10} }
        };

        var aggregations = new[] { aggregation1, aggregation2 };

        // Act
        var result = aggregations.Combine();

        // Assert
        result.MetricId.Should().Be(1);
        result.MetricType.Should().Be("hourly");
        result.TimeWindowStartMs.Should().Be(1000);
        result.TimeWindowEndMs.Should().Be(3000);
        result.TotalItemsProcessed.Should().Be(3000);
        result.TotalItemsFailed.Should().Be(150);
        result.TotalItemsSkipped.Should().Be(75);
        result.AverageProcessingTimeMs.Should().BeApproximately(175.25, 0.001); // (150.5 + 200.0) / 2
        result.MinProcessingTimeMs.Should().Be(50.0);
        result.MaxProcessingTimeMs.Should().Be(600.0);
        result.P95ProcessingTimeMs.Should().BeApproximately(375.0, 0.001); // (350.0 + 400.0) / 2
        result.P99ProcessingTimeMs.Should().BeApproximately(475.0, 0.001); // (450.0 + 500.0) / 2
        result.BackpressureEvents.Should().Be(8);
        result.TotalBackpressureMs.Should().Be(400);
        result.CountBySource.Should().BeEquivalentTo(new Dictionary<string, long> { {"source1", 600}, {"source2", 1000} });
        result.ErrorRateByStage.Should().BeEquivalentTo(new Dictionary<string, double> { {"stage1", 0.05}, {"stage2", 0.10} });
    }

    [Fact]
    public void Combine_WithEmptyAggregationsCollection_ThrowsArgumentException()
    {
        // Arrange
        var aggregations = Array.Empty<MetricAggregation>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => aggregations.Combine());
    }

    [Fact]
    public void Combine_WithNullAggregations_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<MetricAggregation>? aggregations = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => aggregations!.Combine());
    }

    [Fact]
    public void Combine_WithNullElementsInCollection_ThrowsArgumentNullException()
    {
        // Arrange
        var aggregations = new[] { _validAggregation, null! };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => aggregations.Combine());
    }

    [Fact]
    public void Combine_WithSingleAggregation_ReturnsSameAggregationWithUpdatedEndTime()
    {
        // Arrange
        var aggregation = new MetricAggregation
        {
            MetricId = 1,
            MetricType = "hourly",
            TimeWindowStartMs = 1000,
            TimeWindowEndMs = 2000,
            TotalItemsProcessed = 1000,
            TotalItemsFailed = 50,
            TotalItemsSkipped = 25
        };

        var aggregations = new[] { aggregation };

        // Act
        var result = aggregations.Combine();

        // Assert
        result.Should().BeSameAs(aggregation);
        result.TimeWindowEndMs.Should().Be(2000);
    }
}