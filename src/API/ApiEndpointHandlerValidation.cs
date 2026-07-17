#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.API;

using System;
using System.Collections.Generic;

/// <summary>
/// Provides validation helpers for API endpoint handler types and their responses.
/// </summary>
public static class ApiEndpointHandlerValidation
{
    /// <summary>
    /// Validates an API response wrapper instance.
    /// </summary>
    /// <param name="response">The API response to validate.</param>
    /// <returns>A list of human-readable validation problems, or an empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="response"/> is null.</exception>
    public static IReadOnlyList<string> Validate<T>(this ApiEndpointHandler.ApiResponse<T> response)
    {
        ArgumentNullException.ThrowIfNull(response);

        var errors = new List<string>();

        // Validate Success
        // Success can be true or false, no specific validation needed

        // Validate Message
        if (response.Message is not null)
        {
            if (response.Message.Length == 0)
            {
                errors.Add("Message cannot be empty");
            }
            else if (response.Message.Length > 1000)
            {
                errors.Add("Message cannot exceed 1000 characters");
            }
        }

        // Validate StatusCode
        if (response.StatusCode is 0)
        {
            errors.Add("StatusCode must be set to a non-default value");
        }
        else if (response.StatusCode < 100 || response.StatusCode > 599)
        {
            errors.Add("StatusCode must be between 100 and 599");
        }

        // Validate Timestamp
        if (response.Timestamp == default)
        {
            errors.Add("Timestamp must be set to a non-default DateTime");
        }
        else if (response.Timestamp > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("Timestamp cannot be in the future");
        }
        else if (response.Timestamp < DateTime.UtcNow.AddYears(-1))
        {
            errors.Add("Timestamp cannot be more than one year in the past");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates a BatchIngestResult instance.
    /// </summary>
    /// <param name="result">The batch ingest result to validate.</param>
    /// <returns>A list of human-readable validation problems, or an empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this BatchIngestResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var errors = new List<string>();

        // Validate SuccessfulCount
        if (result.SuccessfulCount < 0)
        {
            errors.Add("SuccessfulCount cannot be negative");
        }

        // Validate FailedCount
        if (result.FailedCount < 0)
        {
            errors.Add("FailedCount cannot be negative");
        }

        // Validate TotalCount
        if (result.TotalCount < 0)
        {
            errors.Add("TotalCount cannot be negative");
        }
        else if (result.SuccessfulCount + result.FailedCount != result.TotalCount)
        {
            errors.Add("TotalCount must equal SuccessfulCount + FailedCount");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates a PipelineStatusInfo instance.
    /// </summary>
    /// <param name="status">The pipeline status info to validate.</param>
    /// <returns>A list of human-readable validation problems, or an empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="status"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this PipelineStatusInfo status)
    {
        ArgumentNullException.ThrowIfNull(status);

        var errors = new List<string>();

        // Validate PipelineName
        if (string.IsNullOrWhiteSpace(status.PipelineName))
        {
            errors.Add("PipelineName cannot be null or whitespace");
        }
        else if (status.PipelineName.Length > 200)
        {
            errors.Add("PipelineName cannot exceed 200 characters");
        }

        // Validate Version
        if (string.IsNullOrWhiteSpace(status.Version))
        {
            errors.Add("Version cannot be null or whitespace");
        }
        else if (!status.Version.StartsWith("v", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("Version should start with 'v' (e.g., v1.0.0)");
        }

        // Validate IsRunning
        // No specific validation needed for boolean

        // Validate TotalProcessed
        if (status.TotalProcessed < 0)
        {
            errors.Add("TotalProcessed cannot be negative");
        }

        // Validate TotalFailed
        if (status.TotalFailed < 0)
        {
            errors.Add("TotalFailed cannot be negative");
        }

        // Validate Pending
        if (status.Pending < 0)
        {
            errors.Add("Pending cannot be negative");
        }

        // Validate HealthStatus
        if (string.IsNullOrWhiteSpace(status.HealthStatus))
        {
            errors.Add("HealthStatus cannot be null or whitespace");
        }
        else if (status.HealthStatus.Length > 50)
        {
            errors.Add("HealthStatus cannot exceed 50 characters");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified API response is valid.
    /// </summary>
    /// <param name="response">The API response to check.</param>
    /// <returns>True if the response is valid; otherwise, false.</returns>
    public static bool IsValid<T>(this ApiEndpointHandler.ApiResponse<T> response) => response.Validate().Count == 0;

    /// <summary>
    /// Determines whether the specified batch ingest result is valid.
    /// </summary>
    /// <param name="result">The batch ingest result to check.</param>
    /// <returns>True if the result is valid; otherwise, false.</returns>
    public static bool IsValid(this BatchIngestResult result) => result.Validate().Count == 0;

    /// <summary>
    /// Determines whether the specified pipeline status info is valid.
    /// </summary>
    /// <param name="status">The pipeline status info to check.</param>
    /// <returns>True if the status info is valid; otherwise, false.</returns>
    public static bool IsValid(this PipelineStatusInfo status) => status.Validate().Count == 0;

    /// <summary>
    /// Ensures that the specified API response is valid, throwing an exception if not.
    /// </summary>
    /// <param name="response">The API response to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the response is not valid.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="response"/> is null.</exception>
    public static void EnsureValid<T>(this ApiEndpointHandler.ApiResponse<T> response)
    {
        ArgumentNullException.ThrowIfNull(response);

        var errors = response.Validate();
        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"ApiResponse<T> validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
    }

    /// <summary>
    /// Ensures that the specified batch ingest result is valid, throwing an exception if not.
    /// </summary>
    /// <param name="result">The batch ingest result to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the result is not valid.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is null.</exception>
    public static void EnsureValid(this BatchIngestResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var errors = result.Validate();
        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"BatchIngestResult validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
    }

    /// <summary>
    /// Ensures that the specified pipeline status info is valid, throwing an exception if not.
    /// </summary>
    /// <param name="status">The pipeline status info to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the status info is not valid.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="status"/> is null.</exception>
    public static void EnsureValid(this PipelineStatusInfo status)
    {
        ArgumentNullException.ThrowIfNull(status);

        var errors = status.Validate();
        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"PipelineStatusInfo validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
    }
}