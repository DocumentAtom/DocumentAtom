#!/bin/bash

ENDPOINT="${1:-http://localhost:8000}"
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

pip install -e "$SCRIPT_DIR" >/dev/null 2>&1
python "$SCRIPT_DIR/test_harness.py" "$ENDPOINT"
