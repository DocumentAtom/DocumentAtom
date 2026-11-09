@echo off
REM Convenience wrapper to run reinstall.ps1 (uninstall, then install) from cmd.exe.
REM Pass-through examples:
REM   reinstall.cmd
REM   reinstall.cmd -Targets claude,cursor -Endpoint http://localhost:8200/rpc
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0reinstall.ps1" %*
