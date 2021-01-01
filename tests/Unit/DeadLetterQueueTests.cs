#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetRealtimePipeline.DeadLetter;
using DotNetRealtimePipeline.Domain.Models;
using Xunit;

namespace DotNetRealtimePipeline.Tests.Unit;

/// <summary>
/// Unit tests for the <see cref="DeadLetterQueue"/> class and its associated functionality.
/// Tests verify the behavior of dead letter queue operations including enqueueing,
/// peeking, retrying, acknowledging, and capacity management.
/// </summary>
public sealed class DeadLetterQueueTests
{
	/// <summary>
	/// Creates a valid test data point for use in dead letter queue tests.
	/// </summary>
	/// <param name="id">The identifier for the data point. Defaults to 1.</param>
	/// <returns>A new <see cref="DataPoint"/> instance with the specified identifier.</returns>
	private static DataPoint ValidPoint(long id = 1)
	=> new DataPoint(id, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 1.0, "sensor-01");

	/// <summary>
	/// Creates a new dead letter queue instance for testing with configurable capacity and retry limits.
	/// </summary>
	/// <param name="capacity">The maximum number of entries the queue can hold. Defaults to 100.</param>
	/// <param name="maxRetries">The default maximum number of retry attempts for entries. Defaults to 3.</param>
	/// <returns>A new <see cref="DeadLetterQueue"/> instance configured with the specified parameters.</returns>
	private static DeadLetterQueue NewQueue(int capacity = 100, int maxRetries = 3)
	=> new(maxCapacity: capacity, defaultMaxRetries: maxRetries);

	// ── Enqueue ──────────────────────────────────────────────────────────────

	/// <summary>
	/// Tests that enqueuing a valid data point increases the queue count.
	/// </summary>
	[Fact]
	public async Task EnqueueAsync_ValidEntry_IncreasesCount()
	{
		var queue = NewQueue();
		await queue.EnqueueAsync(ValidPoint(1), "Ingestion", "Validation failed");
		Assert.Equal(1, queue.Count);
	}

	/// <summary>
	/// Tests that enqueuing a null data point throws an ArgumentNullException.
	/// </summary>
	[Fact]
	public async Task EnqueueAsync_NullDataPoint_Throws()
	{
		var queue = NewQueue();
		await Assert.ThrowsAsync<ArgumentNullException>(() =>
			queue.EnqueueAsync(null!, "Ingestion", "reason"));
	}

	/// <summary>
	/// Tests that enqueuing with an empty reason throws an ArgumentException.
	/// </summary>
	[Fact]
	public async Task EnqueueAsync_EmptyReason_Throws()
	{
		var queue = NewQueue();
		await Assert.ThrowsAsync<ArgumentException>(() =>
			queue.EnqueueAsync(ValidPoint(), "Ingestion", ""));
	}

	/// <summary>
	/// Tests that enqueuing with an exception stores the exception information in the entry.
	/// </summary>
	[Fact]
	public async Task EnqueueAsync_WithException_StoresExceptionInfo()
	{
		var queue = NewQueue();
		var ex = new InvalidOperationException("boom");
		await queue.EnqueueAsync(ValidPoint(), "Transform", "processing error", ex);

		var entries = await queue.PeekAsync(1);
		Assert.Single(entries);
		Assert.Equal(nameof(InvalidOperationException), entries[0].ExceptionType);
		Assert.Equal("boom", entries[0].ExceptionMessage);
	}

	// ── Peek ─────────────────────────────────────────────────────────────────

	/// <summary>
	/// Tests that peeking does not remove entries from the queue.
	/// </summary>
	[Fact]
	public async Task PeekAsync_DoesNotRemoveEntries()
	{
		var queue = NewQueue();
		await queue.EnqueueAsync(ValidPoint(), "Stage", "reason");

		await queue.PeekAsync(10);

		Assert.Equal(1, queue.Count);
	}

	// ── Retry ────────────────────────────────────────────────────────────────

	/// <summary>
	/// Tests that dequeueing for retry returns pending entries.
	/// </summary>
	[Fact]
	public async Task DequeueForRetryAsync_ReturnsPendingEntries()
	{
		var queue = NewQueue(maxRetries: 2);
		await queue.EnqueueAsync(ValidPoint(1), "Stage", "reason1");
		await queue.EnqueueAsync(ValidPoint(2), "Stage", "reason2");

		var batch = await queue.DequeueForRetryAsync(10);

		Assert.Equal(2, batch.Count);
		Assert.All(batch, e => Assert.Equal(DeadLetterStatus.InRetry, e.Status));
	}

