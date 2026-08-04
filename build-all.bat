@ECHO OFF
IF "%1" == "" GOTO :Usage
ECHO.
ECHO Building all DocumentAtom images...
CALL build-server.bat %*
IF ERRORLEVEL 1 GOTO :Failed
CALL build-mcp.bat %*
IF ERRORLEVEL 1 GOTO :Failed
CALL build-dashboard.bat %*
IF ERRORLEVEL 1 GOTO :Failed
GOTO :Done

:Usage
ECHO Provide a tag argument for the build.
ECHO Example: build-all.bat v1.0.0
EXIT /B 1

:Failed
ECHO.
ECHO Build failed.
EXIT /B 1

:Done
ECHO.
ECHO Done
@ECHO ON
