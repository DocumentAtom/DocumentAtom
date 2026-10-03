#!/usr/bin/env bash
set -e

if [ -z "$1" ]; then
  echo "Provide a tag argument for the build."
  echo "Example: ./build-server.sh v1.0.0"
  exit 1
fi

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo
echo "Building DocumentAtom Server for linux/amd64 and linux/arm64/v8..."
cd "$SCRIPT_DIR/src"
docker buildx build -f DocumentAtom.Server/Dockerfile --builder cloud-jchristn77-jchristn77 --platform linux/amd64,linux/arm64/v8 --tag jchristn77/documentatom:"$1" --tag jchristn77/documentatom:latest --push .

echo
echo "Done"
