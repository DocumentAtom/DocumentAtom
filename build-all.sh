#!/usr/bin/env bash
set -e

if [ -z "$1" ]; then
  echo "Provide a tag argument for the build."
  echo "Example: ./build-all.sh v1.0.0"
  exit 1
fi

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo
echo "Building all DocumentAtom images..."
"$SCRIPT_DIR/build-server.sh" "$@"
"$SCRIPT_DIR/build-mcp.sh" "$@"
"$SCRIPT_DIR/build-dashboard.sh" "$@"

echo
echo "Done"
