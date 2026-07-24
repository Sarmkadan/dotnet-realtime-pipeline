#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Domain.Exceptions;

using DotNetRealtimePipeline.Domain.Models;
using System;

/// <summary>
/// Base exception for all pipeline-related errors.
/// </summary>
public class PipelineException : Exception
{
    public string? ErrorCode { get; set; }
    public object? ErrorDetails { get; set; }

    public PipelineException(string message) : base(message) { }

    public PipelineException(string message, Exception innerException)
        : base(message, innerException) { }

    public PipelineException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public PipelineException(string message, string errorCode, object? errorDetails)
        : base(message)
    {
        ErrorCode = errorCode;
        ErrorDetails = errorDetails;
    }
}

/// <summary>
/// Thrown when a data point fails validation.
/// </summary>
public sealed class InvalidDataPointException : PipelineException
{
    public InvalidDataPointException(string message)
        : base(message, "INVALID_DATA_POINT") { }

    public InvalidDataPointException(string message, object? details)
        : base(message, "INVALID_DATA_POINT", details) { }
}

/// <summary>
/// Thrown when backpressure limits are exceeded.
/// </summary>
public sealed class BackpressureException : PipelineException
{
    public long BufferSize { get; set; }
    public long MaxCapacity { get; set; }

    public BackpressureException(string message, long bufferSize, long maxCapacity)
        : base(message, "BACKPRESSURE_EXCEEDED")
    {
        BufferSize = bufferSize;
        MaxCapacity = maxCapacity;
    }
}

/// <summary>
/// Thrown when a processing stage encounters an error.
/// </summary>
public sealed class StageProcessingException : PipelineException
{
    public string? StageName { get; set; }
    public int RetryCount { get; set; }

    public StageProcessingException(string message, string stageName)
        : base(message, "STAGE_PROCESSING_FAILED")
    {
        StageName = stageName;
    }

    public StageProcessingException(string message, string stageName, int retryCount)
        : base(message, "STAGE_PROCESSING_FAILED")
    {
        StageName = stageName;
        RetryCount = retryCount;
    }
}

/// <summary>
/// Thrown when windowing configuration or operation fails.
/// </summary>
public sealed class WindowingException : PipelineException
{
    public long WindowId { get; set; }

    public WindowingException(string message)
        : base(message, "WINDOWING_FAILED") { }

    public WindowingException(string message, long windowId)
        : base(message, "WINDOWING_FAILED")
    {
        WindowId = windowId;
    }
}

/// <summary>
/// Thrown when a timeout occurs during processing.
/// </summary>
public sealed class ProcessingTimeoutException : PipelineException
{
    public long TimeoutMs { get; set; }

    public ProcessingTimeoutException(string message, long timeoutMs)
        : base(message, "PROCESSING_TIMEOUT")
    {
        TimeoutMs = timeoutMs;
    }
}

/// <summary>
/// Thrown when configuration is invalid or incomplete.
/// </summary>
public sealed class InvalidConfigurationException : PipelineException
{
    public InvalidConfigurationException(string message)
        : base(message, "INVALID_CONFIGURATION") { }
}

/// <summary>
/// Thrown internally by <see cref="DotNetRealtimePipeline.Services.DataProcessingService"/> when a single
/// processing attempt for a <see cref="DataPoint"/> does not succeed, so the surrounding
/// <see cref="DotNetRealtimePipeline.DeadLetter.IRetryPolicy"/> can classify and retry the failure.
/// </summary>
public sealed class PipelineProcessingException : PipelineException
{
    /// <summary>
    /// Gets the data point whose processing attempt failed.
    /// </summary>
    public DataPoint DataPoint { get; }

    /// <summary>
    /// Gets the failed processing result produced by the attempt.
    /// </summary>
    public ProcessingResult Result { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PipelineProcessingException"/> class.
    /// </summary>
    /// <param name="message">The failure reason.</param>
    /// <param name="dataPoint">The data point whose processing attempt failed.</param>
    /// <param name="result">The failed processing result produced by the attempt.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dataPoint"/> or <paramref name="result"/> is <see langword="null"/>.</exception>
    public PipelineProcessingException(string message, DataPoint dataPoint, ProcessingResult result)
        : base(message, "PIPELINE_PROCESSING_FAILED")
    {
        ArgumentNullException.ThrowIfNull(dataPoint);
        ArgumentNullException.ThrowIfNull(result);
        DataPoint = dataPoint;
        Result = result;
    }
}

/// <summary>
/// Thrown when a required resource is not found.
/// </summary>
public sealed class ResourceNotFoundException : PipelineException
{
    public string? ResourceId { get; set; }
    public string? ResourceType { get; set; }

    public ResourceNotFoundException(string message, string resourceType)
        : base(message, "RESOURCE_NOT_FOUND")
    {
        ResourceType = resourceType;
    }

    public ResourceNotFoundException(string message, string resourceType, string resourceId)
        : base(message, "RESOURCE_NOT_FOUND")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
}
