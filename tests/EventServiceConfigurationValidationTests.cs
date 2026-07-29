using System;
using System.Collections.Generic;
using DotNetRealtimePipeline.Configuration;
using Xunit;

namespace DotNetRealtimePipeline.Tests;

public class EventServiceConfigurationValidationTests
{
    private static WorkerOptions CreateValidOptions()
    {
        // Assuming WorkerOptions has a parameterless constructor and public setters.
        return new WorkerOptions
        {
            MetricsAggregationIntervalMs = 1_000,
            HealthCheckIntervalMs = 5_000
        };
    }

    [Fact]
    public void Validate_ReturnsEmptyList_WhenOptionsAreValid()
    {
        var options = CreateValidOptions();

        IReadOnlyList<string> problems = options.Validate();

        Assert.Empty(problems);
    }

    [Fact]
    public void Validate_ReturnsProblem_WhenMetricsAggregationIntervalIsNonPositive()
    {
        var options = CreateValidOptions();
        options.MetricsAggregationIntervalMs = 0; // non‑positive

        IReadOnlyList<string> problems = options.Validate();

        Assert.Single(problems);
        Assert.Contains("MetricsAggregationIntervalMs must be a positive integer", problems[0]);
    }

    [Fact]
    public void Validate_ReturnsProblem_WhenHealthCheckIntervalIsNonPositive()
    {
        var options = CreateValidOptions();
        options.HealthCheckIntervalMs = -1; // non‑positive

        IReadOnlyList<string> problems = options.Validate();

        Assert.Single(problems);
        Assert.Contains("HealthCheckIntervalMs must be a positive integer", problems[0]);
    }

    [Fact]
    public void Validate_ReturnsTwoProblems_WhenBothIntervalsAreNonPositive()
    {
        var options = new WorkerOptions
        {
            MetricsAggregationIntervalMs = 0,
            HealthCheckIntervalMs = 0
        };

        IReadOnlyList<string> problems = options.Validate();

        Assert.Equal(2, problems.Count);
        Assert.Contains("MetricsAggregationIntervalMs must be a positive integer", problems[0]);
        Assert.Contains("HealthCheckIntervalMs must be a positive integer", problems[1]);
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenOptionsAreValid()
    {
        var options = CreateValidOptions();

        bool isValid = options.IsValid();

        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_ThrowsArgumentNullException_WhenOptionsAreNull()
    {
        WorkerOptions? options = null;

        Assert.Throws<ArgumentNullException>(() => options!.IsValid());
    }

    [Fact]
    public void EnsureValid_DoesNotThrow_WhenOptionsAreValid()
    {
        var options = CreateValidOptions();

        var exception = Record.Exception(() => options.EnsureValid());

        Assert.Null(exception);
    }

    [Fact]
    public void EnsureValid_ThrowsArgumentException_WithCorrectMessage_WhenInvalid()
    {
        var options = new WorkerOptions
        {
            MetricsAggregationIntervalMs = 0,
            HealthCheckIntervalMs = -5
        };

        var ex = Assert.Throws<ArgumentException>(() => options.EnsureValid());

        Assert.Contains("WorkerOptions validation failed with 2 problem(s):", ex.Message);
        Assert.Contains("MetricsAggregationIntervalMs must be a positive integer", ex.Message);
        Assert.Contains("HealthCheckIntervalMs must be a positive integer", ex.Message);
    }

    [Fact]
    public void EnsureValid_ThrowsArgumentNullException_WhenOptionsAreNull()
    {
        WorkerOptions? options = null;

        Assert.Throws<ArgumentNullException>(() => options!.EnsureValid());
    }
}
