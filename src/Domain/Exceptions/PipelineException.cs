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
    /// <summary>
    /// Gets or sets the error code associated with this exception.
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets additional error details associated with this exception.
    /// </summary>
    public object? ErrorDetails { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PipelineException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public PipelineException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PipelineException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public PipelineException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PipelineException"/> class with a specified error message and error code.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="errorCode">The error code associated with this exception.</param>
    public PipelineException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PipelineException"/> class with a specified error message, error code, and error details.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="errorCode">The error code associated with this exception.</param>
    /// <param name="errorDetails">Additional error details associated with this exception.</param>
    public PipelineException(string message, string errorCode, object? errorDetails)
        : base(message)
    {
        ErrorCode = errorCode;
        ErrorDetails = errorDetails;
    }

    /// <summary>
    /// Returns the base exception string prefixed with the error code when one is set.
    /// </summary>
    public override string ToString()
    {
        var text = base.ToString();

        return string.IsNullOrEmpty(ErrorCode) ? text : $"[{ErrorCode}] {text}";
    }
}

/// <summary>
/// Thrown when a data point fails validation.
/// </summary>
public sealed class InvalidDataPointException : PipelineException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidDataPointException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public InvalidDataPointException(string message)
        : base(message, "INVALID_DATA_POINT") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidDataPointException"/> class with a specified error message and additional details.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="details">Additional error details associated with this exception.</param>
    public InvalidDataPointException(string message, object? details)
        : base(message, "INVALID_DATA_POINT", details) { }
}

/// <summary>
/// Thrown when backpressure limits are exceeded.
/// </summary>
public sealed class BackpressureException : PipelineException
{
    /// <summary>
    /// Gets or sets the current buffer size when the backpressure limit was exceeded.
    /// </summary>
    public long BufferSize { get; set; }

    /// <summary>
    /// Gets or sets the maximum capacity allowed before backpressure is triggered.
    /// </summary>
    public long MaxCapacity { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BackpressureException"/> class with a specified error message, buffer size, and maximum capacity.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="bufferSize">The current buffer size when the backpressure limit was exceeded.</param>
    /// <param name="maxCapacity">The maximum capacity allowed before backpressure is triggered.</param>
    public BackpressureException(string message, long bufferSize, long maxCapacity)
        : base(message, "BACKPRESSURE_EXCEEDED")
    {
        BufferSize = bufferSize;
        MaxCapacity = maxCapacity;
    }

    /// <summary>
    /// Returns the base exception string prefixed with the buffer size and maximum capacity.
    /// </summary>
    public override string ToString()
    {
        return $"[BufferSize={BufferSize}, MaxCapacity={MaxCapacity}] {base.ToString()}";
    }
}

/// <summary>
/// Thrown when a processing stage encounters an error.
/// </summary>
public sealed class StageProcessingException : PipelineException
{
    /// <summary>
    /// Gets or sets the name of the stage that encountered the error.
    /// </summary>
    public string? StageName { get; set; }

    /// <summary>
    /// Gets or sets the number of retry attempts made before the failure.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StageProcessingException"/> class with a specified error message and stage name.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="stageName">The name of the stage that encountered the error.</param>
    public StageProcessingException(string message, string stageName)
        : base(message, "STAGE_PROCESSING_FAILED")
    {
        StageName = stageName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StageProcessingException"/> class with a specified error message, stage name, and retry count.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="stageName">The name of the stage that encountered the error.</param>
    /// <param name="retryCount">The number of retry attempts made before the failure.</param>
    public StageProcessingException(string message, string stageName, int retryCount)
        : base(message, "STAGE_PROCESSING_FAILED")
    {
        StageName = stageName;
        RetryCount = retryCount;
    }

    /// <summary>
    /// Returns the base exception string prefixed with the failing stage name.
    /// </summary>
    public override string ToString()
    {
        return $"[StageName={StageName}] {base.ToString()}";
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
