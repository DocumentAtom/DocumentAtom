@echo off

del /s /f /q *.log

if exist bin rmdir /s /q bin
if exist obj rmdir /s /q obj

@echo on
