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
public class ProcessingResult
{
    public long ResultId { get; set; }
    public bool Success { get; set; }
    public string StageName { get; set; } = "";
    public long ProcessingTimeMs { get; set; }
    public string? ErrorMessage { get; set; }
    public Exception? Exception { get; set; }
    public DateTime ProcessedAt { get; set; }
    public Dictionary<string, object> OutputData { get; set; } = new();
    public int RetryCount { get; set; }
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
