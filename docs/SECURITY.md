# Security Guidelines - dotnet-realtime-pipeline

## Overview

This document outlines security best practices for deploying and using the dotnet-realtime-pipeline in production environments.

## Security Considerations

### Input Validation

**Data Point Validation**

All ingested data points should be validated:

```csharp
// Enable quality analysis for validation
services.AddPipelineServices(config =>
{
    config.EnableQualityAnalysis = true;
    config.MinDataQualityThreshold = 0.8m;
});

// Custom validation example
var dataPoint = new DataPoint(id, timestamp, value, source);

// Check for realistic value ranges
if (dataPoint.Value < -1_000_000_000 || dataPoint.Value > 1_000_000_000)
{
    // Reject outlier
    continue;
}

// Check timestamp sanity
var age = DateTime.UtcNow.Ticks - dataPoint.Timestamp;
if (age > TimeSpan.FromHours(1).Ticks)
{
    // Reject stale data
    continue;
}
```

### Rate Limiting

**Protect against abuse**

```csharp
// Use rate limiting middleware
services.AddMiddleware<RateLimitingMiddleware>(config =>
{
    config.RequestsPerSecond = 10000;
    config.BurstSize = 50000;
});

// Per-source rate limiting
var rateLimiter = new Dictionary<string, RateLimitContext>();

foreach (var dataPoint in incomingData)
{
    if (!rateLimiter.ContainsKey(dataPoint.Source))
    {
        rateLimiter[dataPoint.Source] = new RateLimitContext();
    }

    if (!rateLimiter[dataPoint.Source].IsAllowed())
    {
        // Drop or throttle
        continue;
    }
}
```

### Authentication & Authorization

**REST API Security**

```csharp
// Add authentication middleware
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://authority.example.com";
        options.Audience = "dotnet-realtime-pipeline";
    });

// Protect endpoints
[Authorize]
public async Task<IActionResult> IngestDataAsync(DataPoint point)
{
    // Only authenticated users can ingest
    return Ok(await orchestrator.IngestDataPointAsync(point));
}
```

### Data Encryption

**In Transit**

```csharp
// Always use HTTPS
var app = builder.Build();

app.UseHsts();  // HSTS enabled
app.UseHttpsRedirection();

// Configure TLS 1.2+ minimum
var serverOptions = new ServerOptions
{
    Protocols = HttpProtocols.Http2,
};
```

**At Rest**

```csharp
// Encrypt sensitive data in repositories
public class EncryptedDataPointRepository : IDataPointRepository
{
    private readonly IDataEncryption _encryption;

    public async Task AddAsync(DataPoint point)
    {
        // Encrypt sensitive fields before storage
        var encrypted = _encryption.Encrypt(point.Metadata);
        
        var stored = new StoredDataPoint
        {
            Id = point.Id,
            EncryptedMetadata = encrypted
        };
        
        await _repository.StoreAsync(stored);
    }
}
```

### Audit Logging

**Track all operations**

```csharp
public class AuditLoggingMiddleware
{
    private readonly ILogger<AuditLoggingMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;
        
        _logger.LogInformation(
            "Audit: {Method} {Path} by {User} at {Time}",
            context.Request.Method,
            context.Request.Path,
            context.User.Identity?.Name ?? "Anonymous",
            startTime
        );

        await _next(context);

        var duration = DateTime.UtcNow - startTime;
        _logger.LogInformation(
            "Audit: {Method} {Path} completed in {Duration}ms with status {StatusCode}",
            context.Request.Method,
            context.Request.Path,
            duration.TotalMilliseconds,
            context.Response.StatusCode
        );
    }
}
```

## Deployment Security

### Docker Security

**Secure Docker image**

```dockerfile
# Use non-root user
RUN useradd -m -u 1000 appuser

# Don't run as root
USER appuser

# Use minimal base image
FROM mcr.microsoft.com/dotnet/runtime:10.0-alpine

# Drop unnecessary capabilities
RUN setcap -r /app/dotnet-realtime-pipeline || true
```

**docker-compose.yml security**

```yaml
version: '3.8'
services:
  pipeline:
    image: dotnet-realtime-pipeline:latest
    user: 1000:1000  # Run as non-root
    read_only: true  # Read-only filesystem
    cap_drop:
      - ALL  # Drop all capabilities
    security_opt:
      - no-new-privileges:true
```

### Environment Variables

**Protect secrets**

```csharp
// Load secrets from secure sources
var config = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>()  // Dev secrets
    .AddKeyVault()              // Production secrets
    .Build();

// Never log sensitive values
var connectionString = config["ConnectionString"];
logger.LogInformation("Connected to database");  // Don't log the connection string!
```

### Network Security

**Restrict access**

```yaml
# docker-compose.yml - Restrict to localhost
services:
  pipeline:
    ports:
      - "127.0.0.1:5000:5000"  # Only local access
```

```bash
# Kubernetes - Network policies
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: pipeline-isolation
spec:
  podSelector:
    matchLabels:
      app: dotnet-realtime-pipeline
  policyTypes:
  - Ingress
  - Egress
```

