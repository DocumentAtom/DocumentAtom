#!/bin/bash

ENDPOINT="${1:-http://localhost:8000}"
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

cd "$SCRIPT_DIR"
rm -f test-harness.js test-harness.cjs
npx tsc --esModuleInterop --module commonjs --target es2020 --skipLibCheck --outDir . test-harness.ts
if [ -f test-harness.js ]; then
    mv test-harness.js test-harness.cjs
    node test-harness.cjs "$ENDPOINT"
    rm -f test-harness.cjs
fi
