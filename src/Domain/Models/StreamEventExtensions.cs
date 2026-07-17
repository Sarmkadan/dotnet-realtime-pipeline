#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotNetRealtimePipeline.Domain.Models;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;

/// <summary>
/// Provides extension methods for <see cref="StreamEvent"/> to enhance its functionality
/// with common stream processing operations.
/// </summary>
public static class StreamEventExtensions
{
	/// <summary>
	/// Filters the payload dictionary to only include entries matching the specified keys.
	/// </summary>
	/// <param name="streamEvent">The stream event to filter.</param>
	/// <param name="keys">The keys to include in the filtered result.</param>
	/// <returns>A new dictionary containing only the specified keys and their values.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="streamEvent"/> or <paramref name="keys"/> is null.</exception>
	public static Dictionary<string, object> FilterPayload(this StreamEvent streamEvent, IEnumerable<string> keys)
	{
		ArgumentNullException.ThrowIfNull(streamEvent);
		ArgumentNullException.ThrowIfNull(keys);

		var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
		foreach (var key in keys)
		{
			if (streamEvent.Payload.TryGetValue(key, out var value))
			{
				result[key] = value;
			}
		}

		return result;
	}

	/// <summary>
	/// Gets the payload value as a specific type, with a default value if the key doesn't exist.
	/// </summary>
	/// <typeparam name="T">The type to cast the payload value to.</typeparam>
	/// <param name="streamEvent">The stream event.</param>
	/// <param name="key">The payload key.</param>
	/// <param name="defaultValue">The default value to return if the key doesn't exist.</param>
	/// <returns>The payload value as type T, or the default value if the key doesn't exist.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="streamEvent"/> is null.</exception>
	public static T? GetPayload<T>(this StreamEvent streamEvent, string key, T? defaultValue = default)
	{
		ArgumentNullException.ThrowIfNull(streamEvent);
		ArgumentException.ThrowIfNullOrEmpty(key);

		if (streamEvent.Payload.TryGetValue(key, out var value) && value is T typedValue)
		{
			return typedValue;
		}

		return defaultValue;
	}

	/// <summary>
	/// Determines whether the event has been processed by any of the specified stages.
	/// </summary>
	/// <param name="streamEvent">The stream event.</param>
	/// <param name="stages">The stages to check against.</param>
	/// <returns>True if the event has been processed by any of the specified stages; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="streamEvent"/> or <paramref name="stages"/> is null.</exception>
	public static bool HasBeenProcessedByAnyStage(this StreamEvent streamEvent, IEnumerable<string> stages)
	{
		ArgumentNullException.ThrowIfNull(streamEvent);
		ArgumentNullException.ThrowIfNull(stages);

		return streamEvent.ProcessedByStages.Count > 0
			&& stages.Any(stage => streamEvent.ProcessedByStages.Contains(stage, StringComparer.Ordinal));
	}

	/// <summary>
	/// Gets the number of remaining stages the event needs to be processed by.
	/// </summary>
	/// <param name="streamEvent">The stream event.</param>
	/// <param name="allStages">The complete list of all possible stages.</param>
	/// <returns>The count of stages the event hasn't been processed by yet.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="streamEvent"/> or <paramref name="allStages"/> is null.</exception>
	public static int GetRemainingStagesCount(this StreamEvent streamEvent, IReadOnlyList<string> allStages)
	{
		ArgumentNullException.ThrowIfNull(streamEvent);
		ArgumentNullException.ThrowIfNull(allStages);

		if (allStages.Count == 0)
		{
			return 0;
		}

		var processedSet = new HashSet<string>(streamEvent.ProcessedByStages, StringComparer.Ordinal);
		var remainingCount = 0;

		foreach (var stage in allStages)
		{
			if (!processedSet.Contains(stage))
			{
				remainingCount++;
			}
		}

		return remainingCount;
	}

