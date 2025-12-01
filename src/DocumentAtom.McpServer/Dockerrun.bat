@echo off

IF "%1" == "" GOTO :Usage

if not exist documentatom.json (
  echo Configuration file documentatom.json not found.
  exit /b 1
)

REM Items that require persistence
REM   documentatom.json
REM   logs/
REM   temp/
REM   backups/

REM Argument order matters!

docker run ^
  -p 8000:8000 ^
  -p 8001:8001 ^
  -p 8002:8002 ^
  -t ^
  -i ^
  -e "TERM=xterm-256color" ^
  -v .\documentatom.json:/app/documentatom.json ^
  -v .\logs\:/app/logs/ ^
  -v .\temp\:/app/temp/ ^
  -v .\backups\:/app/backups/ ^
  jchristn/documentatom-mcp:%1

GOTO :Done

:Usage
ECHO Provide one argument indicating the tag.
ECHO Example: dockerrun.bat v1.0.0
:Done
@echo on
