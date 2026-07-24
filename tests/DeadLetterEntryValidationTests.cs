namespace DotNetRealtimePipeline.Tests.Unit;

using System;
using Xunit;
using DotNetRealtimePipeline.DeadLetter;

public class DeadLetterEntryValidationTests
{
    private static DeadLetterEntry CreateValidEntry()
    {
        // A helper that creates an entry that satisfies all validation rules.
        // The default DataPoint constructor is assumed to initialise a positive Id.
        return new DeadLetterEntry(
            EntryId: Guid.NewGuid(),
            DataPoint: new DataPoint(),
            FailureStageName: "stage",
            FailureReason: "reason",
            ExceptionType: "type",
            ExceptionMessage: "message",
            RetryCount: 0,
            MaxRetries: 3,
            EnqueuedAt: DateTime.UtcNow);
    }

    [Fact]
    public void Validate_HappyPath_ReturnsEmptyList()
    {
        // Arrange
        var entry = CreateValidEntry();

        // Act
        var problems = entry.Validate();

        // Assert
        Assert.Empty(problems);
    }

    [Fact]
    public void Validate_NullEntry_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((DeadLetterEntry)null!).Validate());
    }

    [Fact]
    public void IsValid_HappyPath_ReturnsTrue()
    {
        // Arrange
        var entry = CreateValidEntry();

        // Act
        var isValid = entry.IsValid();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_InvalidEntry_ReturnsFalse()
    {
        // Arrange: EntryId is empty GUID which is invalid.
        var entry = new DeadLetterEntry(
            EntryId: Guid.Empty,
            DataPoint: new DataPoint(),
            FailureStageName: "stage",
            FailureReason: "reason",
            ExceptionType: "type",
            ExceptionMessage: "message",
            RetryCount: 0,
            MaxRetries: 3,
            EnqueuedAt: DateTime.UtcNow);

        // Act
        var isValid = entry.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void EnsureValid_HappyPath_DoesNotThrow()
    {
        // Arrange
        var entry = CreateValidEntry();

        // Act & Assert
        var exception = Record.Exception(() => entry.EnsureValid());
        Assert.Null(exception);
    }

    [Fact]
    public void EnsureValid_InvalidEntry_ThrowsArgumentException_WithProblemDetails()
    {
        // Arrange: MaxRetries set to zero triggers a specific validation problem.
        var entry = new DeadLetterEntry(
            EntryId: Guid.NewGuid(),
            DataPoint: new DataPoint(),
            FailureStageName: "stage",
            FailureReason: "reason",
            ExceptionType: "type",
            ExceptionMessage: "message",
            RetryCount: 0,
            MaxRetries: 0, // invalid – must be > 0
            EnqueuedAt: DateTime.UtcNow);

        // Act
        var ex = Assert.Throws<ArgumentException>(() => entry.EnsureValid());

        // Assert
        Assert.Contains("MaxRetries must be greater than zero.", ex.Message);
    }
}
