@echo off
REM Convenience wrapper to run uninstall.ps1 from cmd.exe.
REM Pass-through examples:
REM   uninstall.cmd
REM   uninstall.cmd -Targets cursor
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0uninstall.ps1" %*
