# Webhook Subscription Models

This document describes the webhook subscription models available in the `DotNetRealtimePipeline.Integration` namespace, including `WebhookSubscription`, `WebhookPayload`, and `InboundWebhookHandler`.

## Overview

The webhook subscription models provide functionality for managing webhook subscriptions, handling webhook payloads, and processing incoming webhooks. These models are used to register, unregister, and manage webhook subscriptions, as well as to handle and process webhook events.

## Models

### WebhookSubscription

Represents a webhook subscription.

**Properties:**

- **Id**: A unique identifier for the subscription.
- **Url**: The URL to which the webhook events will be sent.
- **EventTypes**: The types of events the subscription is interested in. This is a bitmask of `WebhookEventType` values.
- **Secret**: An optional secret used to sign the webhook payloads for verification.
- **CreatedAt**: The UTC timestamp when the subscription was created.
- **LastDeliveredAt**: The UTC timestamp when the last webhook event was successfully delivered.
- **IsActive**: A boolean value indicating whether the subscription is active.
- **FailureCount**: The number of consecutive failures in delivering webhook events.

**Methods:**

- **ToString()**: Returns a string representation of the subscription. The secret value is never included; only whether it is set.

### WebhookPayload

Represents the payload of a webhook event.

**Properties:**

- **EventType**: The type of the event.
- **Data**: The data associated with the event.
- **Timestamp**: The UTC timestamp when the event was created.
- **SubscriptionId**: The ID of the subscription that triggered the event.

**Methods:**

- **ToString()**: Returns a string representation of the payload.

### InboundWebhookHandler

Handles incoming webhook events.

**Methods:**

- **RegisterHandler(Func<WebhookPayload, Task> handler)**: Registers a handler for incoming webhooks.
- **ProcessWebhookAsync(WebhookPayload payload)**: Processes an incoming webhook payload.

## WebhookEventType

The `WebhookEventType` enum represents the types of events that can be subscribed to. It is a bitmask enum, allowing multiple event types to be combined.

**Values:**

- **DataIngested**: Indicates that data has been ingested.
- **ProcessingCompleted**: Indicates that processing has been completed.
- **BackpressureDetected**: Indicates that backpressure has been detected.
- **MetricsCollected**: Indicates that metrics have been collected.
- **PipelineError**: Indicates that a pipeline error has occurred.
- **All**: A combination of all the above event types.

## Delivery Retry Logic

The `WebhookHandler` class includes retry logic for delivering webhook events. If a delivery attempt fails, the handler will retry the delivery with an exponential backoff strategy. The initial retry delay is 1000 milliseconds, and the maximum number of delivery attempts is 3. If a delivery attempt fails with a 409 Conflict or 400 Bad Request status code, the handler will not retry the delivery and will mark the subscription as inactive.

## Signature Secret

The `WebhookSubscription` class includes an optional `Secret` property, which can be used to sign webhook payloads for verification. The secret is never included in the string representation of the subscription. The `WebhookPayload` class does not include a signature property, but the payload can be signed using the secret associated with the subscription.
