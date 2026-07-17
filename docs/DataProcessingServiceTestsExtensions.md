# DataProcessingServiceTestsExtensions
The `DataProcessingServiceTestsExtensions` class provides a set of static methods for creating test data and results, facilitating the testing of data processing services in the `dotnet-realtime-pipeline` project. These methods simplify the creation of valid, low-quality, and invalid data points, as well as successful and failed processing results, allowing developers to focus on writing robust tests for their data processing services.

## API
* `public static DataPoint CreateValidDataPoint`: Creates a valid `DataPoint` object for testing purposes. This method does not take any parameters and returns a `DataPoint` instance. It does not throw any exceptions.
* `public static DataPoint CreateLowQualityDataPoint`: Creates a low-quality `DataPoint` object for testing purposes. This method does not take any parameters and returns a `DataPoint` instance. It does not throw any exceptions.
* `public static DataPoint CreateInvalidDataPoint`: Creates an invalid `DataPoint` object for testing purposes. This method does not take any parameters and returns a `DataPoint` instance. It does not throw any exceptions.
* `public static PipelineConfig CreateTestPipelineConfig`: Creates a test `PipelineConfig` object for testing purposes. This method does not take any parameters and returns a `PipelineConfig` instance. It does not throw any exceptions.
* `public static ProcessingResult CreateSuccessfulResult`: Creates a successful `ProcessingResult` object for testing purposes. This method does not take any parameters and returns a `ProcessingResult` instance. It does not throw any exceptions.
* `public static ProcessingResult CreateFailedResult`: Creates a failed `ProcessingResult` object for testing purposes. This method does not take any parameters and returns a `ProcessingResult` instance. It does not throw any exceptions.

## Usage
The following examples demonstrate how to use the `DataProcessingServiceTestsExtensions` class to create test data and results:
```csharp
// Create a valid data point and a successful processing result
DataPoint validDataPoint = DataProcessingServiceTestsExtensions.CreateValidDataPoint();
ProcessingResult successfulResult = DataProcessingServiceTestsExtensions.CreateSuccessfulResult();

// Create an invalid data point and a failed processing result
DataPoint invalidDataPoint = DataProcessingServiceTestsExtensions.CreateInvalidDataPoint();
ProcessingResult failedResult = DataProcessingServiceTestsExtensions.CreateFailedResult();
```
```csharp
// Create a test pipeline configuration and a low-quality data point
PipelineConfig testPipelineConfig = DataProcessingServiceTestsExtensions.CreateTestPipelineConfig();
DataPoint lowQualityDataPoint = DataProcessingServiceTestsExtensions.CreateLowQualityDataPoint();
```

## Notes
When using the `DataProcessingServiceTestsExtensions` class, note that the created test data and results are intended for testing purposes only and may not be suitable for production use. Additionally, the methods in this class do not throw exceptions, but the created objects may still be invalid or incomplete if not properly configured. The thread-safety of these methods is not guaranteed, as they are static and may be accessed concurrently by multiple threads. Therefore, it is recommended to use these methods in a thread-safe manner, such as by synchronizing access to shared resources or using immutable objects.
