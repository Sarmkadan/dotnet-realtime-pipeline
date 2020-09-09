# ExportService
The `ExportService` class is designed to handle the export of data points, results, and metrics from a data pipeline. It provides a range of methods for exporting data in different formats and batches, allowing for flexible and efficient data management.

## API
The `ExportService` class has the following public members:
* `ExportDataPointsAsync`: Exports data points asynchronously. Returns an `ExportResult` object containing information about the export operation.
* `ExportResultsAsync`: Exports results asynchronously. Returns an `ExportResult` object containing information about the export operation.
* `ExportMetricsAsync`: Exports metrics asynchronously. Returns an `ExportResult` object containing information about the export operation.
* `ExportMultiFormatAsync`: Exports data in multiple formats asynchronously. Returns a list of `ExportResult` objects containing information about each export operation.
* `Success`: A boolean property indicating whether the export operation was successful.
* `OutputPath`: A string property containing the path where the exported data is saved.
* `RecordCount`: An integer property containing the number of records exported.
* `FileSizeBytes`: A long property containing the size of the exported file in bytes.
* `ErrorMessage`: A string property containing an error message if the export operation fails.
* `StartTime` and `EndTime`: DateTime properties containing the start and end times of the export operation.
* `ToString`: An overridden method that returns a string representation of the `ExportService` object.
* `BatchExportProcessor`: A property that provides access to batch export processing functionality.
* `ExportInBatchesAsync`: Exports data in batches asynchronously. Returns a `BatchExportResult` object containing information about the batch export operation.
* `BatchExportResult.Success`: A boolean property indicating whether the batch export operation was successful.
* `BatchExportResult.ExportedRecords`: An integer property containing the number of records exported in the batch.
* `BatchExportResult.BatchFiles`: A list of strings containing the paths of the batch files.
* `BatchExportResult.ErrorMessage`: A string property containing an error message if the batch export operation fails.
* `BatchExportResult.StartTime`: A DateTime property containing the start time of the batch export operation.

## Usage
Here are two examples of using the `ExportService` class:
```csharp
// Example 1: Exporting data points
var exportService = new ExportService();
var exportResult = await exportService.ExportDataPointsAsync();
if (exportResult.Success)
{
    Console.WriteLine($"Exported {exportResult.RecordCount} records to {exportResult.OutputPath}");
}
else
{
    Console.WriteLine($"Export failed: {exportResult.ErrorMessage}");
}

// Example 2: Exporting data in batches
var exportService = new ExportService();
var batchExportResult = await exportService.BatchExportProcessor.ExportInBatchesAsync();
if (batchExportResult.Success)
{
    Console.WriteLine($"Exported {batchExportResult.ExportedRecords} records in batches");
    foreach (var batchFile in batchExportResult.BatchFiles)
    {
        Console.WriteLine($"Batch file: {batchFile}");
    }
}
else
{
    Console.WriteLine($"Batch export failed: {batchExportResult.ErrorMessage}");
}
```

## Notes
When using the `ExportService` class, note that the export operations are asynchronous and may throw exceptions if errors occur during the export process. Additionally, the `Success` property and `ErrorMessage` property can be used to determine the outcome of the export operation. The `ExportService` class is designed to be thread-safe, but it is still important to ensure that the export operations are properly synchronized to avoid conflicts. The `BatchExportProcessor` property provides a way to export data in batches, which can be useful for large datasets. However, this may also increase the complexity of the export process and require additional error handling.
