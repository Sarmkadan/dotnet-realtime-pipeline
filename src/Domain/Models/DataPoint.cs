// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Domain.Models;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents a single data point in the pipeline stream.
/// Core entity for processing with timestamp, value, and metadata.
/// </summary>
public class DataPoint
{
    public long Id { get; set; }
    public long Timestamp { get; set; }
    public double Value { get; set; }
    public string Source { get; set; } = "";
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public string? Tags { get; set; }
    public int Quality { get; set; } = 100;

    public DataPoint()
    {
    }

    public DataPoint(long id, long timestamp, double value, string source)
    {
        Id = id;
        Timestamp = timestamp;
        Value = value;
        Source = source ?? throw new ArgumentNullException(nameof(source));
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Validates data point integrity and business rules.
    /// </summary>
    public bool Validate()
    {
        if (Id <= 0) return false;
        if (Timestamp <= 0) return false;
        if (string.IsNullOrWhiteSpace(Source)) return false;
        if (Quality < 0 || Quality > 100) return false;
        return true;
    }

    /// <summary>
    /// Returns the age of this data point in milliseconds.
    /// </summary>
    public long GetAgeMs()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - Timestamp;
    }

    /// <summary>
    /// Marks this data point as having metadata.
    /// </summary>
    public void AddMetadata(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key cannot be null", nameof(key));
        Metadata[key] = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Checks if the data point exceeds a quality threshold.
    /// </summary>
    public bool MeetsQualityThreshold(int threshold)
    {
        return Quality >= threshold;
    }

    /// <summary>
    /// Creates a copy of this data point with new ID.
    /// </summary>
    public DataPoint Clone(long newId)
    {
        var clone = new DataPoint(newId, Timestamp, Value, Source);
        clone.Quality = Quality;
        clone.Tags = Tags;
        clone.CreatedAt = CreatedAt;
        clone.Metadata = new Dictionary<string, object>(Metadata);
        return clone;
    }
}
