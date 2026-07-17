# MetricAggregationTestsExtensions

This static class contains factory methods for creating `MetricAggregation` instances used in unit tests, together with a set of fluent assertion helpers that validate the correctness of those metrics. It is intended solely for test code to simplify the construction and verification of metric values without exposing internal calculation logic.

## API

### CreateThroughputMetric
**Purpose** – Produces a `MetricAggregation` that represents a throughput measurement (items processed per unit time).  
**Parameters** – The method expects the inputs required to compute throughput (e.g., the number of items processed and the elapsed time).  
**Return Value** – A new `MetricAggregation` instance populated with the calculated throughput value.  
**Exceptions** – Throws `ArgumentException` (or a derived type) if any supplied input is invalid, such as a negative item count or a non‑positive duration.

### CreateZeroDurationMetric
**Purpose** – Produces a `MetricAggregation` that represents a metric when the measured duration is zero.  
**Parameters** – Takes the data needed to express a zero‑duration scenario (typically an item count).  
**Return Value** – A `MetricAggregation` whose internal value reflects zero duration handling.  
**Exceptions** – Throws `ArgumentException` if the item count is negative or if the duration parameter is not zero.

### CreateErrorRateMetric
**Purpose** – Produces a `MetricAggregation` that represents an error rate (percentage of failed items).  
**Parameters** – Requires the total item count and the number of error items.  
**Return Value** – A `MetricAggregation` containing the computed error rate.  
**Exceptions** – Throws `ArgumentException` when the error count exceeds the total count or when either count is negative.

### CreateEmptyMetric
**Purpose** – Produces a `MetricAggregation` that represents an empty data set (no items processed).  
**Parameters** – No parameters are required; the method creates a metric initialized to an empty state.  
**Return Value** – A `MetricAggregation` with default empty values.  
**Exceptions** – None.

### CreateUnhealthyMetric
**Purpose** – Produces a `MetricAggregation` that flags an unhealthy condition based on error rate thresholds.  
**Parameters** – Takes the error rate value to be evaluated against the unhealthy threshold.  
**Return Value** – A `MetricAggregation` marked as unhealthy if the supplied rate exceeds the threshold.  
**Exceptions** – Throws `ArgumentException` if the error rate is outside the valid range [0,100].

### CreateProcessingTimeMetric
**Purpose** – Produces a `MetricAggregation` that represents average processing time per item.  
**Parameters** – Expects the total processing time and the number of processed items.  
**Return Value** – A `MetricAggregation` containing the average processing time.  
**Exceptions** – Throws `ArgumentException` if the total time is negative or the item count is less than or equal to zero.

### CreateBackpressureMetric
**Purpose** – Produces a `MetricAggregation` that represents the backpressure ratio (queue depth vs. processing capacity).  
**Parameters** – Requires the current queue depth and the maximum permissible depth (or capacity).  
**Return Value** – A `MetricAggregation` holding the calculated backpressure ratio.  
**Exceptions** – Throws `ArgumentException` if either parameter is negative or if the maximum depth is zero.

### ShouldCalculateThroughput
**Purpose** – Asserts that a given `MetricAggregation` contains the expected throughput value.  
**Parameters** – The actual `MetricAggregation`, the expected throughput (as a `double`), and an optional tolerance for floating‑point comparison.  
**Return Value** – `None`.  
**Exceptions** – Throws an assertion exception (e.g., `Xunit.Assert.TrueException` or `NUnit.Framework.AssertionException`) when the actual value deviates from the expected value beyond the allowed tolerance.

### ShouldReturnZeroThroughputForZeroDuration
**Purpose** – Verifies that a metric created with zero duration yields a throughput of zero.  
**Parameters** – The `MetricAggregation` instance to test.  
**Return Value** – `None`.  
**Exceptions** – Throws an assertion exception if the throughput is not zero.

### ShouldCalculateErrorRate
**Purpose** – Asserts that the error rate contained in a `MetricAggregation` matches the expected value.  
**Parameters** – The metric to evaluate, the expected error rate (percentage), and an optional tolerance.  
**Return Value** – `None`.  
**Exceptions** – Throws an assertion exception on mismatch.

### ShouldReturnHundredPercentSuccessRateWhenNoItems
**Purpose** – Confirms that a metric with zero items reports a 100 % success rate (i.e., zero error rate).  
**Parameters** – The `MetricAggregation` instance representing an empty set.  
**Return Value** – `None`.  
**Exceptions** – Throws an assertion exception if the success rate is not 100 %.

### ShouldDetectUnhealthyWhenErrorRateExceedsFivePercent
**Purpose** – Ensures that the unhealthy flag is set when the error rate exceeds the 5 % threshold.  
**Parameters** – The `MetricAggregation` to inspect.  
**Return Value** – `None`.  
**Exceptions** – Throws an assertion exception if the unhealthy flag is not set when expected, or is set incorrectly.

