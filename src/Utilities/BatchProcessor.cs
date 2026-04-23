// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Generic batch processor for processing large collections in chunks.
/// Handles batching, parallel processing, and result aggregation.
/// </summary>
public class BatchProcessor<TInput, TOutput>
{
    private readonly int _batchSize;
    private readonly int _maxDegreeOfParallelism;

    public BatchProcessor(int batchSize = 1000, int maxDegreeOfParallelism = 4)
    {
        _batchSize = Math.Max(1, batchSize);
        _maxDegreeOfParallelism = Math.Max(1, maxDegreeOfParallelism);
    }

    /// <summary>
    /// Processes a collection of items in batches.
    /// </summary>
    public async Task<List<TOutput>> ProcessAsync(
        IEnumerable<TInput> items,
        Func<List<TInput>, Task<List<TOutput>>> batchProcessor,
        Action<int> progressCallback = null)
    {
        var results = new List<TOutput>();
        var batches = CreateBatches(items).ToList();

        int processedBatches = 0;
        var lockObj = new object();

        var tasks = batches.Select(async batch =>
        {
            try
            {
                var batchResults = await batchProcessor(batch);

                lock (lockObj)
                {
                    results.AddRange(batchResults);
                    processedBatches++;
                    progressCallback?.Invoke(processedBatches);
                }
            }
            catch (Exception ex)
            {
                throw new BatchProcessingException($"Error processing batch", ex);
            }
        });

        await Task.WhenAll(tasks);

        return results;
    }

    /// <summary>
    /// Divides a collection into batches.
    /// </summary>
    public IEnumerable<List<TInput>> CreateBatches(IEnumerable<TInput> items)
    {
        var batch = new List<TInput>(_batchSize);

        foreach (var item in items)
        {
            batch.Add(item);

            if (batch.Count >= _batchSize)
            {
                yield return batch;
                batch = new List<TInput>(_batchSize);
            }
        }

        if (batch.Count > 0)
        {
            yield return batch;
        }
    }

    /// <summary>
    /// Gets the number of batches needed for a given collection size.
    /// </summary>
    public int GetBatchCount(int itemCount)
    {
        return (itemCount + _batchSize - 1) / _batchSize;
    }
}

/// <summary>
/// Exception thrown during batch processing.
/// </summary>
public class BatchProcessingException : Exception
{
    public BatchProcessingException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Progress tracker for batch operations.
/// </summary>
public class BatchProcessingProgress
{
    public int TotalBatches { get; set; }
    public int ProcessedBatches { get; set; }
    public int TotalItems { get; set; }
    public int ProcessedItems { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime LastUpdateTime { get; set; }

    public double ProgressPercent => TotalBatches > 0 ? (ProcessedBatches * 100.0) / TotalBatches : 0;
    public TimeSpan ElapsedTime => DateTime.UtcNow - StartTime;
    public TimeSpan EstimatedTimeRemaining => ElapsedTime.TotalMilliseconds > 0 && ProgressPercent > 0
        ? TimeSpan.FromMilliseconds((ElapsedTime.TotalMilliseconds / ProgressPercent) * (100 - ProgressPercent))
        : TimeSpan.Zero;
    public double ItemsPerSecond => ElapsedTime.TotalSeconds > 0
        ? ProcessedItems / ElapsedTime.TotalSeconds
        : 0;

    public override string ToString()
    {
        return $"Progress: {ProgressPercent:F1}% ({ProcessedBatches}/{TotalBatches} batches), " +
               $"Items: {ProcessedItems}/{TotalItems}, " +
               $"Speed: {ItemsPerSecond:F0} items/sec, " +
               $"ETA: {EstimatedTimeRemaining:hh\\:mm\\:ss}";
    }
}

/// <summary>
/// Pipeline-specific batch processor for DataPoints.
/// </summary>
public class DataPointBatchProcessor
{
    private readonly BatchProcessor<Domain.Models.DataPoint, Domain.Models.ProcessingResult> _processor;

    public DataPointBatchProcessor(int batchSize = 1000, int parallelism = 4)
    {
        _processor = new BatchProcessor<Domain.Models.DataPoint, Domain.Models.ProcessingResult>(batchSize, parallelism);
    }

    /// <summary>
    /// Processes data points in batches.
    /// </summary>
    public async Task<List<Domain.Models.ProcessingResult>> ProcessBatchAsync(
        List<Domain.Models.DataPoint> dataPoints,
        Func<List<Domain.Models.DataPoint>, Task<List<Domain.Models.ProcessingResult>>> processingFunction,
        IProgress<BatchProcessingProgress> progress = null)
    {
        var totalBatches = _processor.GetBatchCount(dataPoints.Count);
        var progressTracker = new BatchProcessingProgress
        {
            TotalBatches = totalBatches,
            TotalItems = dataPoints.Count,
            StartTime = DateTime.UtcNow,
            LastUpdateTime = DateTime.UtcNow
        };

        var results = await _processor.ProcessAsync(
            dataPoints,
            processingFunction,
            batchIndex =>
            {
                progressTracker.ProcessedBatches = batchIndex;
                progressTracker.ProcessedItems = batchIndex * _processor.GetBatchCount(dataPoints.Count);
                progressTracker.LastUpdateTime = DateTime.UtcNow;
                progress?.Report(progressTracker);
            });

        return results;
    }

    /// <summary>
    /// Creates batches of data points.
    /// </summary>
    public IEnumerable<List<Domain.Models.DataPoint>> CreateBatches(List<Domain.Models.DataPoint> dataPoints)
    {
        return _processor.CreateBatches(dataPoints);
    }
}
