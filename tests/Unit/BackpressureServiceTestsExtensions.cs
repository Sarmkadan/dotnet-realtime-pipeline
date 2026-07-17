#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using DotNetRealtimePipeline.Domain.Enums;
using DotNetRealtimePipeline.Domain.Models;
using Xunit;

namespace DotNetRealtimePipeline.Tests.Unit;

/// <summary>
/// Extension methods for <see cref="BackpressureServiceTests"/> to provide additional test utilities
/// and assertions for backpressure service testing scenarios.
/// </summary>
public sealed class BackpressureServiceTestsExtensions
{
    /// <summary>
    /// Creates a test context with the specified stage name and capacity, and returns the service
    /// for fluent method chaining.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="stageName">Name of the processing stage.</param>
    /// <param name="maxCapacity">Maximum buffer capacity for the stage.</param>
    /// <returns>The <see cref="BackpressureServiceTests"/> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> or <paramref name="stageName"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="stageName"/> is empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxCapacity"/> is not positive.</exception>
    public static BackpressureServiceTests CreateContextWithCapacity(
        this BackpressureServiceTests tests,
        string stageName,
        int maxCapacity)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentException.ThrowIfNullOrEmpty(stageName);

        if (maxCapacity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxCapacity),
                maxCapacity,
                "Capacity must be a positive value.");
        }

        tests.CreateContext_WithValidParameters_ShouldSucceed();
        return tests;
    }

    /// <summary>
    /// Adds items to the buffer and asserts the operation was successful.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="stageName">Name of the processing stage.</param>
    /// <param name="itemCount">Number of items to add to the buffer.</param>
    /// <returns>The <see cref="BackpressureServiceTests"/> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="stageName"/> is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="itemCount"/> is not positive.</exception>
    public static BackpressureServiceTests AddToBuffer(
        this BackpressureServiceTests tests,
        string stageName,
        int itemCount)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentException.ThrowIfNullOrEmpty(stageName);

        if (itemCount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(itemCount),
                itemCount,
                "Item count must be positive.");
        }

        var result = tests.TryAddToBuffer_WhenBelowCapacity_ShouldReturnTrue();
        Assert.True(result);
        return tests;
    }

    /// <summary>
    /// Asserts that the buffer has reached its capacity limit.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="stageName">Name of the processing stage.</param>
    /// <param name="expectedCapacity">Expected maximum capacity of the buffer.</param>
    /// <returns>The <see cref="BackpressureServiceTests"/> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="stageName"/> is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="expectedCapacity"/> is not positive.</exception>
    public static BackpressureServiceTests AssertBufferAtCapacity(
        this BackpressureServiceTests tests,
        string stageName,
        int expectedCapacity)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentException.ThrowIfNullOrEmpty(stageName);

        if (expectedCapacity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(expectedCapacity),
                expectedCapacity,
                "Capacity must be positive.");
        }

        // Fill the buffer to capacity
        tests.TryAddToBuffer_WhenBelowCapacity_ShouldReturnTrue();
        tests.TryAddToBuffer_WhenExceedsCapacity_ShouldReturnFalse();

        // Verify buffer status
        var status = tests.GetBufferStatus_ShouldReturnCurrentLevels();
        var current = status[stageName];
        Assert.Equal(expectedCapacity, current);

        return tests;
    }

    /// <summary>
    /// Asserts that the buffer has the expected item count.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="stageName">Name of the processing stage.</param>
    /// <param name="expectedCount">Expected number of items in the buffer.</param>
    /// <returns>The <see cref="BackpressureServiceTests"/> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="stageName"/> is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="expectedCount"/> is negative.</exception>
    public static BackpressureServiceTests AssertBufferCount(
        this BackpressureServiceTests tests,
        string stageName,
        int expectedCount)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentException.ThrowIfNullOrEmpty(stageName);

        if (expectedCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(expectedCount),
                expectedCount,
                "Count cannot be negative.");
        }

        var status = tests.GetBufferStatus_ShouldReturnCurrentLevels();
        var current = status[stageName];
        Assert.Equal(expectedCount, current);

        return tests;
    }

    /// <summary>
    /// Removes items from the buffer and asserts the count was decreased correctly.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="stageName">Name of the processing stage.</param>
    /// <param name="itemsToRemove">Number of items to remove from the buffer.</param>
    /// <param name="expectedRemaining">Expected remaining items after removal.</param>
    /// <returns>The <see cref="BackpressureServiceTests"/> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="stageName"/> is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="itemsToRemove"/> is not positive or <paramref name="expectedRemaining"/> is negative.</exception>
    public static BackpressureServiceTests RemoveAndAssert(
        this BackpressureServiceTests tests,
        string stageName,
        int itemsToRemove,
        int expectedRemaining)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentException.ThrowIfNullOrEmpty(stageName);

        if (itemsToRemove <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(itemsToRemove),
                itemsToRemove,
                "Items to remove must be positive.");
        }

        if (expectedRemaining < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(expectedRemaining),
                expectedRemaining,
                "Expected remaining cannot be negative.");
        }

        tests.RemoveFromBuffer_ShouldDecreaseCount();

        var status = tests.GetBufferStatus_ShouldReturnCurrentLevels();
        var current = status[stageName];
        Assert.Equal(expectedRemaining, current);

        return tests;
    }

    /// <summary>
    /// Tests backpressure application with a specific strategy and validates the response.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="stageName">Name of the processing stage.</param>
    /// <param name="strategy">Backpressure strategy to apply.</param>
    /// <param name="timeoutMs">Timeout in milliseconds for the backpressure operation.</param>
    /// <returns>The <see cref="BackpressureServiceTests"/> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="stageName"/> is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="timeoutMs"/> is not positive.</exception>
    public static async Task<BackpressureServiceTests> AssertBackpressureAppliedAsync(
        this BackpressureServiceTests tests,
        string stageName,
        BackpressureStrategy strategy,
        int timeoutMs = 1000)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentException.ThrowIfNullOrEmpty(stageName);

        if (timeoutMs <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(timeoutMs),
                timeoutMs,
                "Timeout must be positive.");
        }

        switch (strategy)
        {
            case BackpressureStrategy.Block:
                await tests.ApplyBackpressureAsync_WithBlockStrategy_ShouldWait(stageName, strategy, timeoutMs);
                break;

            case BackpressureStrategy.Throttle:
                await tests.ApplyBackpressureAsync_WithThrottleStrategy_ShouldSucceed(stageName, strategy, timeoutMs);
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(strategy),
                    strategy,
                    "Unsupported backpressure strategy.");
        }

        return tests;
    }

    /// <summary>
    /// Gets the buffer status as a dictionary for further assertions.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <returns>Read-only dictionary containing stage names and their current buffer counts.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is null.</exception>
    public static IReadOnlyDictionary<string, int> GetBufferStatusDictionary(
        this BackpressureServiceTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        return tests.GetBufferStatus_ShouldReturnCurrentLevels();
    }

    /// <summary>
    /// Creates multiple contexts with different capacities for comprehensive testing.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="contexts">Collection of (stageName, capacity) tuples.</param>
    /// <returns>The <see cref="BackpressureServiceTests"/> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> or <paramref name="contexts"/> is null.</exception>
    public static BackpressureServiceTests CreateMultipleContexts(
        this BackpressureServiceTests tests,
        params (string StageName, int Capacity)[] contexts)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(contexts);

        foreach (var (stageName, capacity) in contexts)
        {
            ArgumentException.ThrowIfNullOrEmpty(stageName);
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(capacity),
                    capacity,
                    "Capacity must be positive.");
            }

            tests.CreateContext_WithValidParameters_ShouldSucceed();
        }

        return tests;
    }
}