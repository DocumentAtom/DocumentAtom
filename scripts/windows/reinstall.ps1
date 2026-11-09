<#
.SYNOPSIS
    Reinstalls DocumentAtom MCP server integration on Windows: runs uninstall.ps1, then install.ps1.

.DESCRIPTION
    Refreshes the 'documentatom' entry (for example after changing the endpoint) in the per-user
    configuration of Claude Code, OpenAI Codex, Gemini CLI, Cursor, and mux. A failed uninstall
    (for example, nothing registered yet) is reported as a warning and the install still runs.

.PARAMETER Targets
    Comma-separated list of tools: claude, codex, gemini, cursor, mux.
    Use 'all' (the default) for every supported tool.

.PARAMETER Endpoint
    HTTP JSON-RPC endpoint to register. Defaults to http://localhost:8200/rpc.

.EXAMPLE
    .\reinstall.ps1

.EXAMPLE
    .\reinstall.ps1 -Targets claude,cursor -Endpoint http://localhost:8200/rpc
#>
[CmdletBinding()]
param(
    [string[]]$Targets = @("all"),
    [string]$Endpoint = "http://localhost:8200/rpc"
)

$ErrorActionPreference = "Stop"

Write-Host "==> Uninstalling DocumentAtom MCP integration"
try {
    & (Join-Path $PSScriptRoot "uninstall.ps1") -Targets $Targets
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Uninstall exited with code $LASTEXITCODE; continuing with install."
    }
}
catch {
    Write-Warning "Uninstall failed: $($_.Exception.Message); continuing with install."
}

Write-Host "==> Installing DocumentAtom MCP integration"
& (Join-Path $PSScriptRoot "install.ps1") -Targets $Targets -Endpoint $Endpoint
exit $LASTEXITCODE
