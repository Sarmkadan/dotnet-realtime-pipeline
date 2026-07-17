using System;
using System.Collections.Generic;

namespace DotNetRealtimePipeline.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="BackpressureContext"/> instances.
/// </summary>
public static class BackpressureContextValidation
{
    /// <summary>
    /// Validates the specified <see cref="BackpressureContext"/> instance.
    /// </summary>
    /// <param name="value">The backpressure context to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this BackpressureContext value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate ContextId
        if (value.ContextId <= 0)
        {
            errors.Add($"ContextId must be positive, but was {value.ContextId}.");
        }

        // Validate PipelineStageName
        if (string.IsNullOrWhiteSpace(value.PipelineStageName))
        {
            errors.Add("PipelineStageName cannot be null or whitespace.");
        }
        else if (value.PipelineStageName.Length > 100)
        {
            errors.Add($"PipelineStageName exceeds maximum length of 100 characters, but was {value.PipelineStageName.Length}.");
        }

        // Validate BufferSize
        if (value.BufferSize < 0)
        {
            errors.Add($"BufferSize cannot be negative, but was {value.BufferSize}.");
        }

        // Validate MaxBufferCapacity
        if (value.MaxBufferCapacity <= 0)
        {
            errors.Add($"MaxBufferCapacity must be positive, but was {value.MaxBufferCapacity}.");
        }
        else if (value.BufferSize > value.MaxBufferCapacity)
        {
            errors.Add($"BufferSize ({value.BufferSize}) cannot exceed MaxBufferCapacity ({value.MaxBufferCapacity}).");
        }

        // Validate IsBackpressured
        // No validation needed for boolean

        // Validate BackpressureStartTimeMs
        if (value.BackpressureStartTimeMs < 0)
        {
            errors.Add($"BackpressureStartTimeMs cannot be negative, but was {value.BackpressureStartTimeMs}.");
        }

        // Validate TotalBackpressureTimeMs
        if (value.TotalBackpressureTimeMs < 0)
        {
            errors.Add($"TotalBackpressureTimeMs cannot be negative, but was {value.TotalBackpressureTimeMs}.");
        }

        // Validate DroppedItemCount
        if (value.DroppedItemCount < 0)
        {
            errors.Add($"DroppedItemCount cannot be negative, but was {value.DroppedItemCount}.");
        }

        // Validate ActiveConsumers
        if (value.ActiveConsumers < 0)
        {
            errors.Add($"ActiveConsumers cannot be negative, but was {value.ActiveConsumers}.");
        }

        // Validate MaxConcurrentConsumers
        if (value.MaxConcurrentConsumers <= 0)
        {
            errors.Add($"MaxConcurrentConsumers must be positive, but was {value.MaxConcurrentConsumers}.");
        }
        else if (value.ActiveConsumers > value.MaxConcurrentConsumers)
        {
            errors.Add($"ActiveConsumers ({value.ActiveConsumers}) cannot exceed MaxConcurrentConsumers ({value.MaxConcurrentConsumers}).");
        }

        // Validate CreatedAt
        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt cannot be the default DateTime value.");
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add($"CreatedAt cannot be in the future, but was {value.CreatedAt:O}.");
        }

        // Validate LastUpdatedAt
        if (value.LastUpdatedAt != default && value.LastUpdatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add($"LastUpdatedAt cannot be in the future, but was {value.LastUpdatedAt:O}.");
        }

        // Validate BackpressureEventTimestamps
        if (value.BackpressureEventTimestamps is null)
        {
            errors.Add("BackpressureEventTimestamps collection cannot be null.");
        }
        else
        {
            // Check for negative timestamps
            foreach (var timestamp in value.BackpressureEventTimestamps)
            {
                if (timestamp < 0)
                {
                    errors.Add($"BackpressureEventTimestamps contains negative timestamp: {timestamp}.");
                    break;
                }
            }
        }

        // Validate BufferMetrics
        if (value.BufferMetrics is null)
        {
            errors.Add("BufferMetrics dictionary cannot be null.");
        }
        else
        {
            // Check for negative values in metrics
            foreach (var kvp in value.BufferMetrics)
            {
                if (kvp.Value < 0)
                {
                    errors.Add($"BufferMetrics['{kvp.Key}'] cannot be negative, but was {kvp.Value}.");
                }
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="BackpressureContext"/> instance is valid.
    /// </summary>
    /// <param name="value">The backpressure context to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this BackpressureContext value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="BackpressureContext"/> instance is valid.
    /// </summary>
    /// <param name="value">The backpressure context to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, containing a list of validation problems.</exception>
    public static void EnsureValid(this BackpressureContext value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"BackpressureContext is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}