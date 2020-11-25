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
/// Provides validation helpers for <see cref="StreamEvent"/> instances.
/// </summary>
public static class StreamEventValidation
{
    /// <summary>
    /// Validates the specified stream event and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The stream event to validate.</param>
    /// <returns>A read-only list of validation problems; empty if the event is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> ValidateEvent(this StreamEvent value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate EventId
        if (value.EventId <= 0)
        {
            problems.Add($"EventId must be positive (current: {value.EventId}).");
        }

        // Validate DataPointId
        if (value.DataPointId <= 0)
        {
            problems.Add($"DataPointId must be positive (current: {value.DataPointId}).");
        }

        // Validate Timestamp
        if (value.Timestamp <= 0)
        {
            problems.Add($"Timestamp must be positive (current: {value.Timestamp}).");
        }
        else if (value.Timestamp > DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
        {
            problems.Add("Timestamp cannot be in the future.");
        }

        // Validate EventType
        if (string.IsNullOrWhiteSpace(value.EventType))
        {
            problems.Add("EventType cannot be null or whitespace.");
        }
        else if (value.EventType.Length > 100)
        {
            problems.Add($"EventType exceeds maximum length of 100 characters (current: {value.EventType.Length}).");
        }

        // Validate Priority
        if (value.Priority < 1 || value.Priority > 10)
        {
            problems.Add($"Priority must be between 1 and 10 inclusive (current: {value.Priority}).");
        }

        // Validate SourceSystem
        if (value.SourceSystem is { Length: > 100 })
        {
            problems.Add($"SourceSystem exceeds maximum length of 100 characters (current: {value.SourceSystem.Length}).");
        }

        // Validate CorrelationId
        if (value.CorrelationId is { Length: > 100 })
        {
            problems.Add($"CorrelationId exceeds maximum length of 100 characters (current: {value.CorrelationId.Length}).");
        }

        // Validate CausationId
        if (value.CausationId is { Length: > 100 })
        {
            problems.Add($"CausationId exceeds maximum length of 100 characters (current: {value.CausationId.Length}).");
        }

        // Validate Payload
        if (value.Payload is null)
        {
            problems.Add("Payload dictionary cannot be null.");
        }
        else if (value.Payload.Count > 1000)
        {
            problems.Add($"Payload dictionary exceeds maximum size of 1000 items (current: {value.Payload.Count}).");
        }

        // Validate ProcessedByStages
        if (value.ProcessedByStages is null)
        {
            problems.Add("ProcessedByStages list cannot be null.");
        }
        else
        {
            if (value.ProcessedByStages.Count > 50)
            {
                problems.Add($"ProcessedByStages list exceeds maximum size of 50 items (current: {value.ProcessedByStages.Count}).");
            }

            foreach (var stage in value.ProcessedByStages)
            {
                if (string.IsNullOrWhiteSpace(stage))
                {
                    problems.Add("ProcessedByStages contains null or whitespace entries.");
                    break;
                }

                if (stage.Length > 100)
                {
                    problems.Add($"ProcessedByStages entry exceeds maximum length of 100 characters (current: {stage.Length}).");
                    break;
                }
            }
        }

        // Validate CreatedAt
        if (value.CreatedAt == default)
        {
            problems.Add("CreatedAt cannot be default (Unix epoch).");
        }
        else if (value.CreatedAt > DateTime.UtcNow)
        {
            problems.Add("CreatedAt cannot be in the future.");
        }

        // Validate CompletedAt
        if (value.CompletedAt.HasValue)
        {
            if (value.CompletedAt.Value == default)
            {
                problems.Add("CompletedAt cannot be default (Unix epoch) when set.");
            }
            else if (value.CompletedAt.Value < value.CreatedAt)
            {
                problems.Add("CompletedAt cannot be earlier than CreatedAt.");
            }
            else if (value.CompletedAt.Value > DateTime.UtcNow)
            {
                problems.Add("CompletedAt cannot be in the future.");
            }
        }

        // Validate RetryAttempt
        if (value.IsRetry && value.RetryAttempt < 1)
        {
            problems.Add("RetryAttempt must be at least 1 when IsRetry is true.");
        }

        if (value.RetryAttempt > 100)
        {
            problems.Add($"RetryAttempt exceeds maximum of 100 (current: {value.RetryAttempt}).");
        }

        // Validate LastErrorMessage
        if (value.IsRetry && string.IsNullOrWhiteSpace(value.LastErrorMessage))
        {
            problems.Add("LastErrorMessage cannot be null or whitespace when IsRetry is true.");
        }
        else if (value.LastErrorMessage is { Length: > 1000 })
        {
            problems.Add($"LastErrorMessage exceeds maximum length of 1000 characters (current: {value.LastErrorMessage.Length}).");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified stream event is valid.
    /// </summary>
    /// <param name="value">The stream event to check.</param>
    /// <returns><see langword="true"/> if the event is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this StreamEvent value)
    {
        return value.ValidateEvent().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified stream event is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The stream event to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the event is invalid, containing a list of validation problems.</exception>
    public static void EnsureValid(this StreamEvent value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.ValidateEvent();
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"StreamEvent is invalid. Problems: {string.Join("; ", problems)}",
            nameof(value));
    }
}