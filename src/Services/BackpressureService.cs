#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Services;

using DotNetRealtimePipeline.Constants;
using DotNetRealtimePipeline.Domain.Enums;
using DotNetRealtimePipeline.Domain.Exceptions;
using DotNetRealtimePipeline.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Service for managing backpressure and flow control in the pipeline.
/// Prevents buffer overflow and enforces graceful degradation under load.
/// </summary>
public class BackpressureService
{
    private readonly Dictionary<string, BackpressureContext> _contexts = new();
    private readonly object _lockObject = new();
    private long _nextContextId = 1;

    /// <summary>
    /// Creates a backpressure context for a pipeline stage.
    /// </summary>
    public BackpressureContext CreateContext(string stageName, long maxBufferCapacity)
    {
        if (string.IsNullOrWhiteSpace(stageName))
            throw new ArgumentException("Stage name cannot be null", nameof(stageName));
        if (maxBufferCapacity <= 0)
            throw new ArgumentException("Max capacity must be > 0", nameof(maxBufferCapacity));

        lock (_lockObject)
        {
            if (_contexts.ContainsKey(stageName))
                throw new InvalidOperationException($"Context already exists for stage: {stageName}");

            var context = new BackpressureContext(_nextContextId++, stageName, maxBufferCapacity)
            {
                MaxConcurrentConsumers = 4
            };

            _contexts[stageName] = context;
            return context;
        }
    }

    /// <summary>
    /// Gets the backpressure context for a stage.
    /// </summary>
    public BackpressureContext? GetContext(string stageName)
    {
        lock (_lockObject)
        {
            _contexts.TryGetValue(stageName, out var context);
            return context;
        }
    }

    /// <summary>
    /// Attempts to add items to a stage's buffer.
    /// </summary>
    public bool TryAddToBuffer(string stageName, long itemCount)
    {
        if (string.IsNullOrWhiteSpace(stageName))
            throw new ArgumentException("Stage name cannot be null", nameof(stageName));

        lock (_lockObject)
        {
            if (!_contexts.TryGetValue(stageName, out var context))
                throw new ResourceNotFoundException("Stage not found", "BackpressureContext", stageName);

            bool added = context.TryAddToBuffer(itemCount);

            if (!added && !context.IsBackpressured)
            {
                context.ActivateBackpressure();
            }

            return added;
        }
    }

    /// <summary>
    /// Removes items from a stage's buffer.
    /// </summary>
    public void RemoveFromBuffer(string stageName, long itemCount)
    {
        if (string.IsNullOrWhiteSpace(stageName))
            throw new ArgumentException("Stage name cannot be null", nameof(stageName));

        lock (_lockObject)
        {
            if (!_contexts.TryGetValue(stageName, out var context))
                throw new ResourceNotFoundException("Stage not found", "BackpressureContext", stageName);

            context.RemoveFromBuffer(itemCount);
        }
    }

    /// <summary>
    /// Applies backpressure strategy based on current buffer state.
    /// </summary>
    public async Task<BackpressureResponse> ApplyBackpressureAsync(
        string stageName,
        BackpressureStrategy strategy,
        long timeoutMs)
    {
        if (string.IsNullOrWhiteSpace(stageName))
            throw new ArgumentException("Stage name cannot be null", nameof(stageName));

        lock (_lockObject)
        {
            if (!_contexts.TryGetValue(stageName, out var context))
                throw new ResourceNotFoundException("Stage not found", "BackpressureContext", stageName);

            if (!context.IsBackpressured && !context.ShouldApplyBackpressure())
            {
                return new BackpressureResponse
                {
                    Applied = false,
                    Reason = "Buffer level acceptable",
                    BufferFillPercent = context.GetBufferFillPercentage()
                };
            }

            context.ActivateBackpressure();
        }

        // Simulate backpressure delay
        await Task.Delay((int)Math.Min(timeoutMs, 1000));

        lock (_lockObject)
        {
            var ctx = _contexts[stageName];
            return new BackpressureResponse
            {
                Applied = true,
                Reason = GetBackpressureReason(strategy),
                BufferFillPercent = ctx.GetBufferFillPercentage(),
                StrategyUsed = strategy.ToString()
            };
        }
    }

