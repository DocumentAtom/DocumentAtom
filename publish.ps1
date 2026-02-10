param(
    [Parameter(Mandatory=$true)]
    [string]$ApiKey,
    [string]$Source = "https://api.nuget.org/v3/index.json"
)

$ErrorActionPreference = "Stop"

Write-Host "DocumentAtom v2.0 NuGet Publish Script" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host ""

# Clean previous builds
Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
dotnet clean src/DocumentAtom.sln -c Release

# Build the solution
Write-Host "Building solution in Release mode..." -ForegroundColor Yellow
dotnet build src/DocumentAtom.sln -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Create packages directory
if (Test-Path "./packages") {
    Remove-Item -Recurse -Force "./packages"
}
New-Item -ItemType Directory -Path "./packages" | Out-Null

# Pack the solution
Write-Host "Packing NuGet packages..." -ForegroundColor Yellow
dotnet pack src/DocumentAtom.sln -c Release -o ./packages

if ($LASTEXITCODE -ne 0) {
    Write-Host "Pack failed!" -ForegroundColor Red
    exit 1
}

# Define packages in dependency order
$packages = @(
    "DocumentAtom.Core",
    "DocumentAtom.Text",
    "DocumentAtom.Documents",
    "DocumentAtom",
    "DocumentAtom.DataIngestion"
)

Write-Host ""
Write-Host "Publishing packages to $Source" -ForegroundColor Yellow
Write-Host ""

foreach ($pkg in $packages) {
    $nupkg = Get-ChildItem "./packages/$pkg.*.nupkg" -ErrorAction SilentlyContinue | Select-Object -First 1

    if ($nupkg) {
        Write-Host "Publishing $($nupkg.Name)..." -ForegroundColor Green
        dotnet nuget push $nupkg.FullName --api-key $ApiKey --source $Source --skip-duplicate

        if ($LASTEXITCODE -ne 0) {
            Write-Host "Failed to publish $($nupkg.Name)" -ForegroundColor Red
        }
    } else {
        Write-Host "Package $pkg not found in ./packages/" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Publish complete!" -ForegroundColor Cyan
Write-Host ""

# List generated packages
Write-Host "Generated packages:" -ForegroundColor Yellow
Get-ChildItem "./packages/*.nupkg" | ForEach-Object { Write-Host "  - $($_.Name)" }
