#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Utilities;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Extension methods for RetryHelper.
/// </summary>
public static class RetryHelperExtensions
{
	private const int DefaultMaxAttempts = 3;
	private const int DefaultInitialDelayMs = 100;

	/// <summary>
	/// Executes an operation with retry logic and returns the retry statistics.
	/// </summary>
	/// <typeparam name="T">The type of the result.</typeparam>
	/// <param name="operation">The operation to execute.</param>
	/// <param name="maxAttempts">The maximum number of attempts.</param>
	/// <param name="initialDelayMs">The initial delay in milliseconds.</param>
	/// <returns>The result of the operation and the retry statistics.</returns>
	public static async Task<(T result, RetryStatistics statistics)> RetryWithStatisticsAsync<T>(
		Func<Task<T>> operation,
		int maxAttempts = DefaultMaxAttempts,
		int initialDelayMs = DefaultInitialDelayMs)
	{
		ArgumentNullException.ThrowIfNull(operation);

		var statistics = new RetryStatistics();
		var policy = new RetryPolicy
		{
			MaxAttempts = maxAttempts,
			InitialDelayMs = initialDelayMs
		};

		var result = await policy.ExecuteAsync(operation);
		return (result, statistics);
	}

	/// <summary>
	/// Executes an operation with retry logic and returns the retry statistics.
	/// </summary>
	/// <typeparam name="T">The type of the result.</typeparam>
	/// <param name="operation">The operation to execute.</param>
	/// <param name="maxAttempts">The maximum number of attempts.</param>
	/// <param name="initialDelayMs">The initial delay in milliseconds.</param>
	/// <returns>The result of the operation and the retry statistics.</returns>
	public static async Task<(T result, RetryStatistics statistics)> RetryWithStatisticsAsync<T>(
		Func<T> operation,
		int maxAttempts = DefaultMaxAttempts,
		int initialDelayMs = DefaultInitialDelayMs)
	{
		ArgumentNullException.ThrowIfNull(operation);

		var statistics = new RetryStatistics();
		var policy = new RetryPolicy
		{
			MaxAttempts = maxAttempts,
			InitialDelayMs = initialDelayMs
		};

		var result = await policy.ExecuteAsync(() => Task.FromResult(operation()));
		return (result, statistics);
	}

	/// <summary>
	/// Maps retry events to a list of tuples containing timestamp and delay.
	/// </summary>
	/// <param name="statistics">The retry statistics.</param>
	/// <returns>A list of tuples containing timestamp and delay.</returns>
	public static IReadOnlyList<(DateTime timestamp, int delayMs)> GetRetryEvents(
		this RetryStatistics statistics)
	{
		ArgumentNullException.ThrowIfNull(statistics);

		return statistics.Events.Select(e => (e.Timestamp, e.DelayMs)).ToList();
	}
}
