#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetRealtimePipeline.Domain.Models;
using Xunit;

namespace DotNetRealtimePipeline.Tests.Unit;

/// <summary>
/// Extension methods for <see cref="WindowingServiceTests"/> to reduce boilerplate and improve test clarity.
/// </summary>
public static class WindowingServiceTestsExtensions
{
    /// <summary>
    /// Creates a new <see cref="WindowEvent"/> instance with the specified ID and validates it.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="id">The ID of the new window event.</param>
    /// <returns>A new <see cref="WindowEvent"/> instance with the specified ID.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
    public static WindowEvent CreateValidWindowEvent(this WindowingServiceTests tests, long id)
    {
        ArgumentNullException.ThrowIfNull(tests);

        return new WindowEvent
        {
            StartTimeMs = 1000,
            EndTimeMs = 6000,
            Data = new List<DataPoint>
            {
                new DataPoint(1, 2000, 10, "S1"),
                new DataPoint(2, 3000, 20, "S1"),
                new DataPoint(3, 4000, 30, "S1")
            }
        };
    }

    /// <summary>
    /// Verifies that the <see cref="WindowingServiceTests"/> instance has a valid window.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="window">The window to verify.</param>
    /// <returns>A boolean indicating whether the window is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> or <paramref name="window"/> is null.</exception>
    public static bool IsValidWindow(this WindowingServiceTests tests, WindowEvent window)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(window);

        return window.Validate();
    }
}
