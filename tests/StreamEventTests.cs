using System;
using System.Collections.Generic;
using Xunit;

namespace DotNetRealtimePipeline.Domain.Models;

public class StreamEventTests
{
    [Fact]
    public void Constructor_Parameterless_CreatesValidInstance()
    {
        // Act
        var streamEvent = new StreamEvent();

        // Assert
        Assert.NotNull(streamEvent);
        Assert.Equal(0, streamEvent.EventId);
        Assert.Equal(0, streamEvent.DataPointId);
        Assert.Equal(0, streamEvent.Timestamp);
        Assert.Equal(string.Empty, streamEvent.EventType);
        Assert.Equal(5, streamEvent.Priority);
        Assert.NotNull(streamEvent.Payload);
        Assert.NotNull(streamEvent.ProcessedByStages);
        Assert.NotNull(streamEvent.CreatedAt);
    }

    [Fact]
    public void Constructor_WithRequiredParameters_SetsPropertiesCorrectly()
    {
        // Arrange
        var eventId = 123L;
        var dataPointId = 456L;
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var eventType = "data";

        // Act
        var streamEvent = new StreamEvent(eventId, dataPointId, timestamp, eventType);

        // Assert
        Assert.Equal(eventId, streamEvent.EventId);
        Assert.Equal(dataPointId, streamEvent.DataPointId);
        Assert.Equal(timestamp, streamEvent.Timestamp);
        Assert.Equal(eventType, streamEvent.EventType);
        Assert.Equal(5, streamEvent.Priority);
        Assert.NotNull(streamEvent.Payload);
        Assert.Empty(streamEvent.Payload);
        Assert.NotNull(streamEvent.ProcessedByStages);
        Assert.Empty(streamEvent.ProcessedByStages);
        Assert.NotNull(streamEvent.CreatedAt);
        Assert.False(streamEvent.IsRetry);
        Assert.Equal(0, streamEvent.RetryAttempt);
        Assert.Null(streamEvent.LastErrorMessage);
        Assert.Null(streamEvent.CompletedAt);
    }

