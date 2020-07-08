#!/bin/bash
# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# Deployment script for dotnet-realtime-pipeline
# =============================================================================

set -e

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
ENVIRONMENT=${1:-production}
VERSION=${2:-latest}
REGISTRY=${REGISTRY:-docker.io}
IMAGE_NAME=${IMAGE_NAME:-dotnet-realtime-pipeline}
DOCKER_TAG="$REGISTRY/$IMAGE_NAME:$VERSION"

echo -e "${BLUE}=== Deployment Script ===${NC}"
echo "Environment: $ENVIRONMENT"
echo "Version: $VERSION"
echo "Docker Tag: $DOCKER_TAG"
echo ""

# Validate environment
if [[ "$ENVIRONMENT" != "development" && "$ENVIRONMENT" != "staging" && "$ENVIRONMENT" != "production" ]]; then
    echo -e "${RED}Invalid environment: $ENVIRONMENT${NC}"
    echo "Usage: ./deploy.sh [development|staging|production] [version]"
    exit 1
fi

# Build Docker image
echo -e "${BLUE}Building Docker image...${NC}"
docker build \
    --build-arg BUILD_DATE=$(date -u +'%Y-%m-%dT%H:%M:%SZ') \
    --build-arg VCS_REF=$(git rev-parse --short HEAD) \
    --build-arg VERSION="$VERSION" \
    -t "$DOCKER_TAG" \
    -t "$REGISTRY/$IMAGE_NAME:latest" \
    .

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ Docker image built successfully${NC}"
else
    echo -e "${RED}✗ Docker build failed${NC}"
    exit 1
fi

# Push to registry
echo -e "${BLUE}Pushing to registry...${NC}"
docker push "$DOCKER_TAG"
docker push "$REGISTRY/$IMAGE_NAME:latest"

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ Image pushed successfully${NC}"
else
    echo -e "${RED}✗ Push failed${NC}"
    exit 1
fi

# Deploy based on environment
case $ENVIRONMENT in
    development)
        echo -e "${BLUE}Deploying to development environment...${NC}"
        docker-compose -f docker-compose.dev.yml up -d
        echo -e "${GREEN}✓ Development deployment complete${NC}"
        echo "Access at: http://localhost:5000"
        ;;
    staging)
        echo -e "${BLUE}Deploying to staging environment...${NC}"
        docker-compose -f docker-compose.staging.yml up -d
        echo -e "${GREEN}✓ Staging deployment complete${NC}"
        echo "Access at: https://staging.example.com"
        ;;
    production)
        echo -e "${YELLOW}WARNING: Deploying to PRODUCTION environment${NC}"
        read -p "Type 'yes' to continue: " confirm

        if [ "$confirm" != "yes" ]; then
            echo "Deployment cancelled"
            exit 1
        fi

        echo -e "${BLUE}Deploying to production environment...${NC}"

        # Backup current state
        docker-compose -f docker-compose.prod.yml exec app tar czf /backups/backup-$(date +%s).tar.gz /app/data

        # Deploy new version
        docker-compose -f docker-compose.prod.yml pull
        docker-compose -f docker-compose.prod.yml up -d --no-deps app

        # Health check
        sleep 5
        if curl -f http://localhost:5000/health > /dev/null 2>&1; then
            echo -e "${GREEN}✓ Production deployment complete and healthy${NC}"
        else
            echo -e "${RED}✗ Health check failed, rolling back${NC}"
            docker-compose -f docker-compose.prod.yml down
            exit 1
        fi
        ;;
esac

# Post-deployment verification
echo -e "${BLUE}Running post-deployment checks...${NC}"
sleep 2

if docker ps | grep -q "$IMAGE_NAME"; then
    echo -e "${GREEN}✓ Container is running${NC}"
else
    echo -e "${RED}✗ Container is not running${NC}"
    exit 1
fi

# Log tail
echo -e "${BLUE}Recent logs:${NC}"
docker-compose logs --tail=10 app

echo ""
echo -e "${GREEN}✓ Deployment completed successfully!${NC}"
