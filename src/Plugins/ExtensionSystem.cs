#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Plugins;

using DotNetRealtimePipeline.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Plugin interface for extending pipeline functionality.
/// </summary>
public interface IPipelinePlugin
{
    /// <summary>
    /// Gets the name of the plugin.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the version of the plugin.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Initializes the plugin asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task InitializeAsync();

    /// <summary>
    /// Shuts down the plugin asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ShutdownAsync();
}

/// <summary>
/// Plugin for processing data points.
/// </summary>
public interface IDataProcessingPlugin : IPipelinePlugin
{
    /// <summary>
    /// Processes a data point asynchronously.
    /// </summary>
    /// <param name="dataPoint">The data point to process.</param>
    /// <returns>A task representing the asynchronous operation, with the processing result.</returns>
    Task<ProcessingResult> ProcessAsync(DataPoint dataPoint);
}

/// <summary>
/// Plugin for transforming data.
/// </summary>
public interface IDataTransformPlugin : IPipelinePlugin
{
    /// <summary>
    /// Transforms a data point.
    /// </summary>
    /// <param name="dataPoint">The data point to transform.</param>
    /// <returns>The transformed data point.</returns>
    DataPoint Transform(DataPoint dataPoint);
}

/// <summary>
/// Plugin for outputting results.
/// </summary>
public interface IOutputPlugin : IPipelinePlugin
{
    /// <summary>
    /// Outputs a processing result asynchronously.
    /// </summary>
    /// <param name="result">The processing result to output.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task OutputAsync(ProcessingResult result);
}

