# PluginManager

Documentation for `PluginManager`, `PipelinePluginBase` and `LoggingPlugin` from `src/Plugins/ExtensionSystem.cs`.

## Overview

`PluginManager` is a sealed class that manages the registration, initialization and shutdown of pipeline plugins implementing `IPipelinePlugin`. It maintains an internal list of registered plugins and provides typed accessors for retrieving plugins by their specialized interface.

## Lifecycle

### RegisterPlugin(IPipelinePlugin plugin)

Adds a plugin to the internal registry. Throws `ArgumentNullException` if `plugin` is null. Logs the plugin's `Name` and `Version` on registration.

### InitializeAsync()

Iterates over all registered plugins and calls `InitializeAsync()` on each. Errors during individual plugin initialization are caught and logged - they do not prevent other plugins from initializing. After all plugins have been processed, `IsInitialized` is set to `true`.

### ShutdownAsync()

Iterates over all registered plugins in reverse order and calls `ShutdownAsync()` on each. Errors during individual plugin shutdown are caught and logged. After all plugins have been processed, `IsInitialized` is set to `false`.

### IsInitialized

A `bool` property indicating whether `InitializeAsync()` has completed. Backed by a `volatile` field for thread-safe reads.

## Typed Getters

- `GetProcessingPlugins()` - returns a `List<IDataProcessingPlugin>` of all registered plugins implementing `IDataProcessingPlugin`.
- `GetTransformPlugins()` - returns a `List<IDataTransformPlugin>` of all registered plugins implementing `IDataTransformPlugin`.
- `GetOutputPlugins()` - returns a `List<IOutputPlugin>` of all registered plugins implementing `IOutputPlugin`.
- `GetAllPlugins()` - returns a defensive copy (`List<IPipelinePlugin>`) of all registered plugins regardless of type.

## PipelinePluginBase

An abstract base class implementing `IPipelinePlugin`. Provides:

- A constructor accepting `ILogger` (throws `ArgumentNullException` if null), stored in the `_logger` field.
- Abstract `Name` and `Version` properties that derived classes must implement.
- Virtual `InitializeAsync()` and `ShutdownAsync()` methods that log the operation and return a completed task. Derived classes can override these to add custom initialization or teardown logic.

## LoggingPlugin

A sealed class extending `PipelinePluginBase` and implementing `IDataProcessingPlugin`. Serves as a reference implementation for custom plugins.

- **Name:** `"Logging Plugin"`
- **Version:** `"1.0.0"`
- **ProcessAsync(DataPoint dataPoint):** validates the data point (throws `ArgumentNullException` if null), logs the data point's `Id` and `Source` at debug level, and returns a successful `ProcessingResult` with `StageName` set to the plugin name and a fixed processing time of 1 ms.

## Writing a Custom Plugin

```csharp
public sealed class MyTransformPlugin : PipelinePluginBase, IDataTransformPlugin
{
    public override string Name => "My Transform";
    public override string Version => "1.0.0";

    public MyTransformPlugin(ILogger<MyTransformPlugin> logger) : base(logger) { }

    public DataPoint Transform(DataPoint dataPoint)
    {
        ArgumentNullException.ThrowIfNull(dataPoint);
        // Apply transformation logic here
        return dataPoint;
    }
}

// Registration
var manager = new PluginManager(loggerFactory.CreateLogger<PluginManager>());
manager.RegisterPlugin(new MyTransformPlugin(loggerFactory.CreateLogger<MyTransformPlugin>()));
await manager.InitializeAsync();

// Retrieve and use
var transforms = manager.GetTransformPlugins();
foreach (var plugin in transforms)
{
    dataPoint = plugin.Transform(dataPoint);
}

// Cleanup
await manager.ShutdownAsync();
```
