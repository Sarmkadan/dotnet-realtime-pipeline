# Extension System

This document describes the plugin and hook APIs defined in `src/Plugins/ExtensionSystem.cs`.

All types are in the `DotNetRealtimePipeline.Plugins` namespace. The extension system provides plugin lifecycle management, typed plugin discovery, configuration storage, and named hooks.

## Plugin Interfaces

### IPipelinePlugin

The base contract implemented by every pipeline plugin.

#### Properties

- `Name`: Gets the plugin name.
- `Version`: Gets the plugin version.

#### Methods

- `Task InitializeAsync()`: Initializes the plugin.
- `Task ShutdownAsync()`: Shuts down the plugin and releases its resources.

### IDataProcessingPlugin

Extends `IPipelinePlugin` for asynchronous data-point processing.

- `Task<ProcessingResult> ProcessAsync(DataPoint dataPoint)`: Processes a data point and returns its processing result.

### IDataTransformPlugin

Extends `IPipelinePlugin` for synchronous data transformation.

- `DataPoint Transform(DataPoint dataPoint)`: Transforms a data point and returns the transformed value.

### IOutputPlugin

Extends `IPipelinePlugin` for asynchronous delivery of processing results.

- `Task OutputAsync(ProcessingResult result)`: Sends or persists a processing result.

A single class may implement more than one specialized interface. It is then returned by each applicable typed query on `PluginManager`.

## PluginManager

`PluginManager` stores registered `IPipelinePlugin` instances and coordinates their lifecycle. Its constructor requires an `ILogger<PluginManager>`.

### RegisterPlugin(IPipelinePlugin plugin)

Adds a plugin to the manager and logs its name and version. Passing `null` throws `ArgumentNullException`. Registration does not initialize the plugin and does not reject duplicate instances or duplicate names.

### InitializeAsync()

Calls `InitializeAsync()` on every registered plugin in registration order. An exception from one plugin is logged and does not prevent later plugins from being initialized. After all attempts finish, `IsInitialized` is set to `true`.

### ShutdownAsync()

Calls `ShutdownAsync()` in reverse registration order. An exception from one plugin is logged and does not prevent the remaining plugins from being shut down. After all attempts finish, `IsInitialized` is set to `false`.

### Plugin Queries

- `GetProcessingPlugins()`: Returns a new `List<IDataProcessingPlugin>` containing registered processing plugins.
- `GetTransformPlugins()`: Returns a new `List<IDataTransformPlugin>` containing registered transform plugins.
- `GetOutputPlugins()`: Returns a new `List<IOutputPlugin>` containing registered output plugins.
- `GetAllPlugins()`: Returns a new `List<IPipelinePlugin>` containing every registered plugin.

Each method returns a snapshot list, so modifying the returned list does not change the manager's registrations. `IsInitialized` reports whether the manager has completed initialization without a subsequent shutdown; it does not indicate that every individual plugin initialized successfully.

## PipelinePluginBase

`PipelinePluginBase` is an abstract implementation of `IPipelinePlugin`. Its constructor requires an `ILogger`, which is exposed to derived classes through the protected `_logger` field.

Derived classes must implement the abstract `Name` and `Version` properties. The virtual `InitializeAsync()` and `ShutdownAsync()` implementations log lifecycle events and otherwise complete immediately. Override either method when a plugin needs its own resource setup or cleanup.

## PluginConfiguration

`PluginConfiguration` is a mutable configuration model with the following properties:

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Name` | `string` | `null` at runtime until assigned | Plugin name used as the registry key. |
| `Version` | `string` | `null` at runtime until assigned | Configured plugin version. |
| `Enabled` | `bool` | `true` | Whether the configuration is included in enabled-configuration queries. |
| `Settings` | `Dictionary<string, object>` | Empty dictionary | Plugin-specific settings. |
| `Dependencies` | `List<string>` | Empty list | Names of plugin dependencies. |

These values are descriptive configuration data. `PluginManager` does not automatically enforce `Enabled`, resolve `Dependencies`, or apply `Settings`.

## PluginRegistry

`PluginRegistry` stores `PluginConfiguration` instances by `Name` and requires an `ILogger<PluginRegistry>`.

- `RegisterConfiguration(PluginConfiguration config)`: Adds the configuration or replaces the existing entry with the same name.
- `GetConfiguration(string pluginName)`: Returns the matching configuration, or `null` when none is registered.
- `GetEnabledConfigurations()`: Returns a new list containing configurations whose `Enabled` property is `true`.
- `GetAllConfigurations()`: Returns a new dictionary containing all registered name-to-configuration mappings.

The returned list and dictionary are new collections, but their values are the same mutable `PluginConfiguration` objects held by the registry.

## LoggingPlugin

`LoggingPlugin` is the included example processing plugin. It derives from `PipelinePluginBase`, implements `IDataProcessingPlugin`, and identifies itself as `Logging Plugin` version `1.0.0`.

`ProcessAsync(DataPoint dataPoint)` logs the data point ID and source, then returns a successful `ProcessingResult` with the plugin name as its stage, a processing time of one millisecond, and the current UTC time.

## HookManager

`HookManager` maintains ordered lists of delegates under string hook names. Its constructor requires an `ILogger<HookManager>`.

### RegisterHook(string hookName, Delegate handler)

Appends a delegate to the named hook. Multiple handlers can be registered for the same name and are retained in registration order.

### ExecuteHookAsync(string hookName, params object[] args)

Invokes every handler registered for the name, in registration order, using `Delegate.DynamicInvoke`. If a handler returns a `Task`, execution awaits it before moving to the next handler. Other return values are ignored. A missing hook is a no-op.

Arguments must match each registered delegate's signature at runtime. Handler exceptions, including argument mismatch errors, are logged and do not prevent later handlers from running.

## Example: Registering a Plugin and a Hook

The following example registers the included `LoggingPlugin`, runs its lifecycle, and executes an asynchronous hook:

```csharp
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Plugins;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

var pluginManager = new PluginManager(
    loggerFactory.CreateLogger<PluginManager>());

var loggingPlugin = new LoggingPlugin(
    loggerFactory.CreateLogger<LoggingPlugin>());

pluginManager.RegisterPlugin(loggingPlugin);
await pluginManager.InitializeAsync();

var dataPoint = new DataPoint(
    id: 1,
    timestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    value: 42.5,
    source: "sensor-1");

foreach (var plugin in pluginManager.GetProcessingPlugins())
{
    ProcessingResult result = await plugin.ProcessAsync(dataPoint);
    Console.WriteLine($"{plugin.Name}: success={result.Success}");
}

var hookManager = new HookManager(
    loggerFactory.CreateLogger<HookManager>());

hookManager.RegisterHook(
    "data-received",
    new Func<DataPoint, Task>(point =>
    {
        Console.WriteLine($"Received data point {point.Id}");
        return Task.CompletedTask;
    }));

await hookManager.ExecuteHookAsync("data-received", dataPoint);
await pluginManager.ShutdownAsync();
```

## Notes

- `PluginManager`, `PluginRegistry`, and `HookManager` do not synchronize access to their mutable collections. Coordinate registration and execution externally if they are used concurrently.
- Plugin registration and configuration registration are separate operations; registering one does not automatically register the other.
- Hook names are plain strings, so callers should centralize commonly used names to avoid mismatches.
