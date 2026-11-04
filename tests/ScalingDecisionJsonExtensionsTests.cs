using System;
using Xunit;
using src.Domain.Models;

namespace src.Domain.Models.Tests
{
    public class ScalingDecisionJsonExtensionsTests
    {
        [Fact]
        public void ToJson_WithValidScalingDecision_ReturnsValidJsonString()
        {
            // Arrange
            var scalingDecision = new ScalingDecision
            {
                Timestamp = DateTime.UtcNow,
                ScaleUp = true,
                ScaleDown = false,
                Reason = "High load",
                CurrentWorkers = 5,
                TargetWorkers = 10
            };

            // Act
            var json = ScalingDecisionJsonExtensions.ToJson(scalingDecision);

            // Assert
            Assert.NotNull(json);
            Assert.NotEmpty(json);
        }

        [Fact]
        public void FromJson_WithValidJsonString_ReturnsValidScalingDecision()
        {
            // Arrange
            var json = "{\"Timestamp\":\"2023-01-01T00:00:00Z\",\"ScaleUp\":true,\"ScaleDown\":false,\"Reason\":\"High load\",\"CurrentWorkers\":5,\"TargetWorkers\":10}";

            // Act
            var scalingDecision = ScalingDecisionJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(scalingDecision);
            Assert.Equal("High load", scalingDecision.Reason);
            Assert.Equal(5, scalingDecision.CurrentWorkers);
            Assert.Equal(10, scalingDecision.TargetWorkers);
        }

        [Fact]
        public void FromJson_WithNullJsonString_ReturnsNull()
        {
            // Arrange
            string json = null;

            // Act
            var scalingDecision = ScalingDecisionJsonExtensions.FromJson(json);

            // Assert
            Assert.Null(scalingDecision);
        }

        [Fact]
        public void TryFromJson_WithValidJsonString_ReturnsTrueAndValidScalingDecision()
        {
            // Arrange
            var json = "{\"Timestamp\":\"2023-01-01T00:00:00Z\",\"ScaleUp\":true,\"ScaleDown\":false,\"Reason\":\"High load\",\"CurrentWorkers\":5,\"TargetWorkers\":10}";

            // Act
            var result = ScalingDecisionJsonExtensions.TryFromJson(json, out var scalingDecision);

            // Assert
            Assert.True(result);
            Assert.NotNull(scalingDecision);
            Assert.Equal("High load", scalingDecision.Reason);
        }

        [Fact]
        public void TryFromJson_WithInvalidJsonString_ReturnsFalseAndNull()
        {
            // Arrange
            var json = "invalid json";

            // Act
            var result = ScalingDecisionJsonExtensions.TryFromJson(json, out var scalingDecision);

            // Assert
            Assert.False(result);
            Assert.Null(scalingDecision);
        }

        [Fact]
        public void TryFromJson_WithNullJsonString_ReturnsFalseAndNull()
        {
            // Arrange
            string json = null;

            // Act
            var result = ScalingDecisionJsonExtensions.TryFromJson(json, out var scalingDecision);

            // Assert
            Assert.False(result);
            Assert.Null(scalingDecision);
        }
    }
}
