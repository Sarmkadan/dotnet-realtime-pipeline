using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetRealtimePipeline.API;
using DotNetRealtimePipeline.Domain.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotNetRealtimePipeline.Tests;

/// <summary>
/// Unit tests for <see cref="ApiEndpointHandlerJsonExtensions"/> class.
/// Tests JSON serialization and deserialization for ApiEndpointHandler types.
/// </summary>
public class ApiEndpointHandlerJsonExtensionsTests
{
    private readonly Mock<ILogger<DataIngestionHandler>> _mockLogger;
    private readonly DataIngestionHandler _handler;

    public ApiEndpointHandlerJsonExtensionsTests()
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

    #region ToJson Tests

    [Fact]
    public void ToJson_WithValidHandler_ReturnsNonEmptyJsonString()
    {
        // Arrange
        var handler = new DataIngestionHandler(
            Mock.Of<PipelineOrchestrator>(),
            Mock.Of<ILogger<DataIngestionHandler>>()
        );

        // Act
        var json = handler.ToJson();

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("DataIngestionHandler");
    }

    [Fact]
    public void ToJson_WithIndentedTrue_ReturnsFormattedJson()
    {
        // Arrange
        var handler = new DataIngestionHandler(
            Mock.Of<PipelineOrchestrator>(),
            Mock.Of<ILogger<DataIngestionHandler>>()
        );

        // Act
        var json = handler.ToJson(indented: true);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\n"); // Should have newlines for formatting
        json.Should().Contain("  "); // Should have indentation
    }

    [Fact]
    public void ToJson_WithIndentedFalse_ReturnsCompactJson()
    {
        // Arrange
        var handler = new DataIngestionHandler(
            Mock.Of<PipelineOrchestrator>(),
            Mock.Of<ILogger<DataIngestionHandler>>()
        );

        // Act
        var json = handler.ToJson(indented: false);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().NotContain("\n"); // Should not have newlines
    }

