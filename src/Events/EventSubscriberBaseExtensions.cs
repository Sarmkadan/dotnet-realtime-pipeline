#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Events;

using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Extension methods for <see cref="EventSubscriberBase"/> and its derived types.
/// Provides utility methods for managing and querying event subscribers.
/// </summary>
public static class EventSubscriberBaseExtensions
{
    /// <summary>
    /// Safely unsubscribes the subscriber if it is currently subscribed.
    /// </summary>
    /// <param name="subscriber">The subscriber instance.</param>
    /// <exception cref="ArgumentNullException"><paramref name="subscriber"/> is <see langword="null"/></exception>
    public static void SafeUnsubscribe(this EventSubscriberBase subscriber)
    {
        ArgumentNullException.ThrowIfNull(subscriber);

        // Try to unsubscribe - if already unsubscribed, the base implementation handles it gracefully
        try
        {
            subscriber.Unsubscribe();
        }
        catch
        {
            // Ignore exceptions from Unsubscribe if already unsubscribed
        }
    }

    /// <summary>
    /// Gets the subscriber type name for logging and identification purposes.
    /// </summary>
    /// <param name="subscriber">The subscriber instance.</param>
    /// <returns>The type name of the subscriber.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="subscriber"/> is <see langword="null"/></exception>
    public static string GetSubscriberTypeName(this EventSubscriberBase subscriber)
    {
        ArgumentNullException.ThrowIfNull(subscriber);
        return subscriber.GetType().Name;
    }

    /// <summary>
    /// Gets a collection of all subscriber instances from the provided collection.
    /// </summary>
    /// <param name="subscribers">The collection of subscribers.</param>
    /// <returns>A read-only list of subscriber instances.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="subscribers"/> is <see langword="null"/></exception>
    public static IReadOnlyList<EventSubscriberBase> AsReadOnly(this IEnumerable<EventSubscriberBase> subscribers)
    {
        ArgumentNullException.ThrowIfNull(subscribers);
        return new List<EventSubscriberBase>(subscribers).AsReadOnly();
    }

    /// <summary>
    /// Gets the success rate percentage from a <see cref="ProcessingCompletionSubscriber"/>.
    /// Returns 100.0 if the subscriber is not a <see cref="ProcessingCompletionSubscriber"/>.
    /// </summary>
    /// <param name="subscriber">The subscriber instance.</param>
    /// <returns>The success rate percentage (0-100).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="subscriber"/> is <see langword="null"/></exception>
    public static double GetSuccessRatePercent(this EventSubscriberBase subscriber)
    {
        ArgumentNullException.ThrowIfNull(subscriber);

        return subscriber switch
        {
            ProcessingCompletionSubscriber processingSubscriber => processingSubscriber.GetSuccessRatePercent(),
            _ => 100.0
        };
    }

    /// <summary>
    /// Gets the backpressure event count from a <see cref="BackpressureAlertSubscriber"/>.
    /// Returns 0 if the subscriber is not a <see cref="BackpressureAlertSubscriber"/>.
    /// </summary>
    /// <param name="subscriber">The subscriber instance.</param>
    /// <returns>The count of backpressure events detected.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="subscriber"/> is <see langword="null"/></exception>
    public static int GetBackpressureEventCount(this EventSubscriberBase subscriber)
    {
        ArgumentNullException.ThrowIfNull(subscriber);

        return subscriber switch
        {
            BackpressureAlertSubscriber backpressureSubscriber => backpressureSubscriber.GetBackpressureEventCount(),
            _ => 0
        };
    }

    /// <summary>
    /// Gets the average processing time from a <see cref="MetricsAggregationSubscriber"/>.
    /// Returns 0.0 if the subscriber is not a <see cref="MetricsAggregationSubscriber"/>.
    /// </summary>
    /// <param name="subscriber">The subscriber instance.</param>
    /// <returns>The average processing time in milliseconds.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="subscriber"/> is <see langword="null"/></exception>
    public static double GetAverageProcessingTime(this EventSubscriberBase subscriber)
    {
        ArgumentNullException.ThrowIfNull(subscriber);

        return subscriber switch
        {
            MetricsAggregationSubscriber metricsSubscriber => metricsSubscriber.GetAverageProcessingTime(),
            _ => 0.0
        };
    }