    [Fact]
    public void Constructor_WithNullEventType_ThrowsArgumentNullException()
    {
        // Arrange
        var eventId = 123L;
        var dataPointId = 456L;
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new StreamEvent(eventId, dataPointId, timestamp, null!));
    }

    [Fact]
    public void Constructor_WithEmptyEventType_ThrowsArgumentNullException()
    {
        // Arrange
        var eventId = 123L;
        var dataPointId = 456L;
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new StreamEvent(eventId, dataPointId, timestamp, string.Empty));
        Assert.Equal("eventType", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithWhitespaceEventType_ThrowsArgumentNullException()
    {
        // Arrange
        var eventId = 123L;
        var dataPointId = 456L;
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new StreamEvent(eventId, dataPointId, timestamp, "   "));
        Assert.Equal("eventType", exception.ParamName);
    }

    [Fact]
    public void EventId_GetSet_Roundtrip()
    {
        // Arrange
        var streamEvent = new StreamEvent();

        // Act
        streamEvent.EventId = 999L;

        // Assert
        Assert.Equal(999L, streamEvent.EventId);
    }

    [Fact]
    public void DataPointId_GetSet_Roundtrip()
    {
        // Arrange
        var streamEvent = new StreamEvent();

        // Act
        streamEvent.DataPointId = 777L;

        // Assert
        Assert.Equal(777L, streamEvent.DataPointId);
    }

    [Fact]
    public void Timestamp_GetSet_Roundtrip()
    {
        // Arrange
        var streamEvent = new StreamEvent();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Act
        streamEvent.Timestamp = timestamp;

        // Assert
        Assert.Equal(timestamp, streamEvent.Timestamp);
    }

    [Fact]
    public void EventType_GetSet_Roundtrip()
    {
        // Arrange
        var streamEvent = new StreamEvent();

        // Act
        streamEvent.EventType = "error";

        // Assert
        Assert.Equal("error", streamEvent.EventType);
    }

    [Fact]
    public void Priority_GetSet_Roundtrip()
    {
        // Arrange
        var streamEvent = new StreamEvent();

        // Act
        streamEvent.Priority = 8;

        // Assert
        Assert.Equal(8, streamEvent.Priority);
    }

    [Fact]
    public void SourceSystem_GetSet_Roundtrip()
    {
        // Arrange
        var streamEvent = new StreamEvent();

        // Act
        streamEvent.SourceSystem = "sensor-01";

        // Assert
        Assert.Equal("sensor-01", streamEvent.SourceSystem);
    }

    [Fact]
    public void CorrelationId_GetSet_Roundtrip()
    {
        // Arrange
        var streamEvent = new StreamEvent();

        // Act
        streamEvent.CorrelationId = "corr-123";

        // Assert
        Assert.Equal("corr-123", streamEvent.CorrelationId);
    }

    [Fact]
    public void CausationId_GetSet_Roundtrip()
    {
        // Arrange
        var streamEvent = new StreamEvent();

        // Act
        streamEvent.CausationId = "cause-456";

        // Assert
        Assert.Equal("cause-456", streamEvent.CausationId);
    }

    [Fact]
    public void Payload_GetSet_Roundtrip()
    {
        // Arrange
        var streamEvent = new StreamEvent();
        var payload = new Dictionary<string, object> { { "key1", "value1" }, { "key2", 123 } };

        // Act
        streamEvent.Payload = payload;

        // Assert
        Assert.Equal(2, streamEvent.Payload.Count);
        Assert.Equal("value1", streamEvent.Payload["key1"]);
        Assert.Equal(123, streamEvent.Payload["key2"]);
    }

    [Fact]
    public void ProcessedByStages_GetSet_Roundtrip()
    {
        // Arrange
        var streamEvent = new StreamEvent();
        var stages = new List<string> { "stage1", "stage2" };

        // Act
        streamEvent.ProcessedByStages = stages;

        // Assert
        Assert.Equal(2, streamEvent.ProcessedByStages.Count);
        Assert.Equal("stage1", streamEvent.ProcessedByStages[0]);
        Assert.Equal("stage2", streamEvent.ProcessedByStages[1]);
    }

    [Fact]
    public void MarkProcessedByStage_WithValidStageName_AddsStage()
    {
        // Arrange
        var streamEvent = new StreamEvent(1, 2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), "data");

        // Act
        streamEvent.MarkProcessedByStage("validation");

        // Assert
        Assert.Single(streamEvent.ProcessedByStages);
        Assert.Equal("validation", streamEvent.ProcessedByStages[0]);
        Assert.True(streamEvent.HasBeenProcessedByStage("validation"));
    }

    [Fact]
    public void MarkProcessedByStage_WithDuplicateStageName_DoesNotAddDuplicate()
    {
        // Arrange
        var streamEvent = new StreamEvent(1, 2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), "data");
        streamEvent.MarkProcessedByStage("validation");

        // Act
        streamEvent.MarkProcessedByStage("validation");

        // Assert
        Assert.Single(streamEvent.ProcessedByStages);
        Assert.Equal("validation", streamEvent.ProcessedByStages[0]);
    }

    [Fact]
    public void MarkProcessedByStage_WithNullStageName_ThrowsArgumentException()
    {
        // Arrange
        var streamEvent = new StreamEvent(1, 2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), "data");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => streamEvent.MarkProcessedByStage(null!));
        Assert.Equal("Stage name cannot be null", exception.Message);
        Assert.Equal("stageName", exception.ParamName);
    }

    [Fact]
    public void MarkProcessedByStage_WithEmptyStageName_ThrowsArgumentException()
    {
        // Arrange
        var streamEvent = new StreamEvent(1, 2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), "data");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => streamEvent.MarkProcessedByStage(string.Empty));
        Assert.Equal("Stage name cannot be null", exception.Message);
        Assert.Equal("stageName", exception.ParamName);
    }

    [Fact]
    public void MarkProcessedByStage_WithWhitespaceStageName_ThrowsArgumentException()
    {
        // Arrange
        var streamEvent = new StreamEvent(1, 2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), "data");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => streamEvent.MarkProcessedByStage("   "));
        Assert.Equal("Stage name cannot be null", exception.Message);
        Assert.Equal("stageName", exception.ParamName);
    }

    [Fact]
    public void HasBeenProcessedByStage_WithExistingStage_ReturnsTrue()
    {
        // Arrange
        var streamEvent = new StreamEvent(1, 2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), "data");
        streamEvent.MarkProcessedByStage("parser");
        streamEvent.MarkProcessedByStage("validator");

        // Act
        var result = streamEvent.HasBeenProcessedByStage("validator");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasBeenProcessedByStage_WithNonExistingStage_ReturnsFalse()
    {
        // Arrange
        var streamEvent = new StreamEvent(1, 2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), "data");
        streamEvent.MarkProcessedByStage("parser");

        // Act
        var result = streamEvent.HasBeenProcessedByStage("validator");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasBeenProcessedByStage_WithNullStageName_ReturnsFalse()
    {
        // Arrange
        var streamEvent = new StreamEvent(1, 2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), "data");

        // Act
        var result = streamEvent.HasBeenProcessedByStage(null!);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasBeenProcessedByStage_WithEmptyStageName_ReturnsFalse()
    {
        // Arrange
        var streamEvent = new StreamEvent(1, 2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), "data");

        // Act
        var result = streamEvent.HasBeenProcessedByStage(string.Empty);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetProcessingPath_WithMultipleStages_ReturnsCorrectPath()
    {
        // Arrange
        var streamEvent = new StreamEvent(1, 2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), "data");
        streamEvent.MarkProcessedByStage("parser");
        streamEvent.MarkProcessedByStage("validator");
        streamEvent.MarkProcessedByStage("transformer");

        // Act
        var path = streamEvent.GetProcessingPath();

        // Assert
        Assert.Equal("parser -> validator -> transformer", path);
    }

    [Fact]
    public void GetProcessingPath_WithNoStages_ReturnsEmptyString()
    {
        // Arrange
        var streamEvent = new StreamEvent(1, 2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), "data");

        // Act
        var path = streamEvent.GetProcessingPath();

        // Assert
        Assert.Equal(string.Empty, path);
    }

    [Fact]
    public void CompleteProcessing_SetsCompletedAt()
    {
        // Arrange
        var streamEvent = new StreamEvent(1, 2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), "data");
        var beforeCompletion = DateTime.UtcNow.AddSeconds(-1);

        // Act
        streamEvent.CompleteProcessing();
        var afterCompletion = DateTime.UtcNow.AddSeconds(1);

        // Assert
        Assert.NotNull(streamEvent.CompletedAt);
        Assert.InRange(streamEvent.CompletedAt.Value, beforeCompletion, afterCompletion);
    }

    [Fact]
    public void MarkAsRetry_SetsRetryFlagsAndErrorMessage()
    {
        // Arrange
        var streamEvent = new StreamEvent(1, 2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), "data");
        var errorMessage = "Connection timeout";

        // Act
        streamEvent.MarkAsRetry(errorMessage);

        // Assert
        Assert.True(streamEvent.IsRetry);
        Assert.Equal(1, streamEvent.RetryAttempt);
        Assert.Equal(errorMessage, streamEvent.LastErrorMessage);
    }

    [Fact]
    public void MarkAsRetry_MultipleTimes_IncrementsRetryAttempt()
    {
        // Arrange
        var streamEvent = new StreamEvent(1, 2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), "data");

        // Act
        streamEvent.MarkAsRetry("Error 1");
        streamEvent.MarkAsRetry("Error 2");
        streamEvent.MarkAsRetry("Error 3");

        // Assert
        Assert.True(streamEvent.IsRetry);
        Assert.Equal(3, streamEvent.RetryAttempt);
        Assert.Equal("Error 3", streamEvent.LastErrorMessage);
    }

    [Fact]
    public void MarkAsRetry_WithNullErrorMessage_ThrowsArgumentNullException()
    {
        // Arrange
        var streamEvent = new StreamEvent(1, 2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), "data");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => streamEvent.MarkAsRetry(null!));
    }

    [Fact]
    public void AddPayload_WithValidKeyValue_AddsToPayload()
    {
        // Arrange
        var streamEvent = new StreamEvent(1, 2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), "data");

        // Act
        streamEvent.AddPayload("temperature", 23.5);
        streamEvent.AddPayload("humidity", 45);

        // Assert
        Assert.Equal(2, streamEvent.Payload.Count);
        Assert.Equal(23.5, streamEvent.Payload["temperature"]);
        Assert.Equal(45, streamEvent.Payload["humidity"]);
    }

    [Fact]
    public void AddPayload_WithNullKey_ThrowsArgumentException()
    {
        // Arrange
        var streamEvent = new StreamEvent(1, 2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), "data");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => streamEvent.AddPayload(null!, "value"));
        Assert.Equal("Key cannot be null", exception.Message);
        Assert.Equal("key", exception.ParamName);
    }

    [Fact]
    public void AddPayload_WithEmptyKey_ThrowsArgumentException()
    {
        // Arrange
        var streamEvent = new StreamEvent(1, 2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), "data");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => streamEvent.AddPayload(string.Empty, "value"));
        Assert.Equal("Key cannot be null", exception.Message);
        Assert.Equal("key", exception.ParamName);
    }

    [Fact]
    public void AddPayload_WithWhitespaceKey_ThrowsArgumentException()
    {
        // Arrange
        var streamEvent = new StreamEvent(1, 2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), "data");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => streamEvent.AddPayload("   ", "value"));
        Assert.Equal("Key cannot be null", exception.Message);
        Assert.Equal("key", exception.ParamName);
    }

    [Fact]
    public void AddPayload_WithNullValue_ThrowsArgumentNullException()
    {
        // Arrange
        var streamEvent = new StreamEvent(1, 2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), "data");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => streamEvent.AddPayload("key", null!));
    }

    [Fact]
    public void GetPayload_WithExistingKey_ReturnsValue()
    {
        // Arrange
        var streamEvent = new StreamEvent(1, 2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), "data");
        streamEvent.AddPayload("temperature", 23.5);

        // Act
        var result = streamEvent.GetPayload("temperature");

        // Assert
        Assert.Equal(23.5, result);
    }

    [Fact]
    public void GetPayload_WithNonExistingKey_ReturnsNull()
    {
        // Arrange
        var streamEvent = new StreamEvent(1, 2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), "data");
        streamEvent.AddPayload("temperature", 23.5);

        // Act
        var result = streamEvent.GetPayload("humidity");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetPayload_WithNullKey_ReturnsNull()
    {
        // Arrange
        var streamEvent = new StreamEvent(1, 2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), "data");

        // Act
        var result = streamEvent.GetPayload(null!);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetTotalProcessingTimeMs_WithCompletedEvent_ReturnsDuration()
    {
        // Arrange
        var streamEvent = new StreamEvent(1, 2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), "data");
        var beforeCompletion = DateTime.UtcNow;
        streamEvent.CompleteProcessing();
        var afterCompletion = DateTime.UtcNow.AddMilliseconds(150);

        // Act
        var processingTime = streamEvent.GetTotalProcessingTimeMs();

        // Assert
        Assert.True(processingTime >= 0);
        Assert.InRange(processingTime, 0, 200);
    }

    [Fact]
    public void GetTotalProcessingTimeMs_WithIncompleteEvent_ReturnsNegativeOne()
    {
        // Arrange
        var streamEvent = new StreamEvent(1, 2, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), "data");

        // Act
        var processingTime = streamEvent.GetTotalProcessingTimeMs();

        // Assert
        Assert.Equal(-1, processingTime);
    }

    [Fact]
    public void GetAgeMs_ReturnsPositiveValue()
    {
        // Arrange
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - 1000; // 1 second ago
        var streamEvent = new StreamEvent(1, 2, timestamp, "data");

        // Act
        var age = streamEvent.GetAgeMs();

        // Assert
        Assert.True(age >= 1000);
    }

    [Fact]
    public void GetAgeMs_WithZeroTimestamp_ReturnsLargeValue()
    {
        // Arrange
        var streamEvent = new StreamEvent(1, 2, 0, "data");

        // Act
        var age = streamEvent.GetAgeMs();

        // Assert
        Assert.True(age > 0);
    }

    [Fact]
    public void GetSummary_ReturnsFormattedString()
    {
        // Arrange
        var streamEvent = new StreamEvent(123, 456, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), "data");
        streamEvent.MarkProcessedByStage("stage1");
        streamEvent.MarkProcessedByStage("stage2");
        streamEvent.CompleteProcessing();

        // Act
        var summary = streamEvent.GetSummary();

        // Assert
        Assert.Contains("StreamEvent[Id=123", summary);
        Assert.Contains("Type=data", summary);
        Assert.Contains("Priority=5", summary);
        Assert.Contains("Stages=2", summary);
        Assert.Contains("Completed=True", summary);
    }

    [Fact]
    public void CreateChildEvent_CreatesValidChildWithCorrectProperties()
    {
        // Arrange
        var parent = new StreamEvent(100, 200, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), "parent");
        parent.SourceSystem = "parent-system";
        parent.CorrelationId = "corr-123";
        parent.Priority = 8;
        parent.AddPayload("key1", "value1");
        parent.MarkProcessedByStage("stage1");

        // Act
        var child = parent.CreateChildEvent(300, "child");

        // Assert
        Assert.Equal(300, child.EventId);
        Assert.Equal(200, child.DataPointId);
        Assert.Equal(parent.Timestamp, child.Timestamp);
        Assert.Equal("child", child.EventType);
        Assert.Equal(8, child.Priority);
        Assert.Equal("parent-system", child.SourceSystem);
        Assert.NotNull(child.CorrelationId);
        Assert.Equal(parent.EventId.ToString(), child.CausationId);
        Assert.Equal(1, child.Payload.Count);
        Assert.Equal("value1", child.Payload["key1"]);
        Assert.Single(child.ProcessedByStages);
        Assert.Equal("stage1", child.ProcessedByStages[0]);
        Assert.False(child.CompletedAt.HasValue);
        Assert.False(child.IsRetry);
        Assert.Equal(0, child.RetryAttempt);
    }

    [Fact]
    public void CreateChildEvent_WithNullNewEventType_ThrowsArgumentNullException()
    {
        // Arrange
        var parent = new StreamEvent(100, 200, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), "parent");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => parent.CreateChildEvent(300, null!));
    }

    [Fact]
    public void Validate_WithValidEvent_ReturnsTrue()
    {
        // Arrange
        var streamEvent = new StreamEvent(1, 2, 1234567890, "data");
        streamEvent.Priority = 5;

        // Act
        var isValid = streamEvent.Validate();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void Validate_WithInvalidEventId_ReturnsFalse()
    {
        // Arrange
        var streamEvent = new StreamEvent(0, 2, 1234567890, "data");
        streamEvent.Priority = 5;

        // Act
        var isValid = streamEvent.Validate();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void Validate_WithInvalidDataPointId_ReturnsFalse()
    {
        // Arrange
        var streamEvent = new StreamEvent(1, 0, 1234567890, "data");
        streamEvent.Priority = 5;

        // Act
        var isValid = streamEvent.Validate();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void Validate_WithInvalidTimestamp_ReturnsFalse()
    {
        // Arrange
        var streamEvent = new StreamEvent(1, 2, 0, "data");
        streamEvent.Priority = 5;

        // Act
        var isValid = streamEvent.Validate();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void Validate_WithEmptyEventType_ReturnsFalse()
    {
        // Arrange
        var streamEvent = new StreamEvent(1, 2, 1234567890, string.Empty);
        streamEvent.Priority = 5;

        // Act
        var isValid = streamEvent.Validate();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void Validate_WithInvalidPriority_ReturnsFalse()
    {
        // Arrange
        var streamEvent = new StreamEvent(1, 2, 1234567890, "data");
        streamEvent.Priority = 0;

        // Act
        var isValid = streamEvent.Validate();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void Validate_WithPriorityAboveMax_ReturnsFalse()
    {
        // Arrange
        var streamEvent = new StreamEvent(1, 2, 1234567890, "data");
        streamEvent.Priority = 11;

        // Act
        var isValid = streamEvent.Validate();

        // Assert
        Assert.False(isValid);
    }
}