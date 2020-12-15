#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetRealtimePipeline.Tests.Unit;

/// <summary>
/// Provides validation helpers for <see cref="DataPointTests"/> instances.
/// </summary>
public static class DataPointTestsValidation
{
    /// <summary>
    /// Validates the specified <see cref="DataPointTests"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this DataPointTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate test methods exist using reflection
        var methods = typeof(DataPointTests).GetMethods();
        var methodNames = methods.Select(m => m.Name).ToHashSet();

        if (!methodNames.Contains(nameof(DataPointTests.Validate_WithAllValidProperties_ReturnsTrue)))
        {
            problems.Add($"Test method '{nameof(DataPointTests.Validate_WithAllValidProperties_ReturnsTrue)}' is missing.");
        }

        if (!methodNames.Contains(nameof(DataPointTests.Validate_WithZeroId_ReturnsFalse)))
        {
            problems.Add($"Test method '{nameof(DataPointTests.Validate_WithZeroId_ReturnsFalse)}' is missing.");
        }

        if (!methodNames.Contains(nameof(DataPointTests.Validate_WithEmptySource_ReturnsFalse)))
        {
            problems.Add($"Test method '{nameof(DataPointTests.Validate_WithEmptySource_ReturnsFalse)}' is missing.");
        }

        if (!methodNames.Contains(nameof(DataPointTests.Validate_WithQualityAboveUpperBound_ReturnsFalse)))
        {
            problems.Add($"Test method '{nameof(DataPointTests.Validate_WithQualityAboveUpperBound_ReturnsFalse)}' is missing.");
        }

        if (!methodNames.Contains(nameof(DataPointTests.MeetsQualityThreshold_WhenQualityEqualsThreshold_ReturnsTrue)))
        {
            problems.Add($"Test method '{nameof(DataPointTests.MeetsQualityThreshold_WhenQualityEqualsThreshold_ReturnsTrue)}' is missing.");
        }

        if (!methodNames.Contains(nameof(DataPointTests.MeetsQualityThreshold_WhenQualityBelowThreshold_ReturnsFalse)))
        {
            problems.Add($"Test method '{nameof(DataPointTests.MeetsQualityThreshold_WhenQualityBelowThreshold_ReturnsFalse)}' is missing.");
        }

        if (!methodNames.Contains(nameof(DataPointTests.Clone_WithNewId_PreservesValueSourceAndQuality)))
        {
            problems.Add($"Test method '{nameof(DataPointTests.Clone_WithNewId_PreservesValueSourceAndQuality)}' is missing.");
        }

        if (!methodNames.Contains(nameof(DataPointTests.AddMetadata_WithValidKeyAndValue_StoresEntry)))
        {
            problems.Add($"Test method '{nameof(DataPointTests.AddMetadata_WithValidKeyAndValue_StoresEntry)}' is missing.");
        }

        if (!methodNames.Contains(nameof(DataPointTests.AddMetadata_OverwritesExistingKeyWithNewValue)))
        {
            problems.Add($"Test method '{nameof(DataPointTests.AddMetadata_OverwritesExistingKeyWithNewValue)}' is missing.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="DataPointTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this DataPointTests value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="DataPointTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing the validation problems.</exception>
    public static void EnsureValid(this DataPointTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                message: $"DataPointTests instance is invalid. Problems: {string.Join(", ", problems)}",
                paramName: nameof(value));
        }
    }
}
