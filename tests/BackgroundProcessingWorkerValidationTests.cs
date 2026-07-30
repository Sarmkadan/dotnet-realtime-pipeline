// tests/BackgroundProcessingWorkerValidationTests.cs
using System;
using System.Collections.Generic;
using System.Reflection;
using DotNetRealtimePipeline.Workers;
using Xunit;

namespace DotNetRealtimePipeline.Tests.Workers;

public class BackgroundProcessingWorkerValidationTests
{
    // Helper to set a private field via reflection
    private static void SetPrivateField<T>(object instance, string fieldName, T value)
    {
        var field = instance.GetType().GetField(
            fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (field == null)
            throw new InvalidOperationException($"Field '{fieldName}' not found on type {instance.GetType().Name}.");

        field.SetValue(instance, value);
    }

    [Fact]
    public void BackgroundProcessingWorker_Validate_ReturnsEmpty()
    {
        // Arrange
        var worker = new BackgroundProcessingWorker();

        // Act
        IReadOnlyList<string> errors = worker.Validate();

        // Assert
        Assert.Empty(errors);
        Assert.True(worker.IsValid());
    }

    [Fact]
    public void BackgroundProcessingWorker_EnsureValid_DoesNotThrow()
    {
        // Arrange
        var worker = new BackgroundProcessingWorker();

        // Act & Assert
        var exception = Record.Exception(() => worker.EnsureValid());
        Assert.Null(exception);
    }

    [Fact]
    public void MetricsAggregationWorker_ValidInterval_ValidateReturnsEmpty()
    {
        // Arrange
        var worker = new MetricsAggregationWorker(); // assume default interval > 0

        // Act
        var errors = worker.Validate();

        // Assert
        Assert.Empty(errors);
        Assert.True(worker.IsValid());
    }

    [Fact]
    public void MetricsAggregationWorker_InvalidInterval_ValidateReturnsError()
    {
        // Arrange
        var worker = new MetricsAggregationWorker();
        SetPrivateField(worker, "_intervalMs", 0);

        // Act
        var errors = worker.Validate();

        // Assert
        Assert.Single(errors);
        Assert.Contains("Interval must be greater than 0", errors[0]);
        Assert.False(worker.IsValid());
    }

    [Fact]
    public void MetricsAggregationWorker_InvalidInterval_EnsureValid_ThrowsArgumentException()
    {
        // Arrange
        var worker = new MetricsAggregationWorker();
        SetPrivateField(worker, "_intervalMs", -5);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => worker.EnsureValid());
        Assert.Contains("MetricsAggregationWorker is not valid", ex.Message);
        Assert.Contains("Interval must be greater than 0", ex.Message);
    }

    [Fact]
    public void HealthCheckWorker_InvalidInterval_ValidateReturnsError()
    {
        // Arrange
        var worker = new HealthCheckWorker();
        SetPrivateField(worker, "_intervalMs", 0);

        // Act
        var errors = worker.Validate();

        // Assert
        Assert.Single(errors);
        Assert.Contains("Interval must be greater than 0", errors[0]);
        Assert.False(worker.IsValid());
    }

    [Fact]
    public void NullWorker_Validate_ThrowsArgumentNullException()
    {
        // Arrange
        BackgroundProcessingWorker? nullWorker = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullWorker!.Validate());
    }

    [Fact]
    public void NullWorker_EnsureValid_ThrowsArgumentNullException()
    {
        // Arrange
        MetricsAggregationWorker? nullWorker = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullWorker!.EnsureValid());
    }
}
