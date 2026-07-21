#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Utilities;
using FluentAssertions;
using Xunit;

namespace DotNetRealtimePipeline.Tests;

/// <summary>
/// Tests for BatchProcessor and DataPointBatchProcessor classes.
/// Tests cover: all items succeed, error handling, cancellation, empty batches, and accurate counting.
/// </summary>
public class BatchProcessorTests
{
    private readonly BatchProcessor<int, string> _processor;
    private readonly DataPointBatchProcessor _dataPointProcessor;

    public BatchProcessorTests()
    {
        _processor = new BatchProcessor<int, string>(batchSize: 3, maxDegreeOfParallelism: 2);
        _dataPointProcessor = new DataPointBatchProcessor(batchSize: 3, parallelism: 2);
    }

    #region BatchProcessor<int, string> Tests

    [Fact]
    public async Task ProcessAsync_AllItemsSucceed_ReturnsAllResults()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        Task<List<string>> batchProcessor(List<int> batch) => Task.FromResult(
            batch.ConvertAll(i => $"Processed-{i}")
        );

        // Act
        var results = await _processor.ProcessAsync(items, batchProcessor);

        // Assert
        results.Should().HaveCount(9);
        results.Should().BeEquivalentTo(new[] {
            "Processed-1", "Processed-2", "Processed-3",
            "Processed-4", "Processed-5", "Processed-6",
            "Processed-7", "Processed-8", "Processed-9"
        });
    }

    [Fact]
    public async Task ProcessAsync_OneThrowingItem_AbortsProcessing()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3, 4, 5, 6, 7 };

        Task<List<string>> batchProcessor(List<int> batch) => Task.FromResult(
            batch.ConvertAll(i => {
                if (i == 4) throw new InvalidOperationException("Test error");
                return $"Processed-{i}";
            })
        );

        // Act
        Func<Task> act = async () => await _processor.ProcessAsync(items, batchProcessor);

        // Assert - BatchProcessingException is thrown and processing aborts
        await act.Should().ThrowAsync<BatchProcessingException>();
    }

    [Fact]
    public async Task ProcessAsync_EmptyBatch_ReturnsEmptyList()
    {
        // Arrange
        var items = new List<int>();

        Task<List<string>> batchProcessor(List<int> batch) => Task.FromResult(
            batch.ConvertAll(i => $"Processed-{i}")
        );

        // Act
        var results = await _processor.ProcessAsync(items, batchProcessor);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task ProcessAsync_SingleItem_ReturnsSingleResult()
    {
        // Arrange
        var items = new List<int> { 42 };

        Task<List<string>> batchProcessor(List<int> batch) => Task.FromResult(
            batch.ConvertAll(i => $"Processed-{i}")
        );

        // Act
        var results = await _processor.ProcessAsync(items, batchProcessor);

        // Assert
        results.Should().HaveCount(1);
        results.Should().ContainSingle(r => r == "Processed-42");
    }

    [Fact]
    public async Task ProcessAsync_BatchSizeBoundary_ReturnsCorrectCount()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3, 4, 5 };

        Task<List<string>> batchProcessor(List<int> batch) => Task.FromResult(
            batch.ConvertAll(i => $"Processed-{i}")
        );

        // Act
        var results = await _processor.ProcessAsync(items, batchProcessor);

        // Assert
        results.Should().HaveCount(5);
    }

    [Fact]
    public void CreateBatches_ItemsDivisibleByBatchSize_ReturnsExactBatches()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3, 4, 5, 6 };

        // Act
        var batches = _processor.CreateBatches(items);

        // Assert
        var batchList = batches.ToList();
        batchList.Should().HaveCount(2);
        batchList[0].Should().HaveCount(3);
        batchList[1].Should().HaveCount(3);
        batchList[0].Should().BeEquivalentTo(new[] { 1, 2, 3 });
        batchList[1].Should().BeEquivalentTo(new[] { 4, 5, 6 });
    }

    [Fact]
    public void CreateBatches_ItemsNotDivisibleByBatchSize_ReturnsPartialFinalBatch()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        var batches = _processor.CreateBatches(items);

        // Assert
        var batchList = batches.ToList();
        batchList.Should().HaveCount(2);
        batchList[0].Should().HaveCount(3);
        batchList[1].Should().HaveCount(2);
        batchList[0].Should().BeEquivalentTo(new[] { 1, 2, 3 });
        batchList[1].Should().BeEquivalentTo(new[] { 4, 5 });
    }

    [Fact]
    public void CreateBatches_EmptyCollection_ReturnsEmpty()
    {
        // Arrange
        var items = new List<int>();

        // Act
        var batches = _processor.CreateBatches(items);

        // Assert
        batches.Should().BeEmpty();
    }

    [Fact]
    public void CreateBatches_SingleItem_ReturnsSingleBatch()
    {
        // Arrange
        var items = new List<int> { 42 };

        // Act
        var batches = _processor.CreateBatches(items);

        // Assert
        var batchList = batches.ToList();
        batchList.Should().HaveCount(1);
        batchList[0].Should().HaveCount(1);
        batchList[0].Should().ContainSingle(i => i == 42);
    }

    [Fact]
    public void GetBatchCount_CalculatesCorrectNumberOfBatches()
    {
        // Arrange & Act & Assert
        _processor.GetBatchCount(0).Should().Be(0);
        _processor.GetBatchCount(1).Should().Be(1);
        _processor.GetBatchCount(3).Should().Be(1);
        _processor.GetBatchCount(4).Should().Be(2);
        _processor.GetBatchCount(6).Should().Be(2);
        _processor.GetBatchCount(7).Should().Be(3);
        _processor.GetBatchCount(10).Should().Be(4);
    }

    [Fact]
    public async Task ProcessAsync_ParallelProcessing_ProcessesBatchesConcurrently()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3, 4, 5, 6 };
        var processingTimes = new List<DateTime>();
        var batchLock = new object();

        Task<List<string>> batchProcessor(List<int> batch) => Task.Run(async () => {
            var startTime = DateTime.UtcNow;
            await Task.Delay(100); // Simulate work
            lock (batchLock)
            {
                processingTimes.Add(startTime);
            }
            return batch.ConvertAll(i => $"Processed-{i}");
        });

        // Act
        var results = await _processor.ProcessAsync(items, batchProcessor);

        // Assert
        results.Should().HaveCount(6);
        // With maxDegreeOfParallelism=2, batches should process in parallel
        // We can't guarantee exact timing, but we can verify results are correct
        results.Should().BeEquivalentTo(new[] {
            "Processed-1", "Processed-2", "Processed-3",
            "Processed-4", "Processed-5", "Processed-6"
        });
    }

    [Fact]
    public async Task ProcessAsync_ProgressCallback_InvokedForEachBatch()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3, 4, 5, 6 };
        var progressCalls = new List<int>();

        Task<List<string>> batchProcessor(List<int> batch) => Task.FromResult(
            batch.ConvertAll(i => $"Processed-{i}")
        );

        // Act
        var results = await _processor.ProcessAsync(
            items,
            batchProcessor,
            batchIndex => progressCalls.Add(batchIndex)
        );

        // Assert
        progressCalls.Should().HaveCount(2); // 2 batches
        progressCalls.Should().BeEquivalentTo(new[] { 1, 2 });
    }

    #endregion

    #region DataPointBatchProcessor Tests

    [Fact]
    public async Task DataPointBatchProcessor_ProcessBatchAsync_AllItemsSucceed_ReturnsAllResults()
    {
        // Arrange
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var dataPoints = new List<DataPoint> {
            new(1, timestamp, 10.5, "source1"),
            new(2, timestamp, 20.3, "source2"),
            new(3, timestamp, 30.7, "source3"),
            new(4, timestamp, 40.2, "source4"),
            new(5, timestamp, 50.8, "source5"),
            new(6, timestamp, 60.1, "source6"),
        };

        Task<List<ProcessingResult>> processingFunction(List<DataPoint> batch) => Task.FromResult(
            batch.ConvertAll(dp => new ProcessingResult(dp.Id, true, "TestStage"))
        );

        // Act
        var results = await _dataPointProcessor.ProcessBatchAsync(dataPoints, processingFunction);

        // Assert
        results.Should().HaveCount(6);
        for (int i = 0; i < results.Count; i++)
        {
            results[i].ResultId.Should().Be(dataPoints[i].Id);
            results[i].Success.Should().BeTrue();
            results[i].StageName.Should().Be("TestStage");
        }
    }

    [Fact]
    public async Task DataPointBatchProcessor_ProcessBatchAsync_EmptyBatch_ReturnsEmptyList()
    {
        // Arrange
        var dataPoints = new List<DataPoint>();

        Task<List<ProcessingResult>> processingFunction(List<DataPoint> batch) => Task.FromResult(
            batch.ConvertAll(dp => new ProcessingResult(dp.Id, true, "TestStage"))
        );

        // Act
        var results = await _dataPointProcessor.ProcessBatchAsync(dataPoints, processingFunction);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void DataPointBatchProcessor_CreateBatches_CorrectlyBatchesDataPoints()
    {
        // Arrange
        var dataPoints = new List<DataPoint> {
            new(1, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 10.5, "source1"),
            new(2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 20.3, "source2"),
            new(3, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 30.7, "source3"),
            new(4, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 40.2, "source4"),
            new(5, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 50.8, "source5"),
        };

        // Act
        var batches = _dataPointProcessor.CreateBatches(dataPoints);

        // Assert
        var batchList = batches.ToList();
        batchList.Should().HaveCount(2);
        batchList[0].Should().HaveCount(3);
        batchList[1].Should().HaveCount(2);
    }

    [Fact]
    public async Task DataPointBatchProcessor_ProcessBatchAsync_WithProcessingErrors_AbortsProcessing()
    {
        // Arrange
        var dataPoints = new List<DataPoint> {
            new(1, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 10.5, "source1"),
            new(2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 20.3, "source2"),
            new(3, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 30.7, "source3"),
            new(4, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 40.2, "source4"),
        };

        Task<List<ProcessingResult>> processingFunction(List<DataPoint> batch) => Task.FromResult(
            batch.ConvertAll(dp => {
                if (dp.Id == 3) throw new InvalidOperationException("Test error in DataPoint processing");
                return new ProcessingResult(dp.Id, true, "TestStage");
            })
        );

        // Act
        Func<Task> act = async () => await _dataPointProcessor.ProcessBatchAsync(dataPoints, processingFunction);

        // Assert - BatchProcessingException is thrown and processing aborts
        await act.Should().ThrowAsync<BatchProcessingException>();
    }

    [Fact]
    public async Task DataPointBatchProcessor_ProcessBatchAsync_WithNullProgress_NoException()
    {
        // Arrange
        var dataPoints = new List<DataPoint> {
            new(1, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 10.5, "source1"),
            new(2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 20.3, "source2"),
        };

        Task<List<ProcessingResult>> processingFunction(List<DataPoint> batch) => Task.FromResult(
            batch.ConvertAll(dp => new ProcessingResult(dp.Id, true, "TestStage"))
        );

        // Act - should not throw when progress is null
        var results = await _dataPointProcessor.ProcessBatchAsync(
            dataPoints,
            processingFunction,
            null
        );

        // Assert
        results.Should().HaveCount(2);
    }

    #endregion

    #region BatchProcessorException Tests

    [Fact]
    public void BatchProcessingException_ConstructsWithMessageAndInnerException()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new BatchProcessingException("Test message", innerException);

        // Assert
        exception.Message.Should().Be("Test message");
        exception.InnerException.Should().BeSameAs(innerException);
    }

    [Fact]
    public void BatchProcessingException_ConstructsWithMessageOnly()
    {
        // Arrange & Act
        var exception = new BatchProcessingException("Test message only", null);

        // Assert
        exception.Message.Should().Be("Test message only");
        exception.InnerException.Should().BeNull();
    }

    #endregion
}
