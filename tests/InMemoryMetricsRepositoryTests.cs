#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetRealtimePipeline.Data.Repositories;
using DotNetRealtimePipeline.Domain.Models;
using Xunit;

namespace DotNetRealtimePipeline.Data.Repositories.Tests;

public sealed class InMemoryMetricsRepositoryTests
{
    private static MetricAggregation CreateMetric(
        long metricId = 1,
        string metricType = "test",
        long startMs = 1000,
        long endMs = 2000)
    {
        var metric = new MetricAggregation
        {
            MetricId = metricId,
            MetricType = metricType,
            TimeWindowStartMs = startMs,
            TimeWindowEndMs = endMs
        };
        return metric;
    }

    [Fact]
    public async Task SaveAsync_AddsMetric_And_GetByIdReturnsIt()
    {
        var repo = new InMemoryMetricsRepository();
        var metric = CreateMetric(metricId: 42);

        await repo.SaveAsync(metric);
        var retrieved = await repo.GetByIdAsync(42);

        Assert.NotNull(retrieved);
        Assert.Equal(42, retrieved!.MetricId);
    }

    [Fact]
    public async Task SaveAsync_ReplacesExistingMetric_WithSameId()
    {
        var repo = new InMemoryMetricsRepository();
        var first = CreateMetric(metricId: 7, startMs: 100);
        var second = CreateMetric(metricId: 7, startMs: 200);

        await repo.SaveAsync(first);
        await repo.SaveAsync(second);

        var all = await repo.GetHistoryAsync(10);
        Assert.Single(all);
        Assert.Equal(200, all[0].TimeWindowStartMs);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenMetricDoesNotExist()
    {
        var repo = new InMemoryMetricsRepository();
        var result = await repo.GetByIdAsync(999);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByTimeRangeAsync_ReturnsMetricsWithinRange()
    {
        var repo = new InMemoryMetricsRepository();
        await repo.SaveAsync(CreateMetric(metricId: 1, startMs: 100, endMs: 200));
        await repo.SaveAsync(CreateMetric(metricId: 2, startMs: 300, endMs: 400));
        await repo.SaveAsync(CreateMetric(metricId: 3, startMs: 500, endMs: 600));

        var result = await repo.GetByTimeRangeAsync(250, 550);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, m => m.MetricId == 2);
        Assert.Contains(result, m => m.MetricId == 3);
    }

    [Fact]
    public async Task GetByTimeRangeAsync_ThrowsArgumentException_WhenStartGreaterThanEnd()
    {
        var repo = new InMemoryMetricsRepository();
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await repo.GetByTimeRangeAsync(1000, 500));
    }

    [Fact]
    public async Task GetByTypeAsync_ReturnsMetricsOfGivenType()
    {
        var repo = new InMemoryMetricsRepository();
        await repo.SaveAsync(CreateMetric(metricId: 1, metricType: "typeA"));
        await repo.SaveAsync(CreateMetric(metricId: 2, metricType: "typeB"));
        await repo.SaveAsync(CreateMetric(metricId: 3, metricType: "typeA"));

        var result = await repo.GetByTypeAsync("typeA");
        Assert.Equal(2, result.Count);
        Assert.All(result, m => Assert.Equal("typeA", m.MetricType));
    }

    [Fact]
    public async Task GetByTypeAsync_ThrowsArgumentException_WhenTypeIsNullOrWhiteSpace()
    {
        var repo = new InMemoryMetricsRepository();
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await repo.GetByTypeAsync(string.Empty));
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await repo.GetByTypeAsync(null!));
    }

    [Fact]
    public async Task DeleteAsync_RemovesExistingMetric_ReturnsTrue()
    {
        var repo = new InMemoryMetricsRepository();
        await repo.SaveAsync(CreateMetric(metricId: 10));

        var deleted = await repo.DeleteAsync(10);
        Assert.True(deleted);

        var after = await repo.GetByIdAsync(10);
        Assert.Null(after);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenMetricDoesNotExist()
    {
        var repo = new InMemoryMetricsRepository();
        var result = await repo.DeleteAsync(12345);
        Assert.False(result);
    }

    [Fact]
    public async Task GetLatestAsync_ReturnsMostRecentMetric()
    {
        var repo = new InMemoryMetricsRepository();
        await repo.SaveAsync(CreateMetric(metricId: 1, startMs: 100));
        await repo.SaveAsync(CreateMetric(metricId: 2, startMs: 200));

        var latest = await repo.GetLatestAsync();
        Assert.Equal(2, latest.MetricId);
    }

    [Fact]
    public async Task GetLatestAsync_ThrowsInvalidOperationException_WhenRepositoryIsEmpty()
    {
        var repo = new InMemoryMetricsRepository();
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await repo.GetLatestAsync());
    }

    [Fact]
    public async Task GetHistoryAsync_ReturnsLastNMetrics_InReverseChronologicalOrder()
    {
        var repo = new InMemoryMetricsRepository();
        await repo.SaveAsync(CreateMetric(metricId: 1, startMs: 100));
        await repo.SaveAsync(CreateMetric(metricId: 2, startMs: 200));
        await repo.SaveAsync(CreateMetric(metricId: 3, startMs: 300));

        var history = await repo.GetHistoryAsync(2);
        Assert.Equal(2, history.Count);
        Assert.Equal(3, history[0].MetricId);
        Assert.Equal(2, history[1].MetricId);
    }

    [Fact]
    public async Task GetHistoryAsync_ThrowsArgumentException_WhenCountIsLessThanOne()
    {
        var repo = new InMemoryMetricsRepository();
        await Assert.ThrowsAsync<ArgumentException>(async () => await repo.GetHistoryAsync(0));
    }

    [Fact]
    public async Task Clear_RemovesAllMetrics()
    {
        var repo = new InMemoryMetricsRepository();
        await repo.SaveAsync(CreateMetric(metricId: 1));
        await repo.SaveAsync(CreateMetric(metricId: 2));

        repo.Clear();

        var all = await repo.GetHistoryAsync(10);
        Assert.Empty(all);
    }

    [Fact]
    public async Task SaveAsync_ThrowsArgumentNullException_WhenMetricIsNull()
    {
        var repo = new InMemoryMetricsRepository();
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await repo.SaveAsync(null!));
    }
}
