# OutputFormatter

The output formatter types in `DotNetRealtimePipeline.Formatters` convert reference-type data into JSON, CSV, plain-text table, or HTML output. All four implementations share the `IOutputFormatter` contract and can be selected through `OutputFormatterFactory.Create(OutputFormat)`.

## API

| Member | Description |
| --- | --- |
| `IOutputFormatter.Format<T>(T data) where T : class` | Formats `data` synchronously and returns the resulting text. |
| `IOutputFormatter.FormatAsync<T>(T data) where T : class` | Formats `data` asynchronously and returns a `Task<string>`. Each current implementation delegates to its synchronous `Format` method. |
| `JsonOutputFormatter` | Serializes any supported reference-type value with `System.Text.Json`. Output is indented, and its serializer options enable case-insensitive property-name matching. |
| `CsvOutputFormatter` | Produces CSV for `List<DataPoint>` and `List<ProcessingResult>`. Other input types produce an empty string. |
| `TableOutputFormatter` | Produces an ASCII table for `List<DataPoint>` or `Dictionary<string, object>`. Other input types are serialized as compact JSON. |
| `HtmlOutputFormatter` | Produces a complete HTML report. A `List<DataPoint>` is rendered as a table; other input types produce the report shell without a data table. |
| `OutputFormatterFactory.Create(OutputFormat format)` | Returns the formatter selected by `format`. An unrecognized enum value falls back to `JsonOutputFormatter`. |

### `OutputFormat`

| Value | Formatter created |
| --- | --- |
| `OutputFormat.Json` | `JsonOutputFormatter` |
| `OutputFormat.Csv` | `CsvOutputFormatter` |
| `OutputFormat.Table` | `TableOutputFormatter` |
| `OutputFormat.Html` | `HtmlOutputFormatter` |

## Usage

```csharp
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Formatters;
using System;
using System.Collections.Generic;

var dataPoints = new List<DataPoint>
{
    new DataPoint(
        id: 1,
        timestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        value: 42.125,
        source: "sensor-a")
};

IOutputFormatter jsonFormatter =
    OutputFormatterFactory.Create(OutputFormat.Json);
string json = jsonFormatter.Format(dataPoints);

IOutputFormatter tableFormatter =
    OutputFormatterFactory.Create(OutputFormat.Table);
string table = await tableFormatter.FormatAsync(dataPoints);

Console.WriteLine(json);
Console.WriteLine(table);
```

## Notes

* `Format<T>` and `FormatAsync<T>` constrain `T` to reference types with `where T : class`.
* The current asynchronous methods do not perform separate asynchronous I/O; they return the result of the synchronous formatter through `Task.FromResult`.
* CSV output recognizes the exact runtime types `List<DataPoint>` and `List<ProcessingResult>`. It includes a header row, formats `DataPoint.Value` to four decimal places using invariant culture, and quotes fields containing commas, quotation marks, or newlines.
* A `DataPoint` table displays at most 10 data rows. When more records are supplied, it appends `... and N more rows`. The formatter still examines the entire list when calculating column widths.
* An empty `List<DataPoint>` formatted as a table returns `No data points`. Dictionary tables include `Key` and `Value` columns; an empty dictionary is not specially handled.
* HTML reports display at most 50 `DataPoint` rows and add a record-count message when the input contains more than 50 items. Values are inserted directly into the generated markup without HTML encoding.
* `JsonOutputFormatter` uses a shared `JsonSerializerOptions` instance with `WriteIndented` and `PropertyNameCaseInsensitive` set to `true`.
