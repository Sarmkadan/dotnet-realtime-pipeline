#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotNetRealtimePipeline.Domain.Exceptions;

using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Provides validation helpers for <see cref="PipelineException"/> and its derived types.
/// </summary>
public static class PipelineExceptionValidation
{
    /// <summary>
    /// Validates a <see cref="PipelineException"/> instance and returns a list of validation problems.
    /// </summary>
    /// <param name="value">The exception to validate. Cannot be null.</param>
    /// <returns>A list of human-readable validation problems. Empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this PipelineException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate base PipelineException properties
        if (string.IsNullOrWhiteSpace(value.Message))
        {
            errors.Add("Message cannot be null, empty, or whitespace.");
        }

        if (value.ErrorCode is null or { Length: 0 })
        {
            errors.Add("ErrorCode cannot be null or empty.");
        }

        // Validate derived type specific properties using pattern matching
        switch (value)
        {
            case InvalidDataPointException invalidDataPoint:
                ValidateInvalidDataPointException(invalidDataPoint, errors);
                break;

            case BackpressureException backpressure:
                ValidateBackpressureException(backpressure, errors);
                break;

            case StageProcessingException stageProcessing:
                ValidateStageProcessingException(stageProcessing, errors);
                break;

            case WindowingException windowing:
                ValidateWindowingException(windowing, errors);
                break;

            case ProcessingTimeoutException timeout:
                ValidateProcessingTimeoutException(timeout, errors);
                break;

            case InvalidConfigurationException or ResourceNotFoundException:
                // These have no additional validation beyond base
                break;
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="PipelineException"/> instance is valid.
    /// </summary>
    /// <param name="value">The exception to check. Cannot be null.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this PipelineException value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Validates a <see cref="PipelineException"/> instance and throws an <see cref="ArgumentException"/>
    /// if it is not valid.
    /// </summary>
    /// <param name="value">The exception to validate. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the exception is not valid, containing a list of problems.</exception>
    public static void EnsureValid(this PipelineException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"PipelineException is not valid. Problems: {string.Join(" ", errors)}",
                nameof(value));
        }
    }

    private static void ValidateInvalidDataPointException(
        InvalidDataPointException exception,
        List<string> errors)
    {
        // InvalidDataPointException inherits from PipelineException, so base validation already checked Message and ErrorCode
        if (exception.ErrorDetails is null)
        {
            errors.Add("ErrorDetails cannot be null for InvalidDataPointException.");
        }
    }

    private static void ValidateBackpressureException(
        BackpressureException exception,
        List<string> errors)
    {
        // BackpressureException inherits from PipelineException, so base validation already checked Message and ErrorCode
        if (exception.BufferSize <= 0)
        {
            errors.Add("BufferSize must be a positive number for BackpressureException.");
        }

        if (exception.MaxCapacity <= 0)
        {
            errors.Add("MaxCapacity must be a positive number for BackpressureException.");
        }

        if (exception.MaxCapacity < exception.BufferSize)
        {
            errors.Add("MaxCapacity must be greater than or equal to BufferSize for BackpressureException.");
        }
    }

    private static void ValidateStageProcessingException(
        StageProcessingException exception,
        List<string> errors)
    {
        // StageProcessingException inherits from PipelineException, so base validation already checked Message and ErrorCode
        if (string.IsNullOrWhiteSpace(exception.StageName))
        {
            errors.Add("StageName cannot be null, empty, or whitespace for StageProcessingException.");
        }

        if (exception.RetryCount < 0)
        {
            errors.Add("RetryCount cannot be negative for StageProcessingException.");
        }
    }

    private static void ValidateWindowingException(
        WindowingException exception,
        List<string> errors)
    {
        // WindowingException inherits from PipelineException, so base validation already checked Message and ErrorCode
        if (exception.WindowId <= 0)
        {
            errors.Add("WindowId must be a positive number for WindowingException.");
        }
    }

    private static void ValidateProcessingTimeoutException(
        ProcessingTimeoutException exception,
        List<string> errors)
    {
        // ProcessingTimeoutException inherits from PipelineException, so base validation already checked Message and ErrorCode
        if (exception.TimeoutMs <= 0)
        {
            errors.Add("TimeoutMs must be a positive number for ProcessingTimeoutException.");
        }
    }
}