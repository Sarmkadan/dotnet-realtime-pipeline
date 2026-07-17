#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================================

namespace DotNetRealtimePipeline.Domain.Exceptions;

using System;
using System.Collections.Generic;

/// <summary>
/// Extension methods for <see cref="PipelineException"/> and its derived types.
/// </summary>
public static class PipelineExceptionExtensions
{
    /// <summary>
    /// Gets a dictionary representation of the exception's properties.
    /// </summary>
    /// <param name="exception">The <see cref="PipelineException"/> instance. Cannot be null.</param>
    /// <returns>A dictionary containing the exception's properties.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
    public static IReadOnlyDictionary<string, object?> ToDictionary(this PipelineException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var dictionary = new Dictionary<string, object?>();

        if (exception.ErrorCode is not null)
        {
            dictionary[nameof(PipelineException.ErrorCode)] = exception.ErrorCode;
        }

        if (exception.ErrorDetails is not null)
        {
            dictionary[nameof(PipelineException.ErrorDetails)] = exception.ErrorDetails;
        }

        switch (exception)
        {
            case BackpressureException backpressureException:
                dictionary[nameof(BackpressureException.BufferSize)] = backpressureException.BufferSize;
                dictionary[nameof(BackpressureException.MaxCapacity)] = backpressureException.MaxCapacity;
                break;

            case StageProcessingException stageProcessingException:
                dictionary[nameof(StageProcessingException.StageName)] = stageProcessingException.StageName;
                dictionary[nameof(StageProcessingException.RetryCount)] = stageProcessingException.RetryCount;
                break;

            case WindowingException windowingException:
                dictionary[nameof(WindowingException.WindowId)] = windowingException.WindowId;
                break;

            case ProcessingTimeoutException processingTimeoutException:
                dictionary[nameof(ProcessingTimeoutException.TimeoutMs)] = processingTimeoutException.TimeoutMs;
                break;

            case InvalidDataPointException invalidDataPointException:
                dictionary[nameof(InvalidDataPointException.ErrorDetails)] = invalidDataPointException.ErrorDetails;
                break;

            case ResourceNotFoundException resourceNotFoundException:
                if (resourceNotFoundException.ResourceId is not null)
                {
                    dictionary[nameof(ResourceNotFoundException.ResourceId)] = resourceNotFoundException.ResourceId;
                }

                if (resourceNotFoundException.ResourceType is not null)
                {
                    dictionary[nameof(ResourceNotFoundException.ResourceType)] = resourceNotFoundException.ResourceType;
                }

                break;

            case InvalidConfigurationException:
                break;
        }

        return dictionary;
    }

    /// <summary>
    /// Gets a user-friendly error message for the exception.
    /// </summary>
    /// <param name="exception">The <see cref="PipelineException"/> instance. Cannot be null.</param>
    /// <returns>A user-friendly error message.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
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
