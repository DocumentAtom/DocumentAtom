@echo off
REM Convenience wrapper to run install.ps1 from cmd.exe.
REM Pass-through examples:
REM   install.cmd
REM   install.cmd -Targets claude,cursor
REM   install.cmd -Targets mux -Endpoint http://localhost:8200/rpc
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0install.ps1" %*
