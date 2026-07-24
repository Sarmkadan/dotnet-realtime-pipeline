using System;
using System.Collections.Generic;
using Xunit;

namespace DotNetRealtimePipeline.Domain.Models;

public class ProcessingResultExtensionsTests
{
    [Fact]
    public void IsRetryableFailure_WithSuccessfulResult_ReturnsFalse()
    {
        // Arrange
        var result = new ProcessingResult(1, true, "test-stage");

        // Act
        var isRetryable = result.IsRetryableFailure();

        // Assert
        Assert.False(isRetryable);
    }

    [Fact]
    public void IsRetryableFailure_WithFailedResultWithoutExceptionOrError_ReturnsFalse()
    {
        // Arrange
        var result = new ProcessingResult(1, false, "test-stage");

        // Act
        var isRetryable = result.IsRetryableFailure();

        // Assert
        Assert.False(isRetryable);
    }

    [Fact]
    public void IsRetryableFailure_WithFailedResultWithErrorMessageAndRetryCountBelowMax_ReturnsTrue()
    {
        // Arrange
        var result = new ProcessingResult(1, false, "test-stage");
        result.ErrorMessage = "Something went wrong";

        // Act
        var isRetryable = result.IsRetryableFailure();

        // Assert
        Assert.True(isRetryable);
    }

    [Fact]
    public void IsRetryableFailure_WithFailedResultWithExceptionAndRetryCountBelowMax_ReturnsTrue()
    {
        // Arrange
        var result = new ProcessingResult(1, false, "test-stage");
        result.Exception = new InvalidOperationException("Test exception");

        // Act
        var isRetryable = result.IsRetryableFailure();

        // Assert
        Assert.True(isRetryable);
    }

    [Fact]
    public void IsRetryableFailure_WithRetryCountEqualToMax_ReturnsFalse()
    {
        // Arrange
        var result = new ProcessingResult(1, false, "test-stage");
        result.ErrorMessage = "Something went wrong";
        result.RetryCount = 3;

        // Act
        var isRetryable = result.IsRetryableFailure(maxRetryCount: 3);

        // Assert
        Assert.False(isRetryable);
    }

    [Fact]
    public void IsRetryableFailure_WithRetryCountGreaterThanMax_ReturnsFalse()
    {
        // Arrange
        var result = new ProcessingResult(1, false, "test-stage");
        result.ErrorMessage = "Something went wrong";
        result.RetryCount = 4;

        // Act
        var isRetryable = result.IsRetryableFailure(maxRetryCount: 3);

        // Assert
        Assert.False(isRetryable);
    }

