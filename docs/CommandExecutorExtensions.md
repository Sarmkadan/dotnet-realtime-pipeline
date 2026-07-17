# CommandExecutorExtensions

The `CommandExecutorExtensions` class provides a set of asynchronous extension methods designed to simplify interactions with the real-time pipeline's command execution layer. These methods encapsulate common operational patterns such as data ingestion, querying, status monitoring, and file-based export/import operations, returning strongly typed results while handling underlying communication complexities. By leveraging these extensions, developers can execute pipeline commands with reduced boilerplate code, ensuring consistent error handling and return type semantics across the application.

## API

### ExecuteSuccessfullyAsync
Executes a specified command and returns a boolean indicating whether the operation completed without errors.
*   **Parameters**: Accepts the target command executor instance and the command definition to be executed.
*   **Return Value**: Returns a `Task<bool>` which resolves to `true` if the command succeeded, or `false` if it failed.
*   **Throws**: Throws an exception if the underlying communication channel is unavailable or if the command payload is malformed.

### IngestFromFileAsync
Reads data from a specified file path and ingests it into the pipeline as a stream of data points.
*   **Parameters**: Requires the file path to the source data and optional configuration for parsing rules.
*   **Return Value**: Returns a `Task<int>` representing the total number of data points successfully ingested.
*   **Throws**: Throws `FileNotFoundException` if the source path is invalid, or `DataFormatException` if the file content does not match the expected schema.

### QueryDataAsync
Retrieves a collection of data points from the pipeline based on specific query criteria.
*   **Parameters**: Accepts query filters including time ranges, metric identifiers, and aggregation levels.
*   **Return Value**: Returns a `Task<IReadOnlyList<DataPoint>>` containing the matching data points.
*   **Throws**: Throws a timeout exception if the query exceeds the configured execution limit or if the query syntax is invalid.

### GetStatusAsync
Retrieves the current operational state and detailed metrics of the pipeline instance.
*   **Parameters**: No additional parameters required beyond the executor instance.
*   **Return Value**: Returns a `Task<Dictionary<string, object>>` where keys represent status categories (e.g., "QueueDepth", "ActiveConnections") and values hold the corresponding metric data.
*   **Throws**: Generally does not throw unless the pipeline service is completely unreachable.

### CountDataPointsAsync
Calculates the total number of data points currently stored or matching a specific filter within the pipeline.
*   **Parameters**: Accepts optional filtering criteria to narrow the count scope.
*   **Return Value**: Returns a `Task<int>` with the calculated count.
*   **Throws**: Throws an exception if the internal storage index is corrupted or unavailable.

### ExportToFileAsync
Exports a dataset from the pipeline to a local file in a standardized format.
*   **Parameters**: Requires the destination file path, the data selection criteria, and the desired output format.
*   **Return Value**: Returns a `Task<int>` indicating the number of records written to the file.
*   **Throws**: Throws `IOException` if the destination path is inaccessible or if disk space is insufficient.

### CountExportedDataPointsAsync
Verifies the integrity of an export operation by counting the data points present in a previously exported file.
*   **Parameters**: Requires the path to the exported file.
*   **Return Value**: Returns a `Task<int>` representing the verified count of data points in the file.
*   **Throws**: Throws `FileNotFoundException` if the file is missing or `DataFormatException` if the file structure is invalid.

### GetStatusSummaryAsync
Generates a human-readable summary string of the current pipeline health and activity.
*   **Parameters**: No additional parameters required.
*   **Return Value**: Returns a `Task<string>` containing the formatted status summary.
*   **Throws**: Does not typically throw; returns an empty string or error message within the result if status retrieval fails partially.

## Usage

### Example 1: Data Ingestion and Verification
This example demonstrates ingesting data from a CSV file and immediately verifying the count of ingested points to ensure data integrity.

```csharp
using System;
using System.Threading.Tasks;
using DotNetRealtimePipeline.Extensions;

public class IngestionWorkflow
{
    public async Task RunIngestionAsync(ICommandExecutor executor, string sourcePath)
    {
        // Ingest data from the local file system
        int ingestedCount = await executor.IngestFromFileAsync(sourcePath);
        
        Console.WriteLine($"Ingestion complete. Records processed: {ingestedCount}");

        // Verify the count against the live pipeline state
        int liveCount = await executor.CountDataPointsAsync();
        
        if (ingestedCount != liveCount)
        {
            Console.WriteLine("Warning: Ingested count does not match live pipeline count.");
        }
    }
}
```

### Example 2: Status Monitoring and Data Export
This example retrieves the current system status, checks for specific health metrics, and triggers an export if the system is stable.

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetRealtimePipeline.Extensions;

public class MaintenanceTask
{
    public async Task PerformMaintenanceAsync(ICommandExecutor executor, string backupPath)
    {
        // Retrieve detailed status metrics
        Dictionary<string, object> status = await executor.GetStatusAsync(executor);
        
        if (status.TryGetValue("IsHealthy", out var healthyObj) && (bool)healthyObj)
        {
            // Export recent data for backup
            int exportedRows = await executor.ExportToFileAsync(backupPath, "last_24h", "json");
            Console.WriteLine($"Backup successful: {exportedRows} rows exported.");
            
            // Get a readable summary for logging
            string summary = await executor.GetStatusSummaryAsync();
            Console.WriteLine($"System Summary: {summary}");
        }
        else
        {
            Console.WriteLine("Skipping export due to unhealthy system state.");
        }
    }
}
```

## Notes

*   **Thread Safety**: All methods in `CommandExecutorExtensions` are stateless and thread-safe, provided the underlying `ICommandExecutor` instance passed to them is thread-safe. Multiple concurrent calls to `QueryDataAsync` or `GetStatusAsync` are supported.
*   **Resource Management**: Methods involving file I/O (`IngestFromFileAsync`, `ExportToFileAsync`) do not explicitly lock the target files. Callers must ensure that the specified files are not locked by other processes to avoid `IOException`.
*   **Empty Results**: `QueryDataAsync` returns an empty `IReadOnlyList<DataPoint>` rather than `null` when no data matches the criteria. Similarly, `GetStatusAsync` returns an empty dictionary if no metrics are available, rather than throwing.
*   **Cancellation**: While the signatures do not explicitly expose `CancellationToken` parameters, these asynchronous operations respect the default cancellation tokens associated with the underlying execution context. Long-running queries should be monitored for timeout exceptions.
*   **Data Consistency**: `CountDataPointsAsync` provides an eventual consistency count. In high-throughput scenarios, the returned integer may differ slightly from the actual count at the exact millisecond of return due to ongoing ingestion processes.
