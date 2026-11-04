#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Events;

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Manages subscriber channels with per-subscriber isolation and error handling.
/// </summary>
internal sealed class SubscriberChannelManager : IDisposable
{
    private readonly ConcurrentDictionary<string, List<SubscriberChannel>> _channelsByEvent = new();
    private readonly ILogger _logger;
    private readonly SubscriberOptions _defaultOptions;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubscriberChannelManager"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for diagnostic logging.</param>
    /// <param name="defaultOptions">Optional default options for subscribers.</param>
    public SubscriberChannelManager(ILogger logger, SubscriberOptions? defaultOptions = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _defaultOptions = defaultOptions ?? new SubscriberOptions();
    }

    /// <summary>
    /// Registers a subscriber handler with per-subscriber isolation.
    /// </summary>
    /// <typeparam name="T">The type of event arguments, derived from <see cref="PipelineEventArgs"/>.</typeparam>
    /// <param name="eventName">The name of the event to subscribe to.</param>
    /// <param name="handler">The async handler to invoke when the event is posted.</param>
    /// <param name="options">Optional subscriber-specific options.</param>
    public void RegisterSubscriber<T>(string eventName, Func<T, Task> handler, SubscriberOptions? options = null) where T : PipelineEventArgs
    {
        if (string.IsNullOrWhiteSpace(eventName))
        {
            throw new ArgumentException("Event name cannot be null or empty", nameof(eventName));
        }

        if (handler == null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        var subscriberOptions = options ?? _defaultOptions;
        var subscriberName = options?.Name ?? handler.Method.DeclaringType?.Name ?? "AnonymousSubscriber";

        var channel = new SubscriberChannel(
            eventName,
            args => handler((T)args),
            subscriberOptions,
            _logger,
            subscriberName);

        var channels = _channelsByEvent.GetOrAdd(eventName, _ => new List<SubscriberChannel>());
        lock (channels)
        {
            channels.Add(channel);
        }

        _logger.LogDebug("Registered subscriber channel {SubscriberName} for event {EventName} with options: {Options}",
            subscriberName, eventName, subscriberOptions);
    }

    /// <summary>
    /// Posts an event to all registered subscriber channels for the given event name.
    /// Each subscriber processes events independently with its own error handling.
    /// </summary>
    /// <param name="eventName">The name of the event to post.</param>
    /// <param name="args">The event arguments to pass to subscribers.</param>
    /// <returns>A task representing the asynchronous post operation.</returns>
    public async Task PostEventAsync(string eventName, PipelineEventArgs args)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
        ArgumentNullException.ThrowIfNull(args);

        if (_disposed)
        {
            _logger.LogWarning("Attempted to post to disposed channel manager");
            return;
        }

        if (_channelsByEvent.TryGetValue(eventName, out var channels))
        {
            List<SubscriberChannel> channelsCopy;
            lock (channels)
            {
                channelsCopy = new List<SubscriberChannel>(channels);
            }

            var postTasks = new List<Task<bool>>(channelsCopy.Count);
            foreach (var channel in channelsCopy)
            {
                postTasks.Add(Task.Run(() => channel.TryPost(args)));
            }

            // Wait for all posts to complete (but don't await the actual processing)
            await Task.WhenAll(postTasks).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Starts all registered subscriber channels.
    /// </summary>
    public void StartAll()
    {
        if (_disposed)
        {
            _logger.LogWarning("Cannot start disposed channel manager");
            return;
        }

        foreach (var kvp in _channelsByEvent)
        {
            var eventName = kvp.Key;
            var channels = kvp.Value;

            List<SubscriberChannel> channelsCopy;
            lock (channels)
            {
                channelsCopy = new List<SubscriberChannel>(channels);
            }

            foreach (var channel in channelsCopy)
            {
                try
                {
                    channel.Start();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to start subscriber channel for event {EventName}", eventName);
                }
            }
        }

        _logger.LogInformation("Started all subscriber channels ({ChannelCount} total)", _channelsByEvent.Count);
    }

    /// <summary>
    /// Stops all registered subscriber channels gracefully.
    /// </summary>
    /// <returns>A task representing the asynchronous stop operation.</returns>
    public async Task StopAllAsync()
    {
        if (_disposed)
        {
            return;
        }

        var stopTasks = new List<Task>();

        foreach (var kvp in _channelsByEvent)
        {
            var channels = kvp.Value;
            List<SubscriberChannel> channelsCopy;
            lock (channels)
            {
                channelsCopy = new List<SubscriberChannel>(channels);
            }

            foreach (var channel in channelsCopy)
            {
                stopTasks.Add(channel.StopAsync());
            }
        }

        await Task.WhenAll(stopTasks).ConfigureAwait(false);
        _logger.LogInformation("Stopped all subscriber channels");
    }

    /// <summary>
    /// Gets the total error count across all subscriber channels for an event.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <returns>The total error count across all subscriber channels for the event.</returns>
    public int GetTotalErrorCount(string eventName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventName);

        if (_channelsByEvent.TryGetValue(eventName, out var channels))
        {
            lock (channels)
            {
                var total = 0;
                foreach (var channel in channels)
                {
                    total += channel.GetErrorCount();
                }
                return total;
            }
        }
        return 0;
    }

    /// <summary>
    /// Gets the number of registered subscriber channels.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <returns>The number of registered subscriber channels for the event.</returns>
    public int GetSubscriberCount(string eventName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventName);

        if (_channelsByEvent.TryGetValue(eventName, out var channels))
        {
            lock (channels)
            {
                return channels.Count;
            }
        }
        return 0;
    }

    /// <summary>
    /// Asynchronously disposes the channel manager and all registered subscriber channels.
    /// </summary>
    /// <returns>A value task representing the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            await StopAllAsync().ConfigureAwait(false);
        }
        catch
        {
            // Best effort
        }

        // Dispose all channels
        foreach (var kvp in _channelsByEvent)
        {
            var channels = kvp.Value;
            lock (channels)
            {
                foreach (var channel in channels)
                {
                    channel.Dispose();
                }
            }
        }

        _channelsByEvent.Clear();
        _disposed = true;
    }

    /// <summary>
    /// Disposes the channel manager and all registered subscriber channels.
    /// </summary>
    public void Dispose()
    {
        DisposeAsync().GetAwaiter().GetResult();
    }
}