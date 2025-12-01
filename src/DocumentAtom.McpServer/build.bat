@ECHO OFF
ECHO Building DocumentAtom.McpServer...
dotnet build DocumentAtom.McpServer.csproj --configuration Release
ECHO Build complete.

ECHO.
ECHO Publishing DocumentAtom.McpServer...
dotnet publish DocumentAtom.McpServer.csproj --configuration Release --output publish
ECHO Publish complete.
@ECHO ON
