@echo off
setlocal

set ENDPOINT=%1
if "%ENDPOINT%"=="" set ENDPOINT=http://localhost:8000

pip install -e "%~dp0." >nul 2>&1
python "%~dp0test_harness.py" %ENDPOINT%
