using Xunit;
using FluentAssertions;
using Moq;
using DotNetRealtimePipeline.Services;
using DotNetRealtimePipeline.Data.Repositories;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Domain.Exceptions;

namespace DotNetRealtimePipeline.Tests.Unit;

public sealed class DataProcessingServiceTests
{
    private readonly Mock<IDataPointRepository> _mockRepository;
    private readonly PipelineConfig _config;
    private readonly DataProcessingService _service;

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