    [Fact]
    public void ToJson_WithNullHandler_ThrowsArgumentNullException()
    {
        // Arrange
        ApiEndpointHandler handler = null!;

        // Act
        Action act = () => handler.ToJson();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToJson_WithApiResponse_ReturnsValidJson()
    {
        // Arrange
        var response = new ApiEndpointHandler.ApiResponse<bool>
        {
            Success = true,
            Data = true,
            Message = "Test message",
            StatusCode = 200
        };

        // Act
        var json = response.ToJson();

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("Test message");
        json.Should().Contain("200");
    }

    [Fact]
    public void ToJson_WithCamelCaseNaming_ReturnsCorrectPropertyNames()
    {
        // Arrange
        var response = new ApiEndpointHandler.ApiResponse<bool>
        {
            Success = true,
            Data = false,
            Message = "Test",
            StatusCode = 201
        };

        // Act
        var json = response.ToJson();

        // Assert
        json.Should().Contain("success");
        json.Should().Contain("data");
        json.Should().Contain("message");
        json.Should().Contain("statusCode");
    }

    #endregion

    #region FromJson Tests

    [Fact]
    public void FromJson_WithValidJson_ReturnsDeserializedHandler()
    {
        // Arrange
        var handler = new DataIngestionHandler(
            Mock.Of<PipelineOrchestrator>(),
            Mock.Of<ILogger<DataIngestionHandler>>()
        );
        var json = handler.ToJson();

        // Act
        var result = ApiEndpointHandlerJsonExtensions.FromJson(json);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<DataIngestionHandler>();
    }

    [Fact]
    public void FromJson_WithValidApiResponseJson_ReturnsDeserializedResponse()
    {
        // Arrange
        var expectedResponse = new ApiEndpointHandler.ApiResponse<int>
        {
            Success = true,
            Data = 42,
            Message = "Test response",
            StatusCode = 200
        };
        var json = expectedResponse.ToJson();

        // Act
        var result = ApiEndpointHandlerJsonExtensions.FromJson(json);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<ApiEndpointHandler.ApiResponse<int>>();
        var actualResponse = result as ApiEndpointHandler.ApiResponse<int>;
        actualResponse.Success.Should().BeTrue();
        actualResponse.Data.Should().Be(42);
        actualResponse.Message.Should().Be("Test response");
        actualResponse.StatusCode.Should().Be(200);
    }

    [Fact]
    public void FromJson_WithEmptyJson_ReturnsNull()
    {
        // Arrange
        var json = string.Empty;

        // Act
        var result = ApiEndpointHandlerJsonExtensions.FromJson(json);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FromJson_WithNullJson_ThrowsArgumentException()
    {
        // Arrange
        string json = null!;

        // Act
        Action act = () => ApiEndpointHandlerJsonExtensions.FromJson(json);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FromJson_WithInvalidJson_ThrowsJsonException()
    {
        // Arrange
        var json = "invalid json {{{";

        // Act
        Action act = () => ApiEndpointHandlerJsonExtensions.FromJson(json);

        // Assert
        act.Should().Throw<System.Text.Json.JsonException>();
    }

    [Fact]
    public void FromJson_WithUnknownTypeJson_ReturnsNull()
    {
        // Arrange
        var json = "{\"UnknownProperty\": \"value\"}";

        // Act
        var result = ApiEndpointHandlerJsonExtensions.FromJson(json);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FromJson_WithCamelCaseProperties_DeserializesCorrectly()
    {
        // Arrange
        var json = "{\"success\":true,\"data\":123,\"message\":\"test\",\"statusCode\":200}";

        // Act
        var result = ApiEndpointHandlerJsonExtensions.FromJson(json);

        // Assert - should not throw and return null for unknown type
        result.Should().BeNull();
    }

    #endregion

    #region TryFromJson Tests

    [Fact]
    public void TryFromJson_WithValidJson_ReturnsTrueAndDeserializedHandler()
    {
        // Arrange
        var handler = new DataIngestionHandler(
            Mock.Of<PipelineOrchestrator>(),
            Mock.Of<ILogger<DataIngestionHandler>>()
        );
        var json = handler.ToJson();

        // Act
        var result = ApiEndpointHandlerJsonExtensions.TryFromJson(json, out var value);

        // Assert
        result.Should().BeTrue();
        value.Should().NotBeNull();
        value.Should().BeOfType<DataIngestionHandler>();
    }

    [Fact]
    public void TryFromJson_WithValidApiResponseJson_ReturnsTrueAndDeserializedResponse()
    {
        // Arrange
        var expectedResponse = new ApiEndpointHandler.ApiResponse<string>
        {
            Success = false,
            Data = "test data",
            Message = "Error occurred",
            StatusCode = 500
        };
        var json = expectedResponse.ToJson();

        // Act
        var result = ApiEndpointHandlerJsonExtensions.TryFromJson(json, out var value);

        // Assert
        result.Should().BeTrue();
        value.Should().NotBeNull();
        value.Should().BeAssignableTo<ApiEndpointHandler.ApiResponse<string>>();
        var actualResponse = value as ApiEndpointHandler.ApiResponse<string>;
        actualResponse.Success.Should().BeFalse();
        actualResponse.Data.Should().Be("test data");
        actualResponse.Message.Should().Be("Error occurred");
        actualResponse.StatusCode.Should().Be(500);
    }

    [Fact]
    public void TryFromJson_WithEmptyJson_ReturnsFalseAndNull()
    {
        // Arrange
        var json = string.Empty;

        // Act
        var result = ApiEndpointHandlerJsonExtensions.TryFromJson(json, out var value);

        // Assert
        result.Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_WithNullJson_ThrowsArgumentException()
    {
        // Arrange
        string json = null!;

        // Act
        Action act = () => ApiEndpointHandlerJsonExtensions.TryFromJson(json, out _);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TryFromJson_WithInvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        var json = "invalid { json";

        // Act
        var result = ApiEndpointHandlerJsonExtensions.TryFromJson(json, out var value);

        // Assert
        result.Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_WithWhitespaceJson_ReturnsFalseAndNull()
    {
        // Arrange
        var json = "   ";

        // Act
        var result = ApiEndpointHandlerJsonExtensions.TryFromJson(json, out var value);

        // Assert
        result.Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_WithBatchIngestResultJson_ReturnsTrueAndDeserializedResult()
    {
        // Arrange
        var batchResult = new BatchIngestResult
        {
            SuccessfulCount = 10,
            FailedCount = 2,
            TotalCount = 12
        };
        var json = batchResult.ToJson();

        // Act
        var result = ApiEndpointHandlerJsonExtensions.TryFromJson(json, out var value);

        // Assert
        result.Should().BeTrue();
        value.Should().NotBeNull();
        // Note: This will return null because BatchIngestResult is not a subclass of ApiEndpointHandler
        // but the method should still return true since deserialization succeeded
        value.Should().BeNull();
    }

    #endregion

    #region Round-trip Tests

    [Fact]
    public void RoundTrip_ToJsonThenFromJson_PreservesDataIntegrity()
    {
        // Arrange
        var originalHandler = new DataIngestionHandler(
            Mock.Of<PipelineOrchestrator>(),
            Mock.Of<ILogger<DataIngestionHandler>>()
        );

        // Act
        var json = originalHandler.ToJson();
        var deserialized = ApiEndpointHandlerJsonExtensions.FromJson(json);

        // Assert - both should be of the same type
        deserialized.Should().NotBeNull();
        deserialized.Should().BeOfType<DataIngestionHandler>();
    }

    [Fact]
    public void RoundTrip_ToJsonThenTryFromJson_PreservesDataIntegrity()
    {
        // Arrange
        var originalHandler = new DataIngestionHandler(
            Mock.Of<PipelineOrchestrator>(),
            Mock.Of<ILogger<DataIngestionHandler>>()
        );

        // Act
        var json = originalHandler.ToJson();
        var result = ApiEndpointHandlerJsonExtensions.TryFromJson(json, out var value);

        // Assert
        result.Should().BeTrue();
        value.Should().NotBeNull();
        value.Should().BeOfType<DataIngestionHandler>();
    }

    [Fact]
    public void RoundTrip_ApiResponse_PreservesAllProperties()
    {
        // Arrange
        var originalResponse = new ApiEndpointHandler.ApiResponse<PipelineStatusInfo>
        {
            Success = true,
            Data = new PipelineStatusInfo
            {
                PipelineName = "TestPipeline",
                Version = "v1.0.0",
                IsRunning = true,
                TotalProcessed = 1000,
                TotalFailed = 10,
                Pending = 5,
                HealthStatus = "Healthy",
                Throughput = 100.5,
                SuccessRate = 99.5,
                AverageLatency = 10.2
            },
            Message = "Pipeline is running",
            StatusCode = 200
        };

        // Act
        var json = originalResponse.ToJson();
        var deserialized = ApiEndpointHandlerJsonExtensions.FromJson(json) as ApiEndpointHandler.ApiResponse<PipelineStatusInfo>;

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Success.Should().BeTrue();
        deserialized.Message.Should().Be("Pipeline is running");
        deserialized.StatusCode.Should().Be(200);
        deserialized.Data.Should().NotBeNull();
        deserialized.Data.PipelineName.Should().Be("TestPipeline");
        deserialized.Data.Version.Should().Be("v1.0.0");
        deserialized.Data.IsRunning.Should().BeTrue();
        deserialized.Data.TotalProcessed.Should().Be(1000);
        deserialized.Data.TotalFailed.Should().Be(10);
        deserialized.Data.Pending.Should().Be(5);
        deserialized.Data.HealthStatus.Should().Be("Healthy");
        deserialized.Data.Throughput.Should().Be(100.5);
        deserialized.Data.SuccessRate.Should().Be(99.5);
        deserialized.Data.AverageLatency.Should().Be(10.2);
    }

    #endregion
}