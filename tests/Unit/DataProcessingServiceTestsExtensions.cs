#nullable enable
// =============================================================================
// Author: [Your Name]
// CTO & Software Architect
// =============================================================================

using DotNetRealtimePipeline.Services;
using DotNetRealtimePipeline.Domain.Models;
using Xunit;

namespace DotNetRealtimePipeline.Tests.Unit;

/// <summary>
/// Extension methods for <see cref="DataProcessingServiceTests"/> to reduce boilerplate and improve test clarity.
/// </summary>
public static class DataProcessingServiceTestsExtensions
{
    /// <summary>
    /// Creates a new valid <see cref="DataPoint"/> instance.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <returns>A new valid <see cref="DataPoint"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
    public static DataPoint CreateValidDataPoint(this DataProcessingServiceTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        return new DataPoint(1, DateTime.UtcNow.Ticks, 10.0, "Sensor1") { Quality = 80 };
    }

    /// <summary>
    /// Creates a new invalid <see cref="DataPoint"/> instance with low quality.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <returns>A new invalid <see cref="DataPoint"/> instance with low quality.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
    public static DataPoint CreateLowQualityDataPoint(this DataProcessingServiceTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        return new DataPoint(1, DateTime.UtcNow.Ticks, 10.0, "Sensor1") { Quality = 30 };
    }

    /// <summary>
    /// Creates a new invalid <see cref="DataPoint"/> instance.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <returns>A new invalid <see cref="DataPoint"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
    public static DataPoint CreateInvalidDataPoint(this DataProcessingServiceTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        return new DataPoint(0, 0, 0, "");
    }
}
