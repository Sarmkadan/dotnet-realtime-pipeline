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
/// Provides validation helpers for <see cref="DataPointTests"/> test class integrity.
/// Validates that all expected test methods are present in the <see cref="DataPointTests"/> class.
/// </summary>
public static class DataPointTestsValidation
{
    /// <summary>
    /// Validates the specified <see cref="DataPointTests"/> test class.
    /// Checks that all expected test methods are present using reflection.
    /// </summary>
    /// <param name="value">The test class instance to validate.</param>
    /// <returns>A list of validation problems; empty if all expected test methods are present.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this DataPointTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate test methods exist using reflection
        var methods = typeof(DataPointTests).GetMethods(
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.DeclaredOnly);
        var methodNames = methods.Select(m => m.Name).ToHashSet(StringComparer.Ordinal);

        ValidateExpectedMethod(nameof(DataPointTests.Validate_WithAllValidProperties_ReturnsTrue), methodNames, problems);
        ValidateExpectedMethod(nameof(DataPointTests.Validate_WithZeroId_ReturnsFalse), methodNames, problems);
        ValidateExpectedMethod(nameof(DataPointTests.Validate_WithEmptySource_ReturnsFalse), methodNames, problems);
        ValidateExpectedMethod(nameof(DataPointTests.Validate_WithQualityAboveUpperBound_ReturnsFalse), methodNames, problems);
        ValidateExpectedMethod(nameof(DataPointTests.MeetsQualityThreshold_WhenQualityEqualsThreshold_ReturnsTrue), methodNames, problems);
        ValidateExpectedMethod(nameof(DataPointTests.MeetsQualityThreshold_WhenQualityBelowThreshold_ReturnsFalse), methodNames, problems);
        ValidateExpectedMethod(nameof(DataPointTests.Clone_WithNewId_PreservesValueSourceAndQuality), methodNames, problems);
        ValidateExpectedMethod(nameof(DataPointTests.AddMetadata_WithValidKeyAndValue_StoresEntry), methodNames, problems);
        ValidateExpectedMethod(nameof(DataPointTests.AddMetadata_OverwritesExistingKeyWithNewValue), methodNames, problems);

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates that an expected method name exists in the collection of method names.
    /// </summary>
    /// <param name="expectedMethodName">The fully qualified method name to check for.</param>
    /// <param name="actualMethodNames">The collection of actual method names from reflection.</param>
    /// <param name="problems">The list to add validation problems to.</param>
    private static void ValidateExpectedMethod(string expectedMethodName, HashSet<string> actualMethodNames, List<string> problems)
    {
        if (!actualMethodNames.Contains(expectedMethodName))
        {
            problems.Add($"Test method '{expectedMethodName}' is missing.");
        }
    }

    /// <summary>
    /// Determines whether the specified <see cref="DataPointTests"/> test class is valid.
    /// A test class is considered valid when all expected test methods are present.
    /// </summary>
    /// <param name="value">The test class instance to check.</param>
    /// <returns><see langword="true"/> if all expected test methods are present; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this DataPointTests value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="DataPointTests"/> test class contains all expected test methods.
    /// Throws an exception if any expected test methods are missing.
    /// </summary>
    /// <param name="value">The test class instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the test class is missing any expected test methods.</exception>
    public static void EnsureValid(this DataPointTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                message: $"DataPointTests test class is invalid. Missing test methods: {string.Join(", ", problems)}",
                paramName: nameof(value));
        }
    }
}