#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetRealtimePipeline.Tests.Unit;

/// <summary>
/// Provides validation helpers for <see cref="DataPointRepositoryTests"/> instances.
/// </summary>
public static class DataPointRepositoryTestsValidation
{
    /// <summary>
    /// Validates the specified <see cref="DataPointRepositoryTests"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this DataPointRepositoryTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate test methods exist using reflection
        var methods = typeof(DataPointRepositoryTests).GetMethods();
        var methodNames = methods.Select(m => m.Name).ToHashSet();

        if (!methodNames.Contains(nameof(DataPointRepositoryTests.AddAsync_WithValidDataPoint_ShouldSucceed)))
        {
            problems.Add($"Test method '{nameof(DataPointRepositoryTests.AddAsync_WithValidDataPoint_ShouldSucceed)}' is missing.");
        }

        if (!methodNames.Contains(nameof(DataPointRepositoryTests.GetByIdAsync_WithNonExistentId_ShouldReturnNull)))
        {
            problems.Add($"Test method '{nameof(DataPointRepositoryTests.GetByIdAsync_WithNonExistentId_ShouldReturnNull)}' is missing.");
        }

        if (!methodNames.Contains(nameof(DataPointRepositoryTests.GetAllAsync_WithMultiplePoints_ShouldReturnAll)))
        {
            problems.Add($"Test method '{nameof(DataPointRepositoryTests.GetAllAsync_WithMultiplePoints_ShouldReturnAll)}' is missing.");
        }

        if (!methodNames.Contains(nameof(DataPointRepositoryTests.GetBySourceAsync_WithValidSource_ShouldReturnMatching)))
        {
            problems.Add($"Test method '{nameof(DataPointRepositoryTests.GetBySourceAsync_WithValidSource_ShouldReturnMatching)}' is missing.");
        }

        if (!methodNames.Contains(nameof(DataPointRepositoryTests.UpdateAsync_WithExistingId_ShouldUpdate)))
        {
            problems.Add($"Test method '{nameof(DataPointRepositoryTests.UpdateAsync_WithExistingId_ShouldUpdate)}' is missing.");
        }

        if (!methodNames.Contains(nameof(DataPointRepositoryTests.DeleteAsync_WithExistingId_ShouldRemove)))
        {
            problems.Add($"Test method '{nameof(DataPointRepositoryTests.DeleteAsync_WithExistingId_ShouldRemove)}' is missing.");
        }

        if (!methodNames.Contains(nameof(DataPointRepositoryTests.ClearAsync_ShouldRemoveAll)))
        {
            problems.Add($"Test method '{nameof(DataPointRepositoryTests.ClearAsync_ShouldRemoveAll)}' is missing.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="DataPointRepositoryTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this DataPointRepositoryTests value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="DataPointRepositoryTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing the validation problems.</exception>
    public static void EnsureValid(this DataPointRepositoryTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                message: $"DataPointRepositoryTests instance is invalid. Problems: {string.Join(", ", problems)}",
                paramName: nameof(value));
        }
    }
}