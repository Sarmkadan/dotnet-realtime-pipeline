#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Events;

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

/// <summary>
/// Represents a dedicated channel for a single subscriber with configurable error handling and concurrency.
/// </summary>
internal sealed class SubscriberChannel : IDisposable
{
    private readonly Channel<SubscriberWorkItem> _channel;
    private readonly Func<PipelineEventArgs, Task> _handler;
    private readonly ILogger _logger;
    private readonly SubscriberOptions _options;
    private readonly string _subscriberName;
    private readonly string _eventName;
    private int _errorCount;
    private bool _disposed;
    private Task? _consumerTask;
    private CancellationTokenSource? _cts;

    public SubscriberChannel(
        string eventName,
        Func<PipelineEventArgs, Task> handler,
        SubscriberOptions options,
        ILogger logger,
        string subscriberName)
    {
        _eventName = eventName ?? throw new ArgumentNullException(nameof(eventName));
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _subscriberName = subscriberName ?? throw new ArgumentNullException(nameof(subscriberName));

        var boundedChannelOptions = new BoundedChannelOptions(_options.MaxQueueSize)
        {
            FullMode = _options.MaxQueueSizeBehavior switch
            {
                MaxQueueSizeBehavior.DropNew => BoundedChannelFullMode.DropNewest,
                MaxQueueSizeBehavior.Block => BoundedChannelFullMode.Wait,
                MaxQueueSizeBehavior.Reject => BoundedChannelFullMode.DropOldest,
                _ => BoundedChannelFullMode.DropNewest
            }
        };

        _channel = Channel.CreateBounded<SubscriberWorkItem>(boundedChannelOptions);
    }

    /// <summary>
    /// Enqueues an event for processing by this subscriber.
    /// </summary>
    /// <returns>True if the event was successfully enqueued, false otherwise.</returns>
    public bool TryPost(PipelineEventArgs args)
    {
        if (_disposed)
        {
            _logger.LogWarning("Attempted to post to disposed subscriber channel {SubscriberName}", _subscriberName);
            return false;
        }

        var workItem = new SubscriberWorkItem(args, _errorCount);
        return _channel.Writer.TryWrite(workItem);
    }

    /// <summary>
    /// Starts the consumer loop for this subscriber channel.
    /// </summary>
    public void Start()
    {
        if (_consumerTask != null)
        {
            _logger.LogWarning("Subscriber channel {SubscriberName} is already running", _subscriberName);
            return;
        }

        _cts = new CancellationTokenSource();
        _consumerTask = _options.DispatchMode == SubscriberDispatchMode.Parallel
            ? Task.Run(() => ProcessEventsParallelAsync(_cts.Token))
            : Task.Run(() => ProcessEventsSequentialAsync(_cts.Token));

        _logger.LogInformation("Started {DispatchMode} consumer for subscriber {SubscriberName} on event {EventName}",
            _options.DispatchMode, _subscriberName, _eventName);
    }

    /// <summary>
    /// Stops the consumer loop gracefully.
    /// </summary>
    public async Task StopAsync()
    {
        if (_disposed || _cts == null)
        {
            return;
        }

        try
        {
            _cts.Cancel();
            _channel.Writer.Complete();

            if (_consumerTask != null)
            {
                await _consumerTask.ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while stopping subscriber channel {SubscriberName}", _subscriberName);
        }
        finally
        {
            _cts.Dispose();
            _cts = null;
            _consumerTask = null;
        }
    }

    /// <summary>
    /// Gets the current error count for this subscriber.
    /// </summary>
    public int GetErrorCount() => _errorCount;

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _channel.Writer.Complete();
        }
        catch
        {
            // Best effort cleanup
        }

        _disposed = true;
    }

    private async Task ProcessEventsSequentialAsync(CancellationToken cancellationToken)
    {
        await foreach (var workItem in _channel.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
            await ProcessWorkItemAsync(workItem, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task ProcessEventsParallelAsync(CancellationToken cancellationToken)
    {
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = _options.MaxDegreeOfParallelism,
            CancellationToken = cancellationToken
        };

        await Parallel.ForEachAsync(_channel.Reader.ReadAllAsync(cancellationToken), parallelOptions, async (workItem, ct) =>
        {
            await ProcessWorkItemAsync(workItem, ct).ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    private async Task ProcessWorkItemAsync(SubscriberWorkItem workItem, CancellationToken cancellationToken)
    {
        try
        {
            await _handler(workItem.Args).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Interlocked.Increment(ref _errorCount);

            switch (_options.ErrorPolicy)
            {
                case SubscriberErrorPolicy.SwallowAndCount:
                    _logger.LogError(ex, "Error processing event {EventName} in subscriber {SubscriberName} (error #{ErrorCount})",
                        _eventName, _subscriberName, _errorCount);
                    break;

                case SubscriberErrorPolicy.DeadLetter:
                    _logger.LogError(ex, "Error processing event {EventName} in subscriber {SubscriberName} - sending to dead-letter (error #{ErrorCount})",
                        _eventName, _subscriberName, _errorCount);
                    // In a real system, this would send to a dead-letter queue
                    break;

                case SubscriberErrorPolicy.FailFast:
                    _logger.LogError(ex, "Failing fast for subscriber {SubscriberName} on event {EventName} (error #{ErrorCount})",
                        _subscriberName, _eventName, _errorCount);
                    throw; // Re-throw to fail the consumer task
            }
        }
    }

    private readonly struct SubscriberWorkItem
    {
        public readonly PipelineEventArgs Args;
        public readonly int ErrorCountAtEnqueue;

        public SubscriberWorkItem(PipelineEventArgs args, int errorCountAtEnqueue)
        {
            Args = args;
            ErrorCountAtEnqueue = errorCountAtEnqueue;
        }
    }
}