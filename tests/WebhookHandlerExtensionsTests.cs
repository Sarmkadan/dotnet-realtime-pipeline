namespace DotNetRealtimePipeline.Tests.Unit;

using Xunit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetRealtimePipeline.Integration;

public class WebhookHandlerExtensionsTests
{
    [Fact]
    public void SubscribeTo_HappyPath_SubscribesToWebhook()
    {
        // Arrange
        var handler = new WebhookHandler();

        // Act
        handler.SubscribeTo("https://example.com", WebhookEventType.Test);

        // Assert
        Assert.True(handler.HasSubscriptions(WebhookEventType.Test));
    }

    [Fact]
    public void UnsubscribeFrom_HappyPath_UnsubscribesFromWebhook()
    {
        // Arrange
        var handler = new WebhookHandler();
        handler.SubscribeTo("https://example.com", WebhookEventType.Test);

        // Act
        var result = handler.UnsubscribeFrom("https://example.com");

        // Assert
        Assert.True(result);
        Assert.False(handler.HasSubscriptions(WebhookEventType.Test));
    }

    [Fact]
    public void GetSubscriptionsFor_HappyPath_ReturnsSubscriptions()
    {
        // Arrange
        var handler = new WebhookHandler();
        handler.SubscribeTo("https://example.com", WebhookEventType.Test);

        // Act
        var subscriptions = handler.GetSubscriptionsFor(WebhookEventType.Test);

        // Assert
        Assert.NotNull(subscriptions);
        Assert.Single(subscriptions);
    }

    [Fact]
    public void SendWebhookEventAsync_HappyPath_SendsWebhookEvent()
    {
        // Arrange
        var handler = new WebhookHandler();
        handler.SubscribeTo("https://example.com", WebhookEventType.Test);

        // Act
        var task = handler.SendWebhookEventAsync(WebhookEventType.Test, new object());

        // Assert
        Assert.NotNull(task);
        task.Wait();
    }

    [Fact]
    public void DisableFailedSubscriptions_HappyPath_DisablesFailedSubscriptions()
    {
        // Arrange
        var handler = new WebhookHandler();
        handler.SubscribeTo("https://example.com", WebhookEventType.Test);

        // Act
        var result = handler.DisableFailedSubscriptions();

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetOldestActiveSubscription_HappyPath_ReturnsOldestActiveSubscription()
    {
        // Arrange
        var handler = new WebhookHandler();
        handler.SubscribeTo("https://example.com", WebhookEventType.Test);

        // Act
        var subscription = handler.GetOldestActiveSubscription();

        // Assert
        Assert.NotNull(subscription);
    }

    [Fact]
    public void GetMostRecentDelivery_HappyPath_ReturnsMostRecentDelivery()
    {
        // Arrange
        var handler = new WebhookHandler();
        handler.SubscribeTo("https://example.com", WebhookEventType.Test);

        // Act
        var subscription = handler.GetMostRecentDelivery();

        // Assert
        Assert.Null(subscription);
    }
}
