# Deployment Guide

This guide covers deploying dotnet-realtime-pipeline to production environments.

## Deployment Strategies

### Strategy 1: Docker Container (Recommended)

#### Build Docker Image

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 as build
WORKDIR /app
COPY . .
RUN dotnet build -c Release

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app/bin/Release/net10.0 .
ENTRYPOINT ["dotnet", "dotnet-realtime-pipeline.dll"]
```

#### Run Container

```bash
# Build image
docker build -t dotnet-realtime-pipeline:latest .

# Run with resource limits
docker run \
  --name pipeline \
  --memory=2g \
  --cpus="2" \
  -p 5000:5000 \
  -e MAX_BUFFER_SIZE=50000 \
  -e WINDOW_SIZE_MS=10000 \
  dotnet-realtime-pipeline:latest
```

#### Docker Compose

```yaml
version: '3.8'

services:
  pipeline:
    build: .
    container_name: realtime-pipeline
    ports:
      - "5000:5000"
    environment:
      MAX_BUFFER_SIZE: 50000
      WINDOW_SIZE_MS: 10000
      MAX_CONCURRENT_CONSUMERS: 8
    resources:
      limits:
        memory: 2G
        cpus: '2'
      reservations:
        memory: 1G
        cpus: '1'
    restart: unless-stopped
    networks:
      - pipeline-network

  metrics-export:
    image: prom/prometheus:latest
    ports:
      - "9090:9090"
    volumes:
      - ./prometheus.yml:/etc/prometheus/prometheus.yml
    networks:
      - pipeline-network
    depends_on:
      - pipeline

networks:
  pipeline-network:
    driver: bridge
```

### Strategy 2: Kubernetes Deployment

#### ConfigMap for Configuration

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: pipeline-config
data:
  MAX_BUFFER_SIZE: "50000"
  WINDOW_SIZE_MS: "10000"
  MAX_CONCURRENT_CONSUMERS: "8"
  BACKPRESSURE_THRESHOLD: "0.8"
```

#### Deployment

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: realtime-pipeline
spec:
  replicas: 3
  selector:
    matchLabels:
      app: realtime-pipeline
  template:
    metadata:
      labels:
        app: realtime-pipeline
    spec:
      containers:
      - name: pipeline
        image: dotnet-realtime-pipeline:latest
        imagePullPolicy: Always
        ports:
        - containerPort: 5000
          name: api
        envFrom:
        - configMapRef:
            name: pipeline-config
        resources:
          requests:
            memory: "1Gi"
            cpu: "1000m"
          limits:
            memory: "2Gi"
            cpu: "2000m"
        livenessProbe:
          httpGet:
            path: /health
            port: 5000
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /ready
            port: 5000
          initialDelaySeconds: 5
          periodSeconds: 5
        securityContext:
          allowPrivilegeEscalation: false
          readOnlyRootFilesystem: true
          runAsNonRoot: true
          runAsUser: 1000

---
apiVersion: v1
kind: Service
metadata:
  name: realtime-pipeline
spec:
  selector:
    app: realtime-pipeline
  ports:
  - name: api
    port: 5000
    targetPort: 5000
  type: LoadBalancer
```

#### Horizontal Pod Autoscaling

```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: pipeline-hpa
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: realtime-pipeline
  minReplicas: 3
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
```

### Strategy 3: Systemd Service (Linux)

#### Create Service File

```ini
# /etc/systemd/system/dotnet-realtime-pipeline.service

[Unit]
Description=.NET Real-Time Data Processing Pipeline
After=network.target

[Service]
Type=simple
User=pipeline
WorkingDirectory=/opt/pipeline
EnvironmentFile=/opt/pipeline/.env
ExecStart=/usr/bin/dotnet /opt/pipeline/dotnet-realtime-pipeline.dll
Restart=on-failure
RestartSec=10
StandardOutput=journal
StandardError=journal

[Install]
WantedBy=multi-user.target
```

#### Installation

```bash
# Create user
sudo useradd -m -s /bin/false pipeline

# Copy application
sudo mkdir -p /opt/pipeline
sudo cp -r . /opt/pipeline/
sudo chown -R pipeline:pipeline /opt/pipeline

# Install service
sudo cp dotnet-realtime-pipeline.service /etc/systemd/system/

# Start service
sudo systemctl daemon-reload
sudo systemctl enable dotnet-realtime-pipeline
sudo systemctl start dotnet-realtime-pipeline

