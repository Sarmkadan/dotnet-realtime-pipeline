namespace DotNetRealtimePipeline.Tests.Unit;

using Xunit;
using System;
using DotNetRealtimePipeline.DeadLetter;

public class DeadLetterEntryTests
{
    [Fact]
    public void Constructor_HappyPath_ReturnsValidEntry()
    {
        // Arrange
        var entry = new DeadLetterEntry(
            EntryId: Guid.NewGuid(),
            DataPoint: new DataPoint(),
            FailureStageName: "stage",
            FailureReason: "reason",
            ExceptionType: "type",
            ExceptionMessage: "message",
            RetryCount: 1,
            MaxRetries: 3,
            EnqueuedAt: DateTime.UtcNow);

        // Assert
        Assert.NotNull(entry);
        Assert.Equal(entry.EntryId, Guid.NewGuid());
        Assert.Equal(entry.DataPoint, new DataPoint());
        Assert.Equal(entry.FailureStageName, "stage");
        Assert.Equal(entry.FailureReason, "reason");
        Assert.Equal(entry.ExceptionType, "type");
        Assert.Equal(entry.ExceptionMessage, "message");
        Assert.Equal(entry.RetryCount, 1);
        Assert.Equal(entry.MaxRetries, 3);
        Assert.Equal(entry.EnqueuedAt, DateTime.UtcNow);
    }

    [Fact]
    public void Constructor_NullDataPoint_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DeadLetterEntry(
            EntryId: Guid.NewGuid(),
            DataPoint: null,
            FailureStageName: "stage",
            FailureReason: "reason",
            ExceptionType: "type",
            ExceptionMessage: "message",
            RetryCount: 1,
            MaxRetries: 3,
            EnqueuedAt: DateTime.UtcNow));
    }

    [Fact]
    public void BeginRetry_HappyPath_IncrementsRetryCount()
    {
        // Arrange
        var entry = new DeadLetterEntry(
            EntryId: Guid.NewGuid(),
            DataPoint: new DataPoint(),
            FailureStageName: "stage",
            FailureReason: "reason",
            ExceptionType: "type",
            ExceptionMessage: "message",
            RetryCount: 1,
            MaxRetries: 3,
            EnqueuedAt: DateTime.UtcNow);

        // Act
        entry.BeginRetry();

        // Assert
        Assert.Equal(entry.RetryCount, 2);
    }

    [Fact]
    public void RetryFailed_HappyPath_UpdatesStatus()
    {
        // Arrange
        var entry = new DeadLetterEntry(
            EntryId: Guid.NewGuid(),
            DataPoint: new DataPoint(),
            FailureStageName: "stage",
            FailureReason: "reason",
            ExceptionType: "type",
            ExceptionMessage: "message",
            RetryCount: 1,
            MaxRetries: 3,
            EnqueuedAt: DateTime.UtcNow);

        // Act
        entry.RetryFailed("reason");

        // Assert
        Assert.Equal(entry.Status, DeadLetterStatus.PermanentFailure);
    }

    [Fact]
    public void GetSummary_HappyPath_ReturnsSummary()
    {
        // Arrange
        var entry = new DeadLetterEntry(
            EntryId: Guid.NewGuid(),
            DataPoint: new DataPoint(),
            FailureStageName: "stage",
            FailureReason: "reason",
            ExceptionType: "type",
            ExceptionMessage: "message",
            RetryCount: 1,
            MaxRetries: 3,
            EnqueuedAt: DateTime.UtcNow);

        // Act
        var summary = entry.GetSummary();

        // Assert
        Assert.NotNull(summary);
        Assert.Contains(entry.EntryId.ToString(), summary);
        Assert.Contains(entry.DataPoint.Id.ToString(), summary);
        Assert.Contains(entry.FailureStageName, summary);
        Assert.Contains(entry.RetryCount.ToString(), summary);
        Assert.Contains(entry.MaxRetries.ToString(), summary);
        Assert.Contains(entry.Status.ToString(), summary);
    }
}
