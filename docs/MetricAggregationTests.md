# MetricAggregationTests

Unit tests for metric aggregation logic in the realtime pipeline, validating calculations for throughput, error rates, processing times, and backpressure metrics under various conditions.

## API

### `CalculateThroughput_WithValidWindow_ReturnsItemsPerSecond`
Verifies that throughput is correctly calculated as items processed per second when given a valid time window. No parameters or return value; validates internal state after execution.

### `CalculateThroughput_WithZeroDurationWindow_ReturnsZero`
Ensures throughput returns zero when the time window duration is zero to prevent division by zero errors. No parameters or return value; validates internal state after execution.

### `CalculateErrorRate_WithFailedItems_ReturnsCorrectPercentage`
Computes the error rate as a percentage of failed items relative to total items processed. No parameters or return value; validates internal state after execution.

### `CalculateSuccessRate_WhenNoItemsRecorded_ReturnsHundredPercent`
Confirms that success rate defaults to 100% when no items have been processed, avoiding undefined behavior. No parameters or return value; validates internal state after execution.

### `IsUnhealthy_WhenErrorRateExceedsFivePercent_ReturnsTrue`
Checks whether the system is marked as unhealthy when the error rate exceeds the 5% threshold. No parameters or return value; validates internal state after execution.

### `ComputeAverageProcessingTime_WithValidSamples_SetsCorrectAverage`
Calculates the average processing time from a list of valid samples and updates the internal average value. No parameters or return value; validates internal state after execution.

### `ComputeAverageProcessingTime_WithEmptyList_SetsAverageToZero`
Ensures the average processing time is set to zero when no samples are provided. No parameters or return value; validates internal state after execution.

### `CalculateBackpressureRatio_WithKnownBackpressureDuration_ReturnsCorrectRatio`
Computes the backpressure ratio based on the duration of backpressure relative to total runtime. No parameters or return value; validates internal state after execution.

## Usage
