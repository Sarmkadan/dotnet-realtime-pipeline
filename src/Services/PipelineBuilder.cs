#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Services;

using DotNetRealtimePipeline.Data.Repositories;
using DotNetRealtimePipeline.DeadLetter;
using DotNetRealtimePipeline.Domain.Enums;
using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;

/// <summary>
/// Fluent builder for assembling a fully configured <see cref="StreamProcessor"/>
/// pipeline with backpressure, checkpointing, and windowed aggregation.
/// </summary>
public sealed class PipelineBuilder
{
    private readonly PipelineConfig _config = new();
    private IDataPointRepository? _repository;
    private IRetryPolicy? _retryPolicy;
    private IDeadLetterQueue? _deadLetterQueue;
    private string? _checkpointPath;
    private int _consumerCount = 1;
    private readonly List<Action<BackpressureService>> _backpressureSetups = new();

    /// <summary>
    /// Sets the pipeline name.
    /// </summary>
    /// <param name="name">A human-readable name for the pipeline.</param>
    /// <returns>This builder instance for chaining.</returns>
    public PipelineBuilder WithName(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        _config.PipelineName = name;
        return this;
    }

    /// <summary>
    /// Configures the maximum buffer size before backpressure kicks in.
    /// </summary>
    /// <param name="maxBufferSize">Maximum number of items in the ingress buffer.</param>
    /// <returns>This builder instance for chaining.</returns>
    public PipelineBuilder WithBufferSize(long maxBufferSize)
    {
        if (maxBufferSize <= 0)
            throw new ArgumentException("Buffer size must be > 0", nameof(maxBufferSize));
        _config.MaxBufferSize = maxBufferSize;
        return this;
    }

    /// <summary>
    /// Adds a named processing stage to the pipeline.
    /// </summary>
    /// <param name="stageName">Unique stage name.</param>
    /// <param name="stageType">Type descriptor (e.g. "filter", "transform", "aggregate").</param>
    /// <param name="executionOrder">Execution order; lower values execute first.</param>
    /// <returns>This builder instance for chaining.</returns>
    public PipelineBuilder AddStage(string stageName, string stageType, int executionOrder = 0)
    {
        var stage = new PipelineStageDef(stageName, stageType) { ExecutionOrder = executionOrder };
        _config.AddStage(stage);
        return this;
    }

    /// <summary>
    /// Sets the data point repository for persisting processed results.
    /// </summary>
    /// <param name="repository">The repository implementation.</param>
    /// <returns>This builder instance for chaining.</returns>
    public PipelineBuilder WithRepository(IDataPointRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        return this;
    }

    /// <summary>
    /// Configures the retry policy and dead-letter queue for failed items.
    /// </summary>
    /// <param name="retryPolicy">Retry policy to apply on transient errors.</param>
    /// <param name="deadLetterQueue">Queue for items that exhaust their retry budget.</param>
    /// <returns>This builder instance for chaining.</returns>
    public PipelineBuilder WithErrorHandling(IRetryPolicy retryPolicy, IDeadLetterQueue deadLetterQueue)
    {
        _retryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
        _deadLetterQueue = deadLetterQueue ?? throw new ArgumentNullException(nameof(deadLetterQueue));
        return this;
    }

    /// <summary>
    /// Enables durable checkpointing so the pipeline can resume after a restart.
    /// </summary>
    /// <param name="filePath">Path to the checkpoint file on disk.</param>
    /// <returns>This builder instance for chaining.</returns>
    public PipelineBuilder WithCheckpointing(string filePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);
        _checkpointPath = filePath;
        return this;
    }

    /// <summary>
    /// Sets the windowing parameters for time-series aggregation.
    /// </summary>
    /// <param name="windowSizeMs">Window size in milliseconds.</param>
    /// <param name="slideSizeMs">Slide interval in milliseconds.</param>
    /// <param name="windowType">The window type (Tumbling, Sliding, Session, Global).</param>
    /// <returns>This builder instance for chaining.</returns>
    public PipelineBuilder WithWindowing(long windowSizeMs, long slideSizeMs, WindowType windowType = WindowType.Tumbling)
    {
        if (windowSizeMs <= 0)
            throw new ArgumentException("Window size must be > 0", nameof(windowSizeMs));
        if (slideSizeMs <= 0)
            throw new ArgumentException("Slide size must be > 0", nameof(slideSizeMs));

        _config.WindowSizeMs = windowSizeMs;
        _config.WindowSlideMs = slideSizeMs;
        _config.WindowType = windowType.ToString().ToUpperInvariant();
        return this;
    }

    /// <summary>
    /// Sets the number of parallel consumer tasks for the stream processor.
    /// </summary>
    /// <param name="count">Number of consumers (clamped to 1..ProcessorCount*2).</param>
    /// <returns>This builder instance for chaining.</returns>
    public PipelineBuilder WithConsumers(int count)
    {
        _consumerCount = Math.Max(1, count);
        return this;
    }

    /// <summary>
    /// Registers a backpressure context for a named stage.
    /// </summary>
    /// <param name="stageName">Stage to register backpressure for.</param>
    /// <param name="maxCapacity">Maximum buffer capacity for the stage.</param>
    /// <returns>This builder instance for chaining.</returns>
    public PipelineBuilder WithBackpressure(string stageName, long maxCapacity)
    {
        ArgumentException.ThrowIfNullOrEmpty(stageName);
        _backpressureSetups.Add(svc => svc.CreateContext(stageName, maxCapacity));
        return this;
    }

    /// <summary>
    /// Builds and returns a fully configured <see cref="StreamProcessor"/>.
    /// The processor is created but not started; call
    /// <see cref="StreamProcessor.StartConsumers"/> to begin processing.
    /// </summary>
    /// <returns>A configured <see cref="StreamProcessor"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Required dependencies were not set.</exception>
    public StreamProcessor Build()
    {
        if (_repository is null)
            throw new InvalidOperationException("Repository is required. Call WithRepository().");

        var backpressureService = new BackpressureService();
        foreach (var setup in _backpressureSetups)
            setup(backpressureService);

        DataProcessingService processingService = _retryPolicy is not null && _deadLetterQueue is not null
            ? new DataProcessingService(_repository, _config, _retryPolicy, _deadLetterQueue)
            : new DataProcessingService(_repository, _config);

        var processor = new StreamProcessor(processingService, backpressureService, _config);
        processor.StartConsumers(_consumerCount);

        return processor;
    }
}
