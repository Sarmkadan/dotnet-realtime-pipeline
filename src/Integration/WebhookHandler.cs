#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Integration;

using DotNetRealtimePipeline.Events;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

/// <summary>
/// Handles webhook delivery for pipeline events.
/// Manages subscription, retry logic, and delivery tracking.
/// </summary>
public sealed class WebhookHandler
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WebhookHandler> _logger;
    private readonly List<WebhookSubscription> _subscriptions = new();
    private readonly object _lockObject = new();

    public WebhookHandler(HttpClient httpClient, ILogger<WebhookHandler> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Registers a webhook subscription.
    /// </summary>
    public void Subscribe(string url, WebhookEventType eventTypes, string secret = null)
    {
        lock (_lockObject)
        {
            var subscription = new WebhookSubscription
            {
                Id = Guid.NewGuid().ToString(),
                Url = url,
                EventTypes = eventTypes,
                Secret = secret,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _subscriptions.Add(subscription);
            _logger.LogInformation("Webhook subscription registered: {Url} for events {Events}", url, eventTypes);
        }
    }

    /// <summary>
    /// Unsubscribes a webhook.
    /// </summary>
    public bool Unsubscribe(string url)
    {
        lock (_lockObject)
        {
            var removed = _subscriptions.RemoveAll(s => s.Url == url);
            if (removed > 0)
            {
                _logger.LogInformation("Webhook subscription removed: {Url}", url);
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Sends a webhook event to all registered subscribers.
    /// </summary>
    public async Task SendWebhookEventAsync(WebhookEventType eventType, object eventData)
    {
        List<WebhookSubscription> activeSubscriptions;

        lock (_lockObject)
        {
            activeSubscriptions = _subscriptions
                .FindAll(s => s.IsActive && (s.EventTypes & eventType) == eventType);
        }

        var tasks = activeSubscriptions.Select(sub => SendToWebhookAsync(sub, eventType, eventData));
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Sends webhook to a specific subscription with retry logic.
    /// </summary>
    private async Task SendToWebhookAsync(WebhookSubscription subscription, WebhookEventType eventType, object eventData)
    {
        var payload = new WebhookPayload
        {
            EventType = eventType.ToString(),
            Data = eventData,
            Timestamp = DateTime.UtcNow,
            SubscriptionId = subscription.Id
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        int attempt = 0;
        int delayMs = 1000;
        int maxRetries = 3;

        while (attempt < maxRetries)
        {
            try
            {
                var response = await _httpClient.PostAsync(subscription.Url, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogDebug("Webhook delivered successfully: {Url}", subscription.Url);
                    subscription.LastDeliveredAt = DateTime.UtcNow;
                    subscription.FailureCount = 0;
                    return;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Conflict ||
                         response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    // Don't retry on client errors
                    _logger.LogWarning("Webhook rejected: {Url} - {StatusCode}", subscription.Url, response.StatusCode);
                    subscription.IsActive = false;
                    return;
                }
            }
            catch (HttpRequestException ex)
            {
                attempt++;
                _logger.LogWarning("Webhook delivery failed (attempt {Attempt}): {Url} - {Message}",
                    attempt, subscription.Url, ex.Message);

                if (attempt < maxRetries)
                {
                    await Task.Delay(delayMs);
                    delayMs *= 2;
                }
            }
        }

        subscription.FailureCount++;
        if (subscription.FailureCount >= 5)
        {
            subscription.IsActive = false;
            _logger.LogError("Webhook disabled after {Count} failures: {Url}", subscription.FailureCount, subscription.Url);
        }
    }

    /// <summary>
    /// Gets all active webhook subscriptions.
    /// </summary>
    public List<WebhookSubscription> GetSubscriptions()
    {
        lock (_lockObject)
        {
            return new List<WebhookSubscription>(_subscriptions);
        }
    }
}

/// <summary>
/// Represents a webhook subscription.
/// </summary>
public sealed class WebhookSubscription
{
    public string Id { get; set; }
    public string Url { get; set; }
    public WebhookEventType EventTypes { get; set; }
    public string Secret { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastDeliveredAt { get; set; }
    public bool IsActive { get; set; }
    public int FailureCount { get; set; }
}

/// <summary>
/// Webhook event types that can be subscribed to.
/// </summary>
[Flags]
public enum WebhookEventType
{
    DataIngested = 1,
    ProcessingCompleted = 2,
    BackpressureDetected = 4,
    MetricsCollected = 8,
    PipelineError = 16,
    All = DataIngested | ProcessingCompleted | BackpressureDetected | MetricsCollected | PipelineError
}

/// <summary>
/// Webhook payload structure.
/// </summary>
public sealed class WebhookPayload
{
    public string EventType { get; set; }
    public object Data { get; set; }
    public DateTime Timestamp { get; set; }
    public string SubscriptionId { get; set; }
}

/// <summary>
/// Handler for receiving and processing webhook events from external sources.
/// </summary>
public sealed class InboundWebhookHandler
{
    private readonly ILogger<InboundWebhookHandler> _logger;
    private readonly List<Func<WebhookPayload, Task>> _handlers = new();

    public InboundWebhookHandler(ILogger<InboundWebhookHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Registers a handler for incoming webhooks.
    /// </summary>
    public void RegisterHandler(Func<WebhookPayload, Task> handler)
    {
        _handlers.Add(handler ?? throw new ArgumentNullException(nameof(handler)));
    }

    /// <summary>
    /// Processes an incoming webhook payload.
    /// </summary>
    public async Task ProcessWebhookAsync(WebhookPayload payload)
    {
        if (payload is null)
        {
            _logger.LogWarning("Null webhook payload received");
            return;
        }

        _logger.LogInformation("Processing inbound webhook: {EventType}", payload.EventType);

        foreach (var handler in _handlers)
        {
            try
            {
                await handler(payload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook");
            }
        }
    }
}