# Check status
sudo systemctl status dotnet-realtime-pipeline

# View logs
sudo journalctl -u dotnet-realtime-pipeline -f
```

## Configuration Management

### Environment Variables

```bash
# Core Configuration
export MAX_BUFFER_SIZE=50000
export WINDOW_SIZE_MS=10000
export WINDOW_SLIDE_MS=5000
export MAX_CONCURRENT_CONSUMERS=8

# Backpressure
export BACKPRESSURE_THRESHOLD=0.8
export BACKPRESSURE_STRATEGY=Block

# Processing
export MAX_RETRIES=3
export PROCESSING_TIMEOUT_MS=30000

# Quality Control
export MIN_QUALITY_SCORE=0.5
export ENABLE_QUALITY_ANALYSIS=true

# Monitoring
export ENABLE_METRICS=true
export METRICS_HISTORY_SIZE=1000
```

### Configuration File (appsettings.json)

```json
{
  "Pipeline": {
    "MaxBufferSize": 50000,
    "BufferFlushIntervalMs": 1000,
    "MaxConcurrentConsumers": 8,
    "WindowSizeMs": 10000,
    "WindowSlideMs": 5000,
    "WindowType": "SLIDING",
    "MaxRetries": 3,
    "ProcessingTimeoutMs": 30000,
    "BackpressureThreshold": 0.8,
    "BackpressureStrategy": "Block",
    "MinQualityScore": 0.5,
    "EnableQualityAnalysis": true,
    "EnableMetrics": true,
    "MetricsHistorySize": 1000
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "DotNetRealtimePipeline": "Debug"
    }
  }
}
```

## Performance Tuning

### For High Throughput (100k+ items/sec)

```csharp
services.AddPipelineServices(config =>
{
    config.MaxBufferSize = 500000;           // Larger buffer
    config.MaxConcurrentConsumers = 16;      // More parallelism
    config.BufferFlushIntervalMs = 500;      // Faster flushing
    config.ProcessingTimeoutMs = 5000;       // Shorter timeout
    config.EnableQualityAnalysis = false;    // Disable scoring
});
```

### For Low Latency (< 10ms processing)

```csharp
services.AddPipelineServices(config =>
{
    config.MaxBufferSize = 10000;            // Small buffer
    config.MaxConcurrentConsumers = 4;       // Standard parallelism
    config.BufferFlushIntervalMs = 100;      // Frequent flushing
    config.ProcessingTimeoutMs = 10000;      // Generous timeout
    config.WindowSizeMs = 1000;              // Smaller windows
});
```

### For Resource Constrained (1GB RAM)

```csharp
services.AddPipelineServices(config =>
{
    config.MaxBufferSize = 5000;             // Small buffer
    config.MaxConcurrentConsumers = 2;       // Limited parallelism
    config.MetricsHistorySize = 100;         // Minimal history
    config.EnableQualityAnalysis = false;    // Disable scoring
});
```

## Monitoring & Observability

### Prometheus Integration

```csharp
// In Program.cs
var metricsService = serviceProvider.GetRequiredService<MetricsService>();

// Expose metrics endpoint
app.MapGet("/metrics", async () =>
{
    var health = await metricsService.GenerateHealthReportAsync();
    return new
    {
        throughput = health.ThroughputItemsPerSecond,
        latency_ms = health.AverageLatencyMs,
        error_rate = health.ErrorRate,
        status = health.Status.ToString()
    };
});
```

### Health Check Endpoints

```csharp
app.MapGet("/health", async (PipelineOrchestrator orchestrator) =>
{
    var status = orchestrator.GetStatus();
    return new { status = "healthy", running = status.IsRunning };
});

app.MapGet("/health/detailed", async (MetricsService metricsService) =>
{
    return await metricsService.GenerateHealthReportAsync();
});
```

### Structured Logging

```csharp
services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Information);
});
```

## Security Best Practices

### 1. Container Security

```dockerfile
# Use minimal base image
FROM mcr.microsoft.com/dotnet/runtime:10.0-alpine

# Run as non-root
RUN addgroup -g 1000 dotnet && adduser -D -u 1000 -G dotnet dotnet
USER dotnet

