# ServiceCollectionExtensionsJsonExtensions

## Overview
`ServiceCollectionExtensionsJsonExtensions` provides helper members for JSON serialization/deserialization and metadata inspection of service types used within the dotnet-realtime-pipeline library.

## API
### ToJson
- **Purpose:** Serializes an object to its JSON representation.
- **Parameters:** `object value` – the object to serialize.
- **Return value:** A string containing the JSON representation of `value`.
- **Exceptions:** 
  - `ArgumentNullException` if `value` is `null`.
  - `JsonSerializationException` if serialization fails.

### FromJson
- **Purpose:** Deserializes a JSON string into an object.
- **Parameters:** `string json` – the JSON to deserialize.
- **Return value:** The deserialized object, or `null` if `json` is `null` or cannot be deserialized.
- **Exceptions:** 
  - `ArgumentNullException` if `json` is `null`.
  - `JsonSerializationException` if `json` is malformed.

### TryFromJson
- **Purpose:** Attempts to deserialize a JSON string into an object, indicating success via a Boolean return.
- **Parameters:** 
  - `string json` – the JSON to deserialize.
  - `out object? result` – receives the deserialized object or `null` on failure.
- **Return value:** `true` if deserialization succeeded; `false` otherwise.
- **Exceptions:** None (failure is indicated by the return value, not an exception).

### Type
- **Purpose:** Gets the fully qualified name of the service type associated with the instance.
- **Return value:** `string?` containing the type name, or `null` if not set.
- **Exceptions:** None.

### IsStaticClass
- **Purpose:** Indicates whether the declaring type of the service is a static class.
- **Return value:** `true` if the type is static; otherwise `false`.
- **Exceptions:** None.

### SupportsAddPipelineServices
- **Purpose:** Indicates whether the service can be added to the pipeline via `AddPipelineServices`.
- **Return value:** `true` if supported; `false` otherwise.
- **Exceptions:** None.

## Usage
### Example 1: Serializing and deserializing a settings object
```csharp
var settings = new { Interval = 5, Enabled = true };
string json = ServiceCollectionExtensionsJsonExtensions.ToJson(settings);
// json => "{\"Interval\":5,\"Enabled\":true}"

if (ServiceCollectionExtensionsJsonExtensions.TryFromJson(json, out var obj))
{
    var restored = obj; // use restored
}
```

### Example 2: Checking service metadata before registration
```csharp
var descriptor = new ServiceCollectionExtensionsJsonExtensions
{
    Type = typeof(IMyService).FullName,
    IsStaticClass = false,
    SupportsAddPipelineServices = true
};

if (!descriptor.IsStaticClass && descriptor.SupportsAddPipelineServices)
{
    // safe to call services.AddPipelineServices<IMyService, MyServiceImpl>()
}
```

## Notes
- The JSON methods (`ToJson`, `FromJson`, `TryFromJson`) use the default `JsonSerializerOptions` and are not configurable via this type.
- Thread safety: The static JSON methods are thread-safe as they rely on the thread-safe `JsonSerializer`. Instance properties are read-only after initialization and safe for concurrent read.
- `FromJson` returns `null` for a `null` input and does not throw; `TryFromJson` returns `false` with `result` set to `null` in that case.
- The `Type` property may be `null` if the descriptor was created without specifying a type; callers should validate before use.
- `IsStaticClass` is determined at construction based on the type's attributes and does not change after instantiation.
- `SupportsAddPipelineServices` reflects whether the service type implements the required pipeline service interface; attempting to register an unsupported type may result in runtime registration failures.
