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
    string Name { get; }
    string Version { get; }
    Task InitializeAsync();
    Task ShutdownAsync();
}

/// <summary>
/// Plugin for processing data points.
/// </summary>
public interface IDataProcessingPlugin : IPipelinePlugin
{
    Task<ProcessingResult> ProcessAsync(DataPoint dataPoint);
}

/// <summary>
/// Plugin for transforming data.
/// </summary>
public interface IDataTransformPlugin : IPipelinePlugin
{
    DataPoint Transform(DataPoint dataPoint);
}

/// <summary>
/// Plugin for outputting results.
/// </summary>
public interface IOutputPlugin : IPipelinePlugin
{
    Task OutputAsync(ProcessingResult result);
}

/// <summary>
/// Plugin manager for registering and executing plugins.
/// </summary>
public class PluginManager
{
    private readonly List<IPipelinePlugin> _plugins = new();
    private readonly ILogger<PluginManager> _logger;
    private volatile bool _initialized;

    public PluginManager(ILogger<PluginManager> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Registers a plugin.
    /// </summary>
    public void RegisterPlugin(IPipelinePlugin plugin)
    {
        if (plugin == null)
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
    public List<IDataProcessingPlugin> GetProcessingPlugins()
    {
        return _plugins.OfType<IDataProcessingPlugin>().ToList();
    }

    /// <summary>
    /// Gets all transform plugins.
    /// </summary>
    public List<IDataTransformPlugin> GetTransformPlugins()
    {
        return _plugins.OfType<IDataTransformPlugin>().ToList();
    }

    /// <summary>
    /// Gets all output plugins.
    /// </summary>
    public List<IOutputPlugin> GetOutputPlugins()
    {
        return _plugins.OfType<IOutputPlugin>().ToList();
    }

    /// <summary>
    /// Gets all registered plugins.
    /// </summary>
    public List<IPipelinePlugin> GetAllPlugins()
    {
        return new List<IPipelinePlugin>(_plugins);
    }

    public bool IsInitialized => _initialized;
}

/// <summary>
/// Base class for plugin implementations.
/// </summary>
public abstract class PipelinePluginBase : IPipelinePlugin
{
    protected readonly ILogger _logger;

    public abstract string Name { get; }
    public abstract string Version { get; }

    protected PipelinePluginBase(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public virtual async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing plugin: {Name} v{Version}", Name, Version);
        await Task.CompletedTask;
    }

    public virtual async Task ShutdownAsync()
    {
        _logger.LogInformation("Shutting down plugin: {Name}", Name);
        await Task.CompletedTask;
    }
}

/// <summary>
/// Plugin configuration.
/// </summary>
public class PluginConfiguration
{
    public string Name { get; set; }
    public string Version { get; set; }
    public bool Enabled { get; set; } = true;
    public Dictionary<string, object> Settings { get; set; } = new();
    public List<string> Dependencies { get; set; } = new();
}

/// <summary>
/// Plugin registry for discovering and loading plugins.
/// </summary>
public class PluginRegistry
{
    private readonly Dictionary<string, PluginConfiguration> _configurations = new();
    private readonly ILogger<PluginRegistry> _logger;

    public PluginRegistry(ILogger<PluginRegistry> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Registers a plugin configuration.
    /// </summary>
    public void RegisterConfiguration(PluginConfiguration config)
    {
        _configurations[config.Name] = config;
        _logger.LogInformation("Plugin configuration registered: {Name} v{Version}", config.Name, config.Version);
    }

    /// <summary>
    /// Gets a plugin configuration.
    /// </summary>
    public PluginConfiguration GetConfiguration(string pluginName)
    {
        return _configurations.TryGetValue(pluginName, out var config) ? config : null;
    }

    /// <summary>
    /// Gets all enabled plugin configurations.
    /// </summary>
    public List<PluginConfiguration> GetEnabledConfigurations()
    {
        return _configurations.Values.Where(c => c.Enabled).ToList();
    }

    /// <summary>
    /// Gets all plugin configurations.
    /// </summary>
    public Dictionary<string, PluginConfiguration> GetAllConfigurations()
    {
        return new Dictionary<string, PluginConfiguration>(_configurations);
    }
}

/// <summary>
/// Example custom plugin implementation.
/// </summary>
public class LoggingPlugin : PipelinePluginBase, IDataProcessingPlugin
{
    public override string Name => "Logging Plugin";
    public override string Version => "1.0.0";

    public LoggingPlugin(ILogger<LoggingPlugin> logger) : base(logger)
    {
    }

    public async Task<ProcessingResult> ProcessAsync(DataPoint dataPoint)
    {
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
public class HookManager
{
    private readonly Dictionary<string, List<Delegate>> _hooks = new();
    private readonly ILogger<HookManager> _logger;

    public HookManager(ILogger<HookManager> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Registers a hook handler.
    /// </summary>
    public void RegisterHook(string hookName, Delegate handler)
    {
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
    public async Task ExecuteHookAsync(string hookName, params object[] args)
    {
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
