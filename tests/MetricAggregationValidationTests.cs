using System;
using System.Collections.Generic;
using DotNetRealtimePipeline.Domain.Models;
using Xunit;

namespace DotNetRealtimePipeline.Tests.Domain.Models;

public class MetricAggregationValidationTests
{
    [Fact]
    public void Validate_ValidMetricAggregation_ReturnsEmptyList()
    {
        // Arrange
        var metricAggregation = new MetricAggregation
        {
            MetricId = 1,
            TimeWindowStartMs = 0,
            TimeWindowEndMs = 1000,
            MetricType = "TestType",
            TotalItemsProcessed = 100,
            TotalItemsFailed = 10,
            TotalItemsSkipped = 5,
            AverageProcessingTimeMs = 10,
            MinProcessingTimeMs = 1,
            MaxProcessingTimeMs = 20,
            P95ProcessingTimeMs = 15,
            P99ProcessingTimeMs = 18,
            BackpressureEvents = 2,
            TotalBackpressureMs = 200,
            ComputedAt = DateTime.UtcNow,
            CountBySource = new Dictionary<string, long> { { "Source1", 50 }, { "Source2", 50 } },
            ErrorRateByStage = new Dictionary<string, double> { { "Stage1", 5.0 }, { "Stage2", 5.0 } }
        };

        // Act
        var result = metricAggregation.Validate();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_InvalidMetricAggregation_ReturnsErrorList()
    {
        // Arrange
        var metricAggregation = new MetricAggregation
        {
            MetricId = -1,
            TimeWindowStartMs = -1,
            TimeWindowEndMs = -1,
            MetricType = "",
            TotalItemsProcessed = -1,
            TotalItemsFailed = -1,
            TotalItemsSkipped = -1,
            AverageProcessingTimeMs = -1,
            MinProcessingTimeMs = -1,
            MaxProcessingTimeMs = -1,
            P95ProcessingTimeMs = -1,
            P99ProcessingTimeMs = -1,
            BackpressureEvents = -1,
            TotalBackpressureMs = -1,
            ComputedAt = default,
            CountBySource = null,
            ErrorRateByStage = null
        };

        // Act
        var result = metricAggregation.Validate();

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains(result, error => error.Contains("MetricId"));
        Assert.Contains(result, error => error.Contains("TimeWindowStartMs"));
        Assert.Contains(result, error => error.Contains("TimeWindowEndMs"));
        Assert.Contains(result, error => error.Contains("MetricType"));
        Assert.Contains(result, error => error.Contains("TotalItemsProcessed"));
        Assert.Contains(result, error => error.Contains("TotalItemsFailed"));
        Assert.Contains(result, error => error.Contains("TotalItemsSkipped"));
        Assert.Contains(result, error => error.Contains("AverageProcessingTimeMs"));
        Assert.Contains(result, error => error.Contains("MinProcessingTimeMs"));
        Assert.Contains(result, error => error.Contains("MaxProcessingTimeMs"));
        Assert.Contains(result, error => error.Contains("P95ProcessingTimeMs"));
        Assert.Contains(result, error => error.Contains("P99ProcessingTimeMs"));
        Assert.Contains(result, error => error.Contains("BackpressureEvents"));
        Assert.Contains(result, error => error.Contains("TotalBackpressureMs"));
        Assert.Contains(result, error => error.Contains("ComputedAt"));
        Assert.Contains(result, error => error.Contains("CountBySource"));
        Assert.Contains(result, error => error.Contains("ErrorRateByStage"));
    }

    [Fact]
    public void IsValid_ValidMetricAggregation_ReturnsTrue()
    {
        // Arrange
        var metricAggregation = new MetricAggregation
        {
            MetricId = 1,
            TimeWindowStartMs = 0,
            TimeWindowEndMs = 1000,
            MetricType = "TestType",
            TotalItemsProcessed = 100,
            TotalItemsFailed = 10,
            TotalItemsSkipped = 5,
            AverageProcessingTimeMs = 10,
            MinProcessingTimeMs = 1,
            MaxProcessingTimeMs = 20,
            P95ProcessingTimeMs = 15,
            P99ProcessingTimeMs = 18,
            BackpressureEvents = 2,
            TotalBackpressureMs = 200,
            ComputedAt = DateTime.UtcNow,
            CountBySource = new Dictionary<string, long> { { "Source1", 50 }, { "Source2", 50 } },
            ErrorRateByStage = new Dictionary<string, double> { { "Stage1", 5.0 }, { "Stage2", 5.0 } }
        };

        // Act
        var result = metricAggregation.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_InvalidMetricAggregation_ReturnsFalse()
    {
        // Arrange
        var metricAggregation = new MetricAggregation
        {
            MetricId = -1,
            TimeWindowStartMs = -1,
            TimeWindowEndMs = -1,
            MetricType = "",
            TotalItemsProcessed = -1,
            TotalItemsFailed = -1,
            TotalItemsSkipped = -1,
            AverageProcessingTimeMs = -1,
            MinProcessingTimeMs = -1,
            MaxProcessingTimeMs = -1,
            P95ProcessingTimeMs = -1,
            P99ProcessingTimeMs = -1,
            BackpressureEvents = -1,
            TotalBackpressureMs = -1,
            ComputedAt = default,
            CountBySource = null,
            ErrorRateByStage = null
        };

        // Act
        var result = metricAggregation.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EnsureValid_ValidMetricAggregation_DoesNotThrow()
    {
        // Arrange
        var metricAggregation = new MetricAggregation
        {
            MetricId = 1,
            TimeWindowStartMs = 0,
            TimeWindowEndMs = 1000,
            MetricType = "TestType",
            TotalItemsProcessed = 100,
            TotalItemsFailed = 10,
            TotalItemsSkipped = 5,
            AverageProcessingTimeMs = 10,
            MinProcessingTimeMs = 1,
            MaxProcessingTimeMs = 20,
            P95ProcessingTimeMs = 15,
            P99ProcessingTimeMs = 18,
            BackpressureEvents = 2,
            TotalBackpressureMs = 200,
            ComputedAt = DateTime.UtcNow,
            CountBySource = new Dictionary<string, long> { { "Source1", 50 }, { "Source2", 50 } },
            ErrorRateByStage = new Dictionary<string, double> { { "Stage1", 5.0 }, { "Stage2", 5.0 } }
        };

        // Act & Assert
        var exception = Record.Exception(() => metricAggregation.EnsureValid());
        Assert.Null(exception);
    }

    [Fact]
    public void EnsureValid_InvalidMetricAggregation_ThrowsArgumentException()
    {
        // Arrange
        var metricAggregation = new MetricAggregation
        {
            MetricId = -1,
            TimeWindowStartMs = -1,
            TimeWindowEndMs = -1,
            MetricType = "",
            TotalItemsProcessed = -1,
            TotalItemsFailed = -1,
            TotalItemsSkipped = -1,
            AverageProcessingTimeMs = -1,
            MinProcessingTimeMs = -1,
            MaxProcessingTimeMs = -1,
            P95ProcessingTimeMs = -1,
            P99ProcessingTimeMs = -1,
            BackpressureEvents = -1,
            TotalBackpressureMs = -1,
            ComputedAt = default,
            CountBySource = null,
            ErrorRateByStage = null
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => metricAggregation.EnsureValid());
    }

    [Fact]
    public void Validate_NullMetricAggregation_ThrowsArgumentNullException()
    {
        // Arrange
        MetricAggregation metricAggregation = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => metricAggregation.Validate());
    }

    [Fact]
    public void IsValid_NullMetricAggregation_ThrowsArgumentNullException()
    {
        // Arrange
        MetricAggregation metricAggregation = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => metricAggregation.IsValid());
    }

    [Fact]
    public void EnsureValid_NullMetricAggregation_ThrowsArgumentNullException()
    {
        // Arrange
        MetricAggregation metricAggregation = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => metricAggregation.EnsureValid());
    }
}
