# PluginConfiguration and PluginRegistry

`PluginConfiguration` is a mutable settings model that describes one plugin: its name and version, whether it is enabled, arbitrary settings, and the names of plugins it depends on. `PluginRegistry` stores these configurations and looks them up by name.

Both types live in `src/Plugins/ExtensionSystem.cs`, in the `DotNetRealtimePipeline.Plugins` namespace, alongside `IPipelinePlugin`, `PluginManager` and `HookManager`.

`PluginRegistry` does not create, load or discover plugin instances. It only holds configuration. The host decides which plugins to build from the configurations it reads, and it still registers the instances with `PluginManager`. The registry also does not check `Dependencies`: it stores the list as given and never resolves, orders or validates it.

## PluginConfiguration

A `sealed` class with public settable properties and no constructor parameters.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Name` | `string` | none (`null`) | Unique key used by `PluginRegistry`. Should match the `IPipelinePlugin.Name` of the plugin it configures. |
| `Version` | `string` | none (`null`) | Plugin version, typically matching `IPipelinePlugin.Version`. Used only for logging and display. |
| `Enabled` | `bool` | `true` | When `false`, the configuration is left out of `GetEnabledConfigurations()`. |
| `Settings` | `Dictionary<string, object>` | empty dictionary | Free-form plugin settings. The registry does not interpret the values. |
| `Dependencies` | `List<string>` | empty list | Names of other plugins this plugin requires. Informational only; see above. |

`Name` and `Version` have no initializer and are not validated, so set both before registering the configuration. `Name` must not be `null`, because it is used as a dictionary key (see `RegisterConfiguration` below).

### ToString()

Returns a single line with the name, version, enabled flag and the number of settings and dependencies. Settings values are not included, so the output is safe to log:

```
PluginConfiguration(Name=Logging Plugin, Version=1.0.0, Enabled=True, SettingsCount=2, DependenciesCount=0)
```

## PluginRegistry

A `sealed` class that requires an `ILogger<PluginRegistry>` in its constructor. A null logger throws `ArgumentNullException`.

```csharp
public PluginRegistry(ILogger<PluginRegistry> logger)
```

Configurations are kept in a private `Dictionary<string, PluginConfiguration>` keyed by `Name`. The registry is not thread-safe: coordinate registration and lookups externally if they happen concurrently.

### RegisterConfiguration

```csharp
public void RegisterConfiguration(PluginConfiguration config)
```

Adds the configuration, or replaces the existing entry with the same `Name`. The new object replaces the old one; it is not merged with it.

- **Throws `ArgumentNullException`** if `config` is `null`.
- **Throws `ArgumentNullException`** (from the dictionary) if `config.Name` is `null`.
- Logs at Information level: `Plugin configuration registered: {Name} v{Version}`.

The registry stores the reference, not a copy. Later changes to the object are visible through the registry.

### GetConfiguration

```csharp
public PluginConfiguration GetConfiguration(string pluginName)
```

Returns the configuration registered under `pluginName`, or `null` if there is none. The result is returned regardless of `Enabled`; check that flag yourself if it matters.

- **Throws `ArgumentException`** if `pluginName` is `null`, empty or whitespace (`ArgumentException.ThrowIfNullOrWhiteSpace`).
- Lookup is case-sensitive, because the dictionary uses the default string comparer.

### GetEnabledConfigurations

```csharp
public List<PluginConfiguration> GetEnabledConfigurations()
```

Returns a new `List<PluginConfiguration>` with every configuration whose `Enabled` is `true`. The list is a snapshot: it is not updated when the registry changes. The order is the dictionary's enumeration order, which is not guaranteed; sort the result if order matters.

### GetAllConfigurations

```csharp
public Dictionary<string, PluginConfiguration> GetAllConfigurations()
```

Returns a copy of the internal dictionary, including disabled configurations. Changing the copy (adding or removing keys) does not affect the registry. The values are the same `PluginConfiguration` objects, so changing a value's properties does affect the registry.

## Example

The following example registers two configurations, one disabled, and reads them back. It uses `NullLogger` from `Microsoft.Extensions.Logging.Abstractions` (available through the `Microsoft.Extensions.Logging` package the project already references) so it runs without logging setup.

```csharp
using DotNetRealtimePipeline.Plugins;
using Microsoft.Extensions.Logging.Abstractions;

var registry = new PluginRegistry(NullLogger<PluginRegistry>.Instance);

registry.RegisterConfiguration(new PluginConfiguration
{
    Name = "Logging Plugin",
    Version = "1.0.0",
    Settings = new Dictionary<string, object>
    {
        ["Level"] = "Debug",
        ["IncludePayload"] = false
    }
});

registry.RegisterConfiguration(new PluginConfiguration
{
    Name = "Metrics Plugin",
    Version = "2.1.0",
    Enabled = false,
    Dependencies = new List<string> { "Logging Plugin" }
});

// Lookup by name; returns null for unknown names.
PluginConfiguration? logging = registry.GetConfiguration("Logging Plugin");
Console.WriteLine(logging?.Settings["Level"]);          // Debug

// Only the enabled configurations: "Logging Plugin".
foreach (var config in registry.GetEnabledConfigurations())
{
    Console.WriteLine(config);
}

// Everything, including the disabled "Metrics Plugin".
Console.WriteLine(registry.GetAllConfigurations().Count); // 2
```

Replacing a configuration uses the same call. The second `RegisterConfiguration` with the same name overwrites the first:

```csharp
registry.RegisterConfiguration(new PluginConfiguration
{
    Name = "Logging Plugin",
    Version = "1.1.0"
});

Console.WriteLine(registry.GetConfiguration("Logging Plugin")!.Settings.Count); // 0
```

## Related

- [ExtensionSystem](ExtensionSystem.md) - overview of the plugin, hook and registry system.
- [IPipelinePlugin](IPipelinePlugin.md) and [PluginManager](PluginManager.md) - the plugin contract and the component that runs plugin instances.
