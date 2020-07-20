#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetRealtimePipeline.Data.Repositories;
using DotNetRealtimePipeline.Domain.Models;
using Xunit;

namespace DotNetRealtimePipeline.Tests.Unit;

public sealed class DataPointRepositoryTests
{
    private readonly IDataPointRepository _repository;

    public DataPointRepositoryTests()
    {
        _repository = new InMemoryDataPointRepository();
    }

    [Fact]
    public async Task AddAsync_WithValidDataPoint_ShouldSucceed()
    {
        // Arrange
        var dataPoint = new DataPoint(1, DateTime.UtcNow.Ticks, 42.5m, "Sensor-1");

        // Act
        await _repository.AddAsync(dataPoint);

        // Assert
        var retrieved = await _repository.GetByIdAsync(1);
        Assert.NotNull(retrieved);
        Assert.Equal(42.5m, retrieved.Value);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_WithMultiplePoints_ShouldReturnAll()
    {
        // Arrange
        var points = new[]
        {
            new DataPoint(1, DateTime.UtcNow.Ticks, 10, "S1"),
            new DataPoint(2, DateTime.UtcNow.Ticks, 20, "S2"),
            new DataPoint(3, DateTime.UtcNow.Ticks, 30, "S3")
        };

        // Act
        await _repository.AddRangeAsync(points);
        var all = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(3, all.Count());
    }

    [Fact]
    public async Task GetBySourceAsync_WithValidSource_ShouldReturnMatching()
    {
        // Arrange
        var points = new[]
        {
            new DataPoint(1, DateTime.UtcNow.Ticks, 10, "Sensor-1"),
            new DataPoint(2, DateTime.UtcNow.Ticks, 20, "Sensor-2"),
            new DataPoint(3, DateTime.UtcNow.Ticks, 30, "Sensor-1")
        };
        await _repository.AddRangeAsync(points);

        // Act
        var result = await _repository.GetBySourceAsync("Sensor-1");

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task UpdateAsync_WithExistingId_ShouldUpdate()
    {
        // Arrange
        var dataPoint = new DataPoint(1, DateTime.UtcNow.Ticks, 42.5m, "Sensor-1");
        await _repository.AddAsync(dataPoint);

        var updated = new DataPoint(1, DateTime.UtcNow.Ticks, 99.9m, "Sensor-1");

        // Act
        await _repository.UpdateAsync(updated);
        var retrieved = await _repository.GetByIdAsync(1);

        // Assert
        Assert.Equal(99.9m, retrieved.Value);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingId_ShouldRemove()
    {
        // Arrange
        var dataPoint = new DataPoint(1, DateTime.UtcNow.Ticks, 42.5m, "Sensor-1");
        await _repository.AddAsync(dataPoint);

        // Act
        await _repository.DeleteAsync(1);
        var result = await _repository.GetByIdAsync(1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ClearAsync_ShouldRemoveAll()
    {
        // Arrange
        var points = new[]
        {
            new DataPoint(1, DateTime.UtcNow.Ticks, 10, "S1"),
            new DataPoint(2, DateTime.UtcNow.Ticks, 20, "S2")
        };
        await _repository.AddRangeAsync(points);

        // Act
        await _repository.ClearAsync();
        var all = await _repository.GetAllAsync();

        // Assert
        Assert.Empty(all);
    }
}