# Mark filesystem as read-only
RUN chmod 644 /app/*
```

### 2. Network Security

```yaml
# Kubernetes NetworkPolicy
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: pipeline-network-policy
spec:
  podSelector:
    matchLabels:
      app: realtime-pipeline
  policyTypes:
  - Ingress
  - Egress
  ingress:
  - from:
    - podSelector:
        matchLabels:
          app: api-gateway
    ports:
    - protocol: TCP
      port: 5000
  egress:
  - to:
    - podSelector:
        matchLabels:
          app: metrics
    ports:
    - protocol: TCP
      port: 9090
```

### 3. Secrets Management

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: pipeline-secrets
type: Opaque
stringData:
  api-key: your-secret-key
  db-connection: your-db-connection-string
---
apiVersion: v1
kind: ConfigMap
metadata:
  name: pipeline-public-config
data:
  PIPELINE_NAME: "production-pipeline"
  LOG_LEVEL: "Information"
```

### 4. RBAC (Role-Based Access Control)

```yaml
apiVersion: rbac.authorization.k8s.io/v1
kind: Role
metadata:
  name: pipeline-role
rules:
- apiGroups: [""]
  resources: ["configmaps"]
  verbs: ["get", "list", "watch"]
- apiGroups: [""]
  resources: ["secrets"]
  verbs: ["get"]
---
apiVersion: rbac.authorization.k8s.io/v1
kind: RoleBinding
metadata:
  name: pipeline-rolebinding
roleRef:
  apiGroup: rbac.authorization.k8s.io
  kind: Role
  name: pipeline-role
subjects:
- kind: ServiceAccount
  name: pipeline-sa
```

## Disaster Recovery

### Backup Strategy

```bash
#!/bin/bash
# Backup metrics and state
BACKUP_DIR="/backup/pipeline"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")

mkdir -p "$BACKUP_DIR"

# Backup application configuration
cp -r /opt/pipeline/config "$BACKUP_DIR/config_$TIMESTAMP"

# Export metrics
curl http://localhost:5000/metrics > "$BACKUP_DIR/metrics_$TIMESTAMP.json"

# Compress
tar -czf "$BACKUP_DIR/backup_$TIMESTAMP.tar.gz" "$BACKUP_DIR"

# Cleanup old backups (older than 30 days)
find "$BACKUP_DIR" -name "backup_*.tar.gz" -mtime +30 -delete
```

### Recovery Procedure

```bash
#!/bin/bash
# Stop current instance
sudo systemctl stop dotnet-realtime-pipeline

# Restore backup
BACKUP_FILE="/backup/pipeline/backup_20260101_000000.tar.gz"
tar -xzf "$BACKUP_FILE" -C /

# Restart service
sudo systemctl start dotnet-realtime-pipeline

# Verify health
curl http://localhost:5000/health
```

## Scaling Considerations

### Horizontal Scaling (Adding Instances)

✅ **Benefits**:
- Increased throughput
- Better fault tolerance
- Load distribution

⚠️ **Challenges**:
- Data consistency
- Distributed state management
- Network overhead

### Vertical Scaling (Larger Machines)

✅ **Benefits**:
- Simplified architecture
- No coordination overhead
- Lower operational complexity

⚠️ **Challenges**:
- Single point of failure
- Cost per unit performance
- Limited ceiling

## Load Testing

```bash
#!/bin/bash
# Using Apache Bench
ab -n 100000 -c 100 http://localhost:5000/api/datapoints

# Using wrk
wrk -t4 -c100 -d30s --script post.lua http://localhost:5000/api/datapoints
```

### Load Testing Script (post.lua)

```lua
wrk.method = "POST"
wrk.body = '{"id":1,"timestamp":' .. os.time() .. ',"value":42.5,"source":"test"}'
wrk.headers["Content-Type"] = "application/json"
```

## Checklist for Production Deployment

- [ ] All configuration parameters reviewed
- [ ] Environment variables properly set
- [ ] Logging level appropriate (not Debug)
- [ ] Health checks configured and tested
- [ ] Metrics export enabled
- [ ] Backups scheduled
- [ ] Monitoring alerts configured
- [ ] Rate limiting enabled
- [ ] CORS properly configured
- [ ] SSL/TLS certificates installed
- [ ] Database connections tested
- [ ] External API endpoints tested
- [ ] Load testing completed
- [ ] Disaster recovery plan documented
- [ ] Runbooks written for common issues
- [ ] On-call rotation established
- [ ] Automated rollback plan ready