	/// <summary>
	/// Tests that an entry with exhausted retries is not returned for retry.
	/// </summary>
	[Fact]
	public async Task DequeueForRetryAsync_EntryExhaustedRetries_NotReturned()
	{
		var queue = NewQueue(maxRetries: 1);
		await queue.EnqueueAsync(ValidPoint(), "Stage", "reason");

		// First retry — burns the budget
		var first = await queue.DequeueForRetryAsync(1);
		Assert.Single(first);
		first[0].RetryFailed("still failing"); // RetryCount == 1 == MaxRetries → PermanentFailure

		// Should not be eligible any more
		var second = await queue.DequeueForRetryAsync(1);
		Assert.Empty(second);
	}

	// ── Acknowledge ──────────────────────────────────────────────────────────

	/// <summary>
	/// Tests that acknowledging success removes the entry from the queue.
	/// </summary>
	[Fact]
	public async Task AcknowledgeSuccessAsync_RemovesEntry()
	{
		var queue = NewQueue();
		await queue.EnqueueAsync(ValidPoint(), "Stage", "reason");

		var entries = await queue.PeekAsync(1);
		var id = entries[0].EntryId;

		await queue.AcknowledgeSuccessAsync(id);

		Assert.Equal(0, queue.Count);
	}

	/// <summary>
	/// Tests that acknowledging failure marks the entry as permanent failure.
	/// </summary>
	[Fact]
	public async Task AcknowledgeFailureAsync_MarksAsPermanentFailure()
	{
		var queue = NewQueue();
		await queue.EnqueueAsync(ValidPoint(), "Stage", "reason");

		var entries = await queue.PeekAsync(1);
		var id = entries[0].EntryId;

		await queue.AcknowledgeFailureAsync(id, "non-retryable");

		var updated = await queue.PeekAsync(1);
		Assert.Equal(DeadLetterStatus.PermanentFailure, updated[0].Status);
		Assert.Equal("non-retryable", updated[0].ResolutionNote);
	}

	// ── Stats ────────────────────────────────────────────────────────────────

	/// <summary>
	/// Tests that getting stats reflects the current state of the queue.
	/// </summary>
	[Fact]
	public async Task GetStatsAsync_ReflectsCurrentState()
	{
		var queue = NewQueue();
		await queue.EnqueueAsync(ValidPoint(1), "Stage", "r1");
		await queue.EnqueueAsync(ValidPoint(2), "Stage", "r2");

		var entries = await queue.PeekAsync(1);
		await queue.AcknowledgeSuccessAsync(entries[0].EntryId);

		var stats = await queue.GetStatsAsync();

		Assert.Equal(1, stats.TotalEntries);
		Assert.Equal(1, stats.PendingEntries);
		Assert.Equal(1, stats.TotalResolved);
	}

	// ── Capacity ─────────────────────────────────────────────────────────────

	/// <summary>
	/// Tests that when the queue is at capacity, enqueuing evicts the oldest entry.
	/// </summary>
	[Fact]
	public async Task Enqueue_WhenAtCapacity_EvictsOldest()
	{
		var queue = NewQueue(capacity: 2);
		await queue.EnqueueAsync(ValidPoint(1), "Stage", "r1");
		await queue.EnqueueAsync(ValidPoint(2), "Stage", "r2");
		await queue.EnqueueAsync(ValidPoint(3), "Stage", "r3"); // should evict dp1

		Assert.Equal(2, queue.Count);
	}

	// ── DeadLetterEntry model ─────────────────────────────────────────────────

	/// <summary>
	/// Tests that DeadLetterEntry.CanRetry returns true when retry count is under budget.
	/// </summary>
	[Fact]
	public void DeadLetterEntry_CanRetry_TrueWhenUnderBudget()
	{
		var entry = new DeadLetterEntry { MaxRetries = 3, RetryCount = 0, Status = DeadLetterStatus.Pending };
		Assert.True(entry.CanRetry);
	}

	/// <summary>
	/// Tests that DeadLetterEntry.CanRetry returns false when retry budget is exhausted.
	/// </summary>
	[Fact]
	public void DeadLetterEntry_CanRetry_FalseWhenExhausted()
	{
		var entry = new DeadLetterEntry { MaxRetries = 2, RetryCount = 2, Status = DeadLetterStatus.Pending };
		Assert.False(entry.CanRetry);
	}
}
