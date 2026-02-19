#!/bin/bash

ENDPOINT="${1:-http://localhost:8000}"
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

dotnet run --project "$SCRIPT_DIR/src/Test.AutomatedHarness" --framework net10.0 -- "$ENDPOINT"
