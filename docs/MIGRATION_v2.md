# Migration Guide: v1.x to v2.0

This document covers the breaking changes and migration steps required to upgrade from v1.x to v2.0 of the real-time data processing pipeline.

## Breaking Changes

### Port Change: 5000 -> 8080

The default application port has changed from `5000` to `8080` to align with container runtime conventions and avoid conflicts with common development ports.

**Before (v1.x):**
```yaml
ports:
  - "5000:5000"
```

**After (v2.0):**
```yaml
ports:
  - "8080:8080"
```

**Action required:**
- Update any reverse proxy configurations (Caddy, Nginx, etc.) pointing to port 5000
- Update health check URLs in orchestration tools (Kubernetes, Docker Swarm)
- Update monitoring/scrape targets in Prometheus configuration
- Update `ASPNETCORE_URLS` if overridden in environment variables

### Docker Compose Resource Syntax

The `resources` block has been moved under `deploy` to comply with Docker Compose v3 specification. If you were using the top-level `resources` key, update your overrides accordingly.

**Before (v1.x):**
```yaml
resources:
  limits:
    cpus: '2'
    memory: 2G
```

**After (v2.0):**
```yaml
deploy:
  resources:
    limits:
      cpus: '2'
      memory: 2G
```

### Dockerfile Multi-stage Build

The Dockerfile now uses named build stages (`build`, `final`) and sets `UseAppHost=false` during publish for smaller image size. If you have custom build scripts referencing stage indices, switch to named references:

```bash
# Copy artifacts from named stage
docker build --target final .
```

## Migration Steps

1. **Update port mappings** in all deployment configurations (docker-compose overrides, Kubernetes manifests, load balancer rules).

2. **Update health check endpoints** from `http://localhost:5000/health` to `http://localhost:8080/health`.

3. **Rebuild Docker images** - the base image tags and build arguments have changed:
   ```bash
   docker compose build --no-cache
   ```

4. **Update Prometheus scrape config** if you target the pipeline container directly:
   ```yaml
   - targets: ['pipeline:8080']
   ```

5. **Test the upgrade** in a staging environment before deploying to production:
   ```bash
   docker compose up -d
   docker compose ps
   curl http://localhost:8080/health
   ```

## Environment Variables

No environment variable names have changed. All existing configuration variables remain compatible. The only new variable is:

| Variable | Default | Description |
|----------|---------|-------------|
| `ASPNETCORE_URLS` | `http://+:8080` | Listening URL (set in Dockerfile) |

## Rollback

If you need to roll back to v1.x:

1. Check out the v1.0.0 tag: `git checkout v1.0.0`
2. Rebuild: `docker compose build --no-cache`
3. Restart: `docker compose up -d`
