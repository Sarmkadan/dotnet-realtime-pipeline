#!/bin/bash
# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# Build and publish script for dotnet-realtime-pipeline
# =============================================================================

set -e

CONFIG=${1:-Release}
OUTPUT_DIR=${2:-dist}

RED='\033[0;31m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}=== Building dotnet-realtime-pipeline ===${NC}"
echo ""

# Validate config
if [[ "$CONFIG" != "Debug" && "$CONFIG" != "Release" ]]; then
    echo -e "${RED}Invalid configuration: $CONFIG${NC}"
    echo "Usage: ./build.sh [Debug|Release] [output-dir]"
    exit 1
fi

echo "Configuration: $CONFIG"
echo "Output Directory: $OUTPUT_DIR"
echo ""

# Restore
echo -e "${BLUE}Restoring dependencies...${NC}"
dotnet restore
echo -e "${GREEN}✓ Dependencies restored${NC}"
echo ""

# Clean
echo -e "${BLUE}Cleaning previous build...${NC}"
dotnet clean -c $CONFIG -o $OUTPUT_DIR 2>/dev/null || true
echo -e "${GREEN}✓ Clean complete${NC}"
echo ""

# Build
echo -e "${BLUE}Building solution...${NC}"
dotnet build -c $CONFIG

# Count files
if [ "$CONFIG" == "Release" ]; then
    echo -e "${BLUE}Optimizing release build...${NC}"
    dotnet build -c Release --no-incremental
fi

echo -e "${GREEN}✓ Build complete${NC}"
echo ""

# Publish
echo -e "${BLUE}Publishing artifacts...${NC}"
dotnet publish -c $CONFIG -o $OUTPUT_DIR
echo -e "${GREEN}✓ Published to $OUTPUT_DIR${NC}"
echo ""

# Show output
echo -e "${BLUE}Build artifacts:${NC}"
ls -lh $OUTPUT_DIR/dotnet-realtime-pipeline* 2>/dev/null | head -5 || echo "No executables found"

echo ""
echo -e "${GREEN}✓ Build successful!${NC}"
echo ""
echo "Next steps:"
echo "  1. Run: ./$OUTPUT_DIR/dotnet-realtime-pipeline"
echo "  2. Run tests: ./scripts/test.sh"
echo "  3. Create Docker image: docker build -t dotnet-realtime-pipeline:$CONFIG ."