### ShouldCalculateCorrectAverageProcessingTime
**Purpose** – Validates that the average processing time stored in a metric matches the expected value.  
**Parameters** – The metric, the expected average time (as a `TimeSpan` or `double`), and an optional tolerance.  
**Return Value** – `None`.  
**Exceptions** – Throws an assertion exception on mismatch.

### ShouldSetAverageToZeroForEmptySamples
**Purpose** – Checks that a metric created from an empty sample set has an average processing time of zero.  
**Parameters** – The metric to test.  
**Return Value** – `None`.  
**Exceptions** – Throws an assertion exception if the average is non‑zero.

### ShouldCalculateCorrectBackpressureRatio
**Purpose** – Asserts that the backpressure ratio in a metric equals the expected value.  
**Parameters** – The metric, the expected ratio (as a `double`), and an optional tolerance.  
**Return Value** – `None`.  
**Exceptions** – Throws an assertion exception on mismatch.

### ShouldBeWithinRange
**Purpose** – Verifies that a numeric value (exposed via the metric) lies within a specified inclusive range.  
**Parameters** – The actual value, the lower bound, and the upper bound.  
**Return Value** – `None`.  
**Exceptions** – Throws an assertion exception if the value falls outside the range.

### ShouldBeAtLeast
**Purpose** – Asserts that a value is greater than or equal to a supplied minimum.  
**Parameters** – The actual value and the minimum allowed value.  
**Return Value** – `None`.  
**Exceptions** – Throws an assertion exception when the value is below the minimum.

### ShouldBeAtMost
**Purpose** – Asserts that a value is less than or equal to a supplied maximum.  
**Parameters** – The actual value and the maximum allowed value.  
**Return Value** – `None`.  
**Exceptions** – Throws an assertion exception when the value exceeds the maximum.

## Usage

```csharp
using Xunit;
using static DotnetRealtimePipeline.MetricAggregationTestsExtensions;

public class MetricAggregationTests
{
    [Fact]
    public void ThroughputIsCalculatedCorrectly()
    {
        // Arrange: 150 items processed in 3 seconds
        var metric = CreateThroughputMetric(itemCount: 150, duration: TimeSpan.FromSeconds(3));

        // Act & Assert: expected throughput = 50 items/second
        ShouldCalculateThroughput(metric, expected: 50.0, tolerance: 0.001);
    }

    [Fact]
    public void ErrorRateDetectsUnhealthyCondition()
    {
        // Arrange: 12 errors out of 200 items => 6 % error rate
        var metric = CreateErrorRateMetric(totalItems: 200, errorCount: 12);

        // Act & Assert: metric should be flagged as unhealthy (>5 %)
        ShouldDetectUnhealthyWhenErrorRateExceedsFivePercent(metric);
    }
}
```

```csharp
using NUnit.Framework;
using static DotnetRealtimePipeline.MetricAggregationTestsExtensions;

[TestFixture]
public class BackpressureTests
{
    [Test]
    public void BackpressureRatioIsWithinExpectedBounds()
    {
        // Arrange: queue depth 75, max depth 100 => ratio 0.75
        var metric = CreateBackpressureMetric(currentDepth: 75, maxDepth: 100);

        // Act & Assert: ratio should be 0.75 ± 0.01
        ShouldCalculateCorrectBackpressureRatio(metric, expected: 0.75, tolerance: 0.01);

        // Additional range checks
        ShouldBeWithinRange(metric.BackpressureRatio, 0.7, 0.8);
        ShouldBeAtLeast(metric.BackpressureRatio, 0.7);
        ShouldBeAtMost(metric.BackpressureRatio, 0.8);
    }
}
```

## Notes

- All factory methods are **pure**; they do not modify any external state and produce a new `MetricAggregation` instance on each call.  
- The assertion methods (`Should*`) are designed for single‑threaded test execution; they rely on static assertion frameworks (e.g., xUnit, NUnit) and are therefore safe to call from any test thread, provided the test framework itself supports parallel execution.  
- Passing invalid arguments (negative counts, zero or negative durations, error counts exceeding totals, etc.) will result in an `ArgumentException`; callers should validate inputs before invoking the factories in production code, although these methods are intended for test scenarios where inputs are typically controlled.  
- The floating‑point tolerant overloads use a default tolerance of `0.0` unless explicitly supplied; when comparing ratios or percentages, it is advisable to specify a tolerance that reflects the precision required by the domain (commonly `0.001` for three‑decimal places).  
- No static fields are present in this type, so there are no shared mutable resources that could introduce thread‑safety concerns beyond those inherent to the assertion frameworks used.
