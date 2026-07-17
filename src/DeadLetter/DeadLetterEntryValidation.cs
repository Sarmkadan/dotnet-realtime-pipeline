#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.DeadLetter;

using System;
using System.Collections.Generic;

/// <summary>
/// Provides validation helpers for <see cref="DeadLetterEntry"/> instances.
/// </summary>
public static class DeadLetterEntryValidation
{
    /// <summary>
    /// Validates the specified <see cref="DeadLetterEntry"/> instance.
    /// </summary>
    /// <param name="value">The dead-letter entry to validate.</param>
    /// <returns>A list of validation problems; empty if the entry is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this DeadLetterEntry value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate EntryId
        if (value.EntryId == Guid.Empty)
        {
            problems.Add("EntryId must be a non-empty GUID.");
        }

        // Validate DataPoint
        if (value.DataPoint is null)
        {
            problems.Add("DataPoint cannot be null.");
        }
        else if (value.DataPoint.Id <= 0)
        {
            problems.Add("DataPoint.Id must be a positive number.");
        }

        // Validate FailureStageName
        if (string.IsNullOrWhiteSpace(value.FailureStageName))
        {
            problems.Add("FailureStageName cannot be null or whitespace.");
        }

        // Validate FailureReason
        if (string.IsNullOrWhiteSpace(value.FailureReason))
        {
            problems.Add("FailureReason cannot be null or whitespace.");
        }

        // Validate RetryCount
        if (value.RetryCount < 0)
        {
            problems.Add($"RetryCount cannot be negative, but was {value.RetryCount}.");
        }

        if (value.MaxRetries < 0)
        {
            problems.Add($"MaxRetries cannot be negative, but was {value.MaxRetries}.");
        }
        else if (value.MaxRetries == 0)
        {
            problems.Add("MaxRetries must be greater than zero.");
        }

        // Validate RetryCount vs MaxRetries
        if (value.RetryCount > value.MaxRetries)
        {
            problems.Add($"RetryCount ({value.RetryCount}) cannot exceed MaxRetries ({value.MaxRetries}).");
        }

        // Validate EnqueuedAt
        if (value.EnqueuedAt == default)
        {
            problems.Add("EnqueuedAt cannot be the default DateTime value.");
        }
        else if (value.EnqueuedAt.Kind != DateTimeKind.Utc)
        {
            problems.Add("EnqueuedAt must be in UTC, but was not marked as such.");
        }
        else if (value.EnqueuedAt > DateTime.UtcNow.AddMinutes(1))
        {
            problems.Add("EnqueuedAt cannot be in the future.");
        }

        // Validate LastRetryAt if set
        if (value.LastRetryAt.HasValue)
        {
            if (value.LastRetryAt.Value == default)
            {
                problems.Add("LastRetryAt cannot be the default DateTime value when set.");
            }
            else if (value.LastRetryAt.Value.Kind != DateTimeKind.Utc)
            {
                problems.Add("LastRetryAt must be in UTC when set, but was not marked as such.");
            }
            else if (value.LastRetryAt.Value > DateTime.UtcNow.AddMinutes(1))
            {
                problems.Add("LastRetryAt cannot be in the future.");
            }

            // LastRetryAt should not be before EnqueuedAt
            if (value.LastRetryAt.Value < value.EnqueuedAt)
            {
                problems.Add("LastRetryAt cannot be earlier than EnqueuedAt.");
            }
        }

        // Validate ResolvedAt if set
        if (value.ResolvedAt.HasValue)
        {
            if (value.ResolvedAt.Value == default)
            {
                problems.Add("ResolvedAt cannot be the default DateTime value when set.");
            }
            else if (value.ResolvedAt.Value.Kind != DateTimeKind.Utc)
            {
                problems.Add("ResolvedAt must be in UTC when set, but was not marked as such.");
            }
            else if (value.ResolvedAt.Value > DateTime.UtcNow.AddMinutes(1))
            {
                problems.Add("ResolvedAt cannot be in the future.");
            }

            // ResolvedAt should not be before EnqueuedAt
            if (value.ResolvedAt.Value < value.EnqueuedAt)
            {
                problems.Add("ResolvedAt cannot be earlier than EnqueuedAt.");
            }

            // ResolvedAt should not be before LastRetryAt if both are set
            if (value.LastRetryAt.HasValue && value.ResolvedAt.Value < value.LastRetryAt.Value)
            {
                problems.Add("ResolvedAt cannot be earlier than LastRetryAt when both are set.");
            }
        }

        // Validate Status consistency with timestamps using pattern matching
        switch (value.Status)
        {
            case DeadLetterStatus.Resolved or DeadLetterStatus.PermanentFailure when !value.ResolvedAt.HasValue:
                problems.Add($"Status is {value.Status} but ResolvedAt is not set.");
                break;

            case DeadLetterStatus.InRetry when !value.LastRetryAt.HasValue:
                problems.Add("Status is InRetry but LastRetryAt is not set.");
                break;

            case DeadLetterStatus.Pending:
                // No specific timestamp requirements for Pending status
                break;
        }

        // Validate ResolutionNote if set
        if (value.ResolutionNote is not null && string.IsNullOrWhiteSpace(value.ResolutionNote))
        {
            problems.Add("ResolutionNote cannot be whitespace when set.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="DeadLetterEntry"/> is valid.
    /// </summary>
    /// <param name="value">The dead-letter entry to check.</param>
    /// <returns><see langword="true"/> if the entry is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this DeadLetterEntry value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="DeadLetterEntry"/> is valid.
    /// </summary>
    /// <param name="value">The dead-letter entry to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the entry is not valid; the message contains the list of problems.</exception>
    public static void EnsureValid(this DeadLetterEntry value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"DeadLetterEntry is not valid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }
}