// tests/SerializationHelperExtensionsTests.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using DotNetRealtimePipeline.Utilities;

namespace DotNetRealtimePipeline.Tests.Utilities;

public class SerializationHelperExtensionsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    [Fact]
    public async Task SerializeResultsToFileAsync_HappyPath_SerializesResultsToFile()
    {
        // Arrange
        var results = new List<ProcessingResult> { new ProcessingResult() };
        var filePath = "test.json";

        // Act
        await SerializationHelperExtensions.SerializeResultsToFileAsync(results, filePath);

        // Assert
        var content = await System.IO.File.ReadAllTextAsync(filePath, System.Text.Encoding.UTF8);
        var deserializedResults = JsonSerializer.Deserialize<List<ProcessingResult>>(content, JsonOptions);
        Assert.NotNull(deserializedResults);
        Assert.Equal(results.Count, deserializedResults.Count);
    }

    [Fact]
    public async Task SerializeResultsToFileAsync_NullResults_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => SerializationHelperExtensions.SerializeResultsToFileAsync(null, "test.json"));
    }

    [Fact]
    public async Task SerializeResultsToFileAsync_EmptyResults_DoesNotThrow()
    {
        // Act
        await SerializationHelperExtensions.SerializeResultsToFileAsync(new List<ProcessingResult>(), "test.json");
    }

    [Fact]
    public async Task DeserializeResultsFromFileAsync_HappyPath_DeserializesResultsFromFile()
    {
        // Arrange
        var results = new List<ProcessingResult> { new ProcessingResult() };
        var filePath = "test.json";
        var content = SerializationHelperExtensions.SerializeResultsToFileAsync(results, filePath).Result;
        await System.IO.File.WriteAllTextAsync(filePath, content, System.Text.Encoding.UTF8);

        // Act
        var deserializedResults = await SerializationHelperExtensions.DeserializeResultsFromFileAsync(filePath);

        // Assert
        Assert.NotNull(deserializedResults);
        Assert.Equal(results.Count, deserializedResults.Count);
    }

    [Fact]
    public async Task DeserializeResultsFromFileAsync_NullFilePath_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => SerializationHelperExtensions.DeserializeResultsFromFileAsync(null));
    }

    [Fact]
    public async Task DeserializeResultsFromFileAsync_EmptyFilePath_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => SerializationHelperExtensions.DeserializeResultsFromFileAsync(""));
    }

    [Fact]
    public async Task DeserializeResultsFromFileAsync_FileDoesNotExist_ThrowsFileNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => SerializationHelperExtensions.DeserializeResultsFromFileAsync("non-existent-file.json"));
    }

    [Fact]
    public async Task SerializeMetricsToFileAsync_HappyPath_SerializesMetricsToFile()
    {
        // Arrange
        var metrics = new MetricAggregation();
        var filePath = "test.json";

        // Act
        await SerializationHelperExtensions.SerializeMetricsToFileAsync(metrics, filePath);

        // Assert
        var content = await System.IO.File.ReadAllTextAsync(filePath, System.Text.Encoding.UTF8);
        var deserializedMetrics = JsonSerializer.Deserialize<MetricAggregation>(content, JsonOptions);
        Assert.NotNull(deserializedMetrics);
    }

    [Fact]
    public async Task SerializeMetricsToFileAsync_NullMetrics_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => SerializationHelperExtensions.SerializeMetricsToFileAsync(null, "test.json"));
    }
}
