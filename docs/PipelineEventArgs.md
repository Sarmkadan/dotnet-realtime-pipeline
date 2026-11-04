# Pipeline Event Arguments

This document describes the event argument classes used in the `DotNetRealtimePipeline.Events` namespace, including `PipelineEventArgs` and its subclasses, as well as the marker event classes.

## Overview

The event argument classes are used to pass data between event publishers and subscribers. They include a timestamp and a correlation ID for tracking purposes.

## PipelineEventArgs

The base class for all pipeline event arguments.

**Properties:**

- **Timestamp**: The UTC timestamp when the event was created.
- **CorrelationId**: A unique identifier for tracking the event through the pipeline.

**Methods:**

- **ToString()**: Returns a string representation of the event args including timestamp and correlation ID.

## DataIngestedEventArgs

Event args for data ingestion events.

**Properties:**

- **DataPoint**: The ingested data point.

**Methods:**

- **ToString()**: Returns a string representation of the event args including the ingested data point.

## ProcessingCompletedEventArgs

Event args for processing completion events.

**Properties:**

- **Result**: The processing result.

**Methods:**

- **ToString()**: Returns a string representation of the event args including the processing result.

## BackpressureDetectedEventArgs

Event args for backpressure detection events.

**Properties:**

- **StageName**: The name of the stage where backpressure was detected.
- **Context**: The backpressure context.

**Methods:**

- **ToString()**: Returns a string representation of the event args including the stage name and backpressure context.

## MetricsCollectedEventArgs

Event args for metrics collection events.

**Properties:**

- **Metrics**: The collected metrics.

**Methods:**

- **ToString()**: Returns a string representation of the event args including the collected metrics.

## PipelineErrorEventArgs

Event args for pipeline error events.

**Properties:**

- **OperationName**: The name of the operation that failed.
- **Exception**: The exception that was thrown.

**Methods:**

- **ToString()**: Returns a string representation of the event args including the operation name and exception type and message.

## Marker Event Classes

The marker event classes are used to identify the type of event being published.

- **DataIngestedEvent**: Marker for data ingested events.
- **ProcessingCompletedEvent**: Marker for processing completed events.
- **BackpressureDetectedEvent**: Marker for backpressure detected events.
- **MetricsCollectedEvent**: Marker for metrics collected events.
- **PipelineErrorEvent**: Marker for pipeline error events.
