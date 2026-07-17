# ApiEndpointHandlerExtensions

ApiEndpointHandlerExtensions provides a set of static helper methods and associated properties for constructing strongly‑typed API response objects used throughout the dotnet-realtime-pipeline project. The members simplify the creation of successful, error, paginated, and batch‑status responses while exposing the underlying data structures for inspection.

## API

### Ok<T>
**Purpose**  
Creates a successful API response containing a value of type `T`.

**Parameters**  
- `result` – The payload to include in the response.

**Return value**  
An instance of `ApiEndpointHandler.ApiResponse<T>` representing an HTTP 200 OK response with `result` stored in its `Data` property.

**Throws**  
- `ArgumentNullException` if `result` is `null` and the response type does not allow null payloads.

### Error<T>
**Purpose**  
Creates an API response that conveys an error condition.

**Parameters**  
- `errorMessage` – A descriptive message explaining the failure.

**Return value**  
An instance of `ApiEndpointHandler.ApiResponse<T>` representing an HTTP 4xx/5xx response with the error message stored in its `Error` property.

**Throws**  
- `ArgumentException` if `errorMessage` is empty or consists only of whitespace.

### ToPaginatedResponse<T>
**Purpose**  
Wraps an enumerable sequence into a paginated response, calculating paging metadata.

**Parameters**  
- `source` – The sequence of items to paginate.  
- `page` – The 1‑based page number to return.  
- `pageSize` – The maximum number of items per page.

**Return value**  
An instance of `ApiEndpointHandler.ApiResponse<PaginatedResponse<T>>` where the `Data` property contains the requested page of items along with `Page`, `PageSize`, `TotalCount`, and `TotalPages` metadata.

**Throws**  
- `ArgumentNullException` if `source` is `null`.  
- `ArgumentOutOfRangeException` if `page` is less than 1 or `pageSize` is less than 1.

### WithBatchStats
**Purpose**  
Attaches batch processing statistics to an API response.

**Parameters**  
- `batchResult` – An object containing statistics such as items processed, succeeded, and failed.

**Return value**  
An instance of `ApiEndpointHandler.ApiResponse<BatchIngestResult>` with the supplied statistics encapsulated in the `Data` property.

**Throws**  
- `ArgumentNullException` if `batchResult` is `null`.

### Items
**Purpose**  
Gets the read‑only collection of items contained in a paginated response.

**Property value**  
`IReadOnlyList<T>` representing the items for the current page.

### Page
**Purpose**  
Gets the 1‑based index of the current page in a paginated response.

**Property value**  
An `int` indicating the page number.

### PageSize
**Purpose**  
Gets the maximum number of items that a page can contain.

**Property value**  
An `int` indicating the page size.

### TotalCount
**Purpose**  
Gets the total number of items available across all pages.

**Property value**  
An `int` indicating the overall count.

### TotalPages
**Purpose**  
Gets the total number of pages required to expose all items given the current page size.

**Property value**  
An `int` indicating the total page count.

## Usage

```csharp
// Successful response with a simple payload
var okResponse = ApiEndpointHandlerExtensions.Ok<string>("Hello, world!");
// okResponse.Data == "Hello, world!"
// okResponse.IsSuccess == true
```

```csharp
// Creating a paginated response from a collection
var items = Enumerable.Range(1, 125); // 125 numbers
var page = 3;
var pageSize = 20;

var paged = ApiEndpointHandlerExtensions.ToPaginatedResponse<int>(items, page, pageSize);
// paged.Data.Items contains values 41‑60
// paged.Data.Page == 3
// paged.Data.PageSize == 20
// paged.Data.TotalCount == 125
// paged.Data.TotalPages == 7
```

## Notes

- All extension methods are static and therefore thread‑safe; they rely only on their input parameters and do not modify shared state.  
- The properties `Items`, `Page`, `PageSize`, `TotalCount`, and `TotalPages` are read‑only and reflect the state of the `PaginatedResponse<T>` instance at the time it was created; mutating the source sequence after the response is generated will not affect these values.  
- When using `ToPaginatedResponse<T}`, supplying a `page` value greater than `TotalPages` will result in an empty `Items` list but will not throw an exception.  
- Nullability of the payload in `Ok<T>` and `Error<T>` follows the underlying `ApiEndpointHandler.ApiResponse<T>` implementation; consult that type’s documentation for precise null‑handling rules.  
- The `WithBatchStats` method does not perform any validation on the contents of `BatchIngestResult` beyond null checking; callers should ensure the result object is in a valid state before passing it.
