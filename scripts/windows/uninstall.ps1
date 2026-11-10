<#
.SYNOPSIS
    Removes the "documentatom" MCP server entry from the per-user configuration of Claude Code, OpenAI Codex, Gemini CLI, Cursor and mux.

.DESCRIPTION
    Other entries are kept. Succeeds when nothing is registered.

    This builds src\DocumentAtom.McpServer (Release, net10.0 with a .NET 10 SDK, otherwise net8.0)
    when the .NET SDK is installed, then runs its --uninstall command, which edits the config files.
    Without an SDK it uses an existing build (from an earlier run) that the installed runtime can
    run. Nothing is installed as a dotnet tool. Only the Windows configs (under %USERPROFILE%) are changed; for tools
    that run inside WSL, run scripts/wsl2/uninstall.sh (or scripts/wsl/uninstall.sh) inside the distro.

    From cmd.exe, run scripts\windows\uninstall.cmd with the same arguments.

.PARAMETER Targets
    claude, codex, gemini, cursor, mux or all (the default). Comma-separated.

.PARAMETER Help
    Prints usage and exits. --help and /? work too.

.EXAMPLE
    scripts\windows\uninstall.cmd -Targets claude,cursor
#>
[CmdletBinding(PositionalBinding = $false)]
param(
    [string[]]$Targets = @("all"),
    [Alias("h")][switch]$Help,
    [Parameter(ValueFromRemainingArguments = $true)][string[]]$Rest = @()
)

$ErrorActionPreference = "Stop"
# Windows only. Windows PowerShell 5.1 has no $IsWindows and runs only on Windows.
# SCRIPTS_SKIP_OS_CHECK=1 skips this check (for scripts/tests/test-scripts.py).
if ($PSVersionTable.PSEdition -eq "Core" -and -not $IsWindows -and $env:SCRIPTS_SKIP_OS_CHECK -ne "1") {
    [Console]::Error.WriteLine("Error: This script is for Windows. Use scripts/macos on macOS, scripts/linux on Linux, or scripts/wsl2 (scripts/wsl) under WSL.")
    exit 1
}
$RepoRoot = [IO.Path]::GetFullPath([IO.Path]::Combine($PSScriptRoot, "..", ".."))
function Fail([string]$Message) {
    [Console]::Error.WriteLine("Error: $Message")
    exit 1
}

$Usage = @"
Usage: scripts\windows\uninstall.cmd [-Targets all|<csv>]
       (or powershell -File scripts\windows\uninstall.ps1 with the same arguments)
  -Targets   claude, codex, gemini, cursor, mux or all (default: all)
"@

if ($Help -or ($Rest -contains "--help") -or ($Rest -contains "/?")) {
    Write-Host $Usage
    exit 0
}
if ($Rest.Count -gt 0) {
    Fail "Unexpected argument: $($Rest[0]) (run scripts\windows\uninstall.cmd -Help for usage)"
}

# Validate the targets before anything is built or changed (the server would skip unknown names).
$TargetList = @((($Targets -join ",") -replace '\s', '').ToLowerInvariant().Split(",") | Where-Object { $_ -ne "" })
if ($TargetList.Count -eq 0) { Fail "-Targets requires at least one target." }
foreach ($target in $TargetList) {
    if (@("claude", "codex", "gemini", "cursor", "mux", "all") -notcontains $target) {
        Fail "Unknown target '$target'. Valid targets: claude, codex, gemini, cursor, mux, all."
    }
}
$TargetText = $TargetList -join ","

$Csproj = [IO.Path]::Combine($RepoRoot, "src", "DocumentAtom.McpServer", "DocumentAtom.McpServer.csproj")
$BinDir = [IO.Path]::Combine($RepoRoot, "src", "DocumentAtom.McpServer", "bin")

Write-Host "DocumentAtom MCP uninstall (Windows)"
Write-Host "[1/3] Checking for .NET..."
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Fail "dotnet was not found on PATH. Install .NET 8 or later: https://dotnet.microsoft.com/download"
}
# Windows PowerShell turns redirected stderr into errors under "Stop", so list with "Continue".
function Get-DotnetList([string]$Option) {
    $previous = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try { return @(& dotnet $Option 2>$null | Where-Object { "$_".Trim() }) } catch { return @() } finally { $ErrorActionPreference = $previous }
}
$Sdks = @(Get-DotnetList "--list-sdks")
$Runtimes = @(Get-DotnetList "--list-runtimes")

# Returns an existing DocumentAtom.McpServer.dll that the installed runtime can run: net10.0 before
# net8.0, Release before Debug. Returns $null when there is none.
function Find-Server {
    $frameworks = @()
    if ($Runtimes | Where-Object { $_ -match '^Microsoft\.NETCore\.App 10\.' }) { $frameworks += "net10.0" }
    if ($Runtimes | Where-Object { $_ -match '^Microsoft\.NETCore\.App 8\.' }) { $frameworks += "net8.0" }
    foreach ($framework in $frameworks) {
        foreach ($config in @("Release", "Debug")) {
            $candidate = [IO.Path]::Combine($BinDir, $config, $framework, "DocumentAtom.McpServer.dll")
            if (Test-Path -LiteralPath $candidate -PathType Leaf) { return $candidate }
        }
    }
    return $null
}

if ($Sdks.Count -gt 0) {
    # Build one framework only: building both needs both SDKs.
    if ($Sdks | Where-Object { $_ -match '^1[0-9]\.' }) { $Framework = "net10.0" } else { $Framework = "net8.0" }
    Write-Host "[2/3] Building DocumentAtom.McpServer (Release, $Framework)..."
    & dotnet build $Csproj -c Release -f $Framework --nologo -v quiet
    if ($LASTEXITCODE -eq 0) {
        $Server = [IO.Path]::Combine($BinDir, "Release", $Framework, "DocumentAtom.McpServer.dll")
        if (-not (Test-Path -LiteralPath $Server -PathType Leaf)) { Fail "The build succeeded, but $Server was not found." }
    }
    else {
        [Console]::Error.WriteLine("Warning: the build failed; removing with an existing build instead.")
        $Server = Find-Server
        if (-not $Server) { Fail "The build failed and no existing DocumentAtom.McpServer.dll that this .NET can run was found under $BinDir." }
    }
}
else {
    Write-Host "[2/3] No .NET SDK found, so nothing is rebuilt; looking for an existing build..."
    $Server = Find-Server
    if (-not $Server) { Fail "No DocumentAtom.McpServer.dll that this .NET runtime can run was found under $BinDir. Install the .NET SDK so this script can build it." }
}
Write-Host "Using $Server"
Write-Host "[3/3] Removing 'documentatom' from: $TargetText"
& dotnet $Server "--uninstall=$TargetText"
if ($LASTEXITCODE -ne 0) { Fail "Removal failed for one or more tools (see above)." }
Write-Host "These configs are for tools that run on Windows. For tools that run inside WSL, also run scripts/wsl2/uninstall.sh (or scripts/wsl/uninstall.sh) inside the distro."
Write-Host "Restart the tools so they reload their MCP servers."
exit 0
