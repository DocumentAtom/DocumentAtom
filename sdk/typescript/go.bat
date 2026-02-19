@echo off
setlocal

set ENDPOINT=%1
if "%ENDPOINT%"=="" set ENDPOINT=http://localhost:8000

cd /d "%~dp0"
if exist test-harness.js del test-harness.js
if exist test-harness.cjs del test-harness.cjs
call npx tsc --esModuleInterop --module commonjs --target es2020 --skipLibCheck --outDir . test-harness.ts
if exist test-harness.js (
    ren test-harness.js test-harness.cjs
    node test-harness.cjs %ENDPOINT%
    del test-harness.cjs 2>nul
)
