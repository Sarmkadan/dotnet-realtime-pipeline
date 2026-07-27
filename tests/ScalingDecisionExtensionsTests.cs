using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Domain.Services;

namespace DotNetRealtimePipeline.Tests
{
    public class ScalingDecisionExtensionsTests
    {
        [Fact]
        public void IsScaleUp_HappyPath_NullDecision_ThrowsArgumentNullException()
        {
            // Arrange
            var decision = new ScalingDecision(null, null, null, null, null);
            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => ScalingDecisionExtensions.IsScaleUp(decision));
            // Assert
            Assert.NotNull(ex);
            Assert.Equal("decision", ex.ParamName);
        }

        [Fact]
        public void IsScaleUp_HappyPath_ValidDecision_ReturnsTrue()
        {
            // Arrange
            var decision = new ScalingDecision("stage", "reason", ScalingDirection.Up, 10, 15);
            // Act
            var result = ScalingDecisionExtensions.IsScaleUp(decision);
            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsScaleDown_HappyPath_NullDecision_ThrowsArgumentNullException()
        {
            // Arrange
            var decision = new ScalingDecision(null, null, null, null, null);
            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => ScalingDecisionExtensions.IsScaleDown(decision));
            // Assert
            Assert.NotNull(ex);
            Assert.Equal("decision", ex.ParamName);
        }

        [Fact]
        public void IsScaleDown_HappyPath_ValidDecision_ReturnsTrue()
        {
            // Arrange
            var decision = new ScalingDecision("stage", "reason", ScalingDirection.Down, 10, 5);
            // Act
            var result = ScalingDecisionExtensions.IsScaleDown(decision);
            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetSummary_HappyPath_ValidDecision_ReturnsCorrectString()
        {
            // Arrange
            var decision = new ScalingDecision("stage", "reason", ScalingDirection.Up, 10, 15);
            // Act
            var result = ScalingDecisionExtensions.GetSummary(decision);
            // Assert
            Assert.NotNull(result);
            Assert.Equal("Stage \"stage\" scaled up from 10 to 15 consumers. Reason: reason", result);
        }

        [Fact]
        public void GetSummary_NullDecision_ThrowsArgumentNullException()
        {
            // Arrange
            var decision = new ScalingDecision(null, null, null, null, null);
            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => ScalingDecisionExtensions.GetSummary(decision));
            // Assert
            Assert.NotNull(ex);
            Assert.Equal("decision", ex.ParamName);
        }

        [Fact]
        public void ToCsvRow_HappyPath_ValidDecision_ReturnsCorrectString()
        {
            // Arrange
            var decision = new ScalingDecision("stage", "reason", ScalingDirection.Up, 10, 15);
            // Act
            var result = ScalingDecisionExtensions.ToCsvRow(decision);
            // Assert
            Assert.NotNull(result);
            Assert.Equal("stage,2023-03-01T14:30:00.0000000,Up,10,15,0,0", result);
        }

        [Fact]
        public void ToCsvRow_NullDecision_ThrowsArgumentNullException()
        {
            // Arrange
            var decision = new ScalingDecision(null, null, null, null, null);
            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => ScalingDecisionExtensions.ToCsvRow(decision));
            // Assert
            Assert.NotNull(ex);
            Assert.Equal("decision", ex.ParamName);
        }
    }
}