namespace DotNetRealtimePipeline.Tests;

using Xunit;

public class PerformanceHelperJsonExtensionsTests
{
    [Fact]
    public void ToJson_HappyPath_ReturnsJsonString()
    {
        // Arrange
        var performanceHelper = new PerformanceHelper();

        // Act
        var json = PerformanceHelperJsonExtensions.ToJson(performanceHelper);

        // Assert
        Assert.NotNull(json);
    }

    [Fact]
    public void ToJson_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => PerformanceHelperJsonExtensions.ToJson(null));
    }

    [Fact]
    public void FromJson_HappyPath_ReturnsPerformanceHelperInstance()
    {
        // Arrange
        var json = "{\"performanceHelper\": {}}";

        // Act
        var performanceHelper = PerformanceHelperJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(performanceHelper);
    }

    [Fact]
    public void FromJson_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => PerformanceHelperJsonExtensions.FromJson(null));
    }

    [Fact]
    public void FromJson_EmptyJson_ReturnsNull()
    {
        // Act
        var performanceHelper = PerformanceHelperJsonExtensions.FromJson("");

        // Assert
        Assert.Null(performanceHelper);
    }

    [Fact]
    public void TryFromJson_HappyPath_ReturnsTrue()
    {
        // Arrange
        var json = "{\"performanceHelper\": {}}";

        // Act
        var result = PerformanceHelperJsonExtensions.TryFromJson(json, out var performanceHelper);

        // Assert
        Assert.True(result);
        Assert.NotNull(performanceHelper);
    }

    [Fact]
    public void TryFromJson_NullInput_ReturnsFalse()
    {
        // Act
        var result = PerformanceHelperJsonExtensions.TryFromJson(null, out var performanceHelper);

        // Assert
        Assert.False(result);
        Assert.Null(performanceHelper);
    }

    [Fact]
    public void TryFromJson_EmptyJson_ReturnsFalse()
    {
        // Act
        var result = PerformanceHelperJsonExtensions.TryFromJson("", out var performanceHelper);

        // Assert
        Assert.False(result);
        Assert.Null(performanceHelper);
    }
}
