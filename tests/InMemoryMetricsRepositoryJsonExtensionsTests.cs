#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using DotNetRealtimePipeline.Data.Repositories;
using DotNetRealtimePipeline.Domain.Models;
using Xunit;

namespace DotNetRealtimePipeline.Data.Repositories.Tests;

public class InMemoryMetricsRepositoryJsonExtensionsTests
{
    private static List<MetricAggregation> GetInternalMetrics(InMemoryMetricsRepository repository)
    {
        // The repository exposes an internal method GetInternalMetrics().
        // Use reflection to obtain the list so we can manipulate it in tests.
        var method = typeof(InMemoryMetricsRepository)
            .GetMethod("GetInternalMetrics", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (method is null)
            throw new InvalidOperationException("Unable to locate GetInternalMetrics method on InMemoryMetricsRepository.");

        var result = method.Invoke(repository, null);
        return result as List<MetricAggregation>
            ?? throw new InvalidOperationException("GetInternalMetrics did not return a List<MetricAggregation>.");
    }

    [Fact]
    public void ToJson_ReturnsEmptyArray_WhenRepositoryHasNoMetrics()
    {
        // Arrange
        var repository = new InMemoryMetricsRepository();

        // Act
        var json = repository.ToJson();

        // Assert
        Assert.Equal("[]", json);
    }

    [Fact]
    public void ToJson_ReturnsIndentedJson_WhenIndentedTrue()
    {
        // Arrange
        var repository = new InMemoryMetricsRepository();
        var metrics = GetInternalMetrics(repository);
        metrics.Add(new MetricAggregation()); // add a default aggregation

        // Act
        var json = repository.ToJson(indented: true);

        // Assert
        // Indented JSON should contain line breaks; we verify by checking for '\n'
        Assert.Contains('\n', json);
        // Also ensure it is a valid JSON array with one element
        var deserialized = JsonSerializer.Deserialize<List<MetricAggregation>>(json);
        Assert.NotNull(deserialized);
        Assert.Single(deserialized!);
    }

    [Fact]
    public void FromJson_ReturnsRepository_WithEmptyMetrics_WhenJsonIsEmptyArray()
    {
        // Arrange
        const string json = "[]";

        // Act
        var repository = InMemoryMetricsRepositoryJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(repository);
        var internalMetrics = GetInternalMetrics(repository!);
        Assert.Empty(internalMetrics);
    }

    [Fact]
    public void FromJson_ReturnsNull_WhenJsonIsWhiteSpace()
    {
        // Arrange
        const string json = "   ";

        // Act
        var repository = InMemoryMetricsRepositoryJsonExtensions.FromJson(json);

        // Assert
        Assert.Null(repository);
    }

    [Fact]
    public void FromJson_ThrowsArgumentNullException_WhenJsonIsNull()
    {
        // Arrange
        string? json = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => InMemoryMetricsRepositoryJsonExtensions.FromJson(json!));
    }

    [Fact]
    public void TryFromJson_ReturnsTrueAndRepository_WhenValidJson()
    {
        // Arrange
        const string json = "[]";

        // Act
        var result = InMemoryMetricsRepositoryJsonExtensions.TryFromJson(json, out var repository);

        // Assert
        Assert.True(result);
        Assert.NotNull(repository);
        var internalMetrics = GetInternalMetrics(repository!);
        Assert.Empty(internalMetrics);
    }

    [Fact]
    public void TryFromJson_ReturnsFalse_WhenJsonIsInvalid()
    {
        // Arrange
        const string json = "this is not json";

        // Act
        var result = InMemoryMetricsRepositoryJsonExtensions.TryFromJson(json, out var repository);

        // Assert
        Assert.False(result);
        Assert.Null(repository);
    }

    [Fact]
    public void TryFromJson_ThrowsArgumentNullException_WhenJsonIsNull()
    {
        // Arrange
        string? json = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => InMemoryMetricsRepositoryJsonExtensions.TryFromJson(json!, out _));
    }
}
