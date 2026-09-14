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

/// <summary>
/// Extension methods for pipeline plugins.
/// </summary>
public static class PipelinePluginExtensions
{
    /// <summary>
    /// Determines whether the specified plugin is a processing plugin.
    /// </summary>
    /// <param name="plugin">The plugin to check.</param>
    /// <returns>true if the plugin is a processing plugin; otherwise, false.</returns>
    public static bool IsProcessingPlugin(this IPipelinePlugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        return plugin is IDataProcessingPlugin;
    }

    /// <summary>
    /// Determines whether the specified plugin is a transform plugin.
    /// </summary>
    /// <param name="plugin">The plugin to check.</param>
    /// <returns>true if the plugin is a transform plugin; otherwise, false.</returns>
    public static bool IsTransformPlugin(this IPipelinePlugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        return plugin is IDataTransformPlugin;
    }

    /// <summary>
    /// Determines whether the specified plugin is an output plugin.
    /// </summary>
    /// <param name="plugin">The plugin to check.</param>
    /// <returns>true if the plugin is an output plugin; otherwise, false.</returns>
    public static bool IsOutputPlugin(this IPipelinePlugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        return plugin is IOutputPlugin;
    }

    /// <summary>
    /// Gets the display name for the plugin in the format "Name vVersion".
    /// </summary>
    /// <param name="plugin">The plugin to get the display name for.</param>
    /// <returns>A string in the format "Name vVersion".</returns>
    public static string GetDisplayName(this IPipelinePlugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        return $"{plugin.Name} v{plugin.Version}";
    }

    /// <summary>
    /// Converts the plugin to a plugin configuration.
    /// </summary>
    /// <param name="plugin">The plugin to convert.</param>
    /// <param name="enabled">Whether the plugin is enabled. Defaults to true.</param>
    /// <returns>A plugin configuration representing the plugin.</returns>
    public static PluginConfiguration ToConfiguration(this IPipelinePlugin plugin, bool enabled = true)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        return new PluginConfiguration
        {
            Name = plugin.Name,
            Version = plugin.Version,
            Enabled = enabled,
            Settings = new Dictionary<string, object>(),
            Dependencies = new List<string>()
        };
    }

    /// <summary>
    /// Registers a collection of plugins with the plugin manager.
    /// </summary>
    /// <param name="manager">The plugin manager to register plugins with.</param>
    /// <param name="plugins">The plugins to register.</param>
    public static void RegisterPlugins(this PluginManager manager, IEnumerable<IPipelinePlugin> plugins)
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(plugins);

        foreach (var plugin in plugins)
        {
            manager.RegisterPlugin(plugin);
        }
    }
}