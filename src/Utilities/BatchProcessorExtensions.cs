#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Utilities;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Extension methods for <see cref="BatchProcessor{TInput, TOutput}"/> and <see cref="DataPointBatchProcessor"/>.
/// Provides common batch processing patterns and utilities.
/// </summary>
public static class BatchProcessorExtensions
{
    /// <summary>
    /// Processes a collection of items in batches and returns the results as an IReadOnlyList.
    /// </summary>
    /// <typeparam name="TInput">The input type.</typeparam>
    /// <typeparam name="TOutput">The output type.</typeparam>
    /// <param name="processor">The batch processor instance.</param>
    /// <param name="items">The items to process.</param>
    /// <param name="batchProcessor">The function to process each batch.</param>
    /// <param name="progressCallback">Optional progress callback.</param>
    /// <returns>A read-only list of processing results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when processor, items, or batchProcessor is null.</exception>
    public static async Task<IReadOnlyList<TOutput>> ProcessAsync<TInput, TOutput>(
        this BatchProcessor<TInput, TOutput> processor,
        IEnumerable<TInput> items,
        Func<List<TInput>, Task<List<TOutput>>> batchProcessor,
        Action<int> progressCallback = null)
    {
        ArgumentNullException.ThrowIfNull(processor);
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(batchProcessor);

        var results = await processor.ProcessAsync(items, batchProcessor, progressCallback);
        return results.AsReadOnly();
    }

    /// <summary>
    /// Creates batches from a collection and converts each batch using a selector function.
    /// </summary>
    /// <typeparam name="TInput">The input type.</typeparam>
    /// <typeparam name="TOutput">The output type.</typeparam>
    /// <typeparam name="TResult">The result type after transformation.</typeparam>
    /// <param name="processor">The batch processor instance.</param>
    /// <param name="items">The items to batch.</param>
    /// <param name="batchSelector">Function to transform each batch.</param>
    /// <returns>An enumerable of transformed batch results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when processor, items, or batchSelector is null.</exception>
    public static IEnumerable<TResult> BatchSelect<TInput, TOutput, TResult>(
        this BatchProcessor<TInput, TOutput> processor,
        IEnumerable<TInput> items,
        Func<List<TInput>, TResult> batchSelector)
    {
        ArgumentNullException.ThrowIfNull(processor);
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(batchSelector);

        return processor.CreateBatches(items).Select(batchSelector);
    }

