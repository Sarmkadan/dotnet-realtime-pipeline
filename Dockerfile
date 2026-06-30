# Multi-stage build for optimized image size and security
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build

WORKDIR /src

# Copy project file and restore dependencies (layer caching)
COPY dotnet-realtime-pipeline.csproj .
RUN dotnet restore "dotnet-realtime-pipeline.csproj"

# Copy source code
COPY . .

# Build and publish
RUN dotnet publish "dotnet-realtime-pipeline.csproj" \
    -c Release \
    -o /app/publish \
    --self-contained=false \
    /p:UseAppHost=false

# Runtime stage - minimal image
FROM mcr.microsoft.com/dotnet/runtime:10.0-alpine AS final

# Create non-root user for security
RUN addgroup -g 1000 dotnet && adduser -D -u 1000 -G dotnet dotnet

WORKDIR /app

# Copy published application
COPY --from=build --chown=dotnet:dotnet /app/publish .

# Switch to non-root user
USER dotnet

# Expose port
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

# Health check
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "dotnet-realtime-pipeline.dll"]

# Labels for metadata
LABEL maintainer="Vladyslav Zaiets <rutova2@gmail.com>"
LABEL description="Real-time data processing pipeline for .NET"
LABEL version="1.0.0"
