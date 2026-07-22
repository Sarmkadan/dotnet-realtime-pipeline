using System;
using System.Collections.Generic;
using DotNetRealtimePipeline.API;
using FluentAssertions;
using Xunit;

namespace DotNetRealtimePipeline.Tests;

/// <summary>
/// Unit tests for <see cref="ApiEndpointHandlerValidation"/>.
/// </summary>
public class ApiEndpointHandlerValidationTests
{
    #region ApiResponse<T> tests

    [Fact]
    public void Validate_ApiResponse_Valid_ReturnsEmptyList()
    {
        // Arrange
        var response = new ApiEndpointHandler.ApiResponse<string>
        {
            Message = "All good",
            StatusCode = 200,
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = response.Validate();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ApiResponse_Invalid_ReturnsExpectedErrors()
    {
        // Arrange: create a response that violates several rules
        var response = new ApiEndpointHandler.ApiResponse<string>
        {
            Message = "",                                 // empty message
            StatusCode = 0,                              // default status code
            Timestamp = DateTime.UtcNow.AddYears(2)      // too far in the future
        };

        // Act
        var errors = response.Validate();

        // Assert
        errors.Should().Contain("Message cannot be empty");
        errors.Should().Contain("StatusCode must be set to a non-default value");
        errors.Should().Contain("Timestamp cannot be in the future");
    }

    [Fact]
    public void IsValid_ApiResponse_ReturnsTrueWhenValid()
    {
        // Arrange
        var response = new ApiEndpointHandler.ApiResponse<int>
        {
            Message = "OK",
            StatusCode = 201,
            Timestamp = DateTime.UtcNow
        };

        // Act
        var isValid = response.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void EnsureValid_ApiResponse_ThrowsArgumentExceptionWhenInvalid()
    {
        // Arrange
        var response = new ApiEndpointHandler.ApiResponse<object>
        {
            Message = null,
            StatusCode = 0,
            Timestamp = default
        };

        // Act
        Action act = () => response.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*ApiResponse<T> validation failed*");
    }

    [Fact]
    public void Validate_ApiResponse_Null_ThrowsArgumentNullException()
    {
        // Arrange
        ApiEndpointHandler.ApiResponse<string> response = null!;

        // Act
        Action act = () => response!.Validate();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region BatchIngestResult tests

    [Fact]
    public void Validate_BatchIngestResult_Valid_ReturnsEmpty()
    {
        // Arrange
        var result = new BatchIngestResult
        {
            SuccessfulCount = 5,
            FailedCount = 2,
            TotalCount = 7
        };

        // Act
        var errors = result.Validate();

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_BatchIngestResult_Invalid_ReturnsErrors()
    {
        // Arrange: negative values and mismatched totals
        var result = new BatchIngestResult
        {
            SuccessfulCount = -1,
            FailedCount = 3,
            TotalCount = 5 // does not equal -1 + 3
        };

        // Act
        var errors = result.Validate();

        // Assert
        errors.Should().Contain("SuccessfulCount cannot be negative");
        errors.Should().Contain("TotalCount must equal SuccessfulCount + FailedCount");
    }

    [Fact]
    public void EnsureValid_BatchIngestResult_ThrowsWhenInvalid()
    {
        // Arrange
        var result = new BatchIngestResult
        {
            SuccessfulCount = 1,
            FailedCount = -1,
            TotalCount = 0
        };

        // Act
        Action act = () => result.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*BatchIngestResult validation failed*");
    }

    [Fact]
    public void Validate_BatchIngestResult_Null_ThrowsArgumentNullException()
    {
        // Arrange
        BatchIngestResult result = null!;

        // Act
        Action act = () => result!.Validate();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region PipelineStatusInfo tests

    [Fact]
    public void Validate_PipelineStatusInfo_Valid_ReturnsEmpty()
    {
        // Arrange
        var status = new PipelineStatusInfo
        {
            PipelineName = "MyPipeline",
            Version = "v1.2.3",
            IsRunning = true,
            TotalProcessed = 1000,
            TotalFailed = 10,
            Pending = 5,
            HealthStatus = "Healthy"
        };

        // Act
        var errors = status.Validate();

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_PipelineStatusInfo_Invalid_ReturnsExpectedErrors()
    {
        // Arrange: violate several rules
        var status = new PipelineStatusInfo
        {
            PipelineName = "",                                 // empty
            Version = "1.0.0",                                 // missing leading 'v'
            IsRunning = false,
            TotalProcessed = -5,                              // negative
            TotalFailed = -1,                                 // negative
            Pending = -2,                                     // negative
            HealthStatus = new string('x', 60)                // too long
        };

        // Act
        var errors = status.Validate();

        // Assert
        errors.Should().Contain("PipelineName cannot be null or whitespace");
        errors.Should().Contain("Version should start with 'v' (e.g., v1.0.0)");
        errors.Should().Contain("TotalProcessed cannot be negative");
        errors.Should().Contain("TotalFailed cannot be negative");
        errors.Should().Contain("Pending cannot be negative");
        errors.Should().Contain("HealthStatus cannot exceed 50 characters");
    }

    [Fact]
    public void EnsureValid_PipelineStatusInfo_ThrowsWhenInvalid()
    {
        // Arrange
        var status = new PipelineStatusInfo
        {
            PipelineName = "P",
            Version = "v0",
            IsRunning = true,
            TotalProcessed = 0,
            TotalFailed = 0,
            Pending = 0,
            HealthStatus = ""
        };

        // Act
        Action act = () => status.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*PipelineStatusInfo validation failed*");
    }

    [Fact]
    public void Validate_PipelineStatusInfo_Null_ThrowsArgumentNullException()
    {
        // Arrange
        PipelineStatusInfo status = null!;

        // Act
        Action act = () => status!.Validate();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion
}
