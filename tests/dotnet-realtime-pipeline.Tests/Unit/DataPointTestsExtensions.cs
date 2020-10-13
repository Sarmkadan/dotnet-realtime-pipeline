#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetRealtimePipeline.Domain.Models;
using Xunit;

namespace DotNetRealtimePipeline.Tests.Unit;

/// <summary>
/// Extension methods for <see cref="DataPointTests"/> to reduce boilerplate and improve test clarity.
/// </summary>
public static class DataPointTestsExtensions
{
    /// <summary>
    /// Creates a new <see cref="DataPoint"/> instance with the specified ID and validates it.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="id">The ID of the new data point.</param>
    /// <returns>A new <see cref="DataPoint"/> instance with the specified ID.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
    public static DataPoint CreateValidDataPoint(this DataPointTests tests, long id)
    {
        ArgumentNullException.ThrowIfNull(tests);

        return new DataPoint(id, 1_000_000L, 42.5, "sensor-01") { Quality = 85 };
    }

    /// <summary>
    /// Verifies that the <see cref="DataPointTests"/> instance has a valid data point.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="dataPoint">The data point to verify.</param>
    /// <returns>A boolean indicating whether the data point is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> or <paramref name="dataPoint"/> is null.</exception>
    public static bool IsValidDataPoint(this DataPointTests tests, DataPoint dataPoint)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(dataPoint);

        return dataPoint.Validate();
    }
}
