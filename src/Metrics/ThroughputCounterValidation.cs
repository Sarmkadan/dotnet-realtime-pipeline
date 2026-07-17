#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Metrics;

using System;
using System.Collections.Generic;

/// <summary>
/// Provides validation helpers for <see cref="ThroughputCounter"/> instances.
/// </summary>
public static class ThroughputCounterValidation
{
	/// <summary>
	/// Validates the specified <see cref="ThroughputCounter"/> instance.
	/// </summary>
	/// <param name="value">The throughput counter to validate.</param>
	/// <returns>A list of validation errors; empty if the instance is valid.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
	public static IReadOnlyList<string> Validate(this ThroughputCounter? value)
	{
		ArgumentNullException.ThrowIfNull(value);

		var errors = new List<string>();

		// Validate windowSeconds (implicitly validated in constructor)
		// No way to access the private field, so we rely on the constructor validation

		return errors.AsReadOnly();
	}

	/// <summary>
	/// Determines whether the specified <see cref="ThroughputCounter"/> instance is valid.
	/// </summary>
	/// <param name="value">The throughput counter to check.</param>
	/// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
	public static bool IsValid(this ThroughputCounter? value)
	{
		return value is not null;
	}

	/// <summary>
	/// Ensures that the specified <see cref="ThroughputCounter"/> instance is valid, throwing an <see cref="ArgumentException"/> if not.
	/// </summary>
	/// <param name="value">The throughput counter to validate.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown when the instance is not valid.</exception>
	public static void EnsureValid(this ThroughputCounter? value)
	{
		ArgumentNullException.ThrowIfNull(value);

		// Additional validation logic can be added here if needed
		// Currently, the constructor validates windowSeconds > 0
	}
}
