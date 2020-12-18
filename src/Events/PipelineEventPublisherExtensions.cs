#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotNetRealtimePipeline.Events;

using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Extension methods for <see cref="PipelineEventPublisher"/> providing convenient
/// publisher utilities and batch operations.
/// </summary>
public static class PipelineEventPublisherExtensions
{
    /// <summary>
    /// Publishes a data ingestion event with additional metadata.
    /// </summary>
    /// <param name="publisher">The event publisher.</param>
    /// <param name="dataPoint">The data point to publish.</param>
    /// <param name="metadata">Additional metadata to include with the event.</param>
    /// <exception cref="ArgumentNullException">Thrown when publisher or dataPoint is null.</exception>
    public static async Task PublishDataIngestedAsync(this PipelineEventPublisher publisher, DataPoint dataPoint, Dictionary<string, object> metadata)
    {
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentNullException.ThrowIfNull(dataPoint);
        ArgumentNullException.ThrowIfNull(metadata);

        var dataPointWithMetadata = new DataPoint
        {
            Id = dataPoint.Id,
            Timestamp = dataPoint.Timestamp,
            Value = dataPoint.Value,
            Source = dataPoint.Source,
            Metadata = new Dictionary<string, object>(dataPoint.Metadata),
            Tags = dataPoint.Tags,
            Quality = dataPoint.Quality,
            CreatedAt = dataPoint.CreatedAt
        };

        foreach (var kvp in metadata)
        {
            dataPointWithMetadata.AddMetadata(kvp.Key, kvp.Value);
        }

        await publisher.PublishDataIngestedAsync(dataPointWithMetadata);
    }

    /// <summary>
    /// Publishes a processing completed event with success status and result details.
    /// </summary>
    /// <param name="publisher">The event publisher.</param>
    /// <param name="resultId">The unique identifier for the result.</param>
    /// <param name="success">Whether processing succeeded.</param>
    /// <param name="stageName">Name of the pipeline stage that processed the data.</param>
    /// <param name="errorMessage">Optional error message if processing failed.</param>
    /// <param name="processingTimeMs">Optional processing time in milliseconds.</param>
    /// <exception cref="ArgumentNullException">Thrown when publisher is null.</exception>
    /// <exception cref="ArgumentException">Thrown when resultId or stageName is null or empty.</exception>
    public static async Task PublishProcessingCompletedAsync(
        this PipelineEventPublisher publisher,
        string resultId,
        bool success,
        string stageName = "Pipeline",
        string? errorMessage = null,
        long processingTimeMs = 0)
    {
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentException.ThrowIfNullOrEmpty(resultId);
        ArgumentException.ThrowIfNullOrEmpty(stageName);

        var result = new ProcessingResult
        {
            ResultId = long.Parse(resultId),
            Success = success,
            StageName = stageName,
            ErrorMessage = errorMessage,
            ProcessingTimeMs = processingTimeMs
        };

        await publisher.PublishProcessingCompletedAsync(result);
    }

    /// <summary>
    /// Publishes a backpressure detected event with calculated utilization percentage.
    /// </summary>
    /// <param name="publisher">The event publisher.</param>
    /// <param name="stageName">Name of the pipeline stage experiencing backpressure.</param>
    /// <param name="bufferSize">Current buffer size.</param>
    /// <param name="maxBufferCapacity">Maximum buffer capacity.</param>
    /// <param name="isBackpressured">Whether the stage is currently backpressured.</param>
    /// <exception cref="ArgumentNullException">Thrown when publisher or stageName is null.</exception>
    /// <exception cref="ArgumentException">Thrown when stageName is null or empty.</exception>
    public static async Task PublishBackpressureDetectedAsync(
        this PipelineEventPublisher publisher,
        string stageName,
        int bufferSize,
        int maxBufferCapacity,
        bool isBackpressured)
    {
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentException.ThrowIfNullOrEmpty(stageName);

        var context = new BackpressureContext
        {
            BufferSize = bufferSize,
            MaxBufferCapacity = maxBufferCapacity,
            IsBackpressured = isBackpressured
        };

        await publisher.PublishBackpressureDetectedAsync(stageName, context);
    }

