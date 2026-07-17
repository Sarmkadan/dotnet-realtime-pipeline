#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Domain.Models;

using System;
using System.Collections.Generic;

/// <summary>
/// Provides validation helpers for <see cref="ProcessingResult"/> instances.
/// </summary>
public static class ProcessingResultValidation
{
    /// <summary>
    /// Validates a <see cref="ProcessingResult"/> instance and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The result to validate.</param>
    /// <returns>A read-only list of validation error messages; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ProcessingResult value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate ResultId
        if (value.ResultId <= 0)
        {
            errors.Add($"ResultId must be a positive integer, but was {value.ResultId}.");
        }

        // Validate StageName
        if (string.IsNullOrWhiteSpace(value.StageName))
        {
            errors.Add("StageName cannot be null, empty, or whitespace.");
        }
        else if (value.StageName.Length > 256)
        {
            errors.Add("StageName cannot exceed 256 characters.");
        }

        // Validate ProcessingTimeMs
        if (value.ProcessingTimeMs < 0)
        {
            errors.Add($"ProcessingTimeMs cannot be negative, but was {value.ProcessingTimeMs}.");
        }

        // Validate ProcessedAt
        if (value.ProcessedAt == default)
        {
            errors.Add("ProcessedAt cannot be the default DateTime value.");
        }
        else if (value.ProcessedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("ProcessedAt cannot be in the future.");
        }
        else if (value.ProcessedAt < DateTime.UtcNow.AddMinutes(-5))
        {
            errors.Add("ProcessedAt cannot be more than 5 minutes in the past.");
        }

        // Validate RetryCount
        if (value.RetryCount < 0)
        {
            errors.Add($"RetryCount cannot be negative, but was {value.RetryCount}.");
        }

        // Validate ErrorMessage when not successful
        if (!value.Success && string.IsNullOrWhiteSpace(value.ErrorMessage))
        {
            errors.Add("ErrorMessage must be provided when Success is false.");
        }

        // Validate ErrorMessage length
        if (!string.IsNullOrEmpty(value.ErrorMessage) && value.ErrorMessage.Length > 4096)
        {
            errors.Add("ErrorMessage cannot exceed 4096 characters.");
        }

        // Validate Exception consistency
        if (value.Exception is null && !string.IsNullOrEmpty(value.ErrorMessage))
        {
            errors.Add("Exception should be provided when ErrorMessage is set.");
        }
        else if (value.Exception is not null && string.IsNullOrEmpty(value.ErrorMessage))
        {
            errors.Add("ErrorMessage should be provided when Exception is set.");
        }

        // Validate OutputData
        if (value.OutputData is null)
        {
            errors.Add("OutputData dictionary cannot be null.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="ProcessingResult"/> instance is valid.
    /// </summary>
    /// <param name="value">The result to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ProcessingResult value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="ProcessingResult"/> instance is valid, throwing an <see cref="ArgumentException"/>
    /// with detailed validation messages if it is not.
    /// </summary>
    /// <param name="value">The result to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, containing a list of validation problems.</exception>
    public static void EnsureValid(this ProcessingResult value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"ProcessingResult is invalid. Validation errors: {string.Join(" ", errors)}",
            nameof(value)
        );
    }
}