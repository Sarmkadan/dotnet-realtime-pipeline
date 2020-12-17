#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Domain.Models;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents the result of processing a data point or window through the pipeline.
/// Tracks success/failure, processing metrics, and output data.
/// </summary>
public sealed class ProcessingResult
{
    /// <summary>
    /// Gets or sets the unique identifier for this processing result.
    /// </summary>
    public long ResultId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether processing was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the name of the pipeline stage that processed this result.
    /// </summary>
    public string StageName { get; set; } = "";

    /// <summary>
    /// Gets or sets the processing time in milliseconds.
    /// </summary>
    public long ProcessingTimeMs { get; set; }

    /// <summary>
    /// Gets or sets the error message if processing failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the exception if processing failed.
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when processing completed.
    /// </summary>
    public DateTime ProcessedAt { get; set; }

    /// <summary>
    /// Gets or sets the output data as key-value pairs.
    /// </summary>
    public Dictionary<string, object> OutputData { get; set; } = new();

    /// <summary>
    /// Gets or sets the number of retry attempts made for this processing operation.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Gets or sets the correlation identifier for tracing this processing operation.
    /// </summary>
    public string? CorrelationId { get; set; }

    public ProcessingResult()
    {
    }

    public ProcessingResult(long resultId, bool success, string stageName)
    {
        ResultId = resultId;
        Success = success;
        StageName = stageName ?? throw new ArgumentNullException(nameof(stageName));
        ProcessedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks this result as a failure with error details.
    /// </summary>
    public void MarkFailure(string message, Exception? exception = null)
    {
        Success = false;
        ErrorMessage = message ?? throw new ArgumentNullException(nameof(message));
        Exception = exception;
    }

    /// <summary>
    /// Marks this result as successful.
    /// </summary>
    public void MarkSuccess()
    {
        Success = true;
        ErrorMessage = null;
        Exception = null;
    }

    /// <summary>
    /// Adds data to the output payload.
    /// </summary>
    public void AddOutput(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key cannot be null", nameof(key));
        OutputData[key] = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Retrieves a value from the output data.
    /// </summary>
    public object? GetOutput(string key)
    {
        return OutputData.TryGetValue(key, out var value) ? value : null;
    }

    /// <summary>
    /// Increments the retry counter.
    /// </summary>
    public void IncrementRetryCount()
    {
        RetryCount++;
    }

    /// <summary>
    /// Validates that processing result has required data.
    /// </summary>
    public bool IsValid()
    {
        if (ResultId <= 0) return false;
        if (string.IsNullOrWhiteSpace(StageName)) return false;
        if (!Success && string.IsNullOrWhiteSpace(ErrorMessage)) return false;
        return true;
    }

    /// <summary>
    /// Gets a summary of this result for logging.
    /// </summary>
    public string GetSummary()
    {
        return $"Result[Id={ResultId}, Stage={StageName}, Success={Success}, ProcessingTime={ProcessingTimeMs}ms, Retries={RetryCount}]";
    }

    /// <summary>
    /// Creates a copy of this result with a new ID.
    /// </summary>
    public ProcessingResult Clone(long newResultId)
    {
        return new ProcessingResult(newResultId, Success, StageName)
        {
            ProcessingTimeMs = ProcessingTimeMs,
            ErrorMessage = ErrorMessage,
            Exception = Exception,
            ProcessedAt = ProcessedAt,
            RetryCount = RetryCount,
            CorrelationId = CorrelationId,
            OutputData = new Dictionary<string, object>(OutputData)
        };
    }
}
