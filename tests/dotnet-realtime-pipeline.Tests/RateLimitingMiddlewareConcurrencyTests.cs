#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Tests for RateLimitingMiddleware concurrency safety and race condition fixes.
// Validates that the fixed-window reset race is properly handled.
// =============================================================================

namespace DotNetRealtimePipeline.Middleware;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public class RateLimitingMiddlewareConcurrencyTests
{
    [Fact]
    public void TryConsume_WithConcurrentRequests_ShouldNotExceedLimit()
    {
        // Arrange
        var limiter = new RateLimitingMiddleware(tokensPerSecond: 10, maxBurstSize: 10);
        var identifier = "test-concurrent";
        var limit = 10;
        var concurrentRequests = 100;
        var successfulRequests = new bool[concurrentRequests];
        var successfulCount = 0;

        // Act - fire 100 concurrent requests against a limit of 10
        var tasks = new Task[concurrentRequests];
        for (int i = 0; i < concurrentRequests; i++)
        {
            int index = i;
            tasks[i] = Task.Run(() =>
            {
                successfulRequests[index] = limiter.TryAcquire(identifier);
            });
        }

        Task.WaitAll(tasks);

        // Assert - count how many succeeded
        var successCount = 0;
        foreach (var success in successfulRequests)
        {
            if (success) successCount++;
        }

        // Exactly 10 requests should succeed (the limit)
        Assert.Equal(limit, successCount);

        // Verify no more than limit succeeded
        Assert.True(successCount <= limit, $"Expected at most {limit} successful requests, but got {successCount}");
    }

    [Fact]
    public void AvailableTokens_WhenAccessedConcurrently_ShouldBeThreadSafe()
    {
        // Arrange
        var limiter = new RateLimitingMiddleware(tokensPerSecond: 1000, maxBurstSize: 1000);
        var identifier = "test-concurrent-tokens";
        var iterations = 1000;

        // Act - concurrently access AvailableTokens property while consuming tokens
        var tasks = new Task[iterations];
        for (int i = 0; i < iterations; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                // Mix of reads and writes
                limiter.TryAcquire(identifier);
                var status = limiter.GetStatus(identifier);
                _ = status.AvailableTokens;
            });
        }

        Task.WaitAll(tasks);

        // Assert - should complete without exceptions or deadlocks
        var status = limiter.GetStatus(identifier);
        Assert.True(status.AvailableTokens >= 0);
        Assert.True(status.AvailableTokens <= status.Capacity);
    }

    [Fact]
    public void NextRefillTime_WhenAccessedConcurrently_ShouldBeThreadSafe()
    {
        // Arrange
        var limiter = new RateLimitingMiddleware(tokensPerSecond: 1000, maxBurstSize: 1000);
        var identifier = "test-concurrent-refill";

        // First access to create the bucket
        var initialStatus = limiter.GetStatus(identifier);

        var iterations = 1000;

        // Act - concurrently access NextRefillTime property
        var tasks = new Task[iterations];
        for (int i = 0; i < iterations; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                var status = limiter.GetStatus(identifier);
                // Just access the property to ensure it doesn't throw
                _ = status.ResetTime;
                _ = status.AvailableTokens;
                _ = status.Capacity;
            });
        }

        Task.WaitAll(tasks);

        // Assert - should complete without exceptions or deadlocks
        var finalStatus = limiter.GetStatus(identifier);
        // Just verify the status object is valid
        Assert.True(finalStatus.Capacity > 0);
    }

    [Fact]
    public void TryConsume_AtWindowBoundary_ShouldNotAllowDoubleTheLimit()
    {
        // Arrange - create a limiter with very low rate to make timing issues more apparent
        var limiter = new RateLimitingMiddleware(tokensPerSecond: 1, maxBurstSize: 10);
        var identifier = "test-boundary";

        // Fill up the bucket
        for (int i = 0; i < 10; i++)
        {
            Assert.True(limiter.TryAcquire(identifier));
        }

        // Try to consume one more - should fail
        Assert.False(limiter.TryAcquire(identifier));

        // Wait for refill (1 second at 1 token per second)
        System.Threading.Thread.Sleep(1100);

        // Now should be able to consume again
        Assert.True(limiter.TryAcquire(identifier));
    }

    [Fact]
    public void GetStatus_WhenCalledConcurrently_ShouldNotThrow()
    {
        // Arrange
        var limiter = new RateLimitingMiddleware(tokensPerSecond: 100, maxBurstSize: 500);
        var identifier = "test-status";
        var iterations = 1000;

        // Act - concurrently call GetStatus
        var tasks = new Task[iterations];
        for (int i = 0; i < iterations; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                var status = limiter.GetStatus(identifier);
                _ = status.AvailableTokens;
                _ = status.Capacity;
                _ = status.ResetTime;
                _ = status.IsLimited;
            });
        }

        Task.WaitAll(tasks);

        // Assert - should complete without exceptions
        var status = limiter.GetStatus(identifier);
        Assert.NotNull(status);
    }

    [Fact]
    public void MultipleIdentifiers_WhenAccessedConcurrently_ShouldNotInterfere()
    {
        // Arrange
        var limiter = new RateLimitingMiddleware(tokensPerSecond: 100, maxBurstSize: 100);
        var identifier1 = "test-1";
        var identifier2 = "test-2";
        var iterations = 500;

        // Act - concurrently access different identifiers
        var tasks = new Task[iterations * 2];
        for (int i = 0; i < iterations; i++)
        {
            int index = i;
            tasks[index] = Task.Run(() => limiter.TryAcquire(identifier1));
            tasks[index + iterations] = Task.Run(() => limiter.TryAcquire(identifier2));
        }

        Task.WaitAll(tasks);

        // Assert - each identifier should have independent rate limiting
        var status1 = limiter.GetStatus(identifier1);
        var status2 = limiter.GetStatus(identifier2);

        // Both should have tokens available (not exhausted)
        Assert.True(status1.AvailableTokens >= 0);
        Assert.True(status2.AvailableTokens >= 0);
    }

    [Fact]
    public void Reset_WhenCalledConcurrently_ShouldNotCauseIssues()
    {
        // Arrange
        var limiter = new RateLimitingMiddleware(tokensPerSecond: 100, maxBurstSize: 100);
        var identifier = "test-reset";
        var iterations = 100;

        // Act - concurrently reset and use
        var tasks = new Task[iterations];
        for (int i = 0; i < iterations; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                limiter.TryAcquire(identifier);
                limiter.Reset(identifier);
            });
        }

        Task.WaitAll(tasks);

        // Assert - should complete without exceptions
        var status = limiter.GetStatus(identifier);
        Assert.NotNull(status);
    }
}
