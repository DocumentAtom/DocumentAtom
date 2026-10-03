#!/usr/bin/env bash
set -e

if [ -z "$1" ]; then
  echo "Provide a tag argument for the build."
  echo "Example: ./build-dashboard.sh v1.0.0"
  exit 1
fi

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo
echo "Building DocumentAtom Dashboard for linux/amd64 and linux/arm64/v8..."
cd "$SCRIPT_DIR"
docker buildx build -f dashboard/Dockerfile --builder cloud-jchristn77-jchristn77 --platform linux/amd64,linux/arm64/v8 --tag jchristn77/documentatom-ui:"$1" --tag jchristn77/documentatom-ui:latest --push .

echo
echo "Done"
