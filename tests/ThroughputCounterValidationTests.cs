using System.Collections.Generic;
using System.Linq;
using Xunit;
using Metrics;

namespace Tests
{
    public class ThroughputCounterValidationTests
    {
        [Fact]
        public void Validate_Happy_Path_Validate()
        {
            // Arrange
            var counter = new ThroughputCounterValidation(1);

            // Act
            var result = ThroughputCounterValidation.Validate(counter);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void Validate_Happy_Path_IsValid()
        {
            // Arrange
            var counter = new ThroughputCounterValidation(1);

            // Act
            var result = ThroughputCounterValidation.IsValid(counter);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Validate_Happy_Path_EnsureValid()
        {
            // Arrange
            var counter = new ThroughputCounterValidation(1);

            // Act and Assert
            ThroughputCounterValidation.EnsureValid(counter);
        }

        [Fact]
        public void Validate_Null_Validate()
        {
            // Arrange
            ThroughputCounterValidation? counter = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ThroughputCounterValidation.Validate(counter));
        }

        [Fact]
        public void Validate_Null_IsValid()
        {
            // Arrange
            ThroughputCounterValidation? counter = null;

            // Act and Assert
            Assert.False(ThroughputCounterValidation.IsValid(counter));
        }

        [Fact]
        public void Validate_Null_EnsureValid()
        {
            // Arrange
            ThroughputCounterValidation? counter = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ThroughputCounterValidation.EnsureValid(counter));
        }
    }
}