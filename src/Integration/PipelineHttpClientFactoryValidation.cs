#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Integration;

using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Provides validation extensions for <see cref="PipelineHttpClientFactory"/> instances.
/// </summary>
public static class PipelineHttpClientFactoryValidation
{
    /// <summary>
    /// Validates the specified <see cref="PipelineHttpClientFactory"/> instance.
    /// </summary>
    /// <param name="value">The <see cref="PipelineHttpClientFactory"/> to validate.</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this PipelineHttpClientFactory value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Timeout
        if (value.Timeout <= TimeSpan.Zero)
        {
            errors.Add("PipelineHttpClientFactory.Timeout must be greater than zero.");
        }

        // Validate MaxRetries
        if (value.MaxRetries < 0)
        {
            errors.Add("PipelineHttpClientFactory.MaxRetries must be a non-negative integer.");
        }

        // Validate RetryDelay
        if (value.RetryDelay <= TimeSpan.Zero)
        {
            errors.Add("PipelineHttpClientFactory.RetryDelay must be greater than zero.");
        }

        // Validate MaxConnectionsPerHost
        if (value.MaxConnectionsPerHost <= 0)
        {
            errors.Add("PipelineHttpClientFactory.MaxConnectionsPerHost must be greater than zero.");
        }

        // Validate UserAgent
        if (string.IsNullOrWhiteSpace(value.UserAgent))
        {
            errors.Add("PipelineHttpClientFactory.UserAgent must not be null, empty, or whitespace.");
        }

        // Validate DefaultHeaders
        if (value.DefaultHeaders is null)
        {
            errors.Add("PipelineHttpClientFactory.DefaultHeaders must not be null.");
        }
        else
        {
            // Check if the dictionary itself is empty (not an error, but worth noting)
            if (value.DefaultHeaders.Count == 0)
            {
                errors.Add("PipelineHttpClientFactory.DefaultHeaders collection is empty.");
            }

            // Check if any header values are null or whitespace
            foreach (var header in value.DefaultHeaders)
            {
                if (string.IsNullOrWhiteSpace(header.Value))
                {
                    errors.Add($"PipelineHttpClientFactory.DefaultHeaders['{header.Key}'] must not be null, empty, or whitespace.");
                }
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="PipelineHttpClientFactory"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="PipelineHttpClientFactory"/> to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this PipelineHttpClientFactory value) => value?.Validate().Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="PipelineHttpClientFactory"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="PipelineHttpClientFactory"/> to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is not valid.</exception>
    public static void EnsureValid(this PipelineHttpClientFactory value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"PipelineHttpClientFactory is not valid. Errors:{Environment.NewLine}- {
                    string.Join(
                        $"\n- ",
                        errors
                    )
                }",
                nameof(value)
            );
        }
    }
}