namespace DotNetRealtimePipeline.Tests.Unit;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetRealtimePipeline.DeadLetter;
using Xunit;

public class DeadLetterQueueExtensionsTests
{
    private static DeadLetterQueue CreateQueue()
    {
        // The real DeadLetterQueue likely has a parameterless constructor or a simple one.
        // If it requires dependencies, adjust accordingly.
        return new DeadLetterQueue();
    }

    private static DeadLetterEntry CreateEntry(int retryCount = 0, DeadLetterStatus status = DeadLetterStatus.Pending)
    {
        return new DeadLetterEntry(
            EntryId: Guid.NewGuid(),
            DataPoint: new DataPoint(),
            FailureStageName: "stage",
            FailureReason: "reason",
            ExceptionType: null,
            ExceptionMessage: null,
            RetryCount: retryCount,
            MaxRetries: 3,
            EnqueuedAt: DateTime.UtcNow,
            Status: status);
    }

    [Fact]
    public async Task ProcessForRetryAsync_HappyPath_SuccessfulProcessing()
    {
        // Arrange
        var queue = CreateQueue();
        var entry = CreateEntry();
        await queue.EnqueueAsync(entry.DataPoint, entry.FailureStageName, entry.FailureReason, null);

        // Act
        var result = await queue.ProcessForRetryAsync(
            maxCount: 1,
            processEntry: _ => Task.FromResult(true));

        // Assert
        Assert.Equal(1, result.TotalProcessed);
        Assert.Equal(1, result.SuccessfullyProcessed);
        Assert.Equal(0, result.FailedProcessing);
        Assert.Single(result.EntriesProcessed);
        Assert.Equal(entry.EntryId, result.EntriesProcessed[0].EntryId);
    }

    [Fact]
    public async Task ProcessForRetryAsync_HappyPath_FailureRequeuesEntry()
    {
        // Arrange
        var queue = CreateQueue();
        var entry = CreateEntry();
        await queue.EnqueueAsync(entry.DataPoint, entry.FailureStageName, entry.FailureReason, null);

        // Act
        var result = await queue.ProcessForRetryAsync(
            maxCount: 1,
            processEntry: _ => Task.FromResult(false));

        // Assert
        Assert.Equal(1, result.TotalProcessed);
        Assert.Equal(0, result.SuccessfullyProcessed);
        Assert.Equal(1, result.FailedProcessing);
        Assert.Single(result.EntriesProcessed);

        // The entry should be back in the queue
        var pending = await queue.PeekAsync(10);
        Assert.Contains(pending, e => e.EntryId == entry.EntryId);
    }

    [Fact]
    public async Task ProcessForRetryAsync_NullQueue_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await ((DeadLetterQueue)null!).ProcessForRetryAsync(
                maxCount: 1,
                processEntry: _ => Task.FromResult(true)));
    }

    [Fact]
    public async Task ProcessForRetryAsync_MaxCountZero_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var queue = CreateQueue();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            await queue.ProcessForRetryAsync(
                maxCount: 0,
                processEntry: _ => Task.FromResult(true)));
    }

    [Fact]
    public async Task FindAsync_HappyPath_FiltersCorrectly()
    {
        // Arrange
        var queue = CreateQueue();
        var entry1 = CreateEntry();
        var entry2 = CreateEntry();
        await queue.EnqueueAsync(entry1.DataPoint, entry1.FailureStageName, entry1.FailureReason, null);
        await queue.EnqueueAsync(entry2.DataPoint, entry2.FailureStageName, entry2.FailureReason, null);

        // Act
        var results = await queue.FindAsync(e => e.FailureStageName == "stage");

        // Assert
        Assert.Equal(2, results.Count);
        Assert.All(results, e => Assert.Equal("stage", e.FailureStageName));
    }

    [Fact]
    public async Task FindAsync_NullQueue_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await ((DeadLetterQueue)null!).FindAsync(_ => true));
    }

    [Fact]
    public async Task FindAsync_NullPredicate_ThrowsArgumentNullException()
    {
        // Arrange
        var queue = CreateQueue();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await queue.FindAsync(null!));
    }

    [Fact]
    public async Task FindByStageAsync_HappyPath_ReturnsMatchingEntries()
    {
        // Arrange
        var queue = CreateQueue();
        var entryA = CreateEntry();
        var entryB = new DeadLetterEntry(
            EntryId: Guid.NewGuid(),
            DataPoint: new DataPoint(),
            FailureStageName: "other",
            FailureReason: "reason",
            ExceptionType: null,
            ExceptionMessage: null,
            RetryCount: 0,
            MaxRetries: 3,
            EnqueuedAt: DateTime.UtcNow);
        await queue.EnqueueAsync(entryA.DataPoint, entryA.FailureStageName, entryA.FailureReason, null);
        await queue.EnqueueAsync(entryB.DataPoint, entryB.FailureStageName, entryB.FailureReason, null);

        // Act
        var results = await queue.FindByStageAsync("stage");

        // Assert
        Assert.Single(results);
        Assert.Equal(entryA.FailureStageName, results[0].FailureStageName);
    }

    [Fact]
    public async Task FindByStageAsync_NullStageName_ThrowsArgumentException()
    {
        // Arrange
        var queue = CreateQueue();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await queue.FindByStageAsync(null!));
    }

    [Fact]
    public async Task GetReportAsync_HappyPath_ContainsSummary()
    {
        // Arrange
        var queue = CreateQueue();
        var entry = CreateEntry();
        await queue.EnqueueAsync(entry.DataPoint, entry.FailureStageName, entry.FailureReason, null);

        // Act
        var report = await queue.GetReportAsync(includeDetails: true);

        // Assert
        Assert.Contains("Dead Letter Queue Report", report);
        Assert.Contains(entry.EntryId.ToString(), report);
        Assert.Contains(entry.FailureStageName, report);
    }

    [Fact]
    public async Task GetReportAsync_NullQueue_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await ((DeadLetterQueue)null!).GetReportAsync());
    }
}
