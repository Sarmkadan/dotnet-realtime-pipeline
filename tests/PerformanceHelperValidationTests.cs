using Xunit;
using System.Collections.Generic;
using System;
using DotNetRealtimePipeline.Utilities;

namespace DotNetRealtimePipeline.Tests.Utilities
{
    public class PerformanceHelperValidationTests
    {
        [Fact]
        public void Validate_BenchmarkResult_HappyPath_ReturnsEmptyList()
        {
            var result = new BenchmarkResult
            {
                Iterations = 1,
                Measurements = new List<long> { 10, 20 },
                AverageMs = 15.0,
                MinMs = 10,
                MaxMs = 20,
                MedianMs = 15.0,
                P95Ms = 18.0,
                P99Ms = 19.0
            };
            var errors = result.Validate();
            Assert.Empty(errors);
        }

        [Fact]
        public void Validate_BenchmarkResult_InvalidCases_ReturnsErrors()
        {
            var result = new BenchmarkResult
            {
                Iterations = 0,
                Measurements = new List<long> { -1 },
                AverageMs = -1.0,
                MinMs = -1,
                MaxMs = -1,
                MedianMs = -1.0,
                P95Ms = -1.0,
                P99Ms = -1.0
            };
            var errors = result.Validate();
            Assert.NotEmpty(errors);
            Assert.True(errors.Count >= 8);
        }

        [Fact]
        public void Validate_MemoryStats_HappyPath_ReturnsEmptyList()
        {
            var stats = new MemoryStats
            {
                WorkingSetMb = 100.0,
                PrivateMemoryMb = 100.0,
                PeakWorkingSetMb = 150.0,
                GC0Collections = 1,
                GC1Collections = 1,
                GC2Collections = 1,
                TotalMemoryMb = 200.0
            };
            var errors = stats.Validate();
            Assert.Empty(errors);
        }

        [Fact]
        public void Validate_MemoryStats_InvalidCases_ReturnsErrors()
        {
            var stats = new MemoryStats
            {
                WorkingSetMb = -1.0,
                PrivateMemoryMb = -1.0,
                PeakWorkingSetMb = -1.0,
                GC0Collections = -1,
                GC1Collections = -1,
                GC2Collections = -1,
                TotalMemoryMb = -1.0
            };
            var errors = stats.Validate();
            Assert.NotEmpty(errors);
            Assert.Equal(7, errors.Count);
        }

        [Fact]
        public void Validate_ExecutionResult_HappyPath_ReturnsEmptyList()
        {
            var executionResult = (Result: "Success", ElapsedMs: 100L);
            var errors = executionResult.Validate();
            Assert.Empty(errors);
        }

        [Fact]
        public void Validate_ExecutionResult_InvalidElapsedMs_ReturnsErrors()
        {
            var executionResult = (Result: "Fail", ElapsedMs: -100L);
            var errors = executionResult.Validate();
            Assert.Single(errors);
        }

        [Fact]
        public void IsValid_BenchmarkResult_ReturnsFalseForInvalidResult()
        {
            var result = new BenchmarkResult { Iterations = 0 };
            Assert.False(result.IsValid());
        }

        [Fact]
        public void EnsureValid_BenchmarkResult_ThrowsExceptionForInvalidResult()
        {
            var result = new BenchmarkResult { Iterations = 0 };
            Assert.Throws<ArgumentException>(() => result.EnsureValid());
        }
    }
}
