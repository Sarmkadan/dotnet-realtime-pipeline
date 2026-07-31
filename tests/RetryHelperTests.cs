namespace DotNetRealtimePipeline.Tests;

using Xunit;

public class RetryHelperTests
{
    [Fact]
    public async Task RetryAsync_HappyPath_ReturnsResult()
    {
        // Arrange
        var operation = new Func<Task<int>>(async () => 42);
        var retryPolicy = new RetryPolicyBuilder().WithMaxAttempts(3).Build();

        // Act
        var result = await RetryHelper.RetryAsync(operation, retryPolicy);

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task RetryAsync_NullOperation_ThrowsArgumentNullException()
    {
        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => RetryHelper.RetryAsync(null, new RetryPolicyBuilder().Build()));
    }

    [Fact]
    public async Task RetryAsync_NullRetryPolicy_ThrowsArgumentNullException()
    {
        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => RetryHelper.RetryAsync(new Func<Task<int>>(async () => 42), null));
    }

    [Fact]
    public async Task RetryAsync_MaxAttemptsReached_ReturnsResult()
    {
        // Arrange
        var operation = new Func<Task<int>>(async () => 42);
        var retryPolicy = new RetryPolicyBuilder().WithMaxAttempts(1).Build();

        // Act
        var result = await RetryHelper.RetryAsync(operation, retryPolicy);

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task RetryAsync_DelayIncreased_ReturnsResult()
    {
        // Arrange
        var operation = new Func<Task<int>>(async () => 42);
        var retryPolicy = new RetryPolicyBuilder().WithMaxAttempts(3).WithInitialDelay(100).Build();

        // Act
        var result = await RetryHelper.RetryAsync(operation, retryPolicy);

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void Retry_HappyPath_ReturnsResult()
    {
        // Arrange
        var operation = new Func<int>(() => 42);
        var retryPolicy = new RetryPolicyBuilder().WithMaxAttempts(3).Build();

        // Act
        var result = RetryHelper.Retry(operation, retryPolicy);

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void Retry_NullOperation_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => RetryHelper.Retry(null, new RetryPolicyBuilder().Build()));
    }

    [Fact]
    public void Retry_NullRetryPolicy_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => RetryHelper.Retry(new Func<int>(() => 42), null));
    }

    [Fact]
    public void Retry_MaxAttemptsReached_ReturnsResult()
    {
        // Arrange
        var operation = new Func<int>(() => 42);
        var retryPolicy = new RetryPolicyBuilder().WithMaxAttempts(1).Build();

        // Act
        var result = RetryHelper.Retry(operation, retryPolicy);

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void Retry_DelayIncreased_ReturnsResult()
    {
        // Arrange
        var operation = new Func<int>(() => 42);
        var retryPolicy = new RetryPolicyBuilder().WithMaxAttempts(3).WithInitialDelay(100).Build();

        // Act
        var result = RetryHelper.Retry(operation, retryPolicy);

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void RetryPolicyBuilder_WithMaxAttempts_ReturnsBuilder()
    {
        // Act
        var builder = new RetryPolicyBuilder().WithMaxAttempts(3);

        // Assert
        Assert.Equal(3, builder.MaxAttempts);
    }

    [Fact]
    public void RetryPolicyBuilder_WithInitialDelay_ReturnsBuilder()
    {
        // Act
        var builder = new RetryPolicyBuilder().WithInitialDelay(100);

        // Assert
        Assert.Equal(100, builder.InitialDelayMs);
    }

    [Fact]
    public void RetryPolicyBuilder_WithMaxDelay_ReturnsBuilder()
    {
        // Act
        var builder = new RetryPolicyBuilder().WithMaxDelay(100);

        // Assert
        Assert.Equal(100, builder.MaxDelayMs);
    }

    [Fact]
    public void RetryPolicyBuilder_WithJitter_ReturnsBuilder()
    {
        // Act
        var builder = new RetryPolicyBuilder().WithJitter(true);

        // Assert
        Assert.True(builder.UseJitter);
    }

    [Fact]
    public void RetryPolicyBuilder_RetryOn_ReturnsBuilder()
    {
        // Act
        var builder = new RetryPolicyBuilder().RetryOn<Exception>();

        // Assert
        Assert.Single(builder.RetryableExceptions);
    }

    [Fact]
    public void RetryPolicyBuilder_Build_ReturnsPolicy()
    {
        // Act
        var policy = new RetryPolicyBuilder().Build();

        // Assert
        Assert.NotNull(policy);
    }

    [Fact]
    public void RetryPolicy_MaxAttempts_ReturnsValue()
    {
        // Act
        var policy = new RetryPolicyBuilder().WithMaxAttempts(3).Build();

        // Assert
        Assert.Equal(3, policy.MaxAttempts);
    }

    [Fact]
    public void RetryPolicy_InitialDelay_ReturnsValue()
    {
        // Act
        var policy = new RetryPolicyBuilder().WithInitialDelay(100).Build();

        // Assert
        Assert.Equal(100, policy.InitialDelayMs);
    }

    [Fact]
    public void RetryPolicy_MaxDelay_ReturnsValue()
    {
        // Act
        var policy = new RetryPolicyBuilder().WithMaxDelay(100).Build();

        // Assert
        Assert.Equal(100, policy.MaxDelayMs);
    }

    [Fact]
    public void RetryPolicy_UseJitter_ReturnsValue()
    {
        // Act
        var policy = new RetryPolicyBuilder().WithJitter(true).Build();

        // Assert
        Assert.True(policy.UseJitter);
    }

    [Fact]
    public void RetryPolicy_RetryableExceptions_ReturnsValue()
    {
        // Act
        var policy = new RetryPolicyBuilder().RetryOn<Exception>().Build();

        // Assert
        Assert.Single(policy.RetryableExceptions);
    }
}
