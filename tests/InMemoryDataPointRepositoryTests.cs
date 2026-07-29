#nullable enable

using Xunit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetRealtimePipeline.Data.Repositories;
using DotNetRealtimePipeline.Domain.Models;

namespace DotNetRealtimePipeline.Data.Repositories.Tests;

public class InMemoryDataPointRepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_HappyPath_ReturnsDataPoint()
    {
        // Arrange
        var repository = new InMemoryDataPointRepository();
        var dataPoint = new DataPoint { Id = 1, Source = "TestSource" };
        await repository.CreateAsync(dataPoint);

        // Act
        var result = await repository.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("TestSource", result.Source);
    }

    [Fact]
    public async Task GetByIdAsync_NullId_ThrowsArgumentNullException()
    {
        // Arrange
        var repository = new InMemoryDataPointRepository();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => repository.GetByIdAsync(null));
    }

    [Fact]
    public async Task GetBySourceAsync_HappyPath_ReturnsDataPoints()
    {
        // Arrange
        var repository = new InMemoryDataPointRepository();
        var dataPoints = new List<DataPoint>
        {
            new DataPoint { Id = 1, Source = "TestSource" },
            new DataPoint { Id = 2, Source = "TestSource" }
        };
        foreach (var dataPoint in dataPoints)
        {
            await repository.CreateAsync(dataPoint);
        }

        // Act
        var result = await repository.GetBySourceAsync("TestSource");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].Id);
        Assert.Equal("TestSource", result[0].Source);
        Assert.Equal(2, result[1].Id);
        Assert.Equal("TestSource", result[1].Source);
    }

    [Fact]
    public async Task GetByTimeRangeAsync_HappyPath_ReturnsDataPoints()
    {
        // Arrange
        var repository = new InMemoryDataPointRepository();
        var dataPoints = new List<DataPoint>
        {
            new DataPoint { Id = 1, Timestamp = 100, Source = "TestSource" },
            new DataPoint { Id = 2, Timestamp = 200, Source = "TestSource" },
            new DataPoint { Id = 3, Timestamp = 300, Source = "TestSource" }
        };
        foreach (var dataPoint in dataPoints)
        {
            await repository.CreateAsync(dataPoint);
        }

        // Act
        var result = await repository.GetByTimeRangeAsync(150, 250);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Count);
        Assert.Equal(2, result[0].Id);
        Assert.Equal(200, result[0].Timestamp);
        Assert.Equal("TestSource", result[0].Source);
    }

    [Fact]
    public async Task GetByQualityThresholdAsync_HappyPath_ReturnsDataPoints()
    {
        // Arrange
        var repository = new InMemoryDataPointRepository();
        var dataPoints = new List<DataPoint>
        {
            new DataPoint { Id = 1, Quality = 50, Source = "TestSource" },
            new DataPoint { Id = 2, Quality = 75, Source = "TestSource" },
            new DataPoint { Id = 3, Quality = 90, Source = "TestSource" }
        };
        foreach (var dataPoint in dataPoints)
        {
            await repository.CreateAsync(dataPoint);
        }

        // Act
        var result = await repository.GetByQualityThresholdAsync(50);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Equal(1, result[0].Id);
        Assert.Equal(50, result[0].Quality);
        Assert.Equal("TestSource", result[0].Source);
        Assert.Equal(2, result[1].Id);
        Assert.Equal(75, result[1].Quality);
        Assert.Equal("TestSource", result[1].Source);
        Assert.Equal(3, result[2].Id);
        Assert.Equal(90, result[2].Quality);
        Assert.Equal("TestSource", result[2].Source);
    }

    [Fact]
    public async Task CreateAsync_NullDataPoint_ThrowsArgumentNullException()
    {
        // Arrange
        var repository = new InMemoryDataPointRepository();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => repository.CreateAsync(null));
    }

    [Fact]
    public async Task UpdateAsync_NullDataPoint_ThrowsArgumentNullException()
    {
        // Arrange
        var repository = new InMemoryDataPointRepository();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => repository.UpdateAsync(null));
    }

    [Fact]
    public async Task DeleteAsync_NullId_ThrowsArgumentNullException()
    {
        // Arrange
        var repository = new InMemoryDataPointRepository();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => repository.DeleteAsync(null));
    }

    [Fact]
    public async Task CountAsync_HappyPath_ReturnsCount()
    {
        // Arrange
        var repository = new InMemoryDataPointRepository();
        var dataPoints = new List<DataPoint>
        {
            new DataPoint { Id = 1, Source = "TestSource" },
            new DataPoint { Id = 2, Source = "TestSource" }
        };
        foreach (var dataPoint in dataPoints)
        {
            await repository.CreateAsync(dataPoint);
        }

        // Act
        var result = await repository.CountAsync();

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public async Task GetPagedAsync_HappyPath_ReturnsDataPoints()
    {
        // Arrange
        var repository = new InMemoryDataPointRepository();
        var dataPoints = new List<DataPoint>
        {
            new DataPoint { Id = 1, Source = "TestSource" },
            new DataPoint { Id = 2, Source = "TestSource" },
            new DataPoint { Id = 3, Source = "TestSource" },
            new DataPoint { Id = 4, Source = "TestSource" },
            new DataPoint { Id = 5, Source = "TestSource" }
        };
        foreach (var dataPoint in dataPoints)
        {
            await repository.CreateAsync(dataPoint);
        }

        // Act
        var result = await repository.GetPagedAsync(1, 2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].Id);
        Assert.Equal("TestSource", result[0].Source);
        Assert.Equal(2, result[1].Id);
        Assert.Equal("TestSource", result[1].Source);
    }

    [Fact]
    public async Task Clear_HappyPath_ClearsDataPoints()
    {
        // Arrange
        var repository = new InMemoryDataPointRepository();
        var dataPoints = new List<DataPoint>
        {
            new DataPoint { Id = 1, Source = "TestSource" },
            new DataPoint { Id = 2, Source = "TestSource" }
        };
        foreach (var dataPoint in dataPoints)
        {
            await repository.CreateAsync(dataPoint);
        }

        // Act
        repository.Clear();

        // Assert
        Assert.Empty(repository.GetInternalStore());
    }
}
