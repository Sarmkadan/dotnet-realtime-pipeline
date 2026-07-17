#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetRealtimePipeline.Integration;

using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Provides validation helpers for <see cref="WebhookHandler"/> instances.
/// </summary>
public static class WebhookHandlerValidation
{
    /// <summary>
    /// Validates a <see cref="WebhookHandler"/> instance.
    /// </summary>
    /// <param name="value">The webhook handler to validate.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this WebhookHandler? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether a <see cref="WebhookHandler"/> instance is valid.
    /// </summary>
    /// <param name="value">The webhook handler to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this WebhookHandler? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="WebhookHandler"/> instance is valid.
    /// </summary>
    /// <param name="value">The webhook handler to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid.</exception>
    public static void EnsureValid(this WebhookHandler? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                FormattableString.Invariant($"""WebhookHandler validation failed:{Environment.NewLine}- {
                    string.Join(
                        "\n- ",
                        errors
                    )
                }"""),
                nameof(value)
            );
        }
    }

    /// <summary>
    /// Validates a <see cref="WebhookSubscription"/> instance.
    /// </summary>
    /// <param name="subscription">The webhook subscription to validate.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="subscription"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this WebhookSubscription? subscription)
    {
        ArgumentNullException.ThrowIfNull(subscription);

        var errors = new List<string>();

        // Validate Id using pattern matching
        if (string.IsNullOrWhiteSpace(subscription.Id) || !Guid.TryParse(subscription.Id, out _))
        {
            errors.Add("WebhookSubscription.Id must be a valid GUID.");
        }

        // Validate Url using pattern matching
        if (string.IsNullOrWhiteSpace(subscription.Url) || !Uri.IsWellFormedUriString(subscription.Url, UriKind.Absolute))
        {
            errors.Add("WebhookSubscription.Url must be a well-formed absolute URI.");
        }

        // Validate EventTypes
        if (subscription.EventTypes == 0)
        {
            errors.Add("WebhookSubscription.EventTypes must be a valid combination of WebhookEventType values.");
        }

        // Validate Secret (optional but must be valid if provided)
        if (!string.IsNullOrEmpty(subscription.Secret) && subscription.Secret.Length < 8)
        {
            errors.Add("WebhookSubscription.Secret must be at least 8 characters long if provided.");
        }

        // Validate CreatedAt
        var now = DateTime.UtcNow;
        if (subscription.CreatedAt == default)
        {
            errors.Add("WebhookSubscription.CreatedAt must be a valid DateTime, not the default value.");
        }
        else if (subscription.CreatedAt > now.AddMinutes(5))
        {
            errors.Add("WebhookSubscription.CreatedAt cannot be in the future.");
        }

        // Validate LastDeliveredAt (if set)
        if (subscription.LastDeliveredAt.HasValue)
        {
            var lastDelivered = subscription.LastDeliveredAt.Value;
            if (lastDelivered > now.AddMinutes(5))
            {
                errors.Add("WebhookSubscription.LastDeliveredAt cannot be in the future.");
            }
            else if (lastDelivered < subscription.CreatedAt)
            {
                errors.Add("WebhookSubscription.LastDeliveredAt cannot be earlier than CreatedAt.");
            }
        }

        // Validate FailureCount
        if (subscription.FailureCount < 0)
        {
            errors.Add("WebhookSubscription.FailureCount must be a non-negative integer.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="WebhookSubscription"/> instance is valid.
    /// </summary>
    /// <param name="subscription">The webhook subscription to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="subscription"/> is null.</exception>
    public static bool IsValid(this WebhookSubscription? subscription)
    {
        ArgumentNullException.ThrowIfNull(subscription);
        return subscription.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="WebhookSubscription"/> instance is valid.
    /// </summary>
    /// <param name="subscription">The webhook subscription to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="subscription"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="subscription"/> is not valid.</exception>
    public static void EnsureValid(this WebhookSubscription? subscription)
    {
        ArgumentNullException.ThrowIfNull(subscription);

        var errors = subscription.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                FormattableString.Invariant($"""WebhookSubscription validation failed:{Environment.NewLine}- {
                    string.Join(
                        "\n- ",
                        errors
                    )
                }"""),
                nameof(subscription)
            );
        }
    }

    /// <summary>
    /// Validates a <see cref="WebhookPayload"/> instance.
    /// </summary>
    /// <param name="payload">The webhook payload to validate.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="payload"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this WebhookPayload? payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        var errors = new List<string>();
        var now = DateTime.UtcNow;

        // Validate EventType using pattern matching
        if (string.IsNullOrWhiteSpace(payload.EventType))
        {
            errors.Add("WebhookPayload.EventType must not be null, empty, or whitespace.");
        }

        // Validate Data
        // Data can be null for certain event types, so we only validate if it's provided
        if (payload.Data is string str && string.IsNullOrWhiteSpace(str))
        {
            errors.Add("WebhookPayload.Data must not be an empty or whitespace string.");
        }

        // Validate Timestamp
        if (payload.Timestamp == default)
        {
            errors.Add("WebhookPayload.Timestamp must be a valid DateTime, not the default value.");
        }
        else if (payload.Timestamp > now.AddMinutes(5))
        {
            errors.Add("WebhookPayload.Timestamp cannot be in the future.");
        }

        // Validate SubscriptionId using pattern matching
        if (string.IsNullOrWhiteSpace(payload.SubscriptionId) || !Guid.TryParse(payload.SubscriptionId, out _))
        {
            errors.Add("WebhookPayload.SubscriptionId must be a valid GUID.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="WebhookPayload"/> instance is valid.
    /// </summary>
    /// <param name="payload">The webhook payload to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="payload"/> is null.</exception>
    public static bool IsValid(this WebhookPayload? payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        return payload.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="WebhookPayload"/> instance is valid.
    /// </summary>
    /// <param name="payload">The webhook payload to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="payload"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="payload"/> is not valid.</exception>
    public static void EnsureValid(this WebhookPayload? payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        var errors = payload.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                FormattableString.Invariant($"""WebhookPayload validation failed:{Environment.NewLine}- {
                    string.Join(
                        "\n- ",
                        errors
                    )
                }"""),
                nameof(payload)
            );
        }
    }
}

/// <summary>
/// Provides validation extensions for <see cref="InboundWebhookHandler"/> instances.
/// </summary>
public static class InboundWebhookHandlerValidation
{
    /// <summary>
    /// Validates an <see cref="InboundWebhookHandler"/> instance.
    /// </summary>
    /// <param name="value">The inbound webhook handler to validate.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this InboundWebhookHandler? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether an <see cref="InboundWebhookHandler"/> instance is valid.
    /// </summary>
    /// <param name="value">The inbound webhook handler to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this InboundWebhookHandler? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that an <see cref="InboundWebhookHandler"/> instance is valid.
    /// </summary>
    /// <param name="value">The inbound webhook handler to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static void EnsureValid(this InboundWebhookHandler? value)
    {
        ArgumentNullException.ThrowIfNull(value);
    }
}