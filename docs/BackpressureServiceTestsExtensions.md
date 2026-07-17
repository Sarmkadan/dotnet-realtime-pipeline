# BackpressureServiceTestsExtensions

The `BackpressureServiceTestsExtensions` class provides a suite of static extension methods designed to facilitate the unit testing of backpressure mechanisms within the `dotnet-realtime-pipeline` project. These helpers allow developers to programmatically manipulate the internal state of a `BackpressureServiceTests` instance, assert specific buffer conditions, and verify that backpressure logic is correctly applied under load, thereby reducing boilerplate code in test suites and ensuring consistent validation of flow control behaviors.

## API

### CreateContextWithCapacity
Initializes a new `BackpressureServiceTests` instance with a specified maximum buffer capacity. This method serves as the primary entry point for setting up a test environment with defined constraints.
*   **Parameters**: `int capacity` – The maximum number of items the buffer can hold before backpressure is triggered.
*   **Returns**: A new `BackpressureServiceTests` instance configured with the provided capacity.
*   **Throws**: `ArgumentOutOfRangeException` if `capacity` is less than or equal to zero.

### AddToBuffer
Inserts a single item into the buffer of the specified context. This method simulates incoming data flow and is used to incrementally fill the buffer up to its capacity limit.
*   **Parameters**: `this BackpressureServiceTests context`, `TItem item` – The test context and the item to enqueue.
*   **Returns**: The same `BackpressureServiceTests` instance to allow for method chaining.
*   **Throws**: `InvalidOperationException` if the buffer is already at full capacity and the service is configured to reject new items immediately upon reaching the limit.

### AssertBufferAtCapacity
Validates that the current number of items in the buffer exactly matches the defined maximum capacity. This assertion confirms that the system has reached the threshold where backpressure should be active.
*   **Parameters**: `this BackpressureServiceTests context` – The test context to evaluate.
*   **Returns**: The `BackpressureServiceTests` instance.
*   **Throws**: `AssertException` (or standard assertion failure) if the current buffer count does not equal the configured capacity.

### AssertBufferCount
Verifies that the buffer contains a specific number of items. This is useful for testing intermediate states or verifying the exact number of items remaining after consumption.
*   **Parameters**: `this BackpressureServiceTests context`, `int expectedCount` – The test context and the expected number of items.
*   **Returns**: The `BackpressureServiceTests` instance.
*   **Throws**: `AssertException` if the actual buffer count differs from `expectedCount`.

### RemoveAndAssert
Removes a single item from the buffer and immediately asserts that the removal was successful and the item matches expectations. This simulates consumer processing and verifies data integrity during dequeue operations.
*   **Parameters**: `this BackpressureServiceTests context`, `TItem expectedItem` – The test context and the item expected to be at the head of the queue.
*   **Returns**: The `BackpressureServiceTests` instance.
*   **Throws**: `AssertException` if the buffer is empty or if the removed item does not match `expectedItem`.

### AssertBackpressureAppliedAsync
Asynchronously verifies that the backpressure mechanism is actively blocking new additions or delaying processing when the buffer is full. This method typically attempts to add an item and measures the delay or observes the blocking behavior.
*   **Parameters**: `this BackpressureServiceTests context`, `TimeSpan? timeout` – The test context and an optional timeout duration for the assertion.
*   **Returns**: A `Task<BackpressureServiceTests>` that completes when backpressure application is confirmed.
*   **Throws**: `TimeoutException` if backpressure is not detected within the specified `timeout`, or `InvalidOperationException` if the buffer is not at capacity when the check is performed.

### GetBufferStatusDictionary
Retrieves a snapshot of the current buffer metrics as an immutable dictionary. This provides detailed insight into the internal state, such as current count, capacity, and potentially wait queue lengths.
*   **Parameters**: `this BackpressureServiceTests context` – The test context to inspect.
*   **Returns**: An `IReadOnlyDictionary<string, int>` containing key-value pairs representing buffer statistics (e.g., "CurrentCount", "Capacity").
*   **Throws**: None.

