using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Xunit;
using DotNetRealtimePipeline.Workers;

namespace DotNetRealtimePipeline.Tests.Workers;

public class DynamicScalingWorkerValidationTests
{
    private static DynamicScalingWorker CreateWorker(
        object? scalingService,
        object? logger,
        int intervalMs,
        bool isRunning)
    {
        // Create an instance without invoking any constructor.
        var worker = (DynamicScalingWorker)FormatterServices.GetUninitializedObject(typeof(DynamicScalingWorker));

        // Populate the private fields that the validation logic inspects.
        var type = typeof(DynamicScalingWorker);
        type.GetField("_scalingService", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(worker, scalingService);
        type.GetField("_logger", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(worker, logger);
        type.GetField("_intervalMs", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(worker, intervalMs);
        type.GetField("_isRunning", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(worker, isRunning);

        return worker;
    }

    [Fact]
    public void Validate_ReturnsEmptyList_ForValidWorker()
    {
        var worker = CreateWorker(new object(), new object(), 1000, false);
        var result = DynamicScalingWorkerValidation.Validate(worker);
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_ReturnsErrors_ForInvalidWorker()
    {
        var worker = CreateWorker(null, null, 400, true);
        var result = DynamicScalingWorkerValidation.Validate(worker);

        Assert.Contains("DynamicScalingWorker._scalingService cannot be null.", result);
        Assert.Contains("DynamicScalingWorker._logger cannot be null.", result);
        Assert.Contains("DynamicScalingWorker._intervalMs must be at least 500", result);
        Assert.Contains("DynamicScalingWorker state inconsistency", result);
        Assert.Equal(4, result.Count);
    }

    [Fact]
    public void IsValid_ReturnsTrue_ForValidWorker()
    {
        var worker = CreateWorker(new object(), new object(), 800, false);
        var isValid = DynamicScalingWorkerValidation.IsValid(worker);
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_ReturnsFalse_ForInvalidWorker()
    {
        var worker = CreateWorker(null, null, 400, true);
        var isValid = DynamicScalingWorkerValidation.IsValid(worker);
        Assert.False(isValid);
    }

    [Fact]
    public void EnsureValid_DoesNotThrow_ForValidWorker()
    {
        var worker = CreateWorker(new object(), new object(), 600, false);
        DynamicScalingWorkerValidation.EnsureValid(worker);
    }

    [Fact]
    public void EnsureValid_ThrowsArgumentException_ForInvalidWorker()
    {
        var worker = CreateWorker(null, null, 400, true);
        var ex = Assert.Throws<ArgumentException>(() => DynamicScalingWorkerValidation.EnsureValid(worker));

        Assert.Contains("DynamicScalingWorker validation failed", ex.Message);
        Assert.Contains("DynamicScalingWorker._scalingService cannot be null.", ex.Message);
    }

    [Fact]
    public void Validate_ThrowsArgumentNullException_WhenWorkerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => DynamicScalingWorkerValidation.Validate(null));
    }

    [Fact]
    public void EnsureValid_ThrowsArgumentNullException_WhenWorkerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => DynamicScalingWorkerValidation.EnsureValid(null));
    }
}
