# RawPipelineAccessor

A lightweight, unmanaged wrapper around a duplex pipe that provides direct access to the underlying `PipeReader` and `PipeWriter` while exposing minimal lifecycle management. Designed for scenarios where low-level control over pipe operations is required without the overhead of higher-level abstractions.

## API

### Constructors

#### `public RawPipelineAccessor()`
Initializes a new instance using `PipeOptions.Default` for the underlying pipe configuration.

#### `public RawPipelineAccessor(PipeOptions options)`
Initializes a new instance with the specified pipe configuration options.

| Parameter | Description |
|-----------|-------------|
| `options` | Pipe configuration including buffer sizes, pause behavior, and reader/writer options. |

### Properties

#### `public PipeReader AsPipeReader`
Gets the `PipeReader` instance associated with the underlying pipe. The returned reader is valid for the lifetime of the `RawPipelineAccessor` unless `Reset` or `Dispose` is called.

| Return Value | Description |
|-------------|-------------|
| `PipeReader` | A reader that can be used to consume data from the pipe. |

#### `public PipeWriter AsPipeWriter`
Gets the `PipeWriter` instance associated with the underlying pipe. The returned writer is valid for the lifetime of the `RawPipelineAccessor` unless `Reset` or `Dispose` is called.

| Return Value | Description |
|-------------|-------------|
| `PipeWriter` | A writer that can be used to produce data to the pipe. |

### Methods

#### `public void Reset()`
Resets the underlying pipe to its initial state, clearing any buffered data and resetting the reader and writer positions. This method does not dispose of the current reader or writer; new instances are created on subsequent accesses.

| Throws | Condition |
|--------|-----------|
| `ObjectDisposedException` | If the `RawPipelineAccessor` has been disposed. |

#### `public void Dispose()`
Releases all resources held by the `RawPipelineAccessor`, including the underlying pipe and any associated buffers. After disposal, all properties and methods will throw `ObjectDisposedException` if accessed.

## Usage

### Basic Usage
```csharp
using var accessor = new RawPipelineAccessor();
await accessor.AsPipeWriter.WriteAsync(new ReadOnlyMemory<byte>(new byte[] { 0x01, 0x02 }));
var result = await accessor.AsPipeReader.ReadAsync();
Console.WriteLine($"Read {result.Buffer.Length} bytes");
```

### Resetting a Pipeline
```csharp
using var accessor = new RawPipelineAccessor();
await accessor.AsPipeWriter.WriteAsync(new ReadOnlyMemory<byte>(new byte[] { 0xFF }));
accessor.Reset();
await accessor.AsPipeWriter.WriteAsync(new ReadOnlyMemory<byte>(new byte[] { 0xAA }));
var result = await accessor.AsPipeReader.ReadAsync();
Console.WriteLine($"Read {result.Buffer.Length} bytes after reset");
```

## Notes

- **Thread Safety**: The `RawPipelineAccessor` is not thread-safe. Concurrent calls to `AsPipeReader`, `AsPipeWriter`, `Reset`, or `Dispose` may result in undefined behavior or exceptions.
- **Disposal**: Once disposed, the instance cannot be reused. Any attempt to access properties or methods will throw `ObjectDisposedException`.
- **Resource Cleanup**: The underlying pipe and buffers are released when `Dispose` is called. Ensure proper disposal in scenarios involving long-lived pipelines or high-throughput operations.
- **Reset Behavior**: Calling `Reset` does not release resources; it only clears the current pipe state. This allows reuse of the same `RawPipelineAccessor` instance without reallocation, but care must be taken to avoid resource leaks in scenarios involving frequent resets.
