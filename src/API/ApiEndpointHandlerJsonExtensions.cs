#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotNetRealtimePipeline.API;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="ApiEndpointHandler"/> types.
/// </summary>
/// <remarks>
/// This static class provides JSON serialization/deserialization utilities for
/// <see cref="ApiEndpointHandler"/> and its derived types.
/// </remarks>
public static class ApiEndpointHandlerJsonExtensions
{
	/// <summary>
	/// Maximum allowed JSON depth to prevent stack overflow from deeply nested structures.
	/// </summary>
	private const int MaxJsonDepth = 64;

	/// <summary>
	/// Maximum allowed JSON payload size in bytes to prevent memory exhaustion attacks.
	/// </summary>
	private const int MaxJsonPayloadSizeBytes = 10_000_000; // 10 MB

	/// <summary>
	/// Maximum allowed array/list size to prevent excessive memory allocation.
	/// </summary>
	private const int MaxArraySize = 100_000;

	/// <summary>
	/// Maximum allowed dictionary size to prevent excessive memory allocation.
	/// </summary>
	private const int MaxDictionarySize = 10_000;

	/// <summary>
	/// Maximum allowed string length to prevent memory exhaustion.
	/// </summary>
	private const int MaxStringLength = 100_000;

	private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		PropertyNameCaseInsensitive = true,
		MaxDepth = MaxJsonDepth,
		Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
	};

	/// <summary>
	/// Serializes the <see cref="ApiEndpointHandler"/> instance to a JSON string.
	/// </summary>
	/// <param name="value">The handler instance to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
	/// <returns>A JSON string representation of the handler.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
	public static string ToJson(this ApiEndpointHandler value, bool indented = false)
		=> JsonSerializer.Serialize(value, indented ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true } : _jsonOptions);

	/// <summary>
	/// Deserializes a JSON string to an <see cref="ApiEndpointHandler"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>The deserialized handler instance, or null if the JSON is empty.</returns>
	/// <exception cref="ArgumentException"><paramref name="json"/> is null or empty.</exception>
	/// <exception cref="JsonException">Thrown when the JSON is invalid, exceeds size limits, or cannot be deserialized.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when the JSON payload exceeds size limits.</exception>
	public static ApiEndpointHandler? FromJson(string json)
	{
		ArgumentException.ThrowIfNullOrEmpty(json, nameof(json));

		// Security: Validate JSON payload size before deserialization to prevent memory exhaustion
		if (json.Length > MaxJsonPayloadSizeBytes)
		{
			throw new ArgumentOutOfRangeException(
				nameof(json),
				$"JSON payload size ({json.Length} bytes) exceeds maximum allowed size ({MaxJsonPayloadSizeBytes} bytes).");
		}

		return JsonSerializer.Deserialize<ApiEndpointHandler>(json, _jsonOptions);
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to an <see cref="ApiEndpointHandler"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">Receives the deserialized handler instance if successful.</param>
	/// <returns>True if deserialization succeeded; otherwise, false.</returns>
	/// <exception cref="ArgumentException"><paramref name="json"/> is null or empty.</exception>
	public static bool TryFromJson(string json, out ApiEndpointHandler? value)
	{
		ArgumentException.ThrowIfNullOrEmpty(json, nameof(json));

		try
		{
			// Security: Validate JSON payload size before deserialization
			if (json.Length > MaxJsonPayloadSizeBytes)
			{
				value = null;
				return false;
			}

			value = JsonSerializer.Deserialize<ApiEndpointHandler>(json, _jsonOptions);
			return true;
		}
		catch (JsonException)
		{
			value = null;
			return false;
		}
	}
}