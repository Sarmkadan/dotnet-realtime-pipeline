#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetRealtimePipeline.Data.Repositories;
using DotNetRealtimePipeline.Domain.Models;
using Xunit;

namespace DotNetRealtimePipeline.Data.Repositories.Tests;

public class InMemoryMetricsRepositoryExtensionsTests
{
    [Fact]
    public async Task GetLatestByTypeAsync_ReturnsLatestMetric_WhenMetricsExist()
    {
        // Arrange
        var repository = new InMemoryMetricsRepository();
        var metric = new MetricAggregation();
        await repository.AddAsync(metric);

        // Act
        var latestMetric = await InMemoryMetricsRepositoryExtensions.GetLatestByTypeAsync(repository, metric.Type);

        // Assert
        Assert.NotNull(latestMetric);
        Assert.Equal(metric, latestMetric);
    }

    [Fact]
    public async Task GetLatestByTypeAsync_ReturnsNull_WhenNoMetricsExist()
    {
        // Arrange
        var repository = new InMemoryMetricsRepository();

        // Act
        var latestMetric = await InMemoryMetricsRepositoryExtensions.GetLatestByTypeAsync(repository, "metric-type");

        // Assert
        Assert.Null(latestMetric);
    }

    [Fact]
    public async Task GetByTypeAndTimeRangeAsync_ReturnsMetrics_WhenMetricsExist()
    {
        // Arrange
        var repository = new InMemoryMetricsRepository();
        var metric1 = new MetricAggregation { TimeWindowStartMs = 100, TimeWindowEndMs = 200 };
        var metric2 = new MetricAggregation { TimeWindowStartMs = 150, TimeWindowEndMs = 250 };
        await repository.AddAsync(metric1);
        await repository.AddAsync(metric2);

        // Act
        var metrics = await InMemoryMetricsRepositoryExtensions.GetByTypeAndTimeRangeAsync(repository, "metric-type", 100, 250);

        // Assert
        Assert.NotNull(metrics);
        Assert.Equal(2, metrics.Count);
        Assert.Contains(metric1, metrics);
        Assert.Contains(metric2, metrics);
    }

    [Fact]
    public async Task GetByTypeAndTimeRangeAsync_ReturnsEmptyList_WhenNoMetricsExist()
    {
        // Arrange
        var repository = new InMemoryMetricsRepository();

        // Act
        var metrics = await InMemoryMetricsRepositoryExtensions.GetByTypeAndTimeRangeAsync(repository, "metric-type", 100, 250);

        // Assert
        Assert.Empty(metrics);
    }

    [Fact]
    public async Task GetAverageProcessingTimeAsync_ReturnsAverageProcessingTime_WhenMetricsExist()
    {
        // Arrange
        var repository = new InMemoryMetricsRepository();
        var metric1 = new MetricAggregation { AverageProcessingTimeMs = 10 };
        var metric2 = new MetricAggregation { AverageProcessingTimeMs = 20 };
        await repository.AddAsync(metric1);
        await repository.AddAsync(metric2);

        // Act
        var averageProcessingTime = await InMemoryMetricsRepositoryExtensions.GetAverageProcessingTimeAsync(repository, "metric-type", 100, 250);

        // Assert
        Assert.NotNull(averageProcessingTime);
        Assert.Equal(15, averageProcessingTime);
    }

    [Fact]
    public async Task GetAverageProcessingTimeAsync_ReturnsNull_WhenNoMetricsExist()
    {
        // Arrange
        var repository = new InMemoryMetricsRepository();

        // Act
        var averageProcessingTime = await InMemoryMetricsRepositoryExtensions.GetAverageProcessingTimeAsync(repository, "metric-type", 100, 250);

        // Assert
        Assert.Null(averageProcessingTime);
    }

    [Fact]
    public async Task GetMaxProcessingTimeAsync_ReturnsMaxProcessingTime_WhenMetricsExist()
    {
        // Arrange
        var repository = new InMemoryMetricsRepository();
        var metric1 = new MetricAggregation { MaxProcessingTimeMs = 10 };
        var metric2 = new MetricAggregation { MaxProcessingTimeMs = 20 };
        await repository.AddAsync(metric1);
        await repository.AddAsync(metric2);

        // Act
        var maxProcessingTime = await InMemoryMetricsRepositoryExtensions.GetMaxProcessingTimeAsync(repository, "metric-type", 100, 250);

        // Assert
        Assert.NotNull(maxProcessingTime);
        Assert.Equal(20, maxProcessingTime);
    }

    [Fact]
    public async Task GetMaxProcessingTimeAsync_ReturnsNull_WhenNoMetricsExist()
    {
        // Arrange
        var repository = new InMemoryMetricsRepository();

        // Act
        var maxProcessingTime = await InMemoryMetricsRepositoryExtensions.GetMaxProcessingTimeAsync(repository, "metric-type", 100, 250);

        // Assert
        Assert.Null(maxProcessingTime);
    }

