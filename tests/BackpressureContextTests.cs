using System;
using System.Collections.Generic;
using DotNetRealtimePipeline.Domain.Models;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace DotNetRealtimePipeline.Tests
{
    [TestFixture]
    public class BackpressureContextTests
    {
        [Test]
        public void TestGetBufferFillPercentage_EmptyBuffer_ReturnsZero()
        {
            // Arrange
            var backpressureContext = new BackpressureContext(1, "test", 100);

            // Act
            double fillPercentage = backpressureContext.GetBufferFillPercentage();

            // Assert
            Assert.AreEqual(0d, fillPercentage);
        }

        [Test]
        public void TestGetBufferFillPercentage_FullBuffer_Returns100()
        {
            // Arrange
            var backpressureContext = new BackpressureContext(1, "test", 100);
            backpressureContext.BufferSize = 100;

            // Act
            double fillPercentage = backpressureContext.GetBufferFillPercentage();

            // Assert
            Assert.AreEqual(100d, fillPercentage);
        }

        [Test]
        public void TestShouldApplyBackpressure_EmptyBuffer_ReturnsFalse()
        {
            // Arrange
            var backpressureContext = new BackpressureContext(1, "test", 100);

            // Act
            bool shouldApplyBackpressure = backpressureContext.ShouldApplyBackpressure();

            // Assert
            Assert.IsFalse(shouldApplyBackpressure);
        }

        [Test]
        public void TestShouldApplyBackpressure_FullBuffer_ReturnsTrue()
        {
            // Arrange
            var backpressureContext = new BackpressureContext(1, "test", 100);
            backpressureContext.BufferSize = 100;

            // Act
            bool shouldApplyBackpressure = backpressureContext.ShouldApplyBackpressure();

            // Assert
            Assert.IsTrue(shouldApplyBackpressure);
        }

        [Test]
        public void TestTryAddToBuffer_EmptyBuffer_ReturnsTrue()
        {
            // Arrange
            var backpressureContext = new BackpressureContext(1, "test", 100);

            // Act
            bool result = backpressureContext.TryAddToBuffer(10);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void TestTryAddToBuffer_FullBuffer_ReturnsFalse()
        {
            // Arrange
            var backpressureContext = new BackpressureContext(1, "test", 100);
            backpressureContext.BufferSize = 100;

            // Act
            bool result = backpressureContext.TryAddToBuffer(10);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void TestRemoveFromBuffer_EmptyBuffer_ReturnsZero()
        {
            // Arrange
            var backpressureContext = new BackpressureContext(1, "test", 100);

            // Act
            backpressureContext.RemoveFromBuffer(10);

            // Assert
            Assert.AreEqual(0, backpressureContext.BufferSize);
        }

        [Test]
        public void TestRemoveFromBuffer_FullBuffer_ReturnsZero()
        {
            // Arrange
            var backpressureContext = new BackpressureContext(1, "test", 100);
            backpressureContext.BufferSize = 100;

            // Act
            backpressureContext.RemoveFromBuffer(100);

            // Assert
            Assert.AreEqual(0, backpressureContext.BufferSize);
        }

        [Test]
        public void TestActivateBackpressure_EmptyBuffer_SetsIsBackpressuredToTrue()
        {
            // Arrange
            var backpressureContext = new BackpressureContext(1, "test", 100);

            // Act
            backpressureContext.ActivateBackpressure();

            // Assert
            Assert.IsTrue(backpressureContext.IsBackpressured);
        }

        [Test]
        public void TestDeactivateBackpressure_EmptyBuffer_SetsIsBackpressuredToFalse()
        {
            // Arrange
            var backpressureContext = new BackpressureContext(1, "test", 100);
            backpressureContext.ActivateBackpressure();

            // Act
            backpressureContext.DeactivateBackpressure();

            // Assert
            Assert.IsFalse(backpressureContext.IsBackpressured);
        }
    }
}