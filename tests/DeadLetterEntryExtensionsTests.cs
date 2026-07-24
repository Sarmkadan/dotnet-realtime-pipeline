namespace DotNetRealtimePipeline.Tests.Unit;

using Xunit;
using System;
using DotNetRealtimePipeline.DeadLetter;

public class DeadLetterEntryExtensionsTests
{
    [Fact]
    public void IsResolved_HappyPath_ReturnsTrue()
    {
        // Arrange
        var entry = new DeadLetterEntry(
            EntryId: Guid.NewGuid(),
            DataPoint: new DataPoint(),
            FailureStageName: "stage",
            FailureReason: "reason",
            ExceptionType: "type",
            ExceptionMessage: "message",
            RetryCount: 0,
            MaxRetries: 3,
            EnqueuedAt: DateTime.UtcNow,
            Status: DeadLetterStatus.Resolved);

        // Act
        var result = DeadLetterEntryExtensions.IsResolved(entry);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsResolved_NullEntry_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => DeadLetterEntryExtensions.IsResolved(null));
    }

    [Fact]
    public void IsPermanentFailure_HappyPath_ReturnsTrue()
    {
        // Arrange
        var entry = new DeadLetterEntry(
            EntryId: Guid.NewGuid(),
            DataPoint: new DataPoint(),
            FailureStageName: "stage",
            FailureReason: "reason",
            ExceptionType: "type",
            ExceptionMessage: "message",
            RetryCount: 3,
            MaxRetries: 3,
            EnqueuedAt: DateTime.UtcNow,
            Status: DeadLetterStatus.PermanentFailure);

        // Act
        var result = DeadLetterEntryExtensions.IsPermanentFailure(entry);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsPermanentFailure_NullEntry_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => DeadLetterEntryExtensions.IsPermanentFailure(null));
    }

    [Fact]
    public void GetRetryProgress_HappyPath_ReturnsZero()
    {
        // Arrange
        var entry = new DeadLetterEntry(
            EntryId: Guid.NewGuid(),
            DataPoint: new DataPoint(),
            FailureStageName: "stage",
            FailureReason: "reason",
            ExceptionType: "type",
            ExceptionMessage: "message",
            RetryCount: 0,
            MaxRetries: 3,
            EnqueuedAt: DateTime.UtcNow);

        // Act
        var result = DeadLetterEntryExtensions.GetRetryProgress(entry);

        // Assert
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void GetRetryProgress_NullEntry_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => DeadLetterEntryExtensions.GetRetryProgress(null));
    }

    [Fact]
    public void GetLastActivity_HappyPath_ReturnsEnqueuedAt()
    {
        // Arrange
        var entry = new DeadLetterEntry(
            EntryId: Guid.NewGuid(),
            DataPoint: new DataPoint(),
            FailureStageName: "stage",
            FailureReason: "reason",
            ExceptionType: "type",
            ExceptionMessage: "message",
            RetryCount: 0,
            MaxRetries: 3,
            EnqueuedAt: DateTime.UtcNow);

        // Act
        var result = DeadLetterEntryExtensions.GetLastActivity(entry);

        // Assert
        Assert.Equal(entry.EnqueuedAt, result);
    }

    [Fact]
    public void GetLastActivity_NullEntry_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => DeadLetterEntryExtensions.GetLastActivity(null));
    }
}
