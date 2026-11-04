# Health Report Models

This document describes the health report models available in the `DotNetRealtimePipeline.Monitoring` namespace, including `ComponentHealth`, `SystemHealthReport`, `QuickHealthStatus`, `ResourceMonitor`, and `ResourceUsage`.

## Overview

The health report models provide detailed information about the system's health status, including component-level health, overall system health, quick health status, and resource usage metrics.

## Models

### ComponentHealth

Represents the health status of a component.

**Properties:**

- **IsHealthy**: A boolean value indicating whether the component is healthy.
- **Message**: A human-readable status message.
- **CheckedAt**: The UTC timestamp when the health check was performed.
- **Details**: Additional diagnostic details as key-value pairs.

**Methods:**

- **ToString()**: Returns a string representation of the component health.

### SystemHealthReport

Represents a complete health report for the system.

**Properties:**

- **CheckedAt**: The UTC timestamp when the report was generated.
- **OverallStatus**: The overall system health status.
- **Components**: A dictionary of per-component health results keyed by component name.
- **PipelineStatus**: The current pipeline status description.
- **Throughput**: The current pipeline throughput in items per second.
- **SuccessRate**: The pipeline success rate as a percentage.

**Methods:**

- **ToString()**: Returns a string representation of the system health report.

### QuickHealthStatus

Represents a quick health status without detailed information.

**Properties:**

- **IsRunning**: A boolean value indicating whether the pipeline is currently running.
- **HealthStatus**: The overall health status label.
- **PendingItems**: The number of items pending in the pipeline queue.
- **ThroughputOk**: A boolean value indicating whether throughput is above the minimum threshold.
- **ErrorRateAcceptable**: A boolean value indicating whether the error rate is within acceptable limits.

**Methods:**

- **ToString()**: Returns a string representation of the quick health status.

### ResourceMonitor

Monitors resource usage of the system.

**Properties:**

- **Logger**: The logger instance for diagnostic output.

**Methods:**

- **GetResourceUsage()**: Gets current resource usage.
- **IsResourceUsageAcceptable()**: Checks if resource usage is within acceptable limits.

### ResourceUsage

Represents current resource usage metrics.

**Properties:**

- **MemoryMB**: The working-set memory usage in megabytes.
- **ThreadCount**: The current thread count of the process.
- **CpuUsagePercent**: The CPU usage as a percentage.
- **PrivateMemoryMB**: The private memory usage in megabytes.
- **CheckedAt**: The UTC timestamp when the resource snapshot was taken.

**Methods:**

- **ToString()**: Returns a string representation of the resource usage.

## System Health Determination

The overall system health status is determined using the following rules:

1. If all components are healthy, the system is considered **Healthy**.
2. If more than half of the components are unhealthy, the system is considered **Unhealthy**.
3. If any components are unhealthy but not more than half, the system is considered **Degraded**.

This majority rule ensures that the system health status accurately reflects the overall health of the components, with a bias towards reporting degradation rather than full health when there are issues.