    [Fact]
    public async Task GetMinProcessingTimeAsync_ReturnsMinProcessingTime_WhenMetricsExist()
    {
        // Arrange
        var repository = new InMemoryMetricsRepository();
        var metric1 = new MetricAggregation { MinProcessingTimeMs = 10 };
        var metric2 = new MetricAggregation { MinProcessingTimeMs = 20 };
        await repository.AddAsync(metric1);
        await repository.AddAsync(metric2);

        // Act
        var minProcessingTime = await InMemoryMetricsRepositoryExtensions.GetMinProcessingTimeAsync(repository, "metric-type", 100, 250);

        // Assert
        Assert.NotNull(minProcessingTime);
        Assert.Equal(10, minProcessingTime);
    }

    [Fact]
    public async Task GetMinProcessingTimeAsync_ReturnsNull_WhenNoMetricsExist()
    {
        // Arrange
        var repository = new InMemoryMetricsRepository();

        // Act
        var minProcessingTime = await InMemoryMetricsRepositoryExtensions.GetMinProcessingTimeAsync(repository, "metric-type", 100, 250);

        // Assert
        Assert.Null(minProcessingTime);
    }

    [Fact]
    public async Task GetByTypesAsync_ReturnsMetrics_WhenMetricsExist()
    {
        // Arrange
        var repository = new InMemoryMetricsRepository();
        var metric1 = new MetricAggregation { Type = "metric-type-1" };
        var metric2 = new MetricAggregation { Type = "metric-type-2" };
        await repository.AddAsync(metric1);
        await repository.AddAsync(metric2);

        // Act
        var metrics = await InMemoryMetricsRepositoryExtensions.GetByTypesAsync(repository, new[] { "metric-type-1", "metric-type-2" });

        // Assert
        Assert.NotNull(metrics);
        Assert.Equal(2, metrics.Count);
        Assert.Contains(metric1, metrics);
        Assert.Contains(metric2, metrics);
    }

    [Fact]
    public async Task GetByTypesAsync_ReturnsEmptyList_WhenNoMetricsExist()
    {
        // Arrange
        var repository = new InMemoryMetricsRepository();

        // Act
        var metrics = await InMemoryMetricsRepositoryExtensions.GetByTypesAsync(repository, new[] { "metric-type-1", "metric-type-2" });

        // Assert
        Assert.Empty(metrics);
    }

    [Fact]
    public async Task GetByTypeAndTimeRangeWithProcessingTimeFilterAsync_ReturnsMetrics_WhenMetricsExist()
    {
        // Arrange
        var repository = new InMemoryMetricsRepository();
        var metric1 = new MetricAggregation { Type = "metric-type", AverageProcessingTimeMs = 10 };
        var metric2 = new MetricAggregation { Type = "metric-type", AverageProcessingTimeMs = 20 };
        await repository.AddAsync(metric1);
        await repository.AddAsync(metric2);

        // Act
        var metrics = await InMemoryMetricsRepositoryExtensions.GetByTypeAndTimeRangeWithProcessingTimeFilterAsync(repository, "metric-type", 100, 250, 15, 25);

        // Assert
        Assert.NotNull(metrics);
        Assert.Equal(2, metrics.Count);
        Assert.Contains(metric1, metrics);
        Assert.Contains(metric2, metrics);
    }

    [Fact]
    public async Task GetByTypeAndTimeRangeWithProcessingTimeFilterAsync_ReturnsEmptyList_WhenNoMetricsExist()
    {
        // Arrange
        var repository = new InMemoryMetricsRepository();

        // Act
        var metrics = await InMemoryMetricsRepositoryExtensions.GetByTypeAndTimeRangeWithProcessingTimeFilterAsync(repository, "metric-type", 100, 250, 15, 25);

        // Assert
        Assert.Empty(metrics);
    }

    [Fact]
    public async Task GetLastNMetricsAsync_ReturnsMetrics_WhenMetricsExist()
    {
        // Arrange
        var repository = new InMemoryMetricsRepository();
        var metric1 = new MetricAggregation { TimeWindowStartMs = 100, TimeWindowEndMs = 200 };
        var metric2 = new MetricAggregation { TimeWindowStartMs = 150, TimeWindowEndMs = 250 };
        await repository.AddAsync(metric1);
        await repository.AddAsync(metric2);

        // Act
        var metrics = await InMemoryMetricsRepositoryExtensions.GetLastNMetricsAsync(repository, 2);

        // Assert
        Assert.NotNull(metrics);
        Assert.Equal(2, metrics.Count);
        Assert.Contains(metric1, metrics);
        Assert.Contains(metric2, metrics);
    }

    [Fact]
    public async Task GetLastNMetricsAsync_ReturnsEmptyList_WhenNoMetricsExist()
    {
        // Arrange
        var repository = new InMemoryMetricsRepository();

        // Act
        var metrics = await InMemoryMetricsRepositoryExtensions.GetLastNMetricsAsync(repository, 2);

        // Assert
        Assert.Empty(metrics);
    }
}
