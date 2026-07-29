#nullable enable

using Xunit;
using System;
using System.Collections.Generic;
using DotNetRealtimePipeline.Data.Repositories;

namespace DotNetRealtimePipeline.Data.Repositories.Tests;

public class InMemoryDataPointRepositoryValidationTests
{
    [Fact]
    public void Validate_ReturnsEmptyList_ForValidRepository()
    {
        // Arrange
        var repository = new InMemoryDataPointRepository();

        // Act
        var errors = InMemoryDataPointRepositoryValidation.Validate(repository);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_ReturnsListWithErrors_ForInvalidRepository()
    {
        // Arrange
        var repository = new InMemoryDataPointRepository();
        repository.DataPoints.Add(new DataPoint { Id = 1, Timestamp = 0, Source = null, Quality = 101 });

        // Act
        var errors = InMemoryDataPointRepositoryValidation.Validate(repository);

        // Assert
        Assert.Single(errors);
    }

    [Fact]
    public void IsValid_ReturnsTrue_ForValidRepository()
    {
        // Arrange
        var repository = new InMemoryDataPointRepository();

        // Act
        var isValid = InMemoryDataPointRepositoryValidation.IsValid(repository);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_ReturnsFalse_ForInvalidRepository()
    {
        // Arrange
        var repository = new InMemoryDataPointRepository();
        repository.DataPoints.Add(new DataPoint { Id = 1, Timestamp = 0, Source = null, Quality = 101 });

        // Act
        var isValid = InMemoryDataPointRepositoryValidation.IsValid(repository);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void EnsureValid_ThrowsArgumentException_ForInvalidRepository()
    {
        // Arrange
        var repository = new InMemoryDataPointRepository();
        repository.DataPoints.Add(new DataPoint { Id = 1, Timestamp = 0, Source = null, Quality = 101 });

        // Act and Assert
        Assert.Throws<ArgumentException>(() => InMemoryDataPointRepositoryValidation.EnsureValid(repository));
    }

    [Fact]
    public void EnsureValid_DoesNotThrow_ForValidRepository()
    {
        // Arrange
        var repository = new InMemoryDataPointRepository();

        // Act and Assert
        InMemoryDataPointRepositoryValidation.EnsureValid(repository);
    }
}
