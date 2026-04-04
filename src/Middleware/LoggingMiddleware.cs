// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Middleware;

using DotNetRealtimePipeline.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

/// <summary>
/// Middleware for comprehensive request and operation logging.
/// Logs entry/exit points, timing information, and provides correlation IDs.
/// </summary>
public class LoggingMiddleware
{
    private readonly ILogger<LoggingMiddleware> _logger;
    private static readonly string CorrelationIdHeader = "X-Correlation-ID";

    public LoggingMiddleware(ILogger<LoggingMiddleware> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Logs the start of a data ingestion operation.
    /// </summary>
    public void LogDataIngestion(DataPoint dataPoint, string stage)
    {
        var correlationId = Guid.NewGuid().ToString("N").Substring(0, 8);
        _logger.LogInformation(
            "[{CorrelationId}] Data ingestion started - ID: {DataId}, Source: {Source}, Quality: {Quality}, Stage: {Stage}",
            correlationId, dataPoint.Id, dataPoint.Source, dataPoint.Quality, stage);
    }

    /// <summary>
    /// Logs the completion of a processing operation with timing.
    /// </summary>
    public void LogProcessingCompletion(ProcessingResult result, long elapsedMs)
    {
        var status = result.Success ? "SUCCESS" : "FAILED";
        _logger.LogInformation(
            "Processing {Status} - ResultId: {Id}, Duration: {ElapsedMs}ms, Message: {Message}",
            status, result.ResultId, elapsedMs, result.ErrorMessage);
    }

    /// <summary>
    /// Logs backpressure events and flow control decisions.
    /// </summary>
    public void LogBackpressureEvent(string stageName, BackpressureContext context)
    {
        var utilizationPercent = context.BufferSize * 100.0 / context.MaxBufferCapacity;
        _logger.LogWarning(
            "Backpressure triggered - Stage: {Stage}, Utilization: {Util:F1}%, IsBackpressured: {Backpressured}",
            stageName, utilizationPercent, context.IsBackpressured);
    }

    /// <summary>
    /// Logs metrics collection and aggregation events.
    /// </summary>
    public void LogMetricsCollection(string metricName, long value, string unit)
    {
        _logger.LogDebug("Metrics collected - Metric: {Name}, Value: {Value}{Unit}",
            metricName, value, unit);
    }

    /// <summary>
    /// Logs error conditions with context.
    /// </summary>
    public void LogError(string operationName, Exception ex, string context)
    {
        _logger.LogError(ex,
            "Operation failed - Operation: {Operation}, Context: {Context}, Message: {Message}",
            operationName, context, ex.Message);
    }

    /// <summary>
    /// Logs performance warnings for slow operations.
    /// </summary>
    public void LogPerformanceWarning(string operationName, long elapsedMs, long thresholdMs)
    {
        if (elapsedMs > thresholdMs)
        {
            _logger.LogWarning(
                "Slow operation detected - Operation: {Operation}, Duration: {Elapsed}ms (threshold: {Threshold}ms)",
                operationName, elapsedMs, thresholdMs);
        }
    }
}

/// <summary>
/// Middleware for structured operation timing and performance tracking.
/// </summary>
public class PerformanceLoggingMiddleware
{
    private readonly ILogger<PerformanceLoggingMiddleware> _logger;

    public PerformanceLoggingMiddleware(ILogger<PerformanceLoggingMiddleware> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Wraps an async operation with timing and logging.
    /// </summary>
    public async Task<T> MeasureAsync<T>(string operationName, Func<Task<T>> operation)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _logger.LogDebug("Operation started: {Operation}", operationName);
            var result = await operation();
            stopwatch.Stop();
            _logger.LogInformation("Operation completed: {Operation} ({ElapsedMs}ms)",
                operationName, stopwatch.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Operation failed: {Operation} ({ElapsedMs}ms) - {Message}",
                operationName, stopwatch.ElapsedMilliseconds, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Wraps a synchronous operation with timing and logging.
    /// </summary>
    public T Measure<T>(string operationName, Func<T> operation)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _logger.LogDebug("Operation started: {Operation}", operationName);
            var result = operation();
            stopwatch.Stop();
            _logger.LogInformation("Operation completed: {Operation} ({ElapsedMs}ms)",
                operationName, stopwatch.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Operation failed: {Operation} ({ElapsedMs}ms) - {Message}",
                operationName, stopwatch.ElapsedMilliseconds, ex.Message);
            throw;
        }
    }
}

/// <summary>
/// Middleware for distributed tracing and correlation.
/// </summary>
public class CorrelationMiddleware
{
    private static readonly AsyncLocal<string> CorrelationContext = new();

    /// <summary>
    /// Gets the current correlation ID.
    /// </summary>
    public static string GetCorrelationId()
    {
        return CorrelationContext.Value ?? Guid.NewGuid().ToString("N").Substring(0, 8);
    }

    /// <summary>
    /// Sets a correlation ID for the current operation.
    /// </summary>
    public static void SetCorrelationId(string correlationId)
    {
        CorrelationContext.Value = correlationId ?? throw new ArgumentNullException(nameof(correlationId));
    }

    /// <summary>
    /// Clears the current correlation ID.
    /// </summary>
    public static void ClearCorrelationId()
    {
        CorrelationContext.Value = null;
    }

    /// <summary>
    /// Executes an operation within a new correlation context.
    /// </summary>
    public async Task<T> WithCorrelationAsync<T>(Func<string, Task<T>> operation)
    {
        var correlationId = Guid.NewGuid().ToString("N").Substring(0, 16);
        var previous = CorrelationContext.Value;

        try
        {
            CorrelationContext.Value = correlationId;
            return await operation(correlationId);
        }
        finally
        {
            CorrelationContext.Value = previous;
        }
    }
}
