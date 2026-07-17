using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace DotNetRealtimePipeline.Services;

/// <summary>
/// Provides JSON serialization extensions for <see cref="WindowingService"/>.
/// </summary>
public static class WindowingServiceJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
		WriteIndented = false,
	};

	/// <summary>
	/// Serializes the <see cref="WindowingService"/> instance to a JSON string.
	/// </summary>
	/// <param name="value">The windowing service instance to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
	/// <returns>A JSON string representation of the windowing service.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
	public static string ToJson(this WindowingService value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);
		return JsonSerializer.Serialize(value, indented ? GetIndentedOptions() : _jsonOptions);
	}

	/// <summary>
	/// Deserializes a JSON string to a <see cref="WindowingService"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>The deserialized windowing service instance, or <see langword="null"/> if the JSON is empty or whitespace.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is <see langword="null"/>.</exception>
	/// <exception cref="JsonException">Thrown if the JSON is invalid or cannot be deserialized to a <see cref="WindowingService"/> instance.</exception>
	public static WindowingService? FromJson(string json)
	{
		ArgumentNullException.ThrowIfNull(json);

		return string.IsNullOrWhiteSpace(json)
			? null
			: JsonSerializer.Deserialize<WindowingService>(json, _jsonOptions);
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to a <see cref="WindowingService"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">Receives the deserialized windowing service instance if successful.</param>
	/// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is <see langword="null"/>.</exception>
	public static bool TryFromJson(string json, out WindowingService? value)
	{
		ArgumentNullException.ThrowIfNull(json);

		value = null;

		if (string.IsNullOrWhiteSpace(json))
		{
			return true;
		}

		try
		{
			value = JsonSerializer.Deserialize<WindowingService>(json, _jsonOptions);
			return true;
		}
		catch (JsonException)
		{
			return false;
		}
	}

	private static JsonSerializerOptions GetIndentedOptions()
	{
		var options = new JsonSerializerOptions(_jsonOptions)
		{
			WriteIndented = true,
		};

		return options;
	}
}