	/// <summary>
	/// Creates a deep copy of the stream event with the same properties.
	/// </summary>
	/// <param name="streamEvent">The stream event to copy.</param>
	/// <returns>A new StreamEvent instance with identical property values.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="streamEvent"/> is null.</exception>
	public static StreamEvent DeepCopy(this StreamEvent streamEvent)
	{
		ArgumentNullException.ThrowIfNull(streamEvent);

		var copy = new StreamEvent
		{
			EventId = streamEvent.EventId,
			DataPointId = streamEvent.DataPointId,
			Timestamp = streamEvent.Timestamp,
			EventType = streamEvent.EventType,
			Priority = streamEvent.Priority,
			SourceSystem = streamEvent.SourceSystem,
			CorrelationId = streamEvent.CorrelationId,
			CausationId = streamEvent.CausationId,
			IsRetry = streamEvent.IsRetry,
			RetryAttempt = streamEvent.RetryAttempt,
			LastErrorMessage = streamEvent.LastErrorMessage,
			CreatedAt = streamEvent.CreatedAt,
			CompletedAt = streamEvent.CompletedAt
		};

		// Deep copy collections
		copy.Payload = new Dictionary<string, object>(streamEvent.Payload, StringComparer.Ordinal);
		copy.ProcessedByStages = new List<string>(streamEvent.ProcessedByStages);

		return copy;
	}

	/// <summary>
	/// Determines whether the event is considered stale based on its age.
	/// </summary>
	/// <param name="streamEvent">The stream event.</param>
	/// <param name="maxAgeMs">The maximum allowed age in milliseconds before the event is considered stale.</param>
	/// <returns>True if the event age exceeds the maximum allowed age; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="streamEvent"/> is null.</exception>
	public static bool IsStale(this StreamEvent streamEvent, long maxAgeMs)
	{
		ArgumentNullException.ThrowIfNull(streamEvent);

		var ageMs = streamEvent.GetAgeMs();
		return ageMs > maxAgeMs;
	}

	/// <summary>
	/// Gets the priority level as a formatted string.
	/// </summary>
	/// <param name="streamEvent">The stream event.</param>
	/// <returns>A string representation of the priority level.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="streamEvent"/> is null.</exception>
	public static string GetPriorityString(this StreamEvent streamEvent)
	{
		ArgumentNullException.ThrowIfNull(streamEvent);

		return streamEvent.Priority switch
		{
			1 => "Critical",
			2 => "High",
			3 => "Medium-High",
			4 => "Medium",
			5 => "Normal",
			6 => "Medium-Low",
			7 => "Low",
			8 => "Very Low",
			9 or 10 => "Lowest",
			_ => $"Priority {streamEvent.Priority}"
		};
	}

	/// <summary>
	/// Gets the payload value as a JSON-serializable string.
	/// </summary>
	/// <param name="streamEvent">The stream event.</param>
	/// <param name="key">The payload key.</param>
	/// <returns>A JSON-formatted string representation of the payload value, or null if the key doesn't exist.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="streamEvent"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null or empty.</exception>
	public static string? GetPayloadAsJson(this StreamEvent streamEvent, string key)
	{
		ArgumentNullException.ThrowIfNull(streamEvent);
		ArgumentException.ThrowIfNullOrEmpty(key);

		if (streamEvent.Payload.TryGetValue(key, out var value))
		{
			return JsonSerializer.Serialize(value);
		}

		return null;
	}

	/// <summary>
	/// Determines whether the event has failed processing based on the presence of an error message.
	/// </summary>
	/// <param name="streamEvent">The stream event.</param>
	/// <returns>True if the event has a LastErrorMessage set; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="streamEvent"/> is null.</exception>
	public static bool HasFailed(this StreamEvent streamEvent)
	{
		ArgumentNullException.ThrowIfNull(streamEvent);
		return !string.IsNullOrWhiteSpace(streamEvent.LastErrorMessage);
	}

	/// <summary>
	/// Gets the processing completion percentage based on the number of processed stages.
	/// </summary>
	/// <param name="streamEvent">The stream event.</param>
	/// <param name="totalStages">The total number of stages in the pipeline.</param>
	/// <returns>A value between 0 and 100 representing the completion percentage.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="streamEvent"/> is null.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="totalStages"/> is less than or equal to 0.</exception>
	public static int GetProcessingCompletionPercentage(this StreamEvent streamEvent, int totalStages)
	{
		ArgumentNullException.ThrowIfNull(streamEvent);
		ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(totalStages, 0);

		if (streamEvent.ProcessedByStages.Count == 0)
		{
			return 0;
		}

		var percentage = (int)Math.Round((double)streamEvent.ProcessedByStages.Count / totalStages * 100);
		return Math.Clamp(percentage, 0, 100);
	}
}