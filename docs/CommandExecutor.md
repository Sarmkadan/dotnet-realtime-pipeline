# CommandExecutor

The `CommandExecutor` type provides an asynchronous façade for executing pipeline commands, ingesting and querying data, exporting results, and visualizing status information. It is designed to be instantiated once per logical pipeline and reused across multiple operations, with optional cancellation support.

## API

### `public CommandExecutor()`
Initializes a new instance of the `CommandExecutor`. The instance is ready to accept commands after construction; no external configuration is required for basic operation.

### `public async Task<int> ExecuteAsync(string command, CancellationToken cancellationToken = default)`
Executes a pipeline command identified by *command*.  
- **Parameters**  
  - `command`: The identifier or script of the command to run.  
  - `cancellationToken`: Optional token to cancel the operation.  
- **Return Value**  
  - The exit code of the executed command as an `int`. A value of `0` typically indicates success.  
- **Exceptions**  
  - `ArgumentNullException` if *command* is `null`.  
  - `InvalidOperationException` if the executor is not in a usable state.  
  - `OperationCanceledException` if the operation is cancelled via *cancellationToken*.  

### `public async Task<bool> IngestDataAsync(IEnumerable<DataPoint> data, CancellationToken cancellationToken = default)`
Ingests a collection of *DataPoint* instances into the pipeline’s internal store.  
- **Parameters**  
  - `data`: The data points to ingest.  
  - `cancellationToken`: Optional token to cancel the operation.  
- **Return Value**  
  - `true` if ingestion completed successfully; `false` if the operation was cancelled or no data was accepted.  
- **Exceptions**  
  - `ArgumentNullException` if *data* is `null`.  
  - `InvalidOperationException` if the executor cannot accept data at this time.  
  - `OperationCanceledException` if cancellation is requested.  

### `public async Task<List<DataPoint>> QueryDataAsync(string query, CancellationToken cancellationToken = default)`
Queries the pipeline for data points matching *query*.  
- **Parameters**  
  - `query`: A query string or expression understood by the underlying store.  
  - `cancellationToken`: Optional token to cancel the operation.  
- **Return Value**  
  - A list of `DataPoint` objects that satisfy the query. An empty list is returned when no matches are found.  
- **Exceptions**  
  - `ArgumentNullException` if *query* is `null`.  
  - `InvalidOperationException` if the query cannot be executed (e.g., store not initialized).  
  - `OperationCanceledException` if the operation is cancelled.  

### `public async Task<Dictionary<string, object>> GetStatusAsync(CancellationToken cancellationToken = default)`
Retrieves a snapshot of the executor’s current status and metrics.  
- **Parameters**  
  - `cancellationToken`: Optional token to cancel the operation.  
- **Return Value**  
  - A dictionary where keys are status identifiers (e.g., `"IngestedCount"`, `"LastError"`) and values are associated status data.  
- **Exceptions**  
  - `InvalidOperationException` if the executor is unable to report status.  
  - `OperationCanceledException` if cancellation is requested.  

### `public async Task<bool> ExportDataAsync(string destination, CancellationToken cancellationToken = default)`
Exports the currently stored data to the location specified by *destination*.  
- **Parameters**  
  - `destination`: A path or URI indicating where the export target storage.  
  - `cancellationToken`: Optional token to cancel the operation.  
- **Return Value**  
  - `true` if export succeeded; `false` if cancelled or no data was exported.  
- **Exceptions**  
  - `ArgumentNullException` if *destination* is `null` or empty.  
  - `InvalidOperationException` if there is no data to export or the destination is inaccessible.  
  - `OperationCanceledException` if the operation is cancelled.  

### `public Task<string> VisualizeAsync()`
Produces a string representation (e.g., JSON, DOT, or HTML) of the current pipeline state for visualization purposes.  
- **Return Value**  
  - A string containing the visualization data.  
- **Exceptions**  
  - `InvalidOperationException` if visualization cannot be generated (e.g., missing internal state).  

### `public static IDataLoader CreateLoader()`
Factory method that returns a new instance of an `IDataLoader` implementation suitable for loading data into the pipeline.  
- **Return Value**  
  - An object implementing `IDataLoader`.  
- **Exceptions**  
  - None under normal circumstances; implementations may throw if resources cannot be acquired.  

### `public static IDataExporter CreateExporter()`
Factory method that returns a new instance of an `IDataExporter` implementation suitable for exporting data from the pipeline.  
- **Return Value**  
  - An object implementing `IDataExporter`.  
- **Exceptions**  
  - None under normal circumstances; implementations may throw if resources cannot be acquired.  

### `public async Task<List<DataPoint>> LoadAsync(string source, CancellationToken cancellationToken = default)`
Loads data from *source* using the default loader and returns the resulting data points.  
- **Parameters**  
  - `source`: Identifier or path of the data source.  
  - `cancellationToken`: Optional token to cancel the operation.  
- **Return Value**  
  - A list of `DataPoint` objects read from the source.  
