# IExternalDataSource
The `IExternalDataSource` type is designed to provide a standardized interface for interacting with external data sources in the `dotnet-realtime-pipeline` project. It enables the integration of various data sources, allowing for the retrieval of data points and the assessment of data source health. This interface is crucial for building robust and flexible data pipelines.

## API
The `IExternalDataSource` interface includes the following members:
- `HttpDataSource`: An HTTP data source implementation.
- `FetchDataAsync`: An asynchronous method that fetches data points from the external data source. It returns a list of `DataPoint` objects.
- `IsAvailableAsync`: An asynchronous method that checks if the external data source is available. It returns a boolean value indicating availability.
- `DataSourceManager`: A data source manager instance.
- `Register`: A method that registers the external data source.
- `GetSourceHealth`: A method that retrieves the health status of the data source, returning a dictionary with string keys and boolean values.
- `Name`: A property that represents the name of the data source.
- `Source`: A property that references the external data source instance.
- `Priority`: A property that indicates the priority of the data source.
- `IsHealthy`: A property that indicates whether the data source is healthy.
- `CachedDataSource`: A cached data source implementation.
- `ClearCache`: A method that clears the cache of the data source.

## Usage
Here are two examples of using the `IExternalDataSource` interface in C#:
```csharp
// Example 1: Fetching data from an HTTP data source
var httpDataSource = new HttpDataSource("https://example.com/data");
var dataPoints = await httpDataSource.FetchDataAsync();
foreach (var dataPoint in dataPoints)
{
    Console.WriteLine(dataPoint);
}

// Example 2: Registering a data source and checking its health
var dataSourceManager = new DataSourceManager();
var externalDataSource = new ExternalDataSource("MyDataSource");
dataSourceManager.Register(externalDataSource);
var isAvailable = await externalDataSource.IsAvailableAsync();
Console.WriteLine($"Data source is available: {isAvailable}");
var sourceHealth = externalDataSource.GetSourceHealth();
foreach (var healthStatus in sourceHealth)
{
    Console.WriteLine($"{healthStatus.Key}: {healthStatus.Value}");
}
```

## Notes
When working with the `IExternalDataSource` interface, consider the following edge cases and thread-safety remarks:
- The `FetchDataAsync` and `IsAvailableAsync` methods are asynchronous, so they should be used with `await` to avoid blocking the calling thread.
- The `Register` method should be called before attempting to fetch data or check the health of the data source.
- The `GetSourceHealth` method returns a dictionary with string keys and boolean values, where each key represents a specific aspect of the data source's health.
- The `ClearCache` method should be used with caution, as it can impact the performance of the data source.
- The `IExternalDataSource` interface does not provide any inherent thread-safety guarantees, so it is the responsibility of the implementing class to ensure thread safety if necessary.
