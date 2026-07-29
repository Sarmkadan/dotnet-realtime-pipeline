// tests/DynamicScalingServiceJsonExtensionsTests.cs
namespace DotNetRealtimePipeline.Tests;

public class DynamicScalingServiceJsonExtensionsTests
{
    [Fact]
    public void ToJson_HappyPath_ReturnsJsonString()
    {
        // Arrange
        var dynamicScalingService = new DynamicScalingService(new ScalingPolicy(), new PipelineMetrics());

        // Act
        var json = DynamicScalingServiceJsonExtensions.ToJson(dynamicScalingService);

        // Assert
        Assert.IsNotNull(json);
    }

    [Fact]
    public void ToJson_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => DynamicScalingServiceJsonExtensions.ToJson(null));
    }

    [Fact]
    public void FromJson_HappyPath_ReturnsDynamicScalingServiceInstance()
    {
        // Arrange
        var json = "{\"property1\":\"value1\",\"property2\":\"value2\"}";
        var expectedDynamicScalingService = new DynamicScalingService(new ScalingPolicy(), new PipelineMetrics());

        // Act
        var actualDynamicScalingService = DynamicScalingServiceJsonExtensions.FromJson(json);

        // Assert
        Assert.Equal(expectedDynamicScalingService, actualDynamicScalingService);
    }

    [Fact]
    public void FromJson_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => DynamicScalingServiceJsonExtensions.FromJson(null));
    }

    [Fact]
    public void FromJson_EmptyJson_ThrowsJsonException()
    {
        // Act and Assert
        Assert.Throws<JsonException>(() => DynamicScalingServiceJsonExtensions.FromJson(""));
    }

    [Fact]
    public void TryFromJson_HappyPath_ReturnsTrue()
    {
        // Arrange
        var json = "{\"property1\":\"value1\",\"property2\":\"value2\"}";

        // Act
        var result = DynamicScalingServiceJsonExtensions.TryFromJson(json, out var _);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void TryFromJson_NullInput_ReturnsFalse()
    {
        // Act
        var result = DynamicScalingServiceJsonExtensions.TryFromJson(null, out var _);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TryFromJson_EmptyJson_ReturnsFalse()
    {
        // Act
        var result = DynamicScalingServiceJsonExtensions.TryFromJson("", out var _);

        // Assert
        Assert.False(result);
    }
}
