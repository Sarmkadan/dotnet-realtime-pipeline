// tests/MetricAggregationJsonExtensionsTests.cs
namespace DotNetRealtimePipeline.Tests;

public sealed class MetricAggregationJsonExtensionsTests
{
    [Fact]
    public void ToJson_HappyPath_ReturnsExpectedJson()
    {
        // Arrange
        var aggregation = new MetricAggregation
        {
            Source = "Source",
            Count = 10,
            ErrorRate = 0.5
        };

        // Act
        var json = MetricAggregationJsonExtensions.ToJson(aggregation);

        // Assert
        Assert.Equal("{\"source\":\"Source\",\"count\":10,\"errorRate\":0.5}", json);
    }

    [Fact]
    public void ToJson_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        MetricAggregation? aggregation = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => MetricAggregationJsonExtensions.ToJson(aggregation));
    }

    [Fact]
    public void FromJson_HappyPath_ReturnsExpectedAggregation()
    {
        // Arrange
        var json = "{\"source\":\"Source\",\"count\":10,\"errorRate\":0.5}";

        // Act
        var aggregation = MetricAggregationJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(aggregation);
        Assert.Equal("Source", aggregation.Source);
        Assert.Equal(10, aggregation.Count);
        Assert.Equal(0.5, aggregation.ErrorRate);
    }

    [Fact]
    public void FromJson_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        string? json = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => MetricAggregationJsonExtensions.FromJson(json));
    }

    [Fact]
    public void FromJson_InvalidJson_ThrowsJsonException()
    {
        // Arrange
        var json = "{\"source\":\"Source\",\"count\":10\"}";

        // Act & Assert
        Assert.Throws<JsonException>(() => MetricAggregationJsonExtensions.FromJson(json));
    }

    [Fact]
    public void TryFromJson_HappyPath_ReturnsTrue()
    {
        // Arrange
        var json = "{\"source\":\"Source\",\"count\":10,\"errorRate\":0.5}";

        // Act
        var result = MetricAggregationJsonExtensions.TryFromJson(json, out var aggregation);

        // Assert
        Assert.True(result);
        Assert.NotNull(aggregation);
        Assert.Equal("Source", aggregation.Source);
        Assert.Equal(10, aggregation.Count);
        Assert.Equal(0.5, aggregation.ErrorRate);
    }

    [Fact]
    public void TryFromJson_NullInput_ReturnsFalse()
    {
        // Arrange
        string? json = null;

        // Act
        var result = MetricAggregationJsonExtensions.TryFromJson(json, out var aggregation);

        // Assert
        Assert.False(result);
        Assert.Null(aggregation);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalse()
    {
        // Arrange
        var json = "{\"source\":\"Source\",\"count\":10\"}";

        // Act
        var result = MetricAggregationJsonExtensions.TryFromJson(json, out var aggregation);

        // Assert
        Assert.False(result);
        Assert.Null(aggregation);
    }
}
