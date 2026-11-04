#nullable enable

using System;
using DotNetRealtimePipeline.Domain.Models;
using FluentAssertions;
using Xunit;

namespace DotNetRealtimePipeline.Tests;

/// <summary>
/// Unit tests for <see cref="ScalingDecisionValidation"/> covering <see cref="ScalingDecision"/> validation rules.
/// </summary>
public class ScalingDecisionValidationTests
{
    /// <summary>
    /// Builds a scaling decision; defaults satisfy every validation rule.
    /// </summary>
    private static ScalingDecision CreateDecision(
        string stageName = "parse",
        ScalingDirection direction = ScalingDirection.Up,
        string reason = "buffer saturated",
        int fromConsumers = 2,
        int toConsumers = 4,
        double bufferFillPercent = 85.0,
        double backpressureFrequency = 3.5,
        DateTime? decidedAt = null) => new()
    {
        StageName = stageName,
        Direction = direction,
        Reason = reason,
        FromConsumers = fromConsumers,
        ToConsumers = toConsumers,
        BufferFillPercent = bufferFillPercent,
        BackpressureFrequency = backpressureFrequency,
        DecidedAt = decidedAt ?? new DateTime(2026, 11, 4, 12, 0, 0, DateTimeKind.Utc)
    };

    [Fact]
    public void Validate_WithValidDecision_ReturnsNoErrors()
    {
        // Arrange
        var decision = CreateDecision();

        // Act
        var errors = decision.Validate();

        // Assert
        errors.Should().BeEmpty();
        decision.IsValid().Should().BeTrue();
        Action ensure = () => decision.EnsureValid();
        ensure.Should().NotThrow();
    }

    [Fact]
    public void Validate_WithNullDecision_ThrowsArgumentNullException()
    {
        // Arrange
        ScalingDecision? decision = null;

        // Act & Assert
        Action validate = () => decision.Validate();
        Action isValid = () => decision.IsValid();
        Action ensure = () => decision.EnsureValid();
        validate.Should().ThrowExactly<ArgumentNullException>();
        isValid.Should().ThrowExactly<ArgumentNullException>();
        ensure.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void Validate_WithBlankStageNameAndReason_ReportsBothErrors()
    {
        // Arrange
        var decision = CreateDecision(stageName: "   ", reason: string.Empty);

        // Act
        var errors = decision.Validate();

        // Assert
        errors.Should().HaveCount(2);
        errors.Should().Contain("StageName cannot be null or whitespace.");
        errors.Should().Contain("Reason cannot be null or whitespace.");
        decision.IsValid().Should().BeFalse();
    }

    [Theory]
    [InlineData(ScalingDirection.Down, true)]
    [InlineData(ScalingDirection.Up, false)]
    [InlineData(ScalingDirection.None, false)]
    public void Validate_ToConsumersBelowFrom_OnlyAllowedForDown(ScalingDirection direction, bool expectedValid)
    {
        // Arrange
        var decision = CreateDecision(direction: direction, fromConsumers: 5, toConsumers: 3);

        // Act
        var errors = decision.Validate();

        // Assert
        decision.IsValid().Should().Be(expectedValid);
        errors.Should().HaveCount(expectedValid ? 0 : 1);
    }

    [Theory]
    [InlineData(0.0, true)]
    [InlineData(100.0, true)]
    [InlineData(-0.01, false)]
    [InlineData(100.01, false)]
    public void Validate_BufferFillPercentBoundaries_AreInclusive(double bufferFill, bool expectedValid)
    {
        // Arrange
        var decision = CreateDecision(bufferFillPercent: bufferFill);

        // Act
        var errors = decision.Validate();

        // Assert
        decision.IsValid().Should().Be(expectedValid);
        errors.Should().HaveCount(expectedValid ? 0 : 1);
    }

    [Fact]
    public void Validate_NegativeNumbersAndUninitializedTimestamp_ReportsEachError()
    {
        // Arrange
        var decision = CreateDecision(
            fromConsumers: -1,
            toConsumers: -1,
            backpressureFrequency: -0.5,
            decidedAt: DateTime.MinValue);

        // Act
        var errors = decision.Validate();

        // Assert
        errors.Should().HaveCount(4);
        errors.Should().Contain("FromConsumers must be a non-negative integer.");
        errors.Should().Contain("ToConsumers must be a non-negative integer.");
        errors.Should().Contain("BackpressureFrequency must be a non-negative value.");
        errors.Should().Contain("DecidedAt cannot be DateTime.MinValue (uninitialized).");
    }

    [Fact]
    public void EnsureValid_WithInvalidDecision_ThrowsArgumentExceptionWithErrorDetails()
    {
        // Arrange
        var decision = CreateDecision(reason: string.Empty);

        // Act
        Action ensure = () => decision.EnsureValid();

        // Assert
        ensure.Should().ThrowExactly<ArgumentException>()
            .WithMessage("*ScalingDecision is invalid*")
            .WithMessage("*Reason cannot be null or whitespace.*");
    }
}
