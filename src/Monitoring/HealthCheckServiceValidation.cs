#nullable enable

namespace DotNetRealtimePipeline.Monitoring;

using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Provides validation helpers for <see cref="HealthCheckService"/> and related health check types.
/// </summary>
public static class HealthCheckServiceValidation
{
    /// <summary>
    /// Validates a <see cref="HealthCheckService"/> instance.
    /// </summary>
    /// <param name="value">The health check service to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> Validate(this HealthCheckService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // HealthCheckService itself has no state to validate
        // Validation is performed on the results it produces

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="HealthCheckService"/> instance is valid.
    /// </summary>
    /// <param name="value">The health check service to check.</param>
    /// <returns><c>true</c> if the instance is valid; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static bool IsValid(this HealthCheckService value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="HealthCheckService"/> instance is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The health check service to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is not valid, containing a list of validation problems.</exception>
    public static void EnsureValid(this HealthCheckService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"HealthCheckService validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Validates a <see cref="ComponentHealth"/> instance.
    /// </summary>
    /// <param name="value">The component health to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> Validate(this ComponentHealth value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(value.Message))
        {
            problems.Add("ComponentHealth.Message must not be null or whitespace.");
        }

        if (value.CheckedAt == default)
        {
            problems.Add("ComponentHealth.CheckedAt must be set to a valid DateTime.");
        }

        if (value.Details is null)
        {
            problems.Add("ComponentHealth.Details must not be null.");
        }
        else if (value.Details.Count == 0)
        {
            problems.Add("ComponentHealth.Details must not be empty.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="ComponentHealth"/> instance is valid.
    /// </summary>
    /// <param name="value">The component health to check.</param>
    /// <returns><c>true</c> if the instance is valid; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static bool IsValid(this ComponentHealth value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="ComponentHealth"/> instance is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The component health to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is not valid, containing a list of validation problems.</exception>
    public static void EnsureValid(this ComponentHealth value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ComponentHealth validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Validates a <see cref="SystemHealthReport"/> instance.
    /// </summary>
    /// <param name="value">The system health report to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> Validate(this SystemHealthReport value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (value.CheckedAt == default)
        {
            problems.Add("SystemHealthReport.CheckedAt must be set to a valid DateTime.");
        }

        if (value.OverallStatus == default)
        {
            problems.Add("SystemHealthReport.OverallStatus must be set to a valid SystemHealth value.");
        }

        if (value.Components is null)
        {
            problems.Add("SystemHealthReport.Components must not be null.");
        }
        else if (value.Components.Count == 0)
        {
            problems.Add("SystemHealthReport.Components must not be empty.");
        }

        if (value.PipelineStatus is null)
        {
            problems.Add("SystemHealthReport.PipelineStatus must not be null.");
        }
        else if (string.IsNullOrWhiteSpace(value.PipelineStatus))
        {
            problems.Add("SystemHealthReport.PipelineStatus must not be null or whitespace.");
        }

        // Validate throughput is non-negative
        if (value.Throughput < 0)
        {
            problems.Add("SystemHealthReport.Throughput must be a non-negative value.");
        }

        // Validate success rate is between 0 and 100
        if (value.SuccessRate < 0 || value.SuccessRate > 100)
        {
            problems.Add("SystemHealthReport.SuccessRate must be between 0 and 100.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="SystemHealthReport"/> instance is valid.
    /// </summary>
    /// <param name="value">The system health report to check.</param>
    /// <returns><c>true</c> if the instance is valid; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static bool IsValid(this SystemHealthReport value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="SystemHealthReport"/> instance is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The system health report to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is not valid, containing a list of validation problems.</exception>
    public static void EnsureValid(this SystemHealthReport value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"SystemHealthReport validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Validates a <see cref="QuickHealthStatus"/> instance.
    /// </summary>
    /// <param name="value">The quick health status to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> Validate(this QuickHealthStatus value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (value.HealthStatus is null)
        {
            problems.Add("QuickHealthStatus.HealthStatus must not be null.");
        }
        else if (string.IsNullOrWhiteSpace(value.HealthStatus))
        {
            problems.Add("QuickHealthStatus.HealthStatus must not be null or whitespace.");
        }

        if (!value.IsRunning && value.PendingItems > 0)
        {
            problems.Add("QuickHealthStatus.PendingItems must be 0 when IsRunning is false.");
        }

        // PendingItems should be non-negative
        if (value.PendingItems < 0)
        {
            problems.Add("QuickHealthStatus.PendingItems must be a non-negative value.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="QuickHealthStatus"/> instance is valid.
    /// </summary>
    /// <param name="value">The quick health status to check.</param>
    /// <returns><c>true</c> if the instance is valid; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static bool IsValid(this QuickHealthStatus value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="QuickHealthStatus"/> instance is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The quick health status to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is not valid, containing a list of validation problems.</exception>
    public static void EnsureValid(this QuickHealthStatus value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"QuickHealthStatus validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Validates a <see cref="ComponentStatus"/> value.
    /// </summary>
    /// <param name="value">The component status to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if the value is valid.</returns>
    public static IReadOnlyList<string> Validate(this ComponentStatus value)
    {
        // ComponentStatus is an enum with valid values, so no validation needed
        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether a <see cref="ComponentStatus"/> value is valid.
    /// </summary>
    /// <param name="value">The component status to check.</param>
    /// <returns><c>true</c> since all ComponentStatus enum values are valid.</returns>
    public static bool IsValid(this ComponentStatus value) => true;

    /// <summary>
    /// Validates a <see cref="SystemHealth"/> value.
    /// </summary>
    /// <param name="value">The system health to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if the value is valid.</returns>
    public static IReadOnlyList<string> Validate(this SystemHealth value)
    {
        // SystemHealth is an enum with valid values, so no validation needed
        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether a <see cref="SystemHealth"/> value is valid.
    /// </summary>
    /// <param name="value">The system health to check.</param>
    /// <returns><c>true</c> since all SystemHealth enum values are valid.</returns>
    public static bool IsValid(this SystemHealth value) => true;
}