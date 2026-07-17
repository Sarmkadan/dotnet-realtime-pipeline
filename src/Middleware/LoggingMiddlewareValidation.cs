#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Middleware;

using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;

/// <summary>
/// Provides validation helpers for <see cref="LoggingMiddleware"/> and related middleware classes.
/// </summary>
public static class LoggingMiddlewareValidation
{
    /// <summary>
    /// Determines whether the specified <see cref="DataPoint"/> and stage are valid for logging.
    /// </summary>
    /// <param name="dataPoint">The data point to check.</param>
    /// <param name="stage">The processing stage name.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(
        this DataPoint dataPoint,
        string stage)
    {
        return dataPoint.Validate(stage).Count == 0;
    }

    /// <summary>
    /// Validates parameters for <see cref="LoggingMiddleware.LogDataIngestion"/>.
    /// </summary>
    /// <param name="dataPoint">The data point to validate.</param>
    /// <param name="stage">The processing stage name.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if dataPoint is null.</exception>
    /// <exception cref="ArgumentException">Thrown if stage is null or whitespace.</exception>
    public static IReadOnlyList<string> Validate(
        this DataPoint dataPoint,
        string stage)
    {
        ArgumentNullException.ThrowIfNull(dataPoint);
        ArgumentException.ThrowIfNullOrWhiteSpace(stage);

        var problems = new List<string>();

        if (dataPoint.Id <= 0)
        {
            problems.Add($"DataPoint.Id must be positive, but was {dataPoint.Id}.");
        }

        if (dataPoint.Timestamp <= 0)
        {
            problems.Add($"DataPoint.Timestamp must be positive, but was {dataPoint.Timestamp}.");
        }

        if (string.IsNullOrWhiteSpace(dataPoint.Source))
        {
            problems.Add("DataPoint.Source cannot be null or whitespace.");
        }

        if (dataPoint.Quality < 0 || dataPoint.Quality > 100)
        {
            problems.Add($"DataPoint.Quality must be between 0 and 100, but was {dataPoint.Quality}.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Ensures that the specified <see cref="DataPoint"/> and stage are valid for logging, throwing an exception if not.
    /// </summary>
    /// <param name="dataPoint">The data point to validate.</param>
    /// <param name="stage">The processing stage name.</param>
    /// <exception cref="ArgumentNullException">Thrown if dataPoint is null.</exception>
    /// <exception cref="ArgumentException">Thrown if stage is null or whitespace, or if dataPoint is invalid.</exception>
    public static void EnsureValid(
        this DataPoint dataPoint,
        string stage)
    {
        ArgumentNullException.ThrowIfNull(dataPoint);
        ArgumentException.ThrowIfNullOrWhiteSpace(stage);

        var problems = dataPoint.Validate(stage);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"DataPoint is invalid. Problems: {string.Join(" ", problems)}");
        }
    }

    /// <summary>
    /// Determines whether the specified <see cref="ProcessingResult"/> and elapsed time are valid for logging.
    /// </summary>
    /// <param name="result">The processing result to check.</param>
    /// <param name="elapsedMs">The elapsed time in milliseconds.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(
        this ProcessingResult result,
        long elapsedMs)
    {
        return result.Validate(elapsedMs).Count == 0;
    }

    /// <summary>
    /// Validates parameters for <see cref="LoggingMiddleware.LogProcessingCompletion"/>.
    /// </summary>
    /// <param name="result">The processing result to validate.</param>
    /// <param name="elapsedMs">The elapsed time in milliseconds.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if result is null.</exception>
    public static IReadOnlyList<string> Validate(
        this ProcessingResult result,
        long elapsedMs)
    {
        ArgumentNullException.ThrowIfNull(result);

        var problems = new List<string>();

        if (result.ResultId <= 0)
        {
            problems.Add($"ProcessingResult.ResultId must be positive, but was {result.ResultId}.");
        }

        if (string.IsNullOrWhiteSpace(result.StageName))
        {
            problems.Add("ProcessingResult.StageName cannot be null or whitespace.");
        }

        if (elapsedMs < 0)
        {
            problems.Add($"Elapsed time must be non-negative, but was {elapsedMs}ms.");
        }

        if (!result.Success && string.IsNullOrWhiteSpace(result.ErrorMessage))
        {
            problems.Add("ProcessingResult.ErrorMessage must be provided when processing failed.");
        }

        if (result.ProcessedAt == default)
        {
            problems.Add("ProcessingResult.ProcessedAt must be set to a valid date.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Ensures that the specified <see cref="ProcessingResult"/> and elapsed time are valid for logging, throwing an exception if not.
    /// </summary>
    /// <param name="result">The processing result to validate.</param>
    /// <param name="elapsedMs">The elapsed time in milliseconds.</param>
    /// <exception cref="ArgumentNullException">Thrown if result is null.</exception>
    /// <exception cref="ArgumentException">Thrown if result is invalid.</exception>
    public static void EnsureValid(
        this ProcessingResult result,
        long elapsedMs)
    {
        ArgumentNullException.ThrowIfNull(result);

        var problems = result.Validate(elapsedMs);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ProcessingResult is invalid. Problems: {string.Join(" ", problems)}");
        }
    }

    /// <summary>
    /// Determines whether the specified stage name and context are valid for logging.
    /// </summary>
    /// <param name="stageName">The stage name.</param>
    /// <param name="context">The backpressure context.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(
        string stageName,
        BackpressureContext context)
    {
        return Validate(stageName, context).Count == 0;
    }

    /// <summary>
    /// Validates parameters for <see cref="LoggingMiddleware.LogBackpressureEvent"/>.
    /// </summary>
    /// <param name="stageName">The stage name.</param>
    /// <param name="context">The backpressure context.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentException">Thrown if stageName is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown if context is null.</exception>
    public static IReadOnlyList<string> Validate(
        string stageName,
        BackpressureContext context)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stageName);
        ArgumentNullException.ThrowIfNull(context);

        var problems = new List<string>();

        if (context.ContextId <= 0)
        {
            problems.Add($"BackpressureContext.ContextId must be positive, but was {context.ContextId}.");
        }

        if (string.IsNullOrWhiteSpace(context.PipelineStageName))
        {
            problems.Add("BackpressureContext.PipelineStageName cannot be null or whitespace.");
        }

        if (context.MaxBufferCapacity <= 0)
        {
            problems.Add($"BackpressureContext.MaxBufferCapacity must be positive, but was {context.MaxBufferCapacity}.");
        }

        if (context.BufferSize < 0)
        {
            problems.Add($"BackpressureContext.BufferSize must be non-negative, but was {context.BufferSize}.");
        }

        if (context.BufferSize > context.MaxBufferCapacity)
        {
            problems.Add(
                $"BackpressureContext.BufferSize ({context.BufferSize}) cannot exceed MaxBufferCapacity ({context.MaxBufferCapacity}).");
        }

        if (context.CreatedAt == default)
        {
            problems.Add("BackpressureContext.CreatedAt must be set to a valid date.");
        }

        if (context.LastUpdatedAt == default)
        {
            problems.Add("BackpressureContext.LastUpdatedAt must be set to a valid date.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Ensures that the specified stage name and context are valid for logging, throwing an exception if not.
    /// </summary>
    /// <param name="stageName">The stage name.</param>
    /// <param name="context">The backpressure context.</param>
    /// <exception cref="ArgumentException">Thrown if stageName is null or whitespace, or if context is invalid.</exception>
    /// <exception cref="ArgumentNullException">Thrown if context is null.</exception>
    public static void EnsureValid(
        string stageName,
        BackpressureContext context)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stageName);
        ArgumentNullException.ThrowIfNull(context);

        var problems = Validate(stageName, context);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"BackpressureContext is invalid. Problems: {string.Join(" ", problems)}");
        }
    }

    /// <summary>
    /// Determines whether the specified metric parameters are valid for logging.
    /// </summary>
    /// <param name="metricName">The metric name.</param>
    /// <param name="value">The metric value.</param>
    /// <param name="unit">The metric unit.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(
        string metricName,
        long value,
        string unit)
    {
        return Validate(metricName, value, unit).Count == 0;
    }

    /// <summary>
    /// Validates parameters for <see cref="LoggingMiddleware.LogMetricsCollection"/>.
    /// </summary>
    /// <param name="metricName">The metric name.</param>
    /// <param name="value">The metric value.</param>
    /// <param name="unit">The metric unit.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentException">Thrown if any parameter is null or whitespace.</exception>
    public static IReadOnlyList<string> Validate(
        string metricName,
        long value,
        string unit)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(metricName);
        ArgumentException.ThrowIfNullOrWhiteSpace(unit);

        var problems = new List<string>();

        if (value < 0)
        {
            problems.Add($"Metric value must be non-negative, but was {value}.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Ensures that the specified metric parameters are valid for logging, throwing an exception if not.
    /// </summary>
    /// <param name="metricName">The metric name.</param>
    /// <param name="value">The metric value.</param>
    /// <param name="unit">The metric unit.</param>
    /// <exception cref="ArgumentException">Thrown if any parameter is null or whitespace, or if value is negative.</exception>
    public static void EnsureValid(
        string metricName,
        long value,
        string unit)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(metricName);
        ArgumentException.ThrowIfNullOrWhiteSpace(unit);

        var problems = Validate(metricName, value, unit);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Metric parameters are invalid. Problems: {string.Join(" ", problems)}");
        }
    }

    /// <summary>
    /// Determines whether the specified error logging parameters are valid.
    /// </summary>
    /// <param name="operationName">The operation name.</param>
    /// <param name="ex">The exception.</param>
    /// <param name="context">The context description.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(
        string operationName,
        Exception ex,
        string context)
    {
        return Validate(operationName, ex, context).Count == 0;
    }

    /// <summary>
    /// Validates parameters for <see cref="LoggingMiddleware.LogError"/>.
    /// </summary>
    /// <param name="operationName">The operation name.</param>
    /// <param name="ex">The exception.</param>
    /// <param name="context">The context description.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentException">Thrown if operationName or context is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown if ex is null.</exception>
    public static IReadOnlyList<string> Validate(
        string operationName,
        Exception ex,
        string context)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);
        ArgumentNullException.ThrowIfNull(ex);
        ArgumentException.ThrowIfNullOrWhiteSpace(context);

        var problems = new List<string>();

        if (ex is null)
        {
            problems.Add("Exception cannot be null.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Ensures that the specified error logging parameters are valid, throwing an exception if not.
    /// </summary>
    /// <param name="operationName">The operation name.</param>
    /// <param name="ex">The exception.</param>
    /// <param name="context">The context description.</param>
    /// <exception cref="ArgumentException">Thrown if operationName or context is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown if ex is null.</exception>
    public static void EnsureValid(
        string operationName,
        Exception ex,
        string context)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);
        ArgumentNullException.ThrowIfNull(ex);
        ArgumentException.ThrowIfNullOrWhiteSpace(context);

        var problems = Validate(operationName, ex, context);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Error logging parameters are invalid. Problems: {string.Join(" ", problems)}");
        }
    }

    /// <summary>
    /// Determines whether the specified performance warning parameters are valid.
    /// </summary>
    /// <param name="operationName">The operation name.</param>
    /// <param name="elapsedMs">The elapsed time in milliseconds.</param>
    /// <param name="thresholdMs">The performance threshold in milliseconds.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(
        string operationName,
        long elapsedMs,
        long thresholdMs)
    {
        return Validate(operationName, elapsedMs, thresholdMs).Count == 0;
    }

    /// <summary>
    /// Validates parameters for <see cref="LoggingMiddleware.LogPerformanceWarning"/>.
    /// </summary>
    /// <param name="operationName">The operation name.</param>
    /// <param name="elapsedMs">The elapsed time in milliseconds.</param>
    /// <param name="thresholdMs">The performance threshold in milliseconds.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentException">Thrown if operationName is null or whitespace.</exception>
    public static IReadOnlyList<string> Validate(
        string operationName,
        long elapsedMs,
        long thresholdMs)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);

        var problems = new List<string>();

        if (elapsedMs < 0)
        {
            problems.Add($"Elapsed time must be non-negative, but was {elapsedMs}ms.");
        }

        if (thresholdMs < 0)
        {
            problems.Add($"Threshold must be non-negative, but was {thresholdMs}ms.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Ensures that the specified performance warning parameters are valid, throwing an exception if not.
    /// </summary>
    /// <param name="operationName">The operation name.</param>
    /// <param name="elapsedMs">The elapsed time in milliseconds.</param>
    /// <param name="thresholdMs">The performance threshold in milliseconds.</param>
    /// <exception cref="ArgumentException">Thrown if operationName is null or whitespace, or if elapsedMs or thresholdMs is negative.</exception>
    public static void EnsureValid(
        string operationName,
        long elapsedMs,
        long thresholdMs)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);

        var problems = Validate(operationName, elapsedMs, thresholdMs);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Performance warning parameters are invalid. Problems: {string.Join(" ", problems)}");
        }
    }

    /// <summary>
    /// Determines whether the specified correlation ID is valid.
    /// </summary>
    /// <param name="correlationId">The correlation ID to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(string correlationId)
    {
        return Validate(correlationId).Count == 0;
    }

    /// <summary>
    /// Validates a correlation ID.
    /// </summary>
    /// <param name="correlationId">The correlation ID to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentException">Thrown if correlationId is null or whitespace.</exception>
    public static IReadOnlyList<string> Validate(string correlationId)
    {
        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            problems.Add("Correlation ID cannot be null or whitespace.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Ensures that the specified correlation ID is valid, throwing an exception if not.
    /// </summary>
    /// <param name="correlationId">The correlation ID to validate.</param>
    /// <exception cref="ArgumentException">Thrown if correlationId is null or whitespace.</exception>
    public static void EnsureValid(string correlationId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);
    }

    /// <summary>
    /// Determines whether the specified correlation async parameters are valid.
    /// </summary>
    /// <param name="operation">The operation to execute with correlation.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid<T>(
        Func<string, Task<T>> operation)
    {
        return Validate(operation).Count == 0;
    }

    /// <summary>
    /// Validates parameters for <see cref="CorrelationMiddleware.WithCorrelationAsync{T}"/>.
    /// </summary>
    /// <param name="operation">The operation to execute with correlation.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if operation is null.</exception>
    public static IReadOnlyList<string> Validate<T>(
        Func<string, Task<T>> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var problems = new List<string>();

        if (operation is null)
        {
            problems.Add("Operation cannot be null.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Ensures that the specified correlation async parameters are valid, throwing an exception if not.
    /// </summary>
    /// <param name="operation">The operation to execute with correlation.</param>
    /// <exception cref="ArgumentNullException">Thrown if operation is null.</exception>
    public static void EnsureValid<T>(
        Func<string, Task<T>> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var problems = Validate(operation);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"WithCorrelationAsync parameters are invalid. Problems: {string.Join(" ", problems)}");
        }
    }

    /// <summary>
    /// Determines whether the specified measure parameters are valid.
    /// </summary>
    /// <param name="operationName">The operation name.</param>
    /// <param name="operation">The operation to measure.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid<T>(
        string operationName,
        Func<T> operation)
    {
        return Validate(operationName, operation).Count == 0;
    }

    /// <summary>
    /// Validates parameters for <see cref="PerformanceLoggingMiddleware.Measure{T}"/>.
    /// </summary>
    /// <param name="operationName">The operation name.</param>
    /// <param name="operation">The operation to measure.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentException">Thrown if operationName is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown if operation is null.</exception>
    public static IReadOnlyList<string> Validate<T>(
        string operationName,
        Func<T> operation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);
        ArgumentNullException.ThrowIfNull(operation);

        var problems = new List<string>();

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Ensures that the specified measure parameters are valid, throwing an exception if not.
    /// </summary>
    /// <param name="operationName">The operation name.</param>
    /// <param name="operation">The operation to measure.</param>
    /// <exception cref="ArgumentException">Thrown if operationName is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown if operation is null.</exception>
    public static void EnsureValid<T>(
        string operationName,
        Func<T> operation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);
        ArgumentNullException.ThrowIfNull(operation);

        var problems = Validate(operationName, operation);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Measure parameters are invalid. Problems: {string.Join(" ", problems)}");
        }
    }

    /// <summary>
    /// Determines whether the specified measure async parameters are valid.
    /// </summary>
    /// <param name="operationName">The operation name.</param>
    /// <param name="operation">The operation to measure.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid<T>(
        string operationName,
        Func<Task<T>> operation)
    {
        return Validate(operationName, operation).Count == 0;
    }

    /// <summary>
    /// Validates parameters for <see cref="PerformanceLoggingMiddleware.MeasureAsync{T}"/>.
    /// </summary>
    /// <param name="operationName">The operation name.</param>
    /// <param name="operation">The operation to measure.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentException">Thrown if operationName is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown if operation is null.</exception>
    public static IReadOnlyList<string> Validate<T>(
        string operationName,
        Func<Task<T>> operation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);
        ArgumentNullException.ThrowIfNull(operation);

        var problems = new List<string>();

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Ensures that the specified measure async parameters are valid, throwing an exception if not.
    /// </summary>
    /// <param name="operationName">The operation name.</param>
    /// <param name="operation">The operation to measure.</param>
    /// <exception cref="ArgumentException">Thrown if operationName is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown if operation is null.</exception>
    public static void EnsureValid<T>(
        string operationName,
        Func<Task<T>> operation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);
        ArgumentNullException.ThrowIfNull(operation);

        var problems = Validate(operationName, operation);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"MeasureAsync parameters are invalid. Problems: {string.Join(" ", problems)}");
        }
    }
}