<#
    Shared helpers for the DocumentAtom MCP install/uninstall scripts on Windows.
    Dot-sourced by install.ps1 and uninstall.ps1.
#>

function Get-RepoRoot {
    return (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}

function Test-DotNetSdk {
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { return $false }
    try {
        $sdks = & dotnet --list-sdks 2>$null
        return ($null -ne $sdks -and @($sdks).Count -gt 0)
    }
    catch {
        return $false
    }
}

function Find-McpServerDll {
    param([string]$RepoRoot)

    $binDir = Join-Path $RepoRoot "src\DocumentAtom.McpServer\bin"
    if (-not (Test-Path $binDir)) { return $null }

    $dlls = Get-ChildItem -Path $binDir -Recurse -Filter "DocumentAtom.McpServer.dll" -ErrorAction SilentlyContinue
    if (-not $dlls -or $dlls.Count -eq 0) { return $null }

    # Prefer Release over Debug; prefer net8.0 (LTS, broadest runtime availability) over net10.0;
    # then most recently written.
    $ranked = $dlls | Sort-Object `
        @{ Expression = { if ($_.FullName -match '\\Release\\') { 0 } else { 1 } } }, `
        @{ Expression = { if ($_.FullName -match 'net8') { 0 } elseif ($_.FullName -match 'net10') { 1 } else { 2 } } }, `
        @{ Expression = { $_.LastWriteTime }; Descending = $true }

    return $ranked[0].FullName
}

function Resolve-McpServerDll {
    param([string]$RepoRoot)

    $csproj = Join-Path $RepoRoot "src\DocumentAtom.McpServer\DocumentAtom.McpServer.csproj"

    # When the SDK is available, build (incrementally) so we never run a stale binary.
    if (Test-DotNetSdk) {
        Write-Host "Building DocumentAtom.McpServer (Release)..."
        & dotnet build $csproj -c Release --nologo | Out-Host
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Build failed; attempting to use an existing binary if one is present."
        }
    }

    $dll = Find-McpServerDll -RepoRoot $RepoRoot
    if (-not $dll) {
        throw "Could not locate a compiled DocumentAtom.McpServer.dll. Install the .NET SDK and build the project, then retry."
    }
    return $dll
}

function Invoke-DocumentAtomMcp {
    param(
        [Parameter(Mandatory = $true)][ValidateSet("install", "uninstall")][string]$Action,
        [string]$Targets = "all",
        [string]$Endpoint = ""
    )

    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        throw "The .NET runtime ('dotnet') was not found on PATH. Install the .NET 8 (or later) runtime to run the MCP server tooling."
    }

    $repoRoot = Get-RepoRoot
    $dll = Resolve-McpServerDll -RepoRoot $repoRoot

    $arguments = @($dll, "--$Action=$Targets")
    if ($Action -eq "install" -and $Endpoint) {
        $arguments += "--endpoint=$Endpoint"
    }

    & dotnet @arguments
}
