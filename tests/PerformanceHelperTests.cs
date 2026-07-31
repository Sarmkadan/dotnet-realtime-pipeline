using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System;

namespace Tests
{
    public class PerformanceHelperTests
    {
        [Fact]
        public void HappyPath_MeasureExecution()
        {
            // Arrange
            var operation = () => { /* some operation */ };
            var expectedTime = 10;

            // Act
            var (result, time) = PerformanceHelper.MeasureExecution(operation);

            // Assert
            Assert.True(time >= 0 && time =<= expectedTime);
        }

        [Fact]
        public void EdgeCase_MeasureExecution_NullInput()
        {
            // Arrange
            var operation = (Func<object>)null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => PerformanceHelper.MeasureExecution(operation));
        }

        [Fact]
        public void ErrorPath_MeasureExecution_InvalidOperation()
        {
            // Arrange
            var operation = (Func<object>)(() => { throw new Exception(); });

            // Act and Assert
            var ex = Assert.Throws<Exception>(() => PerformanceHelper.MeasureExecution(operation));
            Assert.IsType<Exception>(ex);
        }

        [Fact]
        public async Task HappyPath_MeasureExecutionAsync()
        {
            // Arrange
            var operation = () => { /* some operation */ };
            var expectedTime = 10;

            // Act
            var (result, time) = await PerformanceHelper.MeasureExecutionAsync(operation);

            // Assert
            Assert.True(time >= 0 && time =<= expectedTime);
        }

        [Fact]
        public async Task EdgeCase_MeasureExecutionAsync_NullInput()
        {
            // Arrange
            var operation = (Func<Task<object>>)null;

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => PerformanceHelper.MeasureExecutionAsync(operation));
        }

        [Fact]
        public async Task ErrorPath_MeasureExecutionAsync_InvalidOperation()
        {
            // Arrange
            var operation = (Func<Task<object>>)(() => { throw new Exception(); });

            // Act and Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => PerformanceHelper.MeasureExecutionAsync(operation));
            Assert.IsType<Exception>(ex);
        }

        [Fact]
        public void HappyPath_Benchmark()
        {
            // Arrange
            var operation = () => { /* some operation */ };
            var expectedIterations = 1000;
            var expectedAverageTime = 10.0;

            // Act
            var result = PerformanceHelper.Benchmark(operation, expectedIterations);

            // Assert
            Assert.Equal(expectedIterations, result.Iterations);
            Assert.True(result.AverageMs >= 0 && result.AverageMs =<= expectedAverageTime);
        }

        [Fact]
        public void EdgeCase_Benchmark_NullInput()
        {
            // Arrange
            var operation = (Action)null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => PerformanceHelper.Benchmark(operation));
        }

        [Fact]
        public void EdgeCase_Benchmark_EmptyCollection()
        {
            // Arrange
            var operation = () => { };

            // Act
            var result = PerformanceHelper.Benchmark(operation);

            // Assert
            Assert.Equal(1000, result.Iterations);
            Assert.True(result.AverageMs >= 0);
        }

        [Fact]
        public void EdgeCase_Benchmark_BoundaryValues()
        {
            // Arrange
            var operation = () => { /* some operation */ };
            var expectedIterations = 1000;

            // Act
            var result = PerformanceHelper.Benchmark(operation, expectedIterations);

            // Assert
            Assert.Equal(expectedIterations, result.Iterations);
            Assert.True(result.AverageMs >= 0);
        }

        [Fact]
        public void ErrorPath_Benchmark_InvalidOperation()
        {
            // Arrange
            var operation = (Action)(() => { throw new Exception(); });

            // Act and Assert
            var ex = Assert.Throws<Exception>(() => PerformanceHelper.Benchmark(operation));
            Assert.IsType<Exception>(ex);
        }

        [Fact]
        public void HappyPath_GetMemoryStats()
        {
            // Act
            var stats = PerformanceHelper.GetMemoryStats();

            // Assert
            Assert.NotNull(stats);
            Assert.True(stats.WorkingSetMb >= 0);
            Assert.True(stats.PrivateMemoryMb >= 0);
            Assert.True(stats.PeakWorkingSetMb >= 0);
            Assert.True(stats.GC0Collections >= 0);
            Assert.True(stats.GC1Collections >= 0);
            Assert.True(stats.GC2Collections >= 0);
            Assert.True(stats.TotalMemoryMb >= 0);
        }
    }
}