    [Fact]
    public void IsRetryableFailure_WithNullResult_ThrowsArgumentNullException()
    {
        // Arrange
        ProcessingResult? result = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result!.IsRetryableFailure());
    }

    [Fact]
    public void MergeOutputData_WithNullResult_ThrowsArgumentNullException()
    {
        // Arrange
        ProcessingResult? result = null;
        var source = new ProcessingResult(2, true, "source-stage");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result!.MergeOutputData(source));
    }

    [Fact]
    public void MergeOutputData_WithNullSource_ThrowsArgumentNullException()
    {
        // Arrange
        var result = new ProcessingResult(1, true, "test-stage");
        ProcessingResult? source = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result.MergeOutputData(source!));
    }

    [Fact]
    public void MergeOutputData_WithEmptySource_DoesNotModifyResult()
    {
        // Arrange
        var result = new ProcessingResult(1, true, "test-stage");
        result.AddOutput("key1", "value1");

        var source = new ProcessingResult(2, true, "source-stage");

        // Act
        result.MergeOutputData(source);

        // Assert
        Assert.Single(result.OutputData);
        Assert.Equal("value1", result.OutputData["key1"]);
    }

    [Fact]
    public void MergeOutputData_WithOverwriteExistingTrue_OverwritesExistingKeys()
    {
        // Arrange
        var result = new ProcessingResult(1, true, "test-stage");
        result.AddOutput("key1", "original");

        var source = new ProcessingResult(2, true, "source-stage");
        source.AddOutput("key1", "new-value");

        // Act
        result.MergeOutputData(source, overwriteExisting: true);

        // Assert
        Assert.Single(result.OutputData);
        Assert.Equal("new-value", result.OutputData["key1"]);
    }

    [Fact]
    public void MergeOutputData_WithOverwriteExistingFalse_DoesNotOverwriteExistingKeys()
    {
        // Arrange
        var result = new ProcessingResult(1, true, "test-stage");
        result.AddOutput("key1", "original");

        var source = new ProcessingResult(2, true, "source-stage");
        source.AddOutput("key1", "new-value");
        source.AddOutput("key2", "value2");

        // Act
        result.MergeOutputData(source, overwriteExisting: false);

        // Assert
        Assert.Equal(2, result.OutputData.Count);
        Assert.Equal("original", result.OutputData["key1"]);
        Assert.Equal("value2", result.OutputData["key2"]);
    }

    [Fact]
    public void MergeOutputData_WithMultipleKeys_MergesAllNewKeys()
    {
        // Arrange
        var result = new ProcessingResult(1, true, "test-stage");
        result.AddOutput("existing", "value");

        var source = new ProcessingResult(2, true, "source-stage");
        source.AddOutput("new1", "value1");
        source.AddOutput("new2", "value2");

        // Act
        result.MergeOutputData(source);

        // Assert
        Assert.Equal(3, result.OutputData.Count);
        Assert.Equal("value", result.OutputData["existing"]);
        Assert.Equal("value1", result.OutputData["new1"]);
        Assert.Equal("value2", result.OutputData["new2"]);
    }

    [Fact]
    public void ToDictionary_WithNullResult_ThrowsArgumentNullException()
    {
        // Arrange
        ProcessingResult? result = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result!.ToDictionary());
    }

    [Fact]
    public void ToDictionary_WithSuccessfulResult_ReturnsDictionaryWithCorrectValues()
    {
        // Arrange
        var result = new ProcessingResult(123, true, "processing-stage");
        result.ProcessingTimeMs = 150;
        result.RetryCount = 2;
        result.CorrelationId = "corr-123";
        result.AddOutput("output1", "data1");
        result.AddOutput("output2", 42);

        // Act
        var dict = result.ToDictionary();

        // Assert
        Assert.Equal(9, dict.Count);
        Assert.Equal(123L, dict["ResultId"]);
        Assert.True((bool)dict["Success"]);
        Assert.Equal("processing-stage", dict["StageName"]);
        Assert.Equal(150L, dict["ProcessingTimeMs"]);
        Assert.Equal(result.ProcessedAt.ToString("o"), dict["ProcessedAt"]);
        Assert.Equal(2, dict["RetryCount"]);
        Assert.Equal("corr-123", dict["CorrelationId"]);

        var outputData = Assert.IsType<Dictionary<string, object>>(dict["OutputData"]);
        Assert.Equal(2, outputData.Count);
        Assert.Equal("data1", outputData["output1"]);
        Assert.Equal(42, outputData["output2"]);

        Assert.True((bool)dict["IsValid"]);
    }

    [Fact]
    public void ToDictionary_WithFailedResult_ReturnsDictionaryWithErrorDetails()
    {
        // Arrange
        var result = new ProcessingResult(456, false, "error-stage");
        result.ProcessingTimeMs = 5000;
        result.ErrorMessage = "Critical failure occurred";
        result.RetryCount = 0;
        var exception = new InvalidOperationException("Test error");
        result.Exception = exception;

        // Act
        var dict = result.ToDictionary();

        // Assert
        Assert.Equal(11, dict.Count);
        Assert.Equal("Critical failure occurred", dict["ErrorMessage"]);
        Assert.Equal("System.InvalidOperationException", dict["ExceptionType"]);
        Assert.Equal("Test error", dict["ExceptionMessage"]);
        Assert.False((bool)dict["Success"]);
        Assert.True((bool)dict["IsValid"]);
    }

    [Fact]
    public void ToDictionary_WithEmptyOutputData_ReturnsDictionaryWithEmptyOutputData()
    {
        // Arrange
        var result = new ProcessingResult(789, true, "empty-stage");

        // Act
        var dict = result.ToDictionary();

        // Assert
        var outputData = Assert.IsType<Dictionary<string, object>>(dict["OutputData"]);
        Assert.Empty(outputData);
    }

    [Fact]
    public void ToDictionary_WithNullCorrelationId_DoesNotIncludeCorrelationIdInDictionary()
    {
        // Arrange
        var result = new ProcessingResult(999, true, "test-stage");
        result.CorrelationId = null;

        // Act
        var dict = result.ToDictionary();

        // Assert
        Assert.DoesNotContain(dict, kvp => kvp.Key == "CorrelationId");
    }

    [Fact]
    public void WithProcessingTime_WithNullResult_ThrowsArgumentNullException()
    {
        // Arrange
        ProcessingResult? result = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result!.WithProcessingTime(100));
    }

    [Fact]
    public void WithProcessingTime_WithValidResult_ReturnsNewResultWithUpdatedProcessingTime()
    {
        // Arrange
        var result = new ProcessingResult(1, true, "test-stage");
        result.ProcessingTimeMs = 50;

        // Act
        var updatedResult = result.WithProcessingTime(200);

        // Assert
        Assert.NotSame(result, updatedResult);
        Assert.Equal(200, updatedResult.ProcessingTimeMs);
        Assert.Equal(result.ResultId, updatedResult.ResultId);
        Assert.Equal(result.Success, updatedResult.Success);
        Assert.Equal(result.StageName, updatedResult.StageName);
        Assert.Equal(result.RetryCount, updatedResult.RetryCount);
        Assert.Equal(result.ErrorMessage, updatedResult.ErrorMessage);
        Assert.Equal(result.Exception, updatedResult.Exception);
        Assert.Equal(result.ProcessedAt, updatedResult.ProcessedAt);
        Assert.Equal(result.CorrelationId, updatedResult.CorrelationId);
        Assert.Equal(result.OutputData, updatedResult.OutputData);
    }

    [Fact]
    public void WithProcessingTime_WithFailedResult_ReturnsNewResultWithSameFailureState()
    {
        // Arrange
        var result = new ProcessingResult(2, false, "error-stage");
        result.ErrorMessage = "Failed";
        result.RetryCount = 1;

        // Act
        var updatedResult = result.WithProcessingTime(150);

        // Assert
        Assert.False(updatedResult.Success);
        Assert.Equal("Failed", updatedResult.ErrorMessage);
        Assert.Equal(1, updatedResult.RetryCount);
    }

    [Fact]
    public void IsTimeout_WithNullResult_ThrowsArgumentNullException()
    {
        // Arrange
        ProcessingResult? result = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result!.IsTimeout());
    }

    [Fact]
    public void IsTimeout_WithProcessingTimeBelowThreshold_ReturnsFalse()
    {
        // Arrange
        var result = new ProcessingResult(1, true, "test-stage");
        result.ProcessingTimeMs = 4999;

        // Act
        var isTimeout = result.IsTimeout();

        // Assert
        Assert.False(isTimeout);
    }

    [Fact]
    public void IsTimeout_WithProcessingTimeEqualToThreshold_ReturnsFalse()
    {
        // Arrange
        var result = new ProcessingResult(1, true, "test-stage");
        result.ProcessingTimeMs = 5000;

        // Act
        var isTimeout = result.IsTimeout();

        // Assert
        Assert.False(isTimeout);
    }

    [Fact]
    public void IsTimeout_WithProcessingTimeAboveThreshold_ReturnsTrue()
    {
        // Arrange
        var result = new ProcessingResult(1, true, "test-stage");
        result.ProcessingTimeMs = 5001;

        // Act
        var isTimeout = result.IsTimeout();

        // Assert
        Assert.True(isTimeout);
    }

    [Fact]
    public void IsTimeout_WithCustomThreshold_UsesCustomThreshold()
    {
        // Arrange
        var result = new ProcessingResult(1, true, "test-stage");
        result.ProcessingTimeMs = 100;

        // Act
        var isTimeout = result.IsTimeout(timeoutThresholdMs: 50);

        // Assert
        Assert.True(isTimeout);
    }
}