#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetRealtimePipeline.Tests.Unit;

/// <summary>
/// Provides validation helpers for <see cref="WindowingServiceTests"/> instances.
/// </summary>
public static class WindowingServiceTestsValidation
{
    /// <summary>
    /// Validates the specified <see cref="WindowingServiceTests"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this WindowingServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate test methods exist using reflection
        var methods = typeof(WindowingServiceTests).GetMethods();
        var methodNames = methods.Select(m => m.Name).ToHashSet();

        if (!methodNames.Contains(nameof(WindowingServiceTests.CreateWindow_WithValidTime_ShouldSucceed)))
        {
            problems.Add($"Test method '{nameof(WindowingServiceTests.CreateWindow_WithValidTime_ShouldSucceed)}' is missing.");
        }

        if (!methodNames.Contains(nameof(WindowingServiceTests.AssignDataPointsToWindows_WithValidPoints_ShouldAssign)))
        {
            problems.Add($"Test method '{nameof(WindowingServiceTests.AssignDataPointsToWindows_WithValidPoints_ShouldAssign)}' is missing.");
        }

        if (!methodNames.Contains(nameof(WindowingServiceTests.CalculateWindowStatistics_WithValidWindow_ShouldCalculate)))
        {
            problems.Add($"Test method '{nameof(WindowingServiceTests.CalculateWindowStatistics_WithValidWindow_ShouldCalculate)}' is missing.");
        }

        if (!methodNames.Contains(nameof(WindowingServiceTests.GetActiveWindows_ShouldReturnCurrent)))
        {
            problems.Add($"Test method '{nameof(WindowingServiceTests.GetActiveWindows_ShouldReturnCurrent)}' is missing.");
        }

        if (!methodNames.Contains(nameof(WindowingServiceTests.CalculateWindowStatistics_ShouldComputeStandardDeviation)))
        {
            problems.Add($"Test method '{nameof(WindowingServiceTests.CalculateWindowStatistics_ShouldComputeStandardDeviation)}' is missing.");
        }

        if (!methodNames.Contains(nameof(WindowingServiceTests.CalculateWindowStatistics_ShouldComputePercentiles)))
        {
            problems.Add($"Test method '{nameof(WindowingServiceTests.CalculateWindowStatistics_ShouldComputePercentiles)}' is missing.");
        }

        if (!methodNames.Contains(nameof(WindowingServiceTests.CloseWindow_WithActiveWindow_ShouldArchive)))
        {
            problems.Add($"Test method '{nameof(WindowingServiceTests.CloseWindow_WithActiveWindow_ShouldArchive)}' is missing.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="WindowingServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this WindowingServiceTests value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="WindowingServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing the validation problems.</exception>
    public static void EnsureValid(this WindowingServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                message: $"WindowingServiceTests instance is invalid. Problems: {string.Join(", ", problems)}",
                paramName: nameof(value));
        }
    }
}