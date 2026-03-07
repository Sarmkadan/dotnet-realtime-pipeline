# Docker Guide for Real-Time Data Processing Pipeline

This guide covers Docker usage for the real-time data processing pipeline, including quick start instructions, environment variables, and production deployment guidelines.

## Quick Start with Docker

### Prerequisites
- Docker 20.10+ installed
- 512MB+ available memory
- Internet access for pulling base images

### Running with Docker

```bash
# Pull the latest image
docker pull your-registry/dotnet-realtime-pipeline:latest

# Run the pipeline
docker run -p 8080:8080 your-registry/dotnet-realtime-pipeline:latest
```

The pipeline will be available at `http://localhost:8080`.

## Docker Compose Usage

Create a `docker-compose.yml` file:

```yaml
version: '3.8'
services:
  pipeline:
    image: your-registry/dotnet-realtime-pipeline:latest
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_URLS=http://+:8080
    restart: unless-stopped
```

Run with docker-compose:
```bash
docker-compose up -d
```

Check the service status:
```bash
docker-compose ps
```

View logs:
```bash
docker-compose logs -f pipeline
```

## Environment Variables

| Variable | Default | Description |
|----------|----------|-------------|
| `ASPNETCORE_URLS` | `http://+:8080` | Application bind address |
| `LOG_LEVEL` | `Information` | Application logging level |
| `METRICS_ENABLED` | `true` | Enable metrics collection |
| `MAX_BUFFER_SIZE` | `10000` | Maximum pipeline buffer size |
| `WINDOW_SIZE_MS` | `5000` | Default window size in milliseconds |

## Production Deployment Checklist

### Resource Requirements
- **CPU**: 2+ cores recommended
- **Memory**: 2GB+ RAM
- **Storage**: 10GB+ available

### Configuration
1. Set resource limits in docker-compose:
```yaml
services:
  pipeline:
    # ... other config
    deploy:
      resources:
        limits:
          cpus: '1'
          memory: 2G
```

### Security
1. Use specific user permissions:
```bash
# Create non-root user
RUN adduser -D -s /bin/bash appuser
USER appuser
```

2. Use read-only root filesystem:
```yaml
services:
  pipeline:
    # ... other config
    read_only: true
    tmpfs:
      - /tmp
```

### Monitoring
1. Enable health checks:
```yaml
services:
  pipeline:
    # ... other config
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3
```

### Data Persistence
Mount volumes for data persistence:
```bash
docker run -p 8080:8080 -v /host/data:/app/data your-registry/dotnet-realtime-pipeline:latest
```

### Scaling
For horizontal scaling, use multiple instances with a load balancer:
```yaml
version: '3.8'
services:
  pipeline1:
    # ... pipeline service 1
  pipeline2:
    # ... pipeline service 2
  loadbalancer:
    image: nginx:alpine
    ports:
      - "8080:8080"
```

## Dockerfile Breakdown

The Dockerfile uses a multi-stage build for optimized size:

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# Copy and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish --no-self-contained

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "dotnet-realtime-pipeline.dll"]
```

## Troubleshooting

### Common Issues

**Issue**: Container fails to start
**Solution**: Check Docker logs: `docker logs <container_id>`

**Issue**: Port conflicts
**Solution**: Change port mapping: `docker run -p 8081:8080 ...`

**Issue**: Insufficient memory
**Solution**: Increase Docker memory allocation or reduce pipeline buffer size

### Performance Tuning

For high-throughput scenarios:
```bash
docker run -m 4g --cpus="2" \
  -e MAX_BUFFER_SIZE=50000 \
  -e WINDOW_SIZE_MS=10000 \
  your-registry/dotnet-realtime-pipeline:latest
```

## Health Checks

The container includes a health check endpoint at `http://localhost:8080/health` which returns:
- 200 OK for healthy state
- 503 for unhealthy state

Example:
```bash
curl -f http://localhost:8080/health || exit 1
```