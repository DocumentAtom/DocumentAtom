<#
.SYNOPSIS
    Removes DocumentAtom MCP server integration from supported AI coding tools on Windows.

.DESCRIPTION
    Removes the DocumentAtom MCP server entry from the per-user (global) configuration of
    Claude Code, OpenAI Codex, Gemini CLI, Cursor, and mux. Other entries are preserved.

    This script is a thin wrapper around the DocumentAtom.McpServer '--uninstall' command.
    If a compiled binary cannot be found, the script builds it first (requires the .NET SDK).

.PARAMETER Targets
    Comma-separated list of tools to clean up: claude, codex, gemini, cursor, mux.
    Use 'all' (the default) to clean up every supported tool.

.EXAMPLE
    .\uninstall.ps1

.EXAMPLE
    .\uninstall.ps1 -Targets cursor
#>
[CmdletBinding()]
param(
    [string[]]$Targets = @("all")
)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "_common.ps1")

Invoke-DocumentAtomMcp -Action "uninstall" -Targets ($Targets -join ",")
exit $LASTEXITCODE
