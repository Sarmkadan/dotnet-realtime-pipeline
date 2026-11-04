# Output Formatters

This document describes the output formatters available in the `DotNetRealtimePipeline.Formatters` namespace, along with the `OutputFormat` enum and `OutputFormatterFactory`.

## Overview

The pipeline supports five output formats for serializing `DataPoint` data:

- **Json** - JSON serialization with indented formatting
- **Csv** - Comma-separated values format
- **Table** - ASCII table format
- **Html** - HTML report format
- **Default** - Falls back to JSON

The `OutputFormatterFactory` creates the appropriate formatter based on the `OutputFormat` enum value.

## OutputFormat Enum

```csharp
public enum OutputFormat
{
    Json,
    Csv,
    Table,
    Html
}
```

## Formatters

### JsonOutputFormatter

Serializes data to JSON format with indented formatting and `PropertyNameCaseInsensitive = true`.

**Sample output for a single `DataPoint`:**

```json
{
  "Id": 1,
  "Timestamp": 1730678400000,
  "Value": 42.5,
  "Source": "sensor-1",
  "Metadata": {},
  "CreatedAt": "2025-01-01T00:00:00Z",
  "Tags": "tag1"
}
```

### CsvOutputFormatter

Serializes `List<DataPoint>` or `List<ProcessingResult>` to CSV format. For `List<DataPoint>`, the header row is `Id,Timestamp,Value,Source,Quality,Tags`.

**Sample output for a single `DataPoint`:**

```
Id,Timestamp,Value,Source,Quality,Tags
1,1730678400000,42.5000,sensor-1,90,tag1
```

### TableOutputFormatter

Displays data in an ASCII table format. For `List<DataPoint>`, shows up to 10 rows with column headers, and appends `... and N more rows` if there are more than 10 data points.

**Sample output for 3 `DataPoint` objects:**

```
Id  Timestamp     Value   Source  Quality
-------------------------------------
1   1730678400000 42.5000 sensor-1 90   
2   1730678400001 43.1000 sensor-2 85   
3   1730678400002 41.2500 sensor-3 95   
... and 0 more rows
```

### HtmlOutputFormatter

Generates an HTML report with a styled table. Shows up to 50 rows, with a note if there are more records.

**Sample output for a single `DataPoint`:**

```html
<!DOCTYPE html>
<html>
<head>
<meta charset="utf-8">
<title>Pipeline Data Report</title>
<style>
body { font-family: Arial, sans-serif; margin: 20px; }
table { border-collapse: collapse; width: 100%; }
th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
th { background-color: #4CAF50; color: white; }
</style>
</head>
<body>
<h1>Pipeline Data Report</h1>

<table>
<tr><th>Id</th><th>Timestamp</th><th>Value</th><th>Source</th><th>Quality</th></tr>
<tr><td>1</td><td>1730678400000</td><td>42.5000</td><td>sensor-1</td><td>90</td></tr>
</table>

</body>
</html>
```

### OutputFormatterFactory

Creates an `IOutputFormatter` for the specified `OutputFormat`.

```csharp
public static IOutputFormatter Create(OutputFormat format)
```

**Switch statement mapping:**

| Format | Formatter Created |
|--------|-------------------|
| `OutputFormat.Json` | `JsonOutputFormatter` |
| `OutputFormat.Csv` | `CsvOutputFormatter` |
| `OutputFormat.Table` | `TableOutputFormatter` |
| `OutputFormat.Html` | `HtmlOutputFormatter` |
| **unsupported** | `JsonOutputFormatter` (default fallback) |

**Usage example:**

```csharp
var formatter = OutputFormatterFactory.Create(OutputFormat.Csv);
var csv = formatter.Format(new List<DataPoint>
{
    new DataPoint(1, 1730678400000, 42.5, "sensor-1") { Quality = 90, Tags = "tag1" }
});
```

## Sample DataPoint

The formatters were designed with the following `DataPoint` in mind:

```csharp
var point = new DataPoint
{
    Id = 1,
    Timestamp = 1730678400000, // Unix timestamp in milliseconds
    Value = 42.5,
    Source = "sensor-1",
    Quality = 90,
    Tags = "tag1",
    CreatedAt = DateTime.UtcNow
};
```