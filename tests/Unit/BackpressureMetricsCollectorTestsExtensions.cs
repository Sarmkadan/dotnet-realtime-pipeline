#nullable enable
// =============================================================================
// Author: 
// =============================================================================

using DotNetRealtimePipeline.Metrics;
using Xunit;

namespace DotNetRealtimePipeline.Tests.Unit;

/// <summary>
/// Extension methods for <see cref="BackpressureMetricsCollectorTests"/> to reduce boilerplate and improve test clarity.
/// </summary>
public static class BackpressureMetricsCollectorTestsExtensions
{
    /// <summary>
    /// Creates a new <see cref="BackpressureMetricsCollector"/> instance with the specified parameters.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="maxEventHistory">The maximum event history.</param>
    /// <returns>A new <see cref="BackpressureMetricsCollector"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
    public static BackpressureMetricsCollector NewCollector(this BackpressureMetricsCollectorTests tests, int maxEventHistory)
    {
        ArgumentNullException.ThrowIfNull(tests);
        return new BackpressureMetricsCollector(NewService(), maxEventHistory);
    }

    /// <summary>
    /// Creates a new <see cref="BackpressureService"/> instance.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <returns>A new <see cref="BackpressureService"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
    public static BackpressureService NewService(this BackpressureMetricsCollectorTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);
        return new BackpressureService();
    }

    /// <summary>
    /// Records a manual backpressure event with the specified parameters.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="collector">The collector instance.</param>
    /// <param name="stageName">The stage name.</param>
    /// <param name="bufferFillPercent">The buffer fill percent.</param>
    /// <param name="droppedItems">The number of dropped items.</param>
    /// <param name="isActivation">Whether it's an activation event.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> or <paramref name="collector"/> is null.</exception>
    public static void RecordManualEvent(this BackpressureMetricsCollectorTests tests, BackpressureMetricsCollector collector, string stageName, double bufferFillPercent, long droppedItems, bool isActivation)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(collector);

        collector.RecordManualEvent(stageName, bufferFillPercent, droppedItems, isActivation);
    }
}
