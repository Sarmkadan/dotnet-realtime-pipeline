# IPipelinePlugin

The `IPipelinePlugin` interface defines the contract for plugins in the `dotnet-realtime-pipeline` project. It provides lifecycle management, configuration, and discovery mechanisms for pipeline components such as data processing, transformation, and output plugins. Plugins implementing this interface can be dynamically registered, configured, and managed within a pipeline runtime.

## API

### `PluginManager PluginManager { get; }`
Gets the plugin manager instance associated with this plugin. The plugin manager provides access to the plugin registry and configuration system. This property is read-only and initialized during plugin registration.

### `void RegisterPlugin(IDataProcessingPlugin plugin)`
Registers a data processing plugin with the pipeline. The provided plugin must implement `IDataProcessingPlugin` and will be included in the list of processing plugins returned by `GetProcessingPlugins`.

### `void RegisterPlugin(IDataTransformPlugin plugin)`
Registers a data transformation plugin with the pipeline. The provided plugin must implement `IDataTransformPlugin` and will be included in the list of transform plugins returned by `GetTransformPlugins`.

### `void RegisterPlugin(IOutputPlugin plugin)`
Registers an output plugin with the pipeline. The provided plugin must implement `IOutputPlugin` and will be included in the list of output plugins returned by `GetOutputPlugins`.

### `async Task InitializeAsync()`
Initializes the plugin and its dependencies. This method is called once when the pipeline starts. It should prepare the plugin for operation, validate dependencies, and allocate resources. If initialization fails, the pipeline startup will be aborted.

**Parameters:** None
**Return value:** A `Task` representing the asynchronous initialization operation.
**Exceptions:** Throws if initialization fails or if required dependencies are missing.

### `async Task ShutdownAsync()`
Shuts down the plugin and releases any resources. This method is called once when the pipeline stops. It should clean up any allocated resources and ensure graceful termination.

**Parameters:** None
**Return value:** A `Task` representing the asynchronous shutdown operation.
**Exceptions:** Throws if shutdown fails or if cleanup cannot be completed.

### `List<IDataProcessingPlugin> GetProcessingPlugins()`
Retrieves a list of all registered data processing plugins. The list is a snapshot of the current state and may change if plugins are registered or unregistered dynamically.

**Parameters:** None
**Return value:** A `List<IDataProcessingPlugin>` containing all registered data processing plugins.

### `List<IDataTransformPlugin> GetTransformPlugins()`
Retrieves a list of all registered data transformation plugins. The list is a snapshot of the current state and may change if plugins are registered or unregistered dynamically.

**Parameters:** None
**Return value:** A `List<IDataTransformPlugin>` containing all registered data transformation plugins.

### `List<IOutputPlugin> GetOutputPlugins()`
Retrieves a list of all registered output plugins. The list is a snapshot of the current state and may change if plugins are registered or unregistered dynamically.

**Parameters:** None
**Return value:** A `List<IOutputPlugin>` containing all registered output plugins.

### `List<IPipelinePlugin> GetAllPlugins()`
Retrieves a list of all registered plugins, including this instance. The list is a snapshot of the current state and may change if plugins are registered or unregistered dynamically.

**Parameters:** None
**Return value:** A `List<IPipelinePlugin>` containing all registered plugins.

### `abstract string Name { get; }`
Gets the name of the plugin. This value must be unique among all plugins in the pipeline. The name is used for identification, logging, and configuration.

**Return value:** A `string` representing the plugin name.

### `abstract string Version { get; }`
Gets the version of the plugin. This value follows semantic versioning and is used to ensure compatibility and logging.

**Return value:** A `string` representing the plugin version.

### `virtual async Task InitializeAsync()`
Provides a default implementation of `InitializeAsync` that does nothing. Derived classes can override this method to perform initialization logic.

**Parameters:** None
**Return value:** A `Task` representing the asynchronous operation.

### `virtual async Task ShutdownAsync()`
Provides a default implementation of `ShutdownAsync` that does nothing. Derived classes can override this method to perform shutdown logic.

**Parameters:** None
**Return value:** A `Task` representing the asynchronous operation.

### `string Name { get; set; }`
Gets or sets the name of the plugin. This value must be unique among all plugins in the pipeline. The name is used for identification, logging, and configuration.

**Return value:** A `string` representing the plugin name.

### `string Version { get; set; }`
Gets or sets the version of the plugin. This value follows semantic versioning and is used to ensure compatibility and logging.

**Return value:** A `string` representing the plugin version.

### `bool Enabled { get; set; }`
Gets or sets whether the plugin is enabled. Disabled plugins are not initialized or used in the pipeline.

**Return value:** A `bool` indicating whether the plugin is enabled.

### `Dictionary<string, object> Settings { get; }`
Gets the plugin's configuration settings. These settings are applied during initialization and can be modified at runtime.

**Return value:** A `Dictionary<string, object>` containing the plugin's settings.

### `List<string> Dependencies { get; }`
Gets the list of plugin dependencies. These are plugin names that must be registered and initialized before this plugin.

**Return value:** A `List<string>` containing the names of required plugins.

### `PluginRegistry PluginRegistry { get; }`
Gets the plugin registry associated with this plugin. The registry provides access to other plugins and configuration.

**Return value:** A `PluginRegistry` instance.

### `void RegisterConfiguration(PluginConfiguration configuration)`
Registers a configuration for the plugin. The configuration is applied during initialization and can be modified at runtime.

**Parameters:**
- `configuration`: The `PluginConfiguration` to register.
**Exceptions:** Throws if the configuration is invalid or cannot be applied.

### `PluginConfiguration GetConfiguration()`
Retrieves the current configuration of the plugin.

**Parameters:** None
**Return value:** A `PluginConfiguration` representing the plugin's current configuration.

## Usage

### Example 1: Basic Plugin Registration and Initialization
