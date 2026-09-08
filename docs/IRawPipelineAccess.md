# IRawPipelineAccess

`IRawPipelineAccess` is the opt-in integration contract for components that expose their underlying `System.IO.Pipelines.PipeReader` and `PipeWriter`. It is intended for callers that need direct, low-allocation access to a pipeline instead of the higher-level data-stream abstraction.

`RawPipelineAccessor` is the default implementation. See [RawPipelineAccessor](RawPipelineAccessor.md) for its construction, reset, and disposal APIs.

## Interface

```csharp
public interface IRawPipelineAccess
{
    PipeReader AsPipeReader();
    PipeWriter AsPipeWriter();
}
```

Both methods return the live endpoint owned by the underlying pipe, not a snapshot. A pipe supports one active reader and one active writer. One reader and one writer may operate concurrently, but multiple concurrent operations on the same side are not supported. The extension methods follow the same constraint and do not add synchronization.

The caller must also observe normal `System.IO.Pipelines` lifetime rules. In particular, complete the writer to signal end-of-input and complete endpoints when the caller owns their lifetime. Do not dispose or reset an owning component while an operation is in progress.

## Extension methods

The methods below are defined by `RawPipelineAccessExtensions` in the `DotNetRealtimePipeline.Integration` namespace.

### `CopyToAsync`

```csharp
ValueTask CopyToAsync(
    this IRawPipelineAccess access,
    IBufferWriter<byte> writer,
    CancellationToken cancellationToken = default)
```

Reads segments from the pipe and copies them to an `IBufferWriter<byte>`. The operation finishes after the pipe reader reports completion and all buffered bytes have been copied. It does not complete the source reader or the destination writer.

`access` and `writer` must be non-null. Cancellation is observed by the underlying `PipeReader.ReadAsync` call.

### `CopyToStreamAsync`

```csharp
ValueTask CopyToStreamAsync(
    this IRawPipelineAccess access,
    Stream stream,
    CancellationToken cancellationToken = default)
```

Reads every segment from the pipe and writes it to `stream`. The operation finishes after the source writer has completed and all buffered bytes have been written. It does not complete the reader, flush or dispose the destination stream, or change the stream position.

`access` and `stream` must be non-null. The cancellation token is passed to pipe reads and stream writes.

### `ReadAllAsync`

```csharp
ValueTask<byte[]> ReadAllAsync(
    this IRawPipelineAccess access,
    CancellationToken cancellationToken = default)
```

Consumes the pipe through `CopyToStreamAsync` and returns all bytes as one array. Because it buffers the entire payload in memory, prefer a streaming method for large or unbounded input. The operation waits for the pipe writer to complete.

### `WriteAsync`

```csharp
ValueTask WriteAsync(
    this IRawPipelineAccess access,
    ReadOnlyMemory<byte> data,
    CancellationToken cancellationToken = default)
```

Writes `data` to the exposed `PipeWriter` and immediately calls `FlushAsync`. It does not complete the writer. The returned `ValueTask` completes when the flush completes; cancellation is passed to the flush operation.

### `GetPipeReader`

```csharp
PipeReader GetPipeReader(this object source)
```

Probes an untyped object for `IRawPipelineAccess` and returns its live reader. It throws `ArgumentNullException` when `source` is null and `NotSupportedException` when the object does not implement `IRawPipelineAccess`.

Use `AsPipeReader()` directly when the value is already typed as `IRawPipelineAccess`.

### `GetPipeWriter`

```csharp
PipeWriter GetPipeWriter(this object source)
```

Probes an untyped object for `IRawPipelineAccess` and returns its live writer. It throws `ArgumentNullException` when `source` is null and `NotSupportedException` when the object does not implement `IRawPipelineAccess`.

Use `AsPipeWriter()` directly when the value is already typed as `IRawPipelineAccess`.

## Example

This example writes a message with the convenience API, signals end-of-input with the underlying `PipeWriter`, and consumes the message with a `System.IO.Pipelines.PipeReader`.

```csharp
using System;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using DotNetRealtimePipeline.Integration;

using var pipeline = new RawPipelineAccessor();
IRawPipelineAccess access = pipeline;

await access.WriteAsync(Encoding.UTF8.GetBytes("hello pipeline"));
await access.AsPipeWriter().CompleteAsync();

PipeReader reader = access.AsPipeReader();

while (true)
{
    ReadResult result = await reader.ReadAsync();
    var buffer = result.Buffer;

    foreach (var segment in buffer)
    {
        Console.Write(Encoding.UTF8.GetString(segment.Span));
    }

    reader.AdvanceTo(buffer.End);

    if (result.IsCompleted)
    {
        break;
    }
}

await reader.CompleteAsync();
```

For an untyped value, the probing helpers expose the same endpoints:

```csharp
object component = pipeline;
PipeReader reader = component.GetPipeReader();
PipeWriter writer = component.GetPipeWriter();
```

These calls do not create a new pipe or take ownership of either endpoint.
