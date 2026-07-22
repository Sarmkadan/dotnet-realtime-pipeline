using Xunit;

namespace DotNetRealtimePipeline.Tests.Integration;

public class WebhookHandlerValidationTests
{
    [Fact]
    public void Validate_ReturnsEmptyList_ForValidWebhookHandler()
    {
        // Arrange
        var webhookHandler = new WebhookHandler();

        // Act
        var errors = WebhookHandlerValidation.Validate(webhookHandler);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_ThrowsArgumentNullException_ForNullWebhookHandler()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => WebhookHandlerValidation.Validate(null));
    }

    [Fact]
    public void IsValid_ReturnsTrue_ForValidWebhookHandler()
    {
        // Arrange
        var webhookHandler = new WebhookHandler();

        // Act
        var isValid = WebhookHandlerValidation.IsValid(webhookHandler);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_ReturnsFalse_ForInvalidWebhookHandler()
    {
        // Arrange
        var webhookHandler = new WebhookHandler { Id = null };

        // Act
        var isValid = WebhookHandlerValidation.IsValid(webhookHandler);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void EnsureValid_DoesNotThrow_ForValidWebhookHandler()
    {
        // Arrange
        var webhookHandler = new WebhookHandler();

        // Act and Assert
        WebhookHandlerValidation.EnsureValid(webhookHandler);
    }

    [Fact]
    public void EnsureValid_ThrowsArgumentException_ForInvalidWebhookHandler()
    {
        // Arrange
        var webhookHandler = new WebhookHandler { Id = null };

        // Act and Assert
        Assert.Throws<ArgumentException>(() => WebhookHandlerValidation.EnsureValid(webhookHandler));
    }
}
