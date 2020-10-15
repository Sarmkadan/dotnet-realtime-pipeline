#nullable enable
// =============================================================================
// Author: [Your Name]
// =============================================================================

using DotNetRealtimePipeline.Data.Repositories;
using DotNetRealtimePipeline.Domain.Models;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xunit;

namespace DotNetRealtimePipeline.Tests.Unit;

/// <summary>
/// Extension methods for <see cref="DataPointRepositoryTests"/> to reduce boilerplate and improve test clarity.
/// </summary>
public static class DataPointRepositoryTestsExtensions
{
    /// <summary>
    /// Creates a list of synthetic <see cref="DataPoint"/> instances with sequential IDs and customizable properties.
    /// </summary>
    /// <param name="tests">The test instance providing access to <see cref="DataPointRepositoryTests"/>.</param>
    /// <param name="count">Number of data points to generate.</param>
    /// <param name="sensorName">Sensor identifier for all generated points.</param>
    /// <param name="value">Value for all generated points.</param>
    /// <returns>A list of <see cref="DataPoint"/> instances.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="count"/> is non-positive.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="sensorName"/> is null or empty.</exception>
    public static IReadOnlyList<DataPoint> GenerateDataPoints(this DataPointRepositoryTests tests, int count, string sensorName, decimal value)
    {
        ArgumentException.ThrowIfNullOrEmpty(sensorName);
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be positive.");

        var now = DateTime.UtcNow.Ticks;
        return Enumerable.Range(0, count)
            .Select(i => new DataPoint(i, now + i, value, sensorName))
            .ToList();
    }

    /// <summary>
    /// Verifies that a data point exists in the repository.
    /// </summary>
    /// <param name="tests">The test instance providing access to <see cref="DataPointRepositoryTests"/>.</param>
    /// <param name="id">The ID of the data point to check.</param>
    /// <returns>A boolean indicating whether the data point exists.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="id"/> is negative.</exception>
    public static async Task<bool> ExistsAsync(this DataPointRepositoryTests tests, long id)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id), "ID must be non-negative.");

        return await tests._repository.GetByIdAsync(id) != null;
    }

    /// <summary>
    /// Asserts that a data point with the specified ID has the expected value.
    /// </summary>
    /// <param name="tests">The test instance providing access to <see cref="DataPointRepositoryTests"/>.</param>
    /// <param name="id">The ID of the data point to check.</param>
    /// <param name="expectedValue">The expected value of the data point.</param>
    public static async Task AssertValueAsync(this DataPointRepositoryTests tests, long id, decimal expectedValue)
    {
        var dataPoint = await tests._repository.GetByIdAsync(id);
        Assert.NotNull(dataPoint);
        Assert.Equal(expectedValue, dataPoint.Value, CultureInfo.InvariantCulture);
    }
}