/// <summary>
/// Plugin manager for registering and executing plugins.
/// </summary>
public sealed class PluginManager
{
    private readonly List<IPipelinePlugin> _plugins = new();
    private readonly ILogger<PluginManager> _logger;
    private volatile bool _initialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginManager"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when the logger is null.</exception>
    public PluginManager(ILogger<PluginManager> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Registers a plugin.
    /// </summary>
    /// <param name="plugin">The plugin to register.</param>
    /// <exception cref="ArgumentNullException">Thrown when the plugin is null.</exception>
    public void RegisterPlugin(IPipelinePlugin plugin)
    {
        if (plugin is null)
            throw new ArgumentNullException(nameof(plugin));

        _plugins.Add(plugin);
        _logger.LogInformation("Plugin registered: {Name} v{Version}", plugin.Name, plugin.Version);
    }

    /// <summary>
    /// Initializes all registered plugins.
    /// </summary>
    public async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing {Count} plugins", _plugins.Count);

        foreach (var plugin in _plugins)
        {
            try
            {
                await plugin.InitializeAsync();
                _logger.LogInformation("Plugin initialized: {Name}", plugin.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing plugin: {Name}", plugin.Name);
            }
        }

        _initialized = true;
    }

    /// <summary>
    /// Shuts down all plugins.
    /// </summary>
    public async Task ShutdownAsync()
    {
        _logger.LogInformation("Shutting down {Count} plugins", _plugins.Count);

        foreach (var plugin in _plugins.Reverse<IPipelinePlugin>())
        {
            try
            {
                await plugin.ShutdownAsync();
                _logger.LogInformation("Plugin shutdown: {Name}", plugin.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error shutting down plugin: {Name}", plugin.Name);
            }
        }

        _initialized = false;
    }

    /// <summary>
    /// Gets all processing plugins.
    /// </summary>
    /// <returns>A list of processing plugins.</returns>
    public List<IDataProcessingPlugin> GetProcessingPlugins()
    {
        return _plugins.OfType<IDataProcessingPlugin>().ToList();
    }

    /// <summary>
    /// Gets all transform plugins.
    /// </summary>
    /// <returns>A list of transform plugins.</returns>
    public List<IDataTransformPlugin> GetTransformPlugins()
    {
        return _plugins.OfType<IDataTransformPlugin>().ToList();
    }

    /// <summary>
    /// Gets all output plugins.
    /// </summary>
    /// <returns>A list of output plugins.</returns>
    public List<IOutputPlugin> GetOutputPlugins()
    {
        return _plugins.OfType<IOutputPlugin>().ToList();
    }

    /// <summary>
    /// Gets all registered plugins.
    /// </summary>
    /// <returns>A list of all registered plugins.</returns>
    public List<IPipelinePlugin> GetAllPlugins()
    {
        return new List<IPipelinePlugin>(_plugins);
    }

    /// <summary>
    /// Gets a value indicating whether the plugin manager is initialized.
    /// </summary>
    public bool IsInitialized => _initialized;
}

/// <summary>
/// Base class for plugin implementations.
/// </summary>
public abstract class PipelinePluginBase : IPipelinePlugin
{
    protected readonly ILogger _logger;

    /// <summary>
    /// Gets the name of the plugin.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets the version of the plugin.
    /// </summary>
    public abstract string Version { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PipelinePluginBase"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when the logger is null.</exception>
    protected PipelinePluginBase(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Initializes the plugin asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing plugin: {Name} v{Version}", Name, Version);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Shuts down the plugin asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task ShutdownAsync()
    {
        _logger.LogInformation("Shutting down plugin: {Name}", Name);
        await Task.CompletedTask;
    }
}

/// <summary>
/// Plugin configuration.
/// </summary>
public sealed class PluginConfiguration
{
    /// <summary>
    /// Gets or sets the name of the plugin.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the version of the plugin.
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the plugin is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the settings for the plugin.
    /// </summary>
    public Dictionary<string, object> Settings { get; set; } = new();

    /// <summary>
    /// Gets or sets the dependencies of the plugin.
    /// </summary>
    public List<string> Dependencies { get; set; } = new();

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
    {
        return $"PluginConfiguration(Name={Name}, Version={Version}, Enabled={Enabled}, SettingsCount={Settings.Count}, DependenciesCount={Dependencies.Count})";
    }
}

/// <summary>
/// Plugin registry for discovering and loading plugins.
/// </summary>
public sealed class PluginRegistry
{
    private readonly Dictionary<string, PluginConfiguration> _configurations = new();
    private readonly ILogger<PluginRegistry> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginRegistry"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when the logger is null.</exception>
    public PluginRegistry(ILogger<PluginRegistry> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Registers a plugin configuration.
    /// </summary>
    /// <param name="config">The plugin configuration to register.</param>
    /// <exception cref="ArgumentNullException">Thrown when the configuration is null.</exception>
    public void RegisterConfiguration(PluginConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(config);
        _configurations[config.Name] = config;
        _logger.LogInformation("Plugin configuration registered: {Name} v{Version}", config.Name, config.Version);
    }

    /// <summary>
    /// Gets a plugin configuration.
    /// </summary>
    /// <param name="pluginName">The name of the plugin.</param>
    /// <returns>The plugin configuration, or null if not found.</returns>
    /// <exception cref="ArgumentException">Thrown when the plugin name is null or whitespace.</exception>
    public PluginConfiguration GetConfiguration(string pluginName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginName);
        return _configurations.TryGetValue(pluginName, out var config) ? config : null;
    }

    /// <summary>
    /// Gets all enabled plugin configurations.
    /// </summary>
    /// <returns>A list of enabled plugin configurations.</returns>
    public List<PluginConfiguration> GetEnabledConfigurations()
    {
        return _configurations.Values.Where(c => c.Enabled).ToList();
    }

    /// <summary>
    /// Gets all plugin configurations.
    /// </summary>
    /// <returns>A dictionary of all plugin configurations.</returns>
    public Dictionary<string, PluginConfiguration> GetAllConfigurations()
    {
        return new Dictionary<string, PluginConfiguration>(_configurations);
    }
}

/// <summary>
/// Example custom plugin implementation.
/// </summary>
public sealed class LoggingPlugin : PipelinePluginBase, IDataProcessingPlugin
{
    /// <summary>
    /// Gets the name of the plugin.
    /// </summary>
    public override string Name => "Logging Plugin";

    /// <summary>
    /// Gets the version of the plugin.
    /// </summary>
    public override string Version => "1.0.0";

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggingPlugin"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when the logger is null.</exception>
    public LoggingPlugin(ILogger<LoggingPlugin> logger) : base(logger)
    {
    }

    /// <summary>
    /// Processes a data point asynchronously.
    /// </summary>
    /// <param name="dataPoint">The data point to process.</param>
    /// <returns>A task representing the asynchronous operation, with the processing result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the data point is null.</exception>
    public async Task<ProcessingResult> ProcessAsync(DataPoint dataPoint)
    {
        ArgumentNullException.ThrowIfNull(dataPoint);

        _logger.LogDebug("Processing data point: {Id} from {Source}", dataPoint.Id, dataPoint.Source);

        return await Task.FromResult(new ProcessingResult
        {
            Success = true,
            StageName = Name,
            ProcessingTimeMs = 1,
            ProcessedAt = DateTime.UtcNow
        });
    }
}

/// <summary>
/// Pipeline hook system for extending functionality.
/// </summary>
public sealed class HookManager
{
    private readonly Dictionary<string, List<Delegate>> _hooks = new();
    private readonly ILogger<HookManager> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HookManager"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when the logger is null.</exception>
    public HookManager(ILogger<HookManager> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Registers a hook handler.
    /// </summary>
    /// <param name="hookName">The name of the hook.</param>
    /// <param name="handler">The handler to register.</param>
    /// <exception cref="ArgumentException">Thrown when the hook name is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the handler is null.</exception>
    public void RegisterHook(string hookName, Delegate handler)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hookName);
        ArgumentNullException.ThrowIfNull(handler);

        if (!_hooks.ContainsKey(hookName))
        {
            _hooks[hookName] = new List<Delegate>();
        }

        _hooks[hookName].Add(handler);
        _logger.LogDebug("Hook registered: {Hook}", hookName);
    }

    /// <summary>
    /// Executes all handlers for a hook.
    /// </summary>
    /// <param name="hookName">The name of the hook.</param>
    /// <param name="args">The arguments to pass to the handlers.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown when the hook name is null or whitespace.</exception>
    public async Task ExecuteHookAsync(string hookName, params object[] args)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hookName);

        if (!_hooks.TryGetValue(hookName, out var handlers))
        {
            return;
        }

        foreach (var handler in handlers)
        {
            try
            {
                var result = handler.DynamicInvoke(args);
                if (result is Task task)
                {
                    await task;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing hook: {Hook}", hookName);
            }
        }
    }
}
