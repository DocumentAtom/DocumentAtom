@ECHO OFF
IF "%1" == "" GOTO :Usage
ECHO.
ECHO Building DocumentAtom Dashboard for linux/amd64 and linux/arm64/v8...
docker buildx build -f dashboard/Dockerfile --builder cloud-jchristn77-jchristn77 --no-cache --platform linux/amd64,linux/arm64/v8 --tag jchristn77/documentatom-ui:%1 --tag jchristn77/documentatom-ui:latest --push .
GOTO :Done

:Usage
ECHO Provide a tag argument for the build.
ECHO Example: build-dashboard.bat v1.0.0

:Done
ECHO.
ECHO Done
@ECHO ON