    /// <summary>
    /// Gets the metrics count from a <see cref="MetricsAggregationSubscriber"/>.
    /// Returns 0 if the subscriber is not a <see cref="MetricsAggregationSubscriber"/>.
    /// </summary>
    /// <param name="subscriber">The subscriber instance.</param>
    /// <returns>The count of metrics collected.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="subscriber"/> is <see langword="null"/></exception>
    public static int GetMetricsCount(this EventSubscriberBase subscriber)
    {
        ArgumentNullException.ThrowIfNull(subscriber);

        return subscriber switch
        {
            MetricsAggregationSubscriber metricsSubscriber => metricsSubscriber.GetMetricsCount(),
            _ => 0
        };
    }

    /// <summary>
    /// Gets the error count from a <see cref="ErrorAlertSubscriber"/>.
    /// Returns 0 if the subscriber is not a <see cref="ErrorAlertSubscriber"/>.
    /// </summary>
    /// <param name="subscriber">The subscriber instance.</param>
    /// <returns>The count of errors detected.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="subscriber"/> is <see langword="null"/></exception>
    public static int GetErrorCount(this EventSubscriberBase subscriber)
    {
        ArgumentNullException.ThrowIfNull(subscriber);

        return subscriber switch
        {
            ErrorAlertSubscriber errorSubscriber => errorSubscriber.GetErrorCount(),
            _ => 0
        };
    }

    /// <summary>
    /// Determines whether the subscriber is currently in a critical state based on its metrics.
    /// </summary>
    /// <param name="subscriber">The subscriber instance.</param>
    /// <returns><see langword="true"/> if the subscriber is in a critical state; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="subscriber"/> is <see langword="null"/></exception>
    public static bool IsInCriticalState(this EventSubscriberBase subscriber)
    {
        ArgumentNullException.ThrowIfNull(subscriber);

        return subscriber switch
        {
            BackpressureAlertSubscriber backpressureSubscriber => backpressureSubscriber.GetBackpressureEventCount() > 10,
            ErrorAlertSubscriber errorSubscriber => errorSubscriber.GetErrorCount() > 5,
            _ => false
        };
    }

    /// <summary>
    /// Gets a formatted status string for the subscriber including its type and metrics.
    /// </summary>
    /// <param name="subscriber">The subscriber instance.</param>
    /// <returns>A formatted status string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="subscriber"/> is <see langword="null"/></exception>
    public static string GetStatusString(this EventSubscriberBase subscriber)
    {
        ArgumentNullException.ThrowIfNull(subscriber);

        var typeName = subscriber.GetType().Name;
        var isSubscribed = "Subscribed"; // Since we can't access _isSubscribed directly

        var metricsParts = new List<string>();

        if (subscriber is ProcessingCompletionSubscriber processingSubscriber)
        {
            metricsParts.Add($"Success: {processingSubscriber.GetSuccessRatePercent().ToString("F1", CultureInfo.InvariantCulture)}%");
        }

        if (subscriber is BackpressureAlertSubscriber backpressureSubscriber)
        {
            metricsParts.Add($"Backpressure Events: {backpressureSubscriber.GetBackpressureEventCount()}");
        }

        if (subscriber is MetricsAggregationSubscriber metricsSubscriber)
        {
            metricsParts.Add($"Metrics: {metricsSubscriber.GetMetricsCount()}");
            metricsParts.Add($"Avg Processing: {metricsSubscriber.GetAverageProcessingTime().ToString("F2", CultureInfo.InvariantCulture)}ms");
        }

        if (subscriber is ErrorAlertSubscriber errorSubscriber)
        {
            metricsParts.Add($"Errors: {errorSubscriber.GetErrorCount()}");
        }

        var metricsString = metricsParts.Count > 0 ? $" | {string.Join(", ", metricsParts)}" : string.Empty;

        return $"[{typeName}] {isSubscribed}{metricsString}";
    }
}