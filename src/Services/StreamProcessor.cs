#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Services;

using DotNetRealtimePipeline.Domain.Enums;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

/// <summary>
/// Processes a continuous stream of <see cref="DataPoint"/> items through
/// configurable pipeline stages with bounded concurrency and cancellation support.
/// </summary>
public sealed class StreamProcessor : IDisposable
{
    private readonly Channel<DataPoint> _ingressChannel;
    private readonly DataProcessingService _processingService;
    private readonly BackpressureService _backpressureService;
    private readonly PipelineConfig _config;
    private readonly CancellationTokenSource _cts;
    private readonly List<Task> _consumers;
    private long _totalProcessed;
    private long _totalFailed;
    private volatile bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamProcessor"/> class.
    /// </summary>
    /// <param name="processingService">Service that handles individual data point processing.</param>
    /// <param name="backpressureService">Service that manages flow control.</param>
    /// <param name="config">Pipeline configuration including buffer limits and stage definitions.</param>
    public StreamProcessor(
        DataProcessingService processingService,
        BackpressureService backpressureService,
        PipelineConfig config)
    {
        _processingService = processingService ?? throw new ArgumentNullException(nameof(processingService));
        _backpressureService = backpressureService ?? throw new ArgumentNullException(nameof(backpressureService));
        _config = config ?? throw new ArgumentNullException(nameof(config));

        _ingressChannel = Channel.CreateBounded<DataPoint>(new BoundedChannelOptions((int)config.MaxBufferSize)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = false
        });

        _cts = new CancellationTokenSource();
        _consumers = new List<Task>();
    }

    /// <summary>Gets the total number of data points successfully processed.</summary>
    public long TotalProcessed => Interlocked.Read(ref _totalProcessed);

    /// <summary>Gets the total number of data points that failed processing.</summary>
    public long TotalFailed => Interlocked.Read(ref _totalFailed);

    /// <summary>Gets the number of items currently waiting in the ingress channel.</summary>
    public int PendingCount => _ingressChannel.Reader.Count;

    /// <summary>
    /// Submits a data point to the stream for processing.
    /// Blocks asynchronously when the channel is at capacity (backpressure).
    /// </summary>
    /// <param name="dataPoint">The data point to enqueue.</param>
    /// <param name="cancellationToken">Token to cancel the wait if the channel is full.</param>
    /// <returns>A task that completes when the item is accepted into the channel.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dataPoint"/> is <see langword="null"/>.</exception>
    /// <exception cref="ChannelClosedException">The processor has been stopped or disposed.</exception>
    public async ValueTask SubmitAsync(DataPoint dataPoint, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dataPoint);
        ObjectDisposedException.ThrowIf(_disposed, this);

        using var linked = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);
        await _ingressChannel.Writer.WriteAsync(dataPoint, linked.Token).ConfigureAwait(false);
    }

    /// <summary>
    /// Starts the consumer loop with the specified degree of parallelism.
    /// Each consumer reads from the shared channel and processes items independently.
    /// </summary>
    /// <param name="consumerCount">Number of parallel consumer tasks (default 1).</param>
    /// <exception cref="InvalidOperationException">Consumers are already running.</exception>
    public void StartConsumers(int consumerCount = 1)
    {
        if (_consumers.Count > 0)
            throw new InvalidOperationException("Consumers are already running. Stop first.");

        consumerCount = Math.Clamp(consumerCount, 1, Environment.ProcessorCount * 2);

        for (int i = 0; i < consumerCount; i++)
        {
            int consumerId = i;
            _consumers.Add(Task.Run(() => ConsumeLoopAsync(consumerId, _cts.Token)));
        }
    }

    /// <summary>
    /// Signals the channel as complete and waits for all consumers to drain.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the drain wait.</param>
    /// <returns>A task that completes when all consumers have finished.</returns>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        _ingressChannel.Writer.TryComplete();

        if (_consumers.Count > 0)
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);
            try
            {
                await Task.WhenAll(_consumers).WaitAsync(linked.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) { }
        }

        _consumers.Clear();
    }

    /// <summary>
    /// Reads all available data points from the ingress channel as an async stream.
    /// Useful for external consumers that want to process items outside the built-in loop.
    /// </summary>
    /// <param name="cancellationToken">Token to stop reading.</param>
    /// <returns>An async enumerable of queued data points.</returns>
    public async IAsyncEnumerable<DataPoint> ReadAllAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var item in _ingressChannel.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
            yield return item;
        }
    }

    private async Task ConsumeLoopAsync(int consumerId, CancellationToken ct)
    {
        await foreach (var dataPoint in _ingressChannel.Reader.ReadAllAsync(ct).ConfigureAwait(false))
        {
            try
            {
                var result = await _processingService.ProcessDataPointAsync(dataPoint).ConfigureAwait(false);

                if (result.Success)
                    Interlocked.Increment(ref _totalProcessed);
                else
                    Interlocked.Increment(ref _totalFailed);
            }
            catch
            {
                Interlocked.Increment(ref _totalFailed);
            }
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _cts.Cancel();
        _ingressChannel.Writer.TryComplete();
        _cts.Dispose();
    }
}
