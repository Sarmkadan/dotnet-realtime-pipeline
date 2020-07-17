Migration Guide: v1.x to v2.0
This document covers the breaking changes and migration steps required to upgrade from v1.x to v2.0 of the real-time data processing pipeline.

## Breaking Changes

### Port Change: 5000 -> 8080
The default application port has changed from `5000` to `8080` to align with container runtime conventions and avoid conflicts with common development ports.

## Migration Steps

1. Update port mappings in all deployment configurations (docker-compose overrides, Kubernetes manifests, load balancer rules).
2. Update health check endpoints from `http://localhost:5000/health` to `http://localhost:8080/health`.
3. Rebuild Docker images - the base image tags and build arguments have changed.
4. Update Prometheus scrape config if you target the pipeline container directly.
5. Test the upgrade in a staging environment before deploying to production.

## Environment Variables
No environment variable names have changed. All existing configuration variables remain compatible.

## Rollback
If you need to roll back to v1.x:
1. Check out the v1.0.0 tag: `git checkout v1.0.0`
2. Rebuild: `docker compose build --no-cache`
3. Restart: `docker compose up -d`