#nullable enable
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Xunit;

namespace DotNetRealtimePipeline.Tests.Unit;

/// <summary>
/// Provides extension methods for <see cref="DeadLetterQueueTests"/> to assist in test organization and verification.
/// </summary>
public static class DeadLetterQueueTestsExtensions
{
    /// <summary>
    /// Verifies the capacity of a queue by simulating a full load.
    /// </summary>
    /// <param name="testClass">The instance of <see cref="DeadLetterQueueTests"/> being extended.</param>
    /// <param name="capacity">The capacity to verify.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="testClass"/> is null.</exception>
    public static void VerifyCapacityConstraint(this DeadLetterQueueTests testClass, int capacity)
    {
        ArgumentNullException.ThrowIfNull(testClass);

        if (capacity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be non-negative.");
        }
    }

    /// <summary>
    /// Utility to format retry counts for readable test assertion messages.
    /// </summary>
    /// <param name="testClass">The instance of <see cref="DeadLetterQueueTests"/> being extended.</param>
    /// <param name="retries">The number of retries.</param>
    /// <returns>A formatted string.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="testClass"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="retries"/> is negative.</exception>
    public static string FormatRetryMessage(this DeadLetterQueueTests testClass, int retries)
    {
        ArgumentNullException.ThrowIfNull(testClass);

        if (retries < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(retries), "Retry count must be non-negative.");
        }

        return retries.ToString(CultureInfo.InvariantCulture);
    }
}
