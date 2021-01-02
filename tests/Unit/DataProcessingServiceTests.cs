using Xunit;
using FluentAssertions;
using Moq;
using DotNetRealtimePipeline.Services;
using DotNetRealtimePipeline.Data.Repositories;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Domain.Exceptions;

namespace DotNetRealtimePipeline.Tests.Unit;

/// <summary>
/// Contains unit tests for the <see cref="DataProcessingService"/> class.
/// These tests verify correct behavior of data point processing and quality analysis.
/// </summary>
public sealed class DataProcessingServiceTests
{
    private readonly Mock<IDataPointRepository> _mockRepository;
    private readonly PipelineConfig _config;
    private readonly DataProcessingService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataProcessingServiceTests"/> class.
    /// Sets up a mock repository, a pipeline configuration, and the service under test.
    /// </summary>
    public DataProcessingServiceTests()
    {
        _mockRepository = new Mock<IDataPointRepository>();
        _config = new PipelineConfig
        {
            ValidateOnIngestion = true,
            MinDataQualityThreshold = 50,
            MaxRetries = 2,
            RetryDelayMs = 10
        };
        _service = new DataProcessingService(_mockRepository.Object, _config);
    }

    /// <summary>
    /// Verifies that a valid data point is processed successfully.
    /// A valid data point has quality above the configured threshold and valid properties.
    /// </summary>
    [Fact]
    public async Task ProcessDataPointAsync_ValidPoint_ShouldSucceed()
    {
        // Arrange
        var dataPoint = new DataPoint(1, DateTime.UtcNow.Ticks, 10.0, "Sensor1") { Quality = 80 };
        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<DataPoint>())).ReturnsAsync(dataPoint);

        // Act
        var result = await _service.ProcessDataPointAsync(dataPoint);

        // Assert
        result.Success.Should().BeTrue();
        result.StageName.Should().Be("Output");
        _mockRepository.Verify(r => r.CreateAsync(dataPoint), Times.Once);
    }

    /// <summary>
    /// Verifies that an invalid data point fails processing.
    /// An invalid data point has zero or invalid properties such as ID, timestamp, or source.
    /// </summary>
    [Fact]
    public async Task ProcessDataPointAsync_InvalidPoint_ShouldFail()
    {
        // Arrange
        var dataPoint = new DataPoint(0, 0, 0, ""); // Invalid

        // Act
        var result = await _service.ProcessDataPointAsync(dataPoint);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Validation failed");
    }

    /// <summary>
    /// Verifies that a data point with quality below the threshold fails processing.
    /// The configured minimum quality threshold is 50.
    /// </summary>
    [Fact]
    public async Task ProcessDataPointAsync_LowQuality_ShouldFail()
    {
        // Arrange
        var dataPoint = new DataPoint(1, DateTime.UtcNow.Ticks, 10.0, "Sensor1") { Quality = 30 }; // Below 50 threshold

        // Act
        var result = await _service.ProcessDataPointAsync(dataPoint);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("quality below threshold");
    }

    /// <summary>
    /// Verifies that the <see cref="DataProcessingService.AnalyzeDataQuality"/> method correctly calculates quality statistics.
    /// The test uses a mix of high and low quality data points.
    /// </summary>
    [Fact]
    public void AnalyzeDataQuality_ValidPoints_ShouldReturnCorrectStats()
    {
        // Arrange
        var points = new List<DataPoint>
        {
            new DataPoint(1, 1, 10.0, "S1") { Quality = 80 },
            new DataPoint(2, 2, 20.0, "S2") { Quality = 40 }
        };

        // Act
        var result = _service.AnalyzeDataQuality(points);

        // Assert
        result.TotalPoints.Should().Be(2);
        result.HighQualityCount.Should().Be(1);
        result.LowQualityCount.Should().Be(1);
        result.AverageQuality.Should().Be(60);
    }

    /// <summary>
    /// Verifies that the <see cref="DataProcessingService.AnalyzeDataQuality"/> method returns default statistics when given null input.
    /// </summary>
    [Fact]
    public void AnalyzeDataQuality_NullPoints_ShouldReturnDefault()
    {
        // Act
        var result = _service.AnalyzeDataQuality(null!);

        // Assert
        result.TotalPoints.Should().Be(0);
        result.QualityScore.Should().Be(0);
    }
}