    /// <summary>
    /// Processes batches in parallel and aggregates results using a custom aggregator.
    /// </summary>
    /// <typeparam name="TInput">The input type.</typeparam>
    /// <typeparam name="TOutput">The output type.</typeparam>
    /// <typeparam name="TAggregate">The aggregate result type.</typeparam>
    /// <param name="processor">The batch processor instance.</param>
    /// <param name="items">The items to process.</param>
    /// <param name="batchProcessor">Function to process each batch.</param>
    /// <param name="seed">The initial aggregate value.</param>
    /// <param name="resultSelector">Function to aggregate results.</param>
    /// <param name="progressCallback">Optional progress callback.</param>
    /// <returns>The aggregated result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public static async Task<TAggregate> ProcessAsync<TInput, TOutput, TAggregate>(
        this BatchProcessor<TInput, TOutput> processor,
        IEnumerable<TInput> items,
        Func<List<TInput>, Task<List<TOutput>>> batchProcessor,
        TAggregate seed,
        Func<TAggregate, TOutput, TAggregate> resultSelector,
        Action<int> progressCallback = null)
    {
        ArgumentNullException.ThrowIfNull(processor);
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(batchProcessor);
        ArgumentNullException.ThrowIfNull(resultSelector);

        var aggregate = seed;
        var results = await processor.ProcessAsync(items, batchProcessor, progressCallback);

        foreach (var result in results)
        {
            aggregate = resultSelector(aggregate, result);
        }

        return aggregate;
    }

    /// <summary>
    /// Gets the estimated processing time based on batch count and configured parallelism.
    /// </summary>
    /// <typeparam name="TInput">The input type.</typeparam>
    /// <typeparam name="TOutput">The output type.</typeparam>
    /// <param name="processor">The batch processor instance.</param>
    /// <param name="itemCount">Total number of items to process.</param>
    /// <param name="estimatedBatchProcessingTimeMs">Estimated time per batch in milliseconds.</param>
    /// <returns>A TimeSpan representing the estimated total processing time.</returns>
    /// <exception cref="ArgumentNullException">Thrown when processor is null and itemCount is negative.</exception>
    public static TimeSpan GetEstimatedProcessingTime<TInput, TOutput>(
        this BatchProcessor<TInput, TOutput> processor,
        int itemCount,
        double estimatedBatchProcessingTimeMs)
    {
        ArgumentNullException.ThrowIfNull(processor);
        if (itemCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(itemCount), "Item count cannot be negative.");
        }

        if (estimatedBatchProcessingTimeMs <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(estimatedBatchProcessingTimeMs),
                "Estimated batch processing time must be positive.");
        }

        var batchCount = processor.GetBatchCount(itemCount);
        var parallelism = processor.GetType().GetProperty("_maxDegreeOfParallelism",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.GetValue(processor) as int? ?? 4;

        var totalBatches = (batchCount + parallelism - 1) / parallelism;
        var totalTimeMs = totalBatches * estimatedBatchProcessingTimeMs;

        return TimeSpan.FromMilliseconds(totalTimeMs);
    }

    /// <summary>
    /// Processes DataPoints in batches with progress tracking and returns results as IReadOnlyList.
    /// </summary>
    /// <param name="processor">The DataPoint batch processor instance.</param>
    /// <param name="dataPoints">The data points to process.</param>
    /// <param name="processingFunction">Function to process each batch of data points.</param>
    /// <param name="progress">Optional progress reporter.</param>
    /// <returns>A read-only list of processing results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when processor, dataPoints, or processingFunction is null.</exception>
    public static async Task<IReadOnlyList<Domain.Models.ProcessingResult>> ProcessBatchAsync(
        this DataPointBatchProcessor processor,
        List<Domain.Models.DataPoint> dataPoints,
        Func<List<Domain.Models.DataPoint>, Task<List<Domain.Models.ProcessingResult>>> processingFunction,
        IProgress<BatchProcessingProgress> progress = null)
    {
        ArgumentNullException.ThrowIfNull(processor);
        ArgumentNullException.ThrowIfNull(dataPoints);
        ArgumentNullException.ThrowIfNull(processingFunction);

        return await processor.ProcessBatchAsync(dataPoints, processingFunction, progress);
    }

    /// <summary>
    /// Creates batches from a collection of DataPoints.
    /// </summary>
    /// <param name="processor">The DataPoint batch processor instance.</param>
    /// <param name="dataPoints">The data points to batch.</param>
    /// <returns>An enumerable of DataPoint batches.</returns>
    /// <exception cref="ArgumentNullException">Thrown when processor or dataPoints is null.</exception>
    public static IEnumerable<List<Domain.Models.DataPoint>> CreateBatches(
        this DataPointBatchProcessor processor,
        List<Domain.Models.DataPoint> dataPoints)
    {
        ArgumentNullException.ThrowIfNull(processor);
        ArgumentNullException.ThrowIfNull(dataPoints);

        return processor.CreateBatches(dataPoints);
    }

    /// <summary>
    /// Gets batch statistics for a collection of items.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <typeparam name="TOutput">The output type (unused but required for generic constraint).</typeparam>
    /// <param name="processor">The batch processor instance.</param>
    /// <param name="items">The items to analyze.</param>
    /// <returns>A BatchStatistics object containing batch information.</returns>
    /// <exception cref="ArgumentNullException">Thrown when processor or items is null.</exception>
    public static BatchStatistics GetBatchStatistics<T, TOutput>(
        this BatchProcessor<T, TOutput> processor,
        IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(processor);
        ArgumentNullException.ThrowIfNull(items);

        var itemCount = items.Count();
        var batchCount = processor.GetBatchCount(itemCount);
        var batchSize = processor.GetType().GetProperty("_batchSize",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.GetValue(processor) as int? ?? 1000;

        return new BatchStatistics
        {
            TotalItems = itemCount,
            TotalBatches = batchCount,
            BatchSize = batchSize,
            LastBatchSize = itemCount % batchSize == 0 ? batchSize : itemCount % batchSize,
            IsPerfectFit = itemCount % batchSize == 0
        };
    }
}

/// <summary>
/// Represents statistics about batch processing.
/// </summary>
public sealed class BatchStatistics
{
    /// <summary>
    /// Gets the total number of items.
    /// </summary>
    public int TotalItems { get; init; }

    /// <summary>
    /// Gets the total number of batches.
    /// </summary>
    public int TotalBatches { get; init; }

    /// <summary>
    /// Gets the configured batch size.
    /// </summary>
    public int BatchSize { get; init; }

    /// <summary>
    /// Gets the size of the last batch (may be smaller than BatchSize).
    /// </summary>
    public int LastBatchSize { get; init; }

    /// <summary>
    /// Gets a value indicating whether the total items perfectly fit into batches.
    /// </summary>
    public bool IsPerfectFit { get; init; }

    /// <summary>
    /// Returns a string representation of the batch statistics.
    /// </summary>
    /// <returns>A formatted string with batch statistics.</returns>
    public override string ToString()
    {
        return string.Create(CultureInfo.InvariantCulture,
            $"Batch Statistics: {TotalItems} items in {TotalBatches} batches (size: {BatchSize}), " +
            $"Last batch: {LastBatchSize} items, {(IsPerfectFit ? "Perfect fit" : "Partial last batch")}");
    }
}