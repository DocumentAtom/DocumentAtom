@ECHO OFF
IF "%1" == "" GOTO :Usage
ECHO.
ECHO Building DocumentAtom Server for linux/amd64 and linux/arm64/v8...
cd src
docker buildx build -f DocumentAtom.Server/Dockerfile --builder cloud-jchristn77-jchristn77 --platform linux/amd64,linux/arm64/v8 --tag jchristn77/documentatom:%1 --tag jchristn77/documentatom:latest --push .
cd ..
GOTO :Done

:Usage
ECHO Provide a tag argument for the build.
ECHO Example: build-server.bat v1.0.0

:Done
ECHO.
ECHO Done
@ECHO ON
