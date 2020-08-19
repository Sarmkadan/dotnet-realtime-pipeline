# WebhookHandler

The `WebhookHandler` type encapsulates the lifecycle of a webhook subscription within the dotnet-realtime-pipeline library, providing methods to subscribe to outgoing webhooks, send events, manage subscription metadata, and process inbound webhook requests via an associated handler.

## API

### Constructors

| Member | Description |
|--------|-------------|
| `public WebhookHandler()` | Initializes a new instance of the `WebhookHandler` with default values. Properties such as `Id`, `CreatedAt`, and `IsActive` are set automatically; other properties must be configured before calling `Subscribe`. |

### Methods

| Member | Purpose | Parameters | Return Value | Throws |
|--------|---------|------------|--------------|--------|
| `public void Subscribe()` | Registers the webhook endpoint with the pipeline using the currently configured `Url`, `EventTypes`, and `Secret`. After successful subscription, the handler is ready to send and receive events. | None | `void` | `InvalidOperationException` if required properties (`Url`, `EventTypes`, `Secret`) are null or empty; `WebhookException` if the subscription request fails. |
| `public bool Unsubscribe()` | Removes the subscription for this handler. Returns `true` if the unsubscription succeeded, otherwise `false`. | None | `bool` | `WebhookException` if the unsubscription request encounters a network or protocol error. |
| `public async Task SendWebhookEventAsync()` | Sends a webhook event using the data currently stored in the `Data` property and the event type indicated by `EventType`. The method serializes the payload, computes a signature using `Secret`, and POSTs to the configured `Url`. | None | `Task` | `InvalidOperationException` if the handler is not subscribed (`IsActive` is false) or if `Data` is null; `WebhookException` if the HTTP request fails or the remote endpoint returns a non‑success status code. |
| `public List<WebhookSubscription> GetSubscriptions()` | Retrieves a copy of all active webhook subscriptions known to the handler (typically used for debugging or administrative purposes). | None | `List<WebhookSubscription>` | `WebhookException` if the underlying subscription store cannot be accessed. |
| `public void RegisterHandler()` | Registers an inbound webhook processing delegate that will be invoked by `ProcessWebhookAsync`. The handler must be set prior to calling the processing method. | None | `void` | `InvalidOperationException` if a handler is already registered or if the supplied handler is null. |
| `public async Task ProcessWebhookAsync()` | Processes an incoming webhook request: validates the signature using `Secret`, deserializes the payload into the `Data` property, updates `Timestamp`, and invokes the registered inbound handler. | None | `Task` | `InvalidOperationException` if no handler has been registered; `WebhookException` if signature validation fails or the payload cannot be deserialized. |

### Properties

| Member | Type | Description |
|--------|------|-------------|
| `public string Id { get; set; }` | `string` | Unique identifier for the webhook subscription, generated upon successful subscription. |
| `public string Url { get; set; }` | `string` | Target URL where outgoing webhooks are delivered. Must be a valid HTTP/HTTPS endpoint. |
| `public WebhookEventType EventTypes { get; set; }` | `WebhookEventType` | Bit‑flag enumeration indicating which event types the subscription is interested in. |
| `public string Secret { get; set; }` | `string` | Shared secret used to compute HMAC signatures for both outgoing and incoming webhooks. |
| `public DateTime CreatedAt { get; }` | `DateTime` | UTC timestamp indicating when the subscription was initially created. Set automatically by the constructor. |
| `public DateTime? LastDeliveredAt { get; set; }` | `DateTime?` | UTC timestamp of the last successful webhook delivery; null if no delivery has succeeded yet. |
| `public bool IsActive { get; set; }` | `bool` | Indicates whether the subscription is currently active and able to send/receive events. |
| `public int FailureCount { get; set; }` | `int` | Number of consecutive failed delivery attempts; reset to zero on a successful delivery. |
| `public string EventType { get; set; }` | `string` | The specific event type name associated with the payload held in `Data`. |
| `public object Data { get; set; }` | `object` | Arbitrary payload object to be sent with an outgoing webhook or populated from an incoming webhook. |
| `public DateTime Timestamp { get; set; }` | `DateTime` | UTC timestamp associated with the current `Data` payload (either set before sending or filled after receiving). |
| `public string SubscriptionId { get; set; }` | `string` | Identifier of the subscription as known by the remote webhook service; may differ from `Id`. |
| `public InboundWebhookHandler InboundWebhookHandler { get; }` | `InboundWebhookHandler` | Provides access to the internal component responsible for validating and dispatching inbound webhook requests. |

## Usage

### Example 1: Configuring and sending an outgoing webhook

```csharp
using System;
using System.Threading.Tasks;
using DotnetRealtimePipeline;

class Program
{
    static async Task Main()
    {
        var handler = new WebhookHandler
        {
            Url = "https://example.com/webhooks/receive",
            EventTypes = WebhookEventType.OrderCreated | WebhookEventType.PaymentReceived,
            Secret = "my-shared-secret",
            IsActive = true
        };

        // Subscribe to the remote service
        handler.Subscribe();

        // Prepare payload
        handler.EventType = "OrderCreated";
        handler.Data = new { OrderId = 12345, Amount = 99.99 };
        handler.Timestamp = DateTime.UtcNow;

        // Send the webhook
        await handler.SendWebhookEventAsync();

        // Optional: clean up
        handler.Unsubscribe();
    }
}
```

### Example 2: Processing an inbound webhook request

```csharp
using System;
using System.Threading.Tasks;
using DotnetRealtimePipeline;

class Program
{
    static async Task Main()
    {
        var handler = new WebhookHandler
        {
            Url = "https://example.com/webhooks/receive",
            Secret = "my-shared-secret"
        };

        // Register a delegate to handle incoming payloads
        handler.RegisterHandler = (payload) =>
        {
            Console.WriteLine($"Received webhook of type {handler.EventType} at {handler.Timestamp}");
            // Process payload (handler.Data) as needed
            return Task.CompletedTask;
        };

        // Simulate receiving an HTTP request (in practice this would be called from ASP.NET Core middleware)
        // Assume requestBody and headers are obtained from the incoming HTTP request
        // handler.ProcessWebhookAsync() will validate the signature and populate handler.Data
        await handler.ProcessWebhookAsync();
    }
}
```

## Notes

- The `Subscribe` and `Unsubscribe` methods are **not thread‑safe**; concurrent calls from multiple threads may result in duplicate subscription attempts or inconsistent state. External synchronization is required if the instance is shared.
- Property setters (`Url`, `Secret`, `EventTypes`, etc.) should be configured **before** calling `Subscribe`. Changing these values after subscription does not affect the existing registration; a new subscription must be made via `Unsubscribe` followed by `Subscribe`.
- `SendWebhookEventAsync` relies on the current values of `Data`, `EventType`, and `Timestamp`. If any of these are null or unset, the method will throw an `InvalidOperationException`.
- `ProcessWebhookAsync` expects that the incoming HTTP request has already been passed to the handler (e.g., via middleware). The method validates the HMAC‑SHA256 signature using the `Secret` property; an invalid signature results in a `WebhookException`.
- The `InboundWebhookHandler` property provides access to low‑level validation logic but should not be replaced; altering its state may break signature verification.
- `FailureCount` is incremented automatically after each failed delivery attempt and is reset to zero upon a successful delivery. It can be inspected for retry‑policy decisions.
- All date/time properties (`CreatedAt`, `LastDeliveredAt`, `Timestamp`) are expressed in UTC; consumers should convert to local time only for presentation purposes.
