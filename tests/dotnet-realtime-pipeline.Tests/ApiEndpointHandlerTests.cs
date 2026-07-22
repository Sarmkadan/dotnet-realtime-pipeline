using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetRealtimePipeline.API;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotNetRealtimePipeline.Tests;

/// <summary>
/// Unit tests for <see cref="DataIngestionHandler"/> class.
/// Tests the REST API endpoint handler for data ingestion operations.
/// </summary>
public class ApiEndpointHandlerTests
{
    private readonly Mock<ILogger<DataIngestionHandler>> _mockLogger;
    private readonly DataIngestionHandler _handler;

    public ApiEndpointHandlerTests()
    {
        _mockLogger = new Mock<ILogger<DataIngestionHandler>>();
        var mockOrchestrator = new Mock<PipelineOrchestrator>(
            Mock.Of<DataProcessingService>(),
            Mock.Of<WindowingService>(),
            Mock.Of<MetricsService>(),
            Mock.Of<BackpressureService>(),
            Mock.Of<QueryService>(),
            new PipelineConfig()
        );

        _handler = new DataIngestionHandler(mockOrchestrator.Object, _mockLogger.Object);
    }

    #region DataIngestionHandler Tests

    [Fact]
    public async Task IngestAsync_WithValidDataPoint_ReturnsSuccessResponse()
    {
        // Arrange
        var mockOrchestrator = new Mock<PipelineOrchestrator>(
            Mock.Of<DataProcessingService>(),
            Mock.Of<WindowingService>(),
            Mock.Of<MetricsService>(),
            Mock.Of<BackpressureService>(),
            Mock.Of<QueryService>(),
            new PipelineConfig()
        );
        mockOrchestrator.Setup(x => x.IngestDataPointAsync(It.IsAny<DataPoint>())).ReturnsAsync(true);

        var handler = new DataIngestionHandler(mockOrchestrator.Object, _mockLogger.Object);
        var dataPoint = new DataPoint(1, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 42.5, "test-source");

        // Act
        var result = await handler.IngestAsync(dataPoint);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeTrue();
        result.Message.Should().Be("Data point ingested successfully");
        result.StatusCode.Should().Be(200);
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task IngestAsync_WithValidDataPoint_ReturnsFailureResponseWhenIngestionFails()
    {
        // Arrange
        var mockOrchestrator = new Mock<PipelineOrchestrator>(
            Mock.Of<DataProcessingService>(),
            Mock.Of<WindowingService>(),
            Mock.Of<MetricsService>(),
            Mock.Of<BackpressureService>(),
            Mock.Of<QueryService>(),
            new PipelineConfig()
        );
        mockOrchestrator.Setup(x => x.IngestDataPointAsync(It.IsAny<DataPoint>())).ReturnsAsync(false);

        var handler = new DataIngestionHandler(mockOrchestrator.Object, _mockLogger.Object);
        var dataPoint = new DataPoint(1, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 42.5, "test-source");

        // Act
        var result = await handler.IngestAsync(dataPoint);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Data.Should().BeFalse();
        result.Message.Should().Be("Failed to ingest data point due to backpressure");
        result.StatusCode.Should().Be(429);
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task IngestAsync_WithNullDataPoint_ReturnsBadRequestResponse()
    {
        // Arrange
        var mockOrchestrator = new Mock<PipelineOrchestrator>(
            Mock.Of<DataProcessingService>(),
            Mock.Of<WindowingService>(),
            Mock.Of<MetricsService>(),
            Mock.Of<BackpressureService>(),
            Mock.Of<QueryService>(),
            new PipelineConfig()
        );

        var handler = new DataIngestionHandler(mockOrchestrator.Object, _mockLogger.Object);
        DataPoint dataPoint = null!;

        // Act
        var result = await handler.IngestAsync(dataPoint);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Data.Should().BeFalse();
        result.Message.Should().Be("Data point is null");
        result.StatusCode.Should().Be(400);
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task IngestAsync_WhenOrchestratorThrowsException_ReturnsErrorResponse()
    {
        // Arrange
        var mockOrchestrator = new Mock<PipelineOrchestrator>(
            Mock.Of<DataProcessingService>(),
            Mock.Of<WindowingService>(),
            Mock.Of<MetricsService>(),
            Mock.Of<BackpressureService>(),
            Mock.Of<QueryService>(),
            new PipelineConfig()
        );
        var exception = new InvalidOperationException("Pipeline is not running");
        mockOrchestrator.Setup(x => x.IngestDataPointAsync(It.IsAny<DataPoint>())).ThrowsAsync(exception);

        var handler = new DataIngestionHandler(mockOrchestrator.Object, _mockLogger.Object);
        var dataPoint = new DataPoint(1, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 42.5, "test-source");

        // Act
        var result = await handler.IngestAsync(dataPoint);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Data.Should().BeFalse();
        result.Message.Should().Be(exception.Message);
        result.StatusCode.Should().Be(500);
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task IngestBatchAsync_WithValidBatch_ReturnsSuccessResponseWithBatchResults()
    {
        // Arrange
        var mockOrchestrator = new Mock<PipelineOrchestrator>(
            Mock.Of<DataProcessingService>(),
            Mock.Of<WindowingService>(),
            Mock.Of<MetricsService>(),
            Mock.Of<BackpressureService>(),
            Mock.Of<QueryService>(),
            new PipelineConfig()
        );

        var dataPoints = new List<DataPoint>
        {
            new DataPoint(1, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 42.5, "source1"),
            new DataPoint(2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 43.5, "source2"),
            new DataPoint(3, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 44.5, "source3")
        };

        var batchResult = new BatchProcessingResult { SuccessfulCount = 2, FailedCount = 1 };
        mockOrchestrator.Setup(x => x.ProcessBatchDataPointsAsync(dataPoints)).ReturnsAsync(batchResult);

        var handler = new DataIngestionHandler(mockOrchestrator.Object, _mockLogger.Object);

        // Act
        var result = await handler.IngestBatchAsync(dataPoints);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.SuccessfulCount.Should().Be(2);
        result.Data.FailedCount.Should().Be(1);
        result.Data.TotalCount.Should().Be(3);
        result.Message.Should().Be("Batch processed: 2 successful, 1 failed");
        result.StatusCode.Should().Be(200);
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task IngestBatchAsync_WithEmptyList_ReturnsBadRequestResponse()
    {
        // Arrange
        var mockOrchestrator = new Mock<PipelineOrchestrator>(
            Mock.Of<DataProcessingService>(),
            Mock.Of<WindowingService>(),
            Mock.Of<MetricsService>(),
            Mock.Of<BackpressureService>(),
            Mock.Of<QueryService>(),
            new PipelineConfig()
        );

        var handler = new DataIngestionHandler(mockOrchestrator.Object, _mockLogger.Object);
        var dataPoints = new List<DataPoint>();

        // Act
        var result = await handler.IngestBatchAsync(dataPoints);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be("Data points list is empty");
        result.StatusCode.Should().Be(400);
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task IngestBatchAsync_WithNullList_ReturnsBadRequestResponse()
    {
        // Arrange
        var mockOrchestrator = new Mock<PipelineOrchestrator>(
            Mock.Of<DataProcessingService>(),
            Mock.Of<WindowingService>(),
            Mock.Of<MetricsService>(),
            Mock.Of<BackpressureService>(),
            Mock.Of<QueryService>(),
            new PipelineConfig()
        );

        var handler = new DataIngestionHandler(mockOrchestrator.Object, _mockLogger.Object);
        List<DataPoint> dataPoints = null!;

        // Act
        var result = await handler.IngestBatchAsync(dataPoints);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be("Data points list is empty");
        result.StatusCode.Should().Be(400);
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task IngestBatchAsync_WhenOrchestratorThrowsException_ReturnsErrorResponse()
    {
        // Arrange
        var mockOrchestrator = new Mock<PipelineOrchestrator>(
            Mock.Of<DataProcessingService>(),
            Mock.Of<WindowingService>(),
            Mock.Of<MetricsService>(),
            Mock.Of<BackpressureService>(),
            Mock.Of<QueryService>(),
            new PipelineConfig()
        );

        var dataPoints = new List<DataPoint>
        {
            new DataPoint(1, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 42.5, "source1")
        };

        var exception = new InvalidOperationException("Pipeline is not running");
        mockOrchestrator.Setup(x => x.ProcessBatchDataPointsAsync(dataPoints)).ThrowsAsync(exception);

        var handler = new DataIngestionHandler(mockOrchestrator.Object, _mockLogger.Object);

        // Act
        var result = await handler.IngestBatchAsync(dataPoints);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be(exception.Message);
        result.StatusCode.Should().Be(500);
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task IngestBatchAsync_WithSingleDataPoint_ReturnsCorrectCounts()
    {
        // Arrange
        var mockOrchestrator = new Mock<PipelineOrchestrator>(
            Mock.Of<DataProcessingService>(),
            Mock.Of<WindowingService>(),
            Mock.Of<MetricsService>(),
            Mock.Of<BackpressureService>(),
            Mock.Of<QueryService>(),
            new PipelineConfig()
        );

        var dataPoints = new List<DataPoint>
        {
            new DataPoint(1, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 42.5, "source1")
        };

        var batchResult = new BatchProcessingResult { SuccessfulCount = 1, FailedCount = 0 };
        mockOrchestrator.Setup(x => x.ProcessBatchDataPointsAsync(dataPoints)).ReturnsAsync(batchResult);

        var handler = new DataIngestionHandler(mockOrchestrator.Object, _mockLogger.Object);

        // Act
        var result = await handler.IngestBatchAsync(dataPoints);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.SuccessfulCount.Should().Be(1);
        result.Data.FailedCount.Should().Be(0);
        result.Data.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task IngestBatchAsync_WithAllFailing_ReturnsCorrectFailureCounts()
    {
        // Arrange
        var mockOrchestrator = new Mock<PipelineOrchestrator>(
            Mock.Of<DataProcessingService>(),
            Mock.Of<WindowingService>(),
            Mock.Of<MetricsService>(),
            Mock.Of<BackpressureService>(),
            Mock.Of<QueryService>(),
            new PipelineConfig()
        );

        var dataPoints = new List<DataPoint>
        {
            new DataPoint(1, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 42.5, "source1"),
            new DataPoint(2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 43.5, "source2")
        };

        var batchResult = new BatchProcessingResult { SuccessfulCount = 0, FailedCount = 2 };
        mockOrchestrator.Setup(x => x.ProcessBatchDataPointsAsync(dataPoints)).ReturnsAsync(batchResult);

        var handler = new DataIngestionHandler(mockOrchestrator.Object, _mockLogger.Object);

        // Act
        var result = await handler.IngestBatchAsync(dataPoints);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.SuccessfulCount.Should().Be(0);
        result.Data.FailedCount.Should().Be(2);
        result.Data.TotalCount.Should().Be(2);
    }

    #endregion

    #region ApiResponse<T> Property Tests

    [Fact]
    public void ApiResponse_SuccessProperty_ReturnsCorrectValue()
    {
        // Arrange
        var response = new ApiEndpointHandler.ApiResponse<bool> { Success = true };

        // Assert
        response.Success.Should().BeTrue();
    }

    [Fact]
    public void ApiResponse_DataProperty_ReturnsCorrectValue()
    {
        // Arrange
        var response = new ApiEndpointHandler.ApiResponse<int> { Data = 42 };

        // Assert
        response.Data.Should().Be(42);
    }

    [Fact]
    public void ApiResponse_MessageProperty_ReturnsCorrectValue()
    {
        // Arrange
        var response = new ApiEndpointHandler.ApiResponse<string> { Message = "Test message" };

        // Assert
        response.Message.Should().Be("Test message");
    }

    [Fact]
    public void ApiResponse_StatusCodeProperty_ReturnsCorrectValue()
    {
        // Arrange
        var response = new ApiEndpointHandler.ApiResponse<object> { StatusCode = 201 };

        // Assert
        response.StatusCode.Should().Be(201);
    }

    [Fact]
    public void ApiResponse_TimestampProperty_ReturnsUtcNowByDefault()
    {
        // Arrange
        var before = DateTime.UtcNow;
        var response = new ApiEndpointHandler.ApiResponse<string>();
        var after = DateTime.UtcNow;

        // Assert
        response.Timestamp.Should().BeOnOrAfter(before);
        response.Timestamp.Should().BeOnOrBefore(after);
    }

    #endregion

    #region BatchIngestResult Property Tests

    [Fact]
    public void BatchIngestResult_SuccessfulCountProperty_ReturnsCorrectValue()
    {
        // Arrange
        var result = new BatchIngestResult { SuccessfulCount = 5 };

        // Assert
        result.SuccessfulCount.Should().Be(5);
    }

    [Fact]
    public void BatchIngestResult_FailedCountProperty_ReturnsCorrectValue()
    {
        // Arrange
        var result = new BatchIngestResult { FailedCount = 3 };

        // Assert
        result.FailedCount.Should().Be(3);
    }

    [Fact]
    public void BatchIngestResult_TotalCountProperty_ReturnsCorrectValue()
    {
        // Arrange
        var result = new BatchIngestResult { TotalCount = 10 };

        // Assert
        result.TotalCount.Should().Be(10);
    }

    #endregion
}