### CreateMultipleContexts
Generates a collection of independent `BackpressureServiceTests` instances, useful for concurrency tests or simulating multiple pipeline streams running in parallel.
*   **Parameters**: `int count`, `int capacity` – The number of contexts to create and the capacity for each.
*   **Returns**: An `IEnumerable<BackpressureServiceTests>` containing the newly created instances.
*   **Throws**: `ArgumentOutOfRangeException` if `count` or `capacity` is invalid.

## Usage

### Example 1: Verifying Capacity and Backpressure Trigger
This example demonstrates setting up a context, filling it to the limit, and asserting that the system correctly identifies the full state and applies backpressure.

```csharp
using System;
using System.Threading.Tasks;
using NUnit.Framework;

[TestFixture]
public class BackpressureBehaviorTests
{
    [Test]
    public async Task ShouldApplyBackpressureWhenFull()
    {
        // Arrange: Create a context with a capacity of 5
        var context = BackpressureServiceTestsExtensions.CreateContextWithCapacity(5);

        // Act: Fill the buffer to capacity
        for (int i = 0; i < 5; i++)
        {
            BackpressureServiceTestsExtensions.AddToBuffer(context, $"Item-{i}");
        }

        // Assert: Verify buffer is exactly at capacity
        BackpressureServiceTestsExtensions.AssertBufferAtCapacity(context);

        // Assert: Verify that adding another item triggers backpressure logic
        // This will wait up to 2 seconds to confirm the backpressure mechanism engages
        await BackpressureServiceTestsExtensions.AssertBackpressureAppliedAsync(context, TimeSpan.FromSeconds(2));
    }
}
```

### Example 2: Simulating Producer-Consumer Flow
This example illustrates a scenario where items are added and removed dynamically, verifying the buffer count at various stages and inspecting internal metrics.

```csharp
using System.Collections.Generic;
using NUnit.Framework;

[TestFixture]
public class BufferFlowTests
{
    [Test]
    public void ShouldManageItemCountCorrectly()
    {
        // Arrange
        var context = BackpressureServiceTestsExtensions.CreateContextWithCapacity(10);
        
        // Act: Add 3 items
        BackpressureServiceTestsExtensions.AddToBuffer(context, "A");
        BackpressureServiceTestsExtensions.AddToBuffer(context, "B");
        BackpressureServiceTestsExtensions.AddToBuffer(context, "C");

        // Assert: Check count is 3
        BackpressureServiceTestsExtensions.AssertBufferCount(context, 3);

        // Act: Consume one item
        BackpressureServiceTestsExtensions.RemoveAndAssert(context, "A");

        // Assert: Check count is now 2 and inspect status dictionary
        BackpressureServiceTestsExtensions.AssertBufferCount(context, 2);
        
        var status = BackpressureServiceTestsExtensions.GetBufferStatusDictionary(context);
        Assert.That(status["CurrentCount"], Is.EqualTo(2));
        Assert.That(status["Capacity"], Is.EqualTo(10));
    }
}
```

## Notes

*   **Thread Safety**: While the underlying `BackpressureServiceTests` class may support concurrent access, these extension methods perform compound operations (read-modify-write or check-then-act). When testing multi-threaded scenarios, external synchronization (e.g., `lock` statements) around calls to these extensions is recommended to prevent race conditions during state manipulation and assertion.
*   **State Dependency**: Methods like `AssertBackpressureAppliedAsync` and `AssertBufferAtCapacity` rely heavily on the current state of the buffer. Calling `AssertBackpressureAppliedAsync` on a context that is not currently at capacity will result in an immediate failure or exception, as backpressure logic is typically inactive below the threshold.
*   **Immutability of Status**: The dictionary returned by `GetBufferStatusDictionary` is a snapshot in time. It is safe to read after the call returns, but it will not reflect subsequent changes to the buffer state unless the method is called again.
*   **Chaining**: Most mutation methods return the `BackpressureServiceTests` instance to support fluent chaining. However, async methods (like `AssertBackpressureAppliedAsync`) return a `Task` and break the synchronous chaining pattern; they must be awaited before further synchronous assertions can be safely performed.
