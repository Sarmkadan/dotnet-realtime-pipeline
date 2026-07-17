// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotNetRealtimePipeline.Middleware;

using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// Provides extension methods for the <see cref="LoggingMiddleware"/> class.
/// </summary>
public static class LoggingMiddlewareExtensions
{
    /// <summary>
    /// Logs a collection of data points as ingested.
    /// </summary>
    /// <param name="middleware">The logging middleware instance.</param>
    /// <param name="dataPoints">The collection of data points to log.</param>
    /// <param name="stage">The current processing stage.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="middleware"/> or <paramref name="dataPoints"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="stage"/> is null or empty.</exception>
    public static void LogDataIngestionBatch(this LoggingMiddleware middleware, IEnumerable<DataPoint> dataPoints, string stage)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        ArgumentNullException.ThrowIfNull(dataPoints);
        ArgumentException.ThrowIfNullOrEmpty(stage);

        foreach (var dataPoint in dataPoints)
        {
            middleware.LogDataIngestion(dataPoint, stage);
        }
    }

    /// <summary>
    /// Logs the completion of a processing operation with timing derived from a <see cref="Stopwatch"/>.
    /// </summary>
    /// <param name="middleware">The logging middleware instance.</param>
    /// <param name="result">The processing result.</param>
    /// <param name="stopwatch">The stopwatch tracking the operation duration.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="middleware"/>, <paramref name="result"/>, or <paramref name="stopwatch"/> is null.</exception>
    public static void LogProcessingCompletion(this LoggingMiddleware middleware, ProcessingResult result, Stopwatch stopwatch)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(stopwatch);

        middleware.LogProcessingCompletion(result, stopwatch.ElapsedMilliseconds);
    }

    /// <summary>
    /// Logs a performance warning if the elapsed time of a <see cref="Stopwatch"/> exceeds the threshold.
    /// </summary>
    /// <param name="middleware">The logging middleware instance.</param>
    /// <param name="operationName">The name of the operation.</param>
    /// <param name="stopwatch">The stopwatch tracking the operation duration.</param>
    /// <param name="thresholdMs">The performance threshold in milliseconds.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="middleware"/> or <paramref name="stopwatch"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="operationName"/> is null or empty, or when <paramref name="thresholdMs"/> is negative.</exception>
    public static void LogPerformanceWarning(this LoggingMiddleware middleware, string operationName, Stopwatch stopwatch, long thresholdMs)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        ArgumentNullException.ThrowIfNull(stopwatch);
        ArgumentException.ThrowIfNullOrEmpty(operationName);
        if (thresholdMs < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(thresholdMs), "Threshold must be non-negative.");
        }

        middleware.LogPerformanceWarning(operationName, stopwatch.ElapsedMilliseconds, thresholdMs);
    }
}