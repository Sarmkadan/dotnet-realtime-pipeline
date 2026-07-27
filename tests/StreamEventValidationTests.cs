using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Xunit;
using Moq;
using DotNetRealtimePipeline.Domain.Models;
using DotNetRealtimePipeline.Domain.Services;

namespace DotNetRealtimePipeline.Tests
{
    public class StreamEventValidationTests
    {
        [Fact]
        public void ValidateEvent_HappyPath_ValidEvent()
        {
            // Arrange
            var @event = new StreamEvent
            {
                EventId = 1,
                DataPointId = 2,
                Timestamp = 3,
                EventType = "Type1",
                Priority = 5,
                SourceSystem = "Source1",
                CorrelationId = "Correlation1",
                CausationId = "Causation1",
                Payload = new Dictionary<string, object>
                {
                    {"Key1", "Value1"},
                    {"Key2", "Value2"}
                },
                ProcessedByStages = new List<string> {"Stage1", "Stage2"},
                CreatedAt = new DateTime(2022, 1, 1),
                CompletedAt = new DateTime(2022, 1, 2),
                IsRetry = true,
                RetryAttempt = 2,
                LastErrorMessage = "Error1"
            };

            // Act
            var result = StreamEventValidation.IsValid(@event);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ValidateEvent_HappyPath_InvalidEvent_EventIdZero()
        {
            // Arrange
            var @event = new StreamEvent
            {
                EventId = 0,
                DataPointId = 2,
                Timestamp = 3,
                EventType = "Type1",
                Priority = 5,
                SourceSystem = "Source1",
                CorrelationId = "Correlation1",
                CausationId = "Causation1",
                Payload = new Dictionary<string, object>
                {
                    {"Key1", "Value1"},
                    {"Key2", "Value2"}
                },
                ProcessedByStages = new List<string> {"Stage1", "Stage2"},
                CreatedAt = new DateTime(2022, 1, 1),
                CompletedAt = new DateTime(2022, 1, 2),
                IsRetry = true,
                RetryAttempt = 2,
                LastErrorMessage = "Error1"
            };

            // Act
            var result = StreamEventValidation.IsValid(@event);

            // Assert
            Assert.False(result);
        }
    }
}