- **Exceptions**  
  - `ArgumentNullException` if *source* is `null` or empty.  
  - `InvalidOperationException` if the loader cannot be created or the source is unreadable.  
  - `OperationCanceledException` if the operation is cancelled.  

### `public async Task ExportAsync(string source, string destination, CancellationToken cancellationToken = default)`
Exports data from *source* to *destination* using the default exporter.  
- **Parameters**  
  - `source`: Identifier or path of the data to export.  
  - `destination`: Target location for the exported data.  
  - `cancellationToken`: Optional token to cancel the operation.  
- **Return Value**  
  - A completed `Task` when the export finishes.  
- **Exceptions**  
  - `ArgumentNullException* if either *source* or *destination* is `null` or empty.  
  - `InvalidOperationException` if the exporter cannot be created or the operation fails.  
  - `OperationCanceledException` if the operation is cancelled.  

### `public async Task<List<DataPoint>> LoadAsync(IDataLoader loader, CancellationToken cancellationToken = default)`
Loads data using a supplied *loader* instance.  
- **Parameters**  
  - `loader`: An `IDataLoader` implementation to read data.  
  - `cancellationToken`: Optional token to cancel the operation.  
- **Return Value**  
  - A list of `DataPoint` objects obtained from the loader.  
- **Exceptions**  
  - `ArgumentNullException` if *loader* is `null`.  
  - `InvalidOperationException` if loading fails for any reason.  
  - `OperationCanceledException` if the operation is cancelled.  

### `public async Task ExportAsync(IDataExporter exporter, CancellationToken cancellationToken = default)`
Exports the currently stored data using a supplied *exporter* instance.  
- **Parameters**  
  - `exporter`: An `IDataExporter` implementation to write data.  
  - `cancellationToken`: Optional token to cancel the operation.  
- **Return Value**  
  - A completed `Task` when the export finishes.  
- **Exceptions**  
  - `ArgumentNullException` if *exporter* is `null`.  
  - `InvalidOperationException` if there is no data to export or the exporter fails.  
  - `OperationCanceledException` if the operation is cancelled.  

### `public async Task ExportAsync(string source, string destination, IDataExporter exporter, CancellationToken cancellationToken = default)`
Exports data from *source* to *destination* using the provided *exporter*.  
- **Parameters**  
  - `source`: Identifier or path of the data to export.  
  - `destination`: Target location for the exported data.  
  - `exporter`: An `IDataExporter` implementation to perform the export.  
  - `cancellationToken`: Optional token to cancel the operation.  
- **Return Value**  
  - A completed `Task` when the export finishes.  
- **Exceptions**  
  - `ArgumentNullException` if any of *source*, *destination*, or *exporter* is `null` or empty.  
  - `InvalidOperationException` if the export cannot be performed (e.g., unreadable source, unwritable destination).  
  - `OperationCanceledException` if the operation is cancelled.  

## Usage

### Basic pipeline interaction
```csharp
using var executor = new CommandExecutor();

// Ingest sample data
var points = new List<DataPoint> { /* ... */ };
await executor.IngestDataAsync(points);

// Run a processing command
int exitCode = await executor.ExecuteAsync("transform");
if (exitCode != 0)
{
    Console.WriteLine($"Command failed with code {exitCode}");
}

// Query results
var result = await executor.QueryDataAsync("SELECT * WHERE value > 10");
Console.WriteLine($"Retrieved {result.Count} points");

// Export to file
await executor.ExportAsync("./input.csv", "./output.csv");
```

### Using factory methods for custom loader/exporter
```csharp
var loader = CommandExecutor.CreateLoader();   // returns IDataLoader
var exporter = CommandExecutor.CreateExporter(); // returns IDataExporter

var data = await executor.LoadAsync(loader);   // loads via custom loader
await executor.ExportAsync(exporter);          // exports via custom exporter
```

## Notes
- All instance methods are safe to call concurrently **provided** the executor’s internal state is not mutated by another thread during the call. The type does not enforce internal locking; callers should synchronize access if they intend to mutate state (e.g., by changing configuration) while operations are in flight.  
- The static factory methods `CreateLoader` and `CreateExporter` are thread‑safe and may be invoked from any thread without external synchronization.  
- Methods that accept a `CancellationToken` will throw `OperationCanceledException` when cancellation is requested; they respect the token promptly but do not guarantee immediate termination of ongoing I/O.  
- Passing `null` for any reference‑type parameter results in `ArgumentNullException`.  
- If the executor is used after an internal failure (e.g., a prior command threw and left the object in an unusable state), subsequent calls may throw `InvalidOperationException`. In such cases, discard the instance and create a new one.  
- The `VisualizeAsync` method returns a string whose format is implementation‑specific; consumers should not rely on a particular schema unless documented elsewhere.  
- The overloads of `LoadAsync` and `ExportAsync` exist to support both convention‑based (string paths) and explicit‑dependency (loader/exporter instances) scenarios; choose the overload that best matches your application’s dependency‑management strategy.
