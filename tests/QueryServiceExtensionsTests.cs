using Xunit;

namespace DotNetRealtimePipeline.Services.Tests;

public class QueryServiceExtensionsTests
{
    [Fact]
    public async Task GetAggregateStatisticsAsync_HappyPath()
    {
        // Arrange
        var queryService = new QueryService();
        var start = DateTime.UtcNow;
        var end = DateTime.UtcNow.AddHours(1);

        // Act
        var result = await QueryServiceExtensions.GetAggregateStatisticsAsync(queryService, start, end);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetAggregateStatisticsAsync_NullQueryService_ThrowsArgumentNullException()
    {
        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => QueryServiceExtensions.GetAggregateStatisticsAsync(null, DateTime.UtcNow, DateTime.UtcNow));
    }

    [Fact]
    public async Task GetAggregateStatisticsAsync_EmptyTimeRange_ThrowsArgumentException()
    {
        // Act and Assert
        await Assert.ThrowsAsync<ArgumentException>(() => QueryServiceExtensions.GetAggregateStatisticsAsync(new QueryService(), DateTime.UtcNow, DateTime.UtcNow));
    }

    [Fact]
    public async Task SearchDataPointsAsync_HappyPath()
    {
        // Arrange
        var queryService = new QueryService();
        var predicate = (dataPoint) => true;

        // Act
        var result = await QueryServiceExtensions.SearchDataPointsAsync(queryService, predicate);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task SearchDataPointsAsync_NullQueryService_ThrowsArgumentNullException()
    {
        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => QueryServiceExtensions.SearchDataPointsAsync(null, (dataPoint) => true));
    }

    [Fact]
    public async Task SearchDataPointsAsync_NullPredicate_ThrowsArgumentNullException()
    {
        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => QueryServiceExtensions.SearchDataPointsAsync(new QueryService(), null));
    }

    [Fact]
    public async Task GetRecentMetricsAsync_HappyPath()
    {
        // Arrange
        var queryService = new QueryService();
        var count = 10;

        // Act
        var result = await QueryServiceExtensions.GetRecentMetricsAsync(queryService, count);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetRecentMetricsAsync_NullQueryService_ThrowsArgumentNullException()
    {
        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => QueryServiceExtensions.GetRecentMetricsAsync(null, 10));
    }

    [Fact]
    public async Task GetRecentMetricsAsync_ZeroCount_ThrowsArgumentOutOfRangeException()
    {
        // Act and Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => QueryServiceExtensions.GetRecentMetricsAsync(new QueryService(), 0));
    }
}
