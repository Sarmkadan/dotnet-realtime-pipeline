#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Integration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Provides useful extension methods for <see cref="WebhookHandler"/> to simplify common webhook operations.
/// </summary>
public static class WebhookHandlerExtensions
{
    /// <summary>
    /// Subscribes to a specific event type with a callback URL.
    /// </summary>
    /// <param name="handler">The webhook handler instance.</param>
    /// <param name="url">The callback URL to receive webhooks.</param>
    /// <param name="eventType">The event type to subscribe to.</param>
    /// <param name="secret">Optional secret for webhook signature verification.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> or <paramref name="url"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="url"/> is empty.</exception>
    public static void SubscribeTo(this WebhookHandler handler, string url, WebhookEventType eventType, string? secret = null)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentException.ThrowIfNullOrEmpty(url);

        handler.Subscribe(url, eventType, secret);
    }

    /// <summary>
    /// Subscribes to multiple event types with a callback URL.
    /// </summary>
    /// <param name="handler">The webhook handler instance.</param>
    /// <param name="url">The callback URL to receive webhooks.</param>
    /// <param name="eventTypes">The event types to subscribe to.</param>
    /// <param name="secret">Optional secret for webhook signature verification.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> or <paramref name="url"/> is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="eventTypes"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="url"/> is empty.</exception>
    public static void SubscribeTo(this WebhookHandler handler, string url, IEnumerable<WebhookEventType> eventTypes, string? secret = null)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(eventTypes);
        ArgumentException.ThrowIfNullOrEmpty(url);

        var combinedType = eventTypes.Aggregate((current, next) => current | next);
        handler.Subscribe(url, combinedType, secret);
    }

    /// <summary>
    /// Unsubscribes from all webhook subscriptions matching the specified URL.
    /// </summary>
    /// <param name="handler">The webhook handler instance.</param>
    /// <param name="url">The callback URL to unsubscribe.</param>
    /// <returns>True if any subscriptions were removed; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> or <paramref name="url"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="url"/> is empty.</exception>
    public static bool UnsubscribeFrom(this WebhookHandler handler, string url)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentException.ThrowIfNullOrEmpty(url);

        return handler.Unsubscribe(url);
    }

    /// <summary>
    /// Gets all active subscriptions filtered by event type.
    /// </summary>
    /// <param name="handler">The webhook handler instance.</param>
    /// <param name="eventType">The event type to filter by.</param>
    /// <returns>A read-only list of active subscriptions for the specified event type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
    public static IReadOnlyList<WebhookSubscription> GetSubscriptionsFor(this WebhookHandler handler, WebhookEventType eventType)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var allSubscriptions = handler.GetSubscriptions();
        var filtered = allSubscriptions
            .Where(s => s.IsActive && (s.EventTypes & eventType) == eventType)
            .ToList()
            .AsReadOnly();

        return filtered;
    }

    /// <summary>
    /// Sends a webhook event to all subscribers of the specified event type.
    /// </summary>
    /// <param name="handler">The webhook handler instance.</param>
    /// <param name="eventType">The event type to send.</param>
    /// <param name="eventData">The event data payload.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> or <paramref name="eventData"/> is null.</exception>
    public static async Task SendWebhookEventAsync(this WebhookHandler handler, WebhookEventType eventType, object eventData)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(eventData);

        await handler.SendWebhookEventAsync(eventType, eventData);
    }

    /// <summary>
    /// Checks if there are any active subscriptions for the specified event type.
    /// </summary>
    /// <param name="handler">The webhook handler instance.</param>
    /// <param name="eventType">The event type to check.</param>
    /// <returns>True if there are active subscriptions; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
    public static bool HasSubscriptions(this WebhookHandler handler, WebhookEventType eventType)
    {
        ArgumentNullException.ThrowIfNull(handler);

        return handler.GetSubscriptionsFor(eventType).Count > 0;
    }

    /// <summary>
    /// Gets the count of active subscriptions for the specified event type.
    /// </summary>
    /// <param name="handler">The webhook handler instance.</param>
    /// <param name="eventType">The event type to count.</param>
    /// <returns>The number of active subscriptions.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
    public static int GetSubscriptionCount(this WebhookHandler handler, WebhookEventType eventType)
    {
        ArgumentNullException.ThrowIfNull(handler);

        return handler.GetSubscriptionsFor(eventType).Count;
    }

    /// <summary>
    /// Disables all subscriptions that have failed more than the specified threshold.
    /// </summary>
    /// <param name="handler">The webhook handler instance.</param>
    /// <param name="failureThreshold">The maximum allowed failure count before disabling.</param>
    /// <returns>The number of subscriptions that were disabled.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="failureThreshold"/> is negative.</exception>
    public static int DisableFailedSubscriptions(this WebhookHandler handler, int failureThreshold = 5)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentOutOfRangeException.ThrowIfNegative(failureThreshold);

        var subscriptions = handler.GetSubscriptions();
        var disabledCount = 0;

        foreach (var subscription in subscriptions)
        {
            if (subscription.FailureCount >= failureThreshold && subscription.IsActive)
            {
                subscription.IsActive = false;
                disabledCount++;
            }
        }

        return disabledCount;
    }

    /// <summary>
    /// Gets the oldest active subscription.
    /// </summary>
    /// <param name="handler">The webhook handler instance.</param>
    /// <returns>The oldest active subscription, or null if none exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
    public static WebhookSubscription? GetOldestActiveSubscription(this WebhookHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        return handler.GetSubscriptions()
            .Where(s => s.IsActive)
            .OrderBy(s => s.CreatedAt)
            .FirstOrDefault();
    }

    /// <summary>
    /// Gets the most recently delivered subscription.
    /// </summary>
    /// <param name="handler">The webhook handler instance.</param>
    /// <returns>The most recently delivered subscription, or null if none have been delivered.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
    public static WebhookSubscription? GetMostRecentDelivery(this WebhookHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        return handler.GetSubscriptions()
            .Where(s => s.LastDeliveredAt.HasValue)
            .OrderByDescending(s => s.LastDeliveredAt)
            .FirstOrDefault();
    }
}