# Pipeline JSON utilities

`PipelineJsonUtilities` is the internal home for JSON settings and defensive limits that can be shared by code inside the main pipeline assembly. It is implemented in `src/Utilities/PipelineJsonUtilities.cs` and uses `System.Text.Json`.

## Shared serializer options

`PipelineJsonUtilities.JsonSerializerOptions` is a single, cached `JsonSerializerOptions` instance based on `JsonSerializerDefaults.Web`. It configures:

- camel-case property names;
- compact output (`WriteIndented` is `false`);
- omission of properties whose value is `null`;
- case-insensitive property matching during deserialization;
- a maximum JSON depth of 64; and
- enum values represented as camel-case strings through `JsonStringEnumConverter`.

Code that needs indented output should copy the shared instance and change the copy:

```csharp
var options = new JsonSerializerOptions(PipelineJsonUtilities.JsonSerializerOptions)
{
    WriteIndented = true
};
```

This avoids changing the shared instance for every other caller.

## Limits and validation helpers

The class defines the following internal limits:

| Member | Value | Purpose |
| --- | ---: | --- |
| `MaxJsonDepth` | 64 | Caps serializer nesting through the shared options. |
| `MaxJsonPayloadSizeBytes` | 10,000,000 | Threshold used by `ValidateJsonPayloadSize`. |
| `MaxArraySize` | 100,000 | Common limit available to collection-validation code. |
| `MaxDictionarySize` | 10,000 | Common limit available to dictionary-validation code. |
| `MaxStringLength` | 100,000 | Threshold used by `ValidateJsonStringLength`. |

`ValidateJsonPayloadSize(string json)` throws `ArgumentOutOfRangeException` when `json.Length` exceeds 10,000,000. Despite the member name and exception text referring to bytes, the implementation measures the .NET string's UTF-16 character count; it does not calculate an encoded byte count.

`ValidateJsonStringLength(string json)` returns `true` when the string contains at most 100,000 characters and `false` otherwise. Neither helper performs a null check, so callers should apply their normal null/empty validation first.

`MaxArraySize` and `MaxDictionarySize` are declared policy limits but are not enforced by `JsonSerializerOptions` or by either helper. Callers that accept untrusted collection data must enforce them explicitly. `ValidateJsonStringLength` likewise has no direct caller at present.

## Relationship to the `*JsonExtensions` classes

The repository's many `*JsonExtensions` classes provide type-specific `ToJson`, `FromJson`, and usually `TryFromJson` APIs. Their recurring pattern is to cache compact serializer options, clone those options when `indented: true` is requested, and use the same options for serialization and deserialization.

Two extension classes currently depend on `PipelineJsonUtilities` directly:

- `ApiEndpointHandlerJsonExtensions` reuses `JsonSerializerOptions` and validates payload size before deserialization.
- `EventSubscriberBaseJsonExtensions` does the same for event subscribers.

Most of the other extension classes currently own a local `JsonSerializerOptions` instance rather than referencing the utility. For example, `DataPointJsonExtensions` and `PipelineConfigJsonExtensions` independently configure web defaults, camel-case names, null omission, compact output, and camel-case string enums. They therefore follow the central utility's serialization conventions, but they do not inherit later changes to it automatically and do not currently call its payload-size validator. Their input checks and malformed-JSON behavior also remain type-specific: `DataPointJsonExtensions.FromJson` catches `JsonException` and returns `null`, whereas `PipelineConfigJsonExtensions.FromJson` lets `JsonException` propagate.

Because both the utility and those two model extension classes are `internal`, they are implementation details of the main assembly rather than a public configuration surface. Public extension classes expose the callable JSON API for their corresponding public types.

## Example

The following code, when used inside the main pipeline assembly, serializes a data point with its type-specific extension and reads it back. The JSON uses camel-case property names and omits `Tags` while it is `null`.

```csharp
using DotNetRealtimePipeline.Domain.Models;

var point = new DataPoint(
    id: 42,
    timestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    value: 21.5,
    source: "sensor-a");

string json = point.ToJson(indented: true);
DataPoint? copy = DataPointJsonExtensions.FromJson(json);
```

When adding or updating a JSON extension, preserve its documented type-specific error semantics. Reuse `PipelineJsonUtilities.JsonSerializerOptions` when the shared policy is intended, clone it for per-call formatting changes, and call the appropriate validation helper before deserializing untrusted input.
