#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Domain.Models;

using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Provides extension methods for <see cref="ProcessingResult"/> to enhance pipeline processing scenarios.
/// </summary>
public static class ProcessingResultExtensions
{
    /// <summary>
    /// Determines whether the processing result represents a retryable failure.
    /// A failure is considered retryable if it has an exception or error message but hasn't exceeded retry limits.
    /// </summary>
    /// <param name="result">The processing result to check.</param>
    /// <param name="maxRetryCount">Maximum allowed retry count. Defaults to 3 if not specified.</param>
    /// <returns>True if the failure is retryable; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static bool IsRetryableFailure(this ProcessingResult result, int maxRetryCount = 3)
    {
        ArgumentNullException.ThrowIfNull(result);

        return !result.Success &&
            (result.Exception != null || !string.IsNullOrWhiteSpace(result.ErrorMessage)) &&
            result.RetryCount < maxRetryCount;
    }

    /// <summary>
    /// Merges output data from another processing result into this one.
    /// Useful when combining results from multiple pipeline stages.
    /// </summary>
    /// <param name="result">The processing result to merge into.</param>
    /// <param name="source">The source processing result whose output data will be merged.</param>
    /// <param name="overwriteExisting">Whether to overwrite existing keys with values from the source. Defaults to false.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> or <paramref name="source"/> is null.</exception>
    public static void MergeOutputData(this ProcessingResult result, ProcessingResult source, bool overwriteExisting = false)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(source);

        foreach (var kvp in source.OutputData)
        {
            if (overwriteExisting || !result.OutputData.ContainsKey(kvp.Key))
            {
                result.OutputData[kvp.Key] = kvp.Value;
            }
        }
    }

    /// <summary>
    /// Gets the processing result as a structured dictionary for serialization.
    /// </summary>
    /// <param name="result">The processing result to convert.</param>
    /// <returns>A dictionary containing all result properties.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static Dictionary<string, object> ToDictionary(this ProcessingResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["ResultId"] = result.ResultId,
            ["Success"] = result.Success,
            ["StageName"] = result.StageName,
            ["ProcessingTimeMs"] = result.ProcessingTimeMs,
            ["ProcessedAt"] = result.ProcessedAt.ToString("o", CultureInfo.InvariantCulture),
            ["RetryCount"] = result.RetryCount,
            ["OutputData"] = new Dictionary<string, object>(result.OutputData),
            ["IsValid"] = result.IsValid()
        };

        if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
        {
            dict["ErrorMessage"] = result.ErrorMessage!;
        }

        if (result.Exception != null)
        {
            dict["ExceptionType"] = result.Exception.GetType().FullName ?? "Exception";
            dict["ExceptionMessage"] = result.Exception.Message;
        }

        if (!string.IsNullOrWhiteSpace(result.CorrelationId))
        {
            dict["CorrelationId"] = result.CorrelationId!;
        }

        return dict;
    }

    /// <summary>
    /// Creates a new processing result with updated processing time.
    /// </summary>
    /// <param name="result">The original processing result.</param>
    /// <param name="processingTimeMs">The new processing time in milliseconds.</param>
    /// <returns>A new processing result with the updated processing time.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static ProcessingResult WithProcessingTime(this ProcessingResult result, long processingTimeMs)
    {
        ArgumentNullException.ThrowIfNull(result);

        var clone = result.Clone(result.ResultId);
        clone.ProcessingTimeMs = processingTimeMs;
        return clone;
    }

    /// <summary>
    /// Determines whether the processing result indicates a timeout scenario.
    /// </summary>
    /// <param name="result">The processing result to check.</param>
    /// <param name="timeoutThresholdMs">Maximum allowed processing time in milliseconds. Defaults to 5000ms.</param>
    /// <returns>True if processing time exceeds the threshold; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static bool IsTimeout(this ProcessingResult result, long timeoutThresholdMs = 5000)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.ProcessingTimeMs > timeoutThresholdMs;
    }
}