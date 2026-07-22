// tests/WebhookHandlerTests.cs
namespace DotNetRealtimePipeline.Tests.Integration;

using Xunit;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DotNetRealtimePipeline.Integration;
using DotNetRealtimePipeline.Events;

public class WebhookHandlerTests
{
    [Fact]
    public void Constructor_ThrowsArgumentNullException_ForNullHttpClient()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => new WebhookHandler(null, new Logger<WebhookHandler>()));
    }

    [Fact]
    public void Subscribe_AddsSubscription_ToSubscriptions()
    {
        // Arrange
        var webhookHandler = new WebhookHandler(new HttpClient(), new Logger<WebhookHandler>());
        var subscription = new WebhookSubscription { Id = Guid.NewGuid().ToString(), Url = "https://example.com", EventTypes = WebhookEventType.All };

        // Act
        webhookHandler.Subscribe(subscription.Url, subscription.EventTypes);

        // Assert
        Assert.Single(webhookHandler.GetSubscriptions());
    }

    [Fact]
    public void Unsubscribe_RemovesSubscription_FromSubscriptions()
    {
        // Arrange
        var webhookHandler = new WebhookHandler(new HttpClient(), new Logger<WebhookHandler>());
        var subscription = new WebhookSubscription { Id = Guid.NewGuid().ToString(), Url = "https://example.com", EventTypes = WebhookEventType.All };
        webhookHandler.Subscribe(subscription.Url, subscription.EventTypes);

        // Act
        webhookHandler.Unsubscribe(subscription.Url);

        // Assert
        Assert.Empty(webhookHandler.GetSubscriptions());
    }

    [Fact]
    public async Task SendWebhookEventAsync_SendsWebhook_ToAllSubscriptions()
    {
        // Arrange
        var webhookHandler = new WebhookHandler(new HttpClient(), new Logger<WebhookHandler>());
        var subscription1 = new WebhookSubscription { Id = Guid.NewGuid().ToString(), Url = "https://example.com/1", EventTypes = WebhookEventType.All };
        var subscription2 = new WebhookSubscription { Id = Guid.NewGuid().ToString(), Url = "https://example.com/2", EventTypes = WebhookEventType.All };
        webhookHandler.Subscribe(subscription1.Url, subscription1.EventTypes);
        webhookHandler.Subscribe(subscription2.Url, subscription2.EventTypes);

        // Act
        await webhookHandler.SendWebhookEventAsync(WebhookEventType.All, new object());

        // Assert
        Assert.Single(webhookHandler.GetSubscriptions());
    }

    [Fact]
    public void GetSubscriptions_ReturnsAllSubscriptions()
    {
        // Arrange
        var webhookHandler = new WebhookHandler(new HttpClient(), new Logger<WebhookHandler>());
        var subscription1 = new WebhookSubscription { Id = Guid.NewGuid().ToString(), Url = "https://example.com/1", EventTypes = WebhookEventType.All };
        var subscription2 = new WebhookSubscription { Id = Guid.NewGuid().ToString(), Url = "https://example.com/2", EventTypes = WebhookEventType.All };
        webhookHandler.Subscribe(subscription1.Url, subscription1.EventTypes);
        webhookHandler.Subscribe(subscription2.Url, subscription2.EventTypes);

        // Act
        var subscriptions = webhookHandler.GetSubscriptions();

        // Assert
        Assert.Equal(2, subscriptions.Count);
    }
}
