@echo off
REM Registers the DocumentAtom MCP server entry ("documentatom") in the per-user
REM configuration of Claude Code, Codex, Gemini CLI, Cursor and mux. Runs install.ps1 from this
REM directory with the same arguments. This file is only a launch shim for that script (the
REM .ps1 holds the implementation), so it works from cmd.exe without changing the PowerShell
REM execution policy.
REM Usage: scripts\windows\install.cmd [-Targets all^|^<csv^>] [-Endpoint ^<url^>]
setlocal
where powershell >nul 2>nul
if errorlevel 1 (
    echo Error: powershell was not found on PATH. 1>&2
    endlocal & exit /b 1
)
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0install.ps1" %*
set "RC=%ERRORLEVEL%"
endlocal & exit /b %RC%