    /// <summary>
    /// Checks if backpressure is active for a stage.
    /// </summary>
    public bool IsBackpressured(string stageName)
    {
        lock (_lockObject)
        {
            if (!_contexts.TryGetValue(stageName, out var context))
                return false;

            return context.IsBackpressured;
        }
    }

    /// <summary>
    /// Gets a consumer slot if available.
    /// </summary>
    public bool TryRegisterConsumer(string stageName)
    {
        if (string.IsNullOrWhiteSpace(stageName))
            throw new ArgumentException("Stage name cannot be null", nameof(stageName));

        lock (_lockObject)
        {
            if (!_contexts.TryGetValue(stageName, out var context))
                throw new ResourceNotFoundException("Stage not found", "BackpressureContext", stageName);

            return context.TryRegisterConsumer();
        }
    }

    /// <summary>
    /// Releases a consumer slot.
    /// </summary>
    public void UnregisterConsumer(string stageName)
    {
        if (string.IsNullOrWhiteSpace(stageName))
            throw new ArgumentException("Stage name cannot be null", nameof(stageName));

        lock (_lockObject)
        {
            if (_contexts.TryGetValue(stageName, out var context))
            {
                context.UnregisterConsumer();
            }
        }
    }

    /// <summary>
    /// Gets the aggregated backpressure status across all stages.
    /// </summary>
    public BackpressureSystemStatus GetSystemStatus()
    {
        lock (_lockObject)
        {
            int totalContexts = _contexts.Count;
            int backpressuredContexts = 0;
            double avgBufferFill = 0;
            long totalBackpressureTime = 0;

            foreach (var context in _contexts.Values)
            {
                if (context.IsBackpressured)
                    backpressuredContexts++;

                avgBufferFill += context.GetBufferFillPercentage();
                totalBackpressureTime += context.TotalBackpressureTimeMs;
            }

            if (totalContexts > 0)
                avgBufferFill /= totalContexts;

            return new BackpressureSystemStatus
            {
                TotalStages = totalContexts,
                BackpressuredStages = backpressuredContexts,
                AverageBufferFillPercent = avgBufferFill,
                TotalBackpressureTimeMs = totalBackpressureTime,
                IsSystemBackpressured = avgBufferFill > PipelineConstants.BackpressureHighWaterMark,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Resets backpressure for a stage.
    /// </summary>
    public void ResetBackpressure(string stageName)
    {
        lock (_lockObject)
        {
            if (_contexts.TryGetValue(stageName, out var context))
            {
                context.DeactivateBackpressure();
                context.BufferSize = 0;
            }
        }
    }

    /// <summary>
    /// Clears all contexts (for testing/reset).
    /// </summary>
    public void Clear()
    {
        lock (_lockObject)
        {
            _contexts.Clear();
        }
    }

    private string GetBackpressureReason(BackpressureStrategy strategy)
    {
        return strategy switch
        {
            BackpressureStrategy.Block => "Buffer at capacity - blocking incoming requests",
            BackpressureStrategy.DropNewest => "Buffer at capacity - dropping newest items",
            BackpressureStrategy.DropOldest => "Buffer at capacity - dropping oldest items",
            BackpressureStrategy.Queue => "Buffer at capacity - queuing requests",
            BackpressureStrategy.Throttle => "Buffer approaching capacity - throttling",
            _ => "Backpressure applied"
        };
    }
}

/// <summary>
/// Response from applying backpressure.
/// </summary>
public class BackpressureResponse
{
    public bool Applied { get; set; }
    public string Reason { get; set; } = "";
    public double BufferFillPercent { get; set; }
    public string StrategyUsed { get; set; } = "";
}

/// <summary>
/// System-wide backpressure status.
/// </summary>
public class BackpressureSystemStatus
{
    public int TotalStages { get; set; }
    public int BackpressuredStages { get; set; }
    public double AverageBufferFillPercent { get; set; }
    public long TotalBackpressureTimeMs { get; set; }
    public bool IsSystemBackpressured { get; set; }
    public DateTime Timestamp { get; set; }

    public string GetHealthStatus()
    {
        if (IsSystemBackpressured) return "CRITICAL";
        if (AverageBufferFillPercent > 75) return "WARNING";
        if (AverageBufferFillPercent > 50) return "CAUTION";
        return "HEALTHY";
    }
}
