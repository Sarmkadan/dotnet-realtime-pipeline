using System;
using System.Collections.Generic;
using DotNetRealtimePipeline.Domain.Models;
using FluentAssertions;
using Xunit;

namespace DotNetRealtimePipeline.Tests;

/// <summary>
/// Unit tests for <see cref="ScalingDecisionValidation"/>.
/// </summary>
public class ScalingDecisionValidationTests
{
    private static ScalingDecision CreateValid() => new()
    {
        StageName = "Ingest",
        Direction = ScalingDirection.Up,
        Reason = "High buffer fill",
        FromConsumers = 2,
        ToConsumers = 4,
        BufferFillPercent = 85.0,
        BackpressureFrequency = 1.5,
        DecidedAt = DateTime.UtcNow
    };

    [Fact]
    public void Validate_ValidDecision_ReturnsEmptyList()
    {
        var decision = CreateValid();

        var errors = decision.Validate();

        errors.Should().BeEmpty();
    }

    [Fact]
    public void IsValid_ValidDecision_ReturnsTrue()
    {
        var decision = CreateValid();

        decision.IsValid().Should().BeTrue();
    }

    [Fact]
    public void Validate_NullDecision_ThrowsArgumentNullException()
    {
        ScalingDecision? decision = null;

        var act = () => decision.Validate();

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EnsureValid_ValidDecision_DoesNotThrow()
    {
        var decision = CreateValid();

        var act = () => decision.EnsureValid();

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureValid_InvalidDecision_ThrowsArgumentException()
    {
        var decision = new ScalingDecision
        {
            StageName = "",
            Direction = ScalingDirection.None,
            Reason = "",
            DecidedAt = DateTime.MinValue
        };

        var act = () => decision.EnsureValid();

        act.Should().Throw<ArgumentException>()
            .And.Message.Should().Contain("StageName");
    }

    [Theory]
    [InlineData("", "Reason ok")]
    [InlineData("  ", "Reason ok")]
    public void Validate_EmptyOrWhitespaceStageName_ReturnsError(string stageName, string reason)
    {
        var decision = CreateValid() with { StageName = stageName, Reason = reason };

        var errors = decision.Validate();

        errors.Should().Contain(e => e.Contains("StageName"));
    }

    [Fact]
    public void Validate_NegativeFromConsumers_ReturnsError()
    {
        var decision = CreateValid() with { FromConsumers = -1, Direction = ScalingDirection.Down };

        var errors = decision.Validate();

        errors.Should().Contain(e => e.Contains("FromConsumers"));
    }

    [Fact]
    public void Validate_BufferFillPercentOutOfRange_ReturnsErrors()
    {
        var tooLow = CreateValid() with { BufferFillPercent = -1.0 };
        var tooHigh = CreateValid() with { BufferFillPercent = 101.0 };

        tooLow.Validate().Should().Contain(e => e.Contains("BufferFillPercent"));
        tooHigh.Validate().Should().Contain(e => e.Contains("BufferFillPercent"));
    }

    [Fact]
    public void Validate_UpDirectionWithToConsumersLessThanFrom_ReturnsError()
    {
        var decision = CreateValid() with
        {
            Direction = ScalingDirection.Up,
            FromConsumers = 5,
            ToConsumers = 3
        };

        var errors = decision.Validate();

        errors.Should().Contain(e => e.Contains("ToConsumers"));
    }

    [Fact]
    public void Validate_DownDirectionWithToConsumersLessThanFrom_NoTransitionError()
    {
        var decision = CreateValid() with
        {
            Direction = ScalingDirection.Down,
            FromConsumers = 5,
            ToConsumers = 3
        };

        var errors = decision.Validate();

        errors.Should().NotContain(e => e.Contains("ToConsumers must be greater"));
    }

    [Fact]
    public void Validate_DecidedAtMinValue_ReturnsError()
    {
        var decision = CreateValid() with { DecidedAt = DateTime.MinValue };

        var errors = decision.Validate();

        errors.Should().Contain(e => e.Contains("DecidedAt"));
    }

    [Fact]
    public void Validate_MultipleErrors_ReturnsAll()
    {
        var decision = new ScalingDecision
        {
            StageName = "",
            Reason = "",
            FromConsumers = -1,
            ToConsumers = -1,
            BufferFillPercent = -5,
            BackpressureFrequency = -1,
            DecidedAt = DateTime.MinValue
        };

        var errors = decision.Validate();

        errors.Count.Should().BeGreaterThanOrEqualTo(5);
    }
}
