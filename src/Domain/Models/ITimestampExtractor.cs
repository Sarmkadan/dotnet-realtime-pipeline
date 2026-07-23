#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Domain.Models;

using System;

/// <summary>
/// Extracts event time from data points for windowing operations.
/// Implementations determine how event time is derived from data point timestamps.
/// </summary>
public interface ITimestampExtractor
{
    /// <summary>
    /// Extracts the event timestamp from a data point.
    /// </summary>
    /// <param name="dataPoint">The data point to extract timestamp from.</param>
    /// <returns>The event timestamp in milliseconds (Unix timestamp).</returns>
    /// <exception cref="ArgumentNullException">Thrown when dataPoint is null.</exception>
    long ExtractTimestamp(DataPoint dataPoint);
}

/// <summary>
/// Default timestamp extractor that uses the data point's timestamp directly.
/// This is the standard implementation for most use cases where the data point
/// timestamp represents the actual event time.
/// </summary>
public sealed class EventTimeExtractor : ITimestampExtractor
{
    /// <summary>
    /// Singleton instance of the event time extractor.
    /// </summary>
    public static EventTimeExtractor Instance { get; } = new();

    /// <summary>
    /// Extracts the event timestamp from a data point.
    /// </summary>
    /// <param name="dataPoint">The data point to extract timestamp from.</param>
    /// <returns>The event timestamp in milliseconds (Unix timestamp).</returns>
    /// <exception cref="ArgumentNullException">Thrown when dataPoint is null.</exception>
    public long ExtractTimestamp(DataPoint dataPoint)
    {
        ArgumentNullException.ThrowIfNull(dataPoint);
        return dataPoint.Timestamp;
    }
}

/// <summary>
/// Timestamp extractor that allows for custom timestamp extraction logic.
/// Useful when event time needs to be derived from metadata or other data point properties.
/// </summary>
public sealed class CustomTimestampExtractor : ITimestampExtractor
{
    private readonly Func<DataPoint, long> _timestampExtractor;

    /// <summary>
    /// Creates a new custom timestamp extractor.
    /// </summary>
    /// <param name="timestampExtractor">Function that extracts timestamp from a data point.</param>
    /// <exception cref="ArgumentNullException">Thrown when timestampExtractor is null.</exception>
    public CustomTimestampExtractor(Func<DataPoint, long> timestampExtractor)
    {
        ArgumentNullException.ThrowIfNull(timestampExtractor);
        _timestampExtractor = timestampExtractor;
    }

    /// <summary>
    /// Extracts the event timestamp from a data point using the custom function.
    /// </summary>
    /// <param name="dataPoint">The data point to extract timestamp from.</param>
    /// <returns>The event timestamp in milliseconds (Unix timestamp).</returns>
    /// <exception cref="ArgumentNullException">Thrown when dataPoint is null.</exception>
    public long ExtractTimestamp(DataPoint dataPoint)
    {
        ArgumentNullException.ThrowIfNull(dataPoint);
        return _timestampExtractor(dataPoint);
    }
}
