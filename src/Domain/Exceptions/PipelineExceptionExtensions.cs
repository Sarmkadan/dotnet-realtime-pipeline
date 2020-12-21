#nullable enable
// =============================================================================
// Author: [Your Name]
// =============================================================================

namespace DotNetRealtimePipeline.Domain.Exceptions;

using System;
using System.Collections.Generic;

/// <summary>
/// Extension methods for <see cref="PipelineException"/>.
/// </summary>
public static class PipelineExceptionExtensions
{
    /// <summary>
    /// Gets a dictionary representation of the exception's properties.
    /// </summary>
    /// <param name="exception">The <see cref="PipelineException"/> instance.</param>
    /// <returns>A dictionary containing the exception's properties.</returns>
    public static IReadOnlyDictionary<string, object?> ToDictionary(this PipelineException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var dictionary = new Dictionary<string, object?>();

        if (exception.ErrorCode is not null)
        {
            dictionary.Add(nameof(PipelineException.ErrorCode), exception.ErrorCode);
        }

        if (exception.ErrorDetails is not null)
        {
            dictionary.Add(nameof(PipelineException.ErrorDetails), exception.ErrorDetails);
        }

        if (exception is BackpressureException backpressureException)
        {
            dictionary.Add(nameof(BackpressureException.BufferSize), backpressureException.BufferSize);
            dictionary.Add(nameof(BackpressureException.MaxCapacity), backpressureException.MaxCapacity);
        }
        else if (exception is StageProcessingException stageProcessingException)
        {
            dictionary.Add(nameof(StageProcessingException.StageName), stageProcessingException.StageName);
            dictionary.Add(nameof(StageProcessingException.RetryCount), stageProcessingException.RetryCount);
        }
        else if (exception is WindowingException windowingException)
        {
            dictionary.Add(nameof(WindowingException.WindowId), windowingException.WindowId);
        }
        else if (exception is ProcessingTimeoutException processingTimeoutException)
        {
            dictionary.Add(nameof(ProcessingTimeoutException.TimeoutMs), processingTimeoutException.TimeoutMs);
        }

        return dictionary;
    }

    /// <summary>
    /// Gets a user-friendly error message for the exception.
    /// </summary>
    /// <param name="exception">The <see cref="PipelineException"/> instance.</param>
    /// <returns>A user-friendly error message.</returns>
    public static string GetUserFriendlyErrorMessage(this PipelineException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var errorMessage = exception.Message;

        if (exception.ErrorCode is not null)
        {
            errorMessage += $" (Error Code: {exception.ErrorCode})";
        }

        return errorMessage;
    }
}