## Dependency Security

### Vulnerable Dependencies

**Check for known vulnerabilities**

```bash
# NuGet Package Audit
dotnet package add Microsoft.NuGet.SecurityAudit

# Run audit
dotnet audit
```

### Dependency Updates

```bash
# Check for updates
dotnet outdated

# Update dependencies
dotnet package upgrade --interactive
```

### Third-Party Libraries

- Only use trusted, well-maintained packages
- Monitor security advisories
- Pin versions in production
- Review package sources

## Code Security

### SQL Injection Prevention

```csharp
// Bad - vulnerable to SQL injection
var query = $"SELECT * FROM data WHERE source = '{source}'";

// Good - parameterized query
var query = "SELECT * FROM data WHERE source = @source";
cmd.Parameters.AddWithValue("@source", source);
```

### Input Sanitization

```csharp
// Sanitize JSON input
public async Task<IActionResult> IngestAsync([FromBody] DataPoint point)
{
    // Validate input
    if (string.IsNullOrWhiteSpace(point.Source))
        return BadRequest("Source is required");

    if (point.Value > decimal.MaxValue || point.Value < decimal.MinValue)
        return BadRequest("Value out of range");

    // Process validated input
    return Ok(await orchestrator.IngestDataPointAsync(point));
}
```

### Serialization Security

```csharp
// Secure JSON deserialization
var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    MaxDepth = 10,  // Prevent billion laughs attack
};

var point = JsonSerializer.Deserialize<DataPoint>(json, options);
```

## Monitoring & Alerting

### Security Monitoring

```csharp
// Monitor for suspicious activity
public class SecurityMonitor
{
    private readonly Dictionary<string, int> _failedAttempts = new();

    public bool CheckRateLimit(string clientId, int maxFailures = 10)
    {
        if (!_failedAttempts.ContainsKey(clientId))
            _failedAttempts[clientId] = 0;

        if (_failedAttempts[clientId] > maxFailures)
        {
            _logger.LogWarning("Suspicious activity from {ClientId}", clientId);
            return false;
        }

        return true;
    }
}
```

### Alerting Thresholds

- Unusual throughput spikes
- High error rates
- Failed authentication attempts
- Rate limiting triggers
- Memory/CPU anomalies

## Compliance

### Data Privacy (GDPR/CCPA)

```csharp
// Right to be forgotten
public async Task DeleteUserDataAsync(string userId)
{
    var points = await _repository.GetByUserAsync(userId);
    foreach (var point in points)
    {
        await _repository.DeleteAsync(point.Id);
    }
    
    _logger.LogInformation("Deleted data for user {UserId}", userId);
}

// Data portability
public async Task<byte[]> ExportUserDataAsync(string userId)
{
    var points = await _repository.GetByUserAsync(userId);
    return JsonSerializer.SerializeToUtf8Bytes(points);
}
```

### Audit Trail

```csharp
// Maintain audit log
public class AuditLog
{
    public long Id { get; set; }
    public string Action { get; set; }
    public string UserId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Details { get; set; }
}

await _auditRepository.LogAsync(new AuditLog
{
    Action = "DataIngestion",
    UserId = context.User.Identity?.Name,
    Timestamp = DateTime.UtcNow,
    Details = $"Ingested {count} data points from {source}"
});
```

## Security Checklist

### Development
- [ ] Input validation on all endpoints
- [ ] No hardcoded secrets or credentials
- [ ] No sensitive data in logs
- [ ] Use HTTPS for all external communication
- [ ] Enable authentication/authorization
- [ ] Regular dependency updates
- [ ] Code review before merge

### Deployment
- [ ] Use secrets management (KeyVault, Secrets Manager)
- [ ] Run as non-root user
- [ ] Restrict network access
- [ ] Enable audit logging
- [ ] Monitor for security events
- [ ] Regular security scanning
- [ ] Least privilege access

### Operations
- [ ] Keep dependencies updated
- [ ] Monitor logs for suspicious activity
- [ ] Regular penetration testing
- [ ] Incident response plan
- [ ] Backup strategy
- [ ] Disaster recovery plan
- [ ] Security training for team

## Incident Response

### Breach Detection

1. Monitor logs for unusual patterns
2. Check rate limiting triggers
3. Review authentication failures
4. Analyze throughput anomalies

### Response Steps

1. Isolate affected systems
2. Review audit logs
3. Notify affected users if needed
4. Patch vulnerabilities
5. Update security policies
6. Post-incident review

## Resources

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [.NET Security Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/security/)
- [CWE Top 25](https://cwe.mitre.org/top25/)
- [Docker Security Best Practices](https://docs.docker.com/engine/security/)

---

**Built by [Vladyslav Zaiets](https://sarmkadan.com) - CTO & Software Architect**

[Portfolio](https://sarmkadan.com) | [GitHub](https://github.com/Sarmkadan) | [Telegram](https://t.me/sarmkadan)
