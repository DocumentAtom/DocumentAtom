@echo off
setlocal

set ENDPOINT=%1
if "%ENDPOINT%"=="" set ENDPOINT=http://localhost:8000

dotnet run --project "%~dp0src\Test.AutomatedHarness" --framework net10.0 -- %ENDPOINT%