    /// <summary>
    /// Publishes a metrics collected event with pre-calculated aggregations.
    /// </summary>
    /// <param name="publisher">The event publisher.</param>
    /// <param name="totalItemsProcessed">Total items processed.</param>
    /// <param name="averageProcessingTimeMs">Average processing time per item in milliseconds.</param>
    /// <param name="metricType">Type of metric aggregation (e.g., "hourly", "daily").</param>
    /// <exception cref="ArgumentNullException">Thrown when publisher is null.</exception>
    /// <exception cref="ArgumentException">Thrown when metricType is null or empty.</exception>
    public static async Task PublishMetricsCollectedAsync(
        this PipelineEventPublisher publisher,
        long totalItemsProcessed,
        double averageProcessingTimeMs,
        string metricType = "default")
    {
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentException.ThrowIfNullOrEmpty(metricType);

        var metrics = new MetricAggregation
        {
            MetricId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            TimeWindowStartMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            TimeWindowEndMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            MetricType = metricType,
            TotalItemsProcessed = totalItemsProcessed,
            AverageProcessingTimeMs = averageProcessingTimeMs,
            ComputedAt = DateTime.UtcNow
        };

        await publisher.PublishMetricsCollectedAsync(metrics);
    }

    /// <summary>
    /// Publishes a pipeline error event with formatted error details.
    /// </summary>
    /// <param name="publisher">The event publisher.</param>
    /// <param name="operationName">Name of the operation that failed.</param>
    /// <param name="exception">The exception that occurred.</param>
    /// <param name="context">Optional context information about the error.</param>
    /// <exception cref="ArgumentNullException">Thrown when publisher or exception is null.</exception>
    /// <exception cref="ArgumentException">Thrown when operationName is null or empty.</exception>
    public static async Task PublishPipelineErrorAsync(
        this PipelineEventPublisher publisher,
        string operationName,
        Exception exception,
        string? context = null)
    {
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrEmpty(operationName);

        var message = context is null
            ? exception.Message
            : $"{exception.Message} (Context: {context})\nStack Trace:\n{exception.StackTrace}";

        var formattedException = new InvalidOperationException(message, exception);

        await publisher.PublishPipelineErrorAsync(operationName, formattedException);
    }

    /// <summary>
    /// Gets all subscriber counts for all registered events.
    /// </summary>
    /// <param name="publisher">The event publisher.</param>
    /// <returns>Dictionary mapping event names to subscriber counts.</returns>
    /// <exception cref="ArgumentNullException">Thrown when publisher is null.</exception>
    public static IReadOnlyDictionary<string, int> GetAllSubscriberCounts(this PipelineEventPublisher publisher)
    {
        ArgumentNullException.ThrowIfNull(publisher);

        var result = new Dictionary<string, int>(StringComparer.Ordinal);

        // These are the known event names based on the publisher's methods
        var eventNames = new[]
        {
            nameof(DataIngestedEvent),
            nameof(ProcessingCompletedEvent),
            nameof(BackpressureDetectedEvent),
            nameof(MetricsCollectedEvent),
            nameof(PipelineErrorEvent)
        };

        foreach (var eventName in eventNames)
        {
            result[eventName] = publisher.GetSubscriberCount(eventName);
        }

        return result;
    }

    /// <summary>
    /// Checks if any subscribers are registered for the specified event.
    /// </summary>
    /// <param name="publisher">The event publisher.</param>
    /// <param name="eventName">Name of the event to check.</param>
    /// <returns>True if subscribers exist, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when publisher is null.</exception>
    /// <exception cref="ArgumentException">Thrown when eventName is null or empty.</exception>
    public static bool HasSubscribers(this PipelineEventPublisher publisher, string eventName)
    {
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentException.ThrowIfNullOrEmpty(eventName);

        return publisher.GetSubscriberCount(eventName) > 0;
    }

    /// <summary>
    /// Publishes a batch of data ingestion events for multiple data points.
    /// </summary>
    /// <param name="publisher">The event publisher.</param>
    /// <param name="dataPoints">Collection of data points to publish.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when publisher or dataPoints is null.</exception>
    public static async Task PublishDataIngestedBatchAsync(this PipelineEventPublisher publisher, IEnumerable<DataPoint> dataPoints)
    {
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentNullException.ThrowIfNull(dataPoints);

        foreach (var dataPoint in dataPoints)
        {
            await publisher.PublishDataIngestedAsync(dataPoint);
        }
    }

    /// <summary>
    /// Publishes a batch of processing completed events for multiple results.
    /// </summary>
    /// <param name="publisher">The event publisher.</param>
    /// <param name="results">Collection of processing results to publish.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when publisher or results is null.</exception>
    public static async Task PublishProcessingCompletedBatchAsync(this PipelineEventPublisher publisher, IEnumerable<ProcessingResult> results)
    {
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentNullException.ThrowIfNull(results);

        foreach (var result in results)
        {
            await publisher.PublishProcessingCompletedAsync(result);
        }
    }
}