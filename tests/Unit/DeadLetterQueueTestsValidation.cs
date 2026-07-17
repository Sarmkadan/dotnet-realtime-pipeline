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
/// Provides validation helpers for <see cref="DeadLetterQueueTests"/> instances.
/// </summary>
public static class DeadLetterQueueTestsValidation
{
    /// <summary>
    /// Validates the specified <see cref="DeadLetterQueueTests"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this DeadLetterQueueTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate test methods exist using reflection
        var methods = typeof(DeadLetterQueueTests).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var methodNames = methods.Select(m => m.Name).ToHashSet(StringComparer.Ordinal);

        // Use pattern matching for cleaner validation
        if (!methodNames.Contains(nameof(DeadLetterQueueTests.EnqueueAsync_ValidEntry_IncreasesCount)))
        {
            problems.Add($"Test method '{nameof(DeadLetterQueueTests.EnqueueAsync_ValidEntry_IncreasesCount)}' is missing.");
        }

        if (!methodNames.Contains(nameof(DeadLetterQueueTests.EnqueueAsync_NullDataPoint_Throws)))
        {
            problems.Add($"Test method '{nameof(DeadLetterQueueTests.EnqueueAsync_NullDataPoint_Throws)}' is missing.");
        }

        if (!methodNames.Contains(nameof(DeadLetterQueueTests.EnqueueAsync_EmptyReason_Throws)))
        {
            problems.Add($"Test method '{nameof(DeadLetterQueueTests.EnqueueAsync_EmptyReason_Throws)}' is missing.");
        }

        if (!methodNames.Contains(nameof(DeadLetterQueueTests.EnqueueAsync_WithException_StoresExceptionInfo)))
        {
            problems.Add($"Test method '{nameof(DeadLetterQueueTests.EnqueueAsync_WithException_StoresExceptionInfo)}' is missing.");
        }

        if (!methodNames.Contains(nameof(DeadLetterQueueTests.PeekAsync_DoesNotRemoveEntries)))
        {
            problems.Add($"Test method '{nameof(DeadLetterQueueTests.PeekAsync_DoesNotRemoveEntries)}' is missing.");
        }

        if (!methodNames.Contains(nameof(DeadLetterQueueTests.DequeueForRetryAsync_ReturnsPendingEntries)))
        {
            problems.Add($"Test method '{nameof(DeadLetterQueueTests.DequeueForRetryAsync_ReturnsPendingEntries)}' is missing.");
        }

        if (!methodNames.Contains(nameof(DeadLetterQueueTests.DequeueForRetryAsync_EntryExhaustedRetries_NotReturned)))
        {
            problems.Add($"Test method '{nameof(DeadLetterQueueTests.DequeueForRetryAsync_EntryExhaustedRetries_NotReturned)}' is missing.");
        }

        if (!methodNames.Contains(nameof(DeadLetterQueueTests.AcknowledgeSuccessAsync_RemovesEntry)))
        {
            problems.Add($"Test method '{nameof(DeadLetterQueueTests.AcknowledgeSuccessAsync_RemovesEntry)}' is missing.");
        }

        if (!methodNames.Contains(nameof(DeadLetterQueueTests.AcknowledgeFailureAsync_MarksAsPermanentFailure)))
        {
            problems.Add($"Test method '{nameof(DeadLetterQueueTests.AcknowledgeFailureAsync_MarksAsPermanentFailure)}' is missing.");
        }

        if (!methodNames.Contains(nameof(DeadLetterQueueTests.GetStatsAsync_ReflectsCurrentState)))
        {
            problems.Add($"Test method '{nameof(DeadLetterQueueTests.GetStatsAsync_ReflectsCurrentState)}' is missing.");
        }

        if (!methodNames.Contains(nameof(DeadLetterQueueTests.Enqueue_WhenAtCapacity_EvictsOldest)))
        {
            problems.Add($"Test method '{nameof(DeadLetterQueueTests.Enqueue_WhenAtCapacity_EvictsOldest)}' is missing.");
        }

        if (!methodNames.Contains(nameof(DeadLetterQueueTests.DeadLetterEntry_CanRetry_TrueWhenUnderBudget)))
        {
            problems.Add($"Test method '{nameof(DeadLetterQueueTests.DeadLetterEntry_CanRetry_TrueWhenUnderBudget)}' is missing.");
        }

        if (!methodNames.Contains(nameof(DeadLetterQueueTests.DeadLetterEntry_CanRetry_FalseWhenExhausted)))
        {
            problems.Add($"Test method '{nameof(DeadLetterQueueTests.DeadLetterEntry_CanRetry_FalseWhenExhausted)}' is missing.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="DeadLetterQueueTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this DeadLetterQueueTests value) => value.Validate().Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="DeadLetterQueueTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing the validation problems.</exception>
    public static void EnsureValid(this DeadLetterQueueTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                message: $"DeadLetterQueueTests instance is invalid. Problems:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}",
                paramName: nameof(value));
        }
    }
}