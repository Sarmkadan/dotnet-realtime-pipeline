# Multi-stage build for optimized image size and security
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build

WORKDIR /src

# Copy project file
COPY dotnet-realtime-pipeline.csproj .

# Restore dependencies
RUN dotnet restore "dotnet-realtime-pipeline.csproj"

# Copy source code
COPY . .

# Build and publish
RUN dotnet publish "dotnet-realtime-pipeline.csproj" -c Release -o /app/publish --self-contained=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:10.0-alpine

# Create non-root user for security
RUN addgroup -g 1000 dotnet && adduser -D -u 1000 -G dotnet dotnet

WORKDIR /app

# Copy published application
COPY --from=build --chown=dotnet:dotnet /app/publish .

# Switch to non-root user
USER dotnet

# Expose port
EXPOSE 5000

# Health check
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:5000/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "dotnet-realtime-pipeline.dll"]

# Labels for metadata
LABEL maintainer="Vladyslav Zaiets <vladyslav.zaiets@amdaris.com>"
LABEL description="Real-time data processing pipeline for .NET"
LABEL version="1.2.0"
