// tests/CacheServiceValidationTests.cs
using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit;

namespace DotNetRealtimePipeline.Tests.Caching;

public class CacheServiceValidationTests
{
    [Fact]
    public void Validate_HappyPath_ReturnsEmpty()
    {
        // Arrange
        var cacheService = new CacheService<string, string>();

        // Act
        var problems = CacheServiceValidation.Validate(cacheService);

        // Assert
        Assert.Empty(problems);
        Assert.True(CacheServiceValidation.IsValid(cacheService));
    }

    [Fact]
    public void Validate_NullCacheService_ThrowsArgumentNullException()
    {
        // Arrange
        CacheService<string, string>? nullCacheService = null;

        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => CacheServiceValidation.Validate(nullCacheService));
    }

    [Fact]
    public void Validate_EmptyStatistics_ReturnsEmpty()
    {
        // Arrange
        var cacheService = new CacheService<string, string>();
        cacheService.GetStatistics = () => new CacheStatistics();

        // Act
        var problems = CacheServiceValidation.Validate(cacheService);

        // Assert
        Assert.Empty(problems);
        Assert.True(CacheServiceValidation.IsValid(cacheService));
    }

    [Fact]
    public void Validate_InvalidStatistics_ReturnsError()
    {
        // Arrange
        var cacheService = new CacheService<string, string>();
        cacheService.GetStatistics = () => new CacheStatistics
        {
            TotalHits = -1,
            TotalMisses = -1,
            CurrentSize = -1,
            MaxCapacity = 0,
            UtilizationPercent = 101,
            HitRate = 101
        };

        // Act
        var problems = CacheServiceValidation.Validate(cacheService);

        // Assert
        Assert.Single(problems);
        Assert.Contains("Statistics.TotalHits cannot be negative, but was -1.", problems[0]);
    }

    [Fact]
    public void IsValid_HappyPath_ReturnsTrue()
    {
        // Arrange
        var cacheService = new CacheService<string, string>();

        // Act
        var isValid = CacheServiceValidation.IsValid(cacheService);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_NullCacheService_ThrowsArgumentNullException()
    {
        // Arrange
        CacheService<string, string>? nullCacheService = null;

        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => CacheServiceValidation.IsValid(nullCacheService));
    }

    [Fact]
    public void EnsureValid_HappyPath_DoesNotThrow()
    {
        // Arrange
        var cacheService = new CacheService<string, string>();

        // Act and Assert
        var exception = Record.Exception(() => CacheServiceValidation.EnsureValid(cacheService));
        Assert.Null(exception);
    }

    [Fact]
    public void EnsureValid_NullCacheService_ThrowsArgumentNullException()
    {
        // Arrange
        CacheService<string, string>? nullCacheService = null;

        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => CacheServiceValidation.EnsureValid(nullCacheService));
    }

    [Fact]
    public void EnsureValid_InvalidStatistics_ThrowsArgumentException()
    {
        // Arrange
        var cacheService = new CacheService<string, string>();
        cacheService.GetStatistics = () => new CacheStatistics
        {
            TotalHits = -1,
            TotalMisses = -1,
            CurrentSize = -1,
            MaxCapacity = 0,
            UtilizationPercent = 101,
            HitRate = 101
        };

        // Act and Assert
        var ex = Assert.Throws<ArgumentException>(() => CacheServiceValidation.EnsureValid(cacheService));
        Assert.Contains("CacheService is invalid. Problems:", ex.Message);
    }
}
