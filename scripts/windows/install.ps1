<#
.SYNOPSIS
    Installs DocumentAtom MCP server integration into supported AI coding tools on Windows.

.DESCRIPTION
    Registers the DocumentAtom MCP server (HTTP JSON-RPC transport) with the per-user
    (global) configuration of Claude Code, OpenAI Codex, Gemini CLI, Cursor, and mux.

    This script is a thin wrapper around the DocumentAtom.McpServer '--install' command,
    which performs the actual configuration edits. If a compiled binary cannot be found,
    the script builds it first (requires the .NET SDK).

.PARAMETER Targets
    Comma-separated list of tools to configure: claude, codex, gemini, cursor, mux.
    Use 'all' (the default) to configure every supported tool.

.PARAMETER Endpoint
    HTTP JSON-RPC endpoint to register. Defaults to http://localhost:8200/rpc.

.EXAMPLE
    .\install.ps1

.EXAMPLE
    .\install.ps1 -Targets claude,cursor

.EXAMPLE
    .\install.ps1 -Targets mux -Endpoint http://localhost:8200/rpc
#>
[CmdletBinding()]
param(
    [string[]]$Targets = @("all"),
    [string]$Endpoint = "http://localhost:8200/rpc"
)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "_common.ps1")

Invoke-DocumentAtomMcp -Action "install" -Targets ($Targets -join ",") -Endpoint $Endpoint
exit $LASTEXITCODE
