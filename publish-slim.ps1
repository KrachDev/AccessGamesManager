# Publish slim executable
# Usage: ./publish-slim.ps1

param(
    [string]$Configuration = "Release",
    [string]$OutputDir = "bin\Publish"
)

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host " SLIM BUILD: Creating optimized single-file executable" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$projectPath = "AccessGames Manager\AccessGames Manager.csproj"

# Check if project exists
if (-not (Test-Path $projectPath)) {
    Write-Host "ERROR: Project file not found at $projectPath" -ForegroundColor Red
    exit 1
}

# Clean previous builds
Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
$cleanPaths = @(
    "AccessGames Manager\bin",
    "AccessGames Manager\obj",
    $OutputDir
)

foreach ($path in $cleanPaths) {
    if (Test-Path $path) {
        Remove-Item -Path $path -Recurse -Force -ErrorAction SilentlyContinue
    }
}

Write-Host ""
Write-Host "Publishing slim build (this may take 1-2 minutes)..." -ForegroundColor Yellow
Write-Host ""

$sw = [System.Diagnostics.Stopwatch]::StartNew()

try {
    # Publish with optimizations
    & dotnet publish $projectPath `
        -c $Configuration `
        -r win-x64 `
        --self-contained `
        -p:PublishSingleFile=true `
        -p:PublishReadyToRun=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -p:EnableCompressionInSingleFile=true `
        -p:DebugType=embedded `
        -p:DebugSymbols=false `
        -p:Deterministic=true `
        -o $OutputDir

    if ($LASTEXITCODE -ne 0) {
        Write-Host ""
        Write-Host "ERROR: Build failed!" -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "ERROR: $_" -ForegroundColor Red
    exit 1
}

$sw.Stop()

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host " BUILD COMPLETE!" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host ""

$exePath = Join-Path $OutputDir "AccessGames Manager.exe"
if (Test-Path $exePath) {
    $sizeBytes = (Get-Item $exePath).Length
    $sizeMB = [math]::Round($sizeBytes / 1MB, 2)
    
    Write-Host "Output: $exePath" -ForegroundColor Green
    Write-Host "Size:   $sizeMB MB" -ForegroundColor Green
}

Write-Host ""
Write-Host "Build time: $([math]::Round($sw.Elapsed.TotalSeconds, 2))s" -ForegroundColor Cyan
Write-Host ""
Write-Host "Changes in this slim build:" -ForegroundColor Yellow
Write-Host "  ✓ Removed WebView2 runtime (~20MB)" -ForegroundColor Gray
Write-Host "  ✓ Removed Store functionality (~1MB)" -ForegroundColor Gray
Write-Host "  ✓ Single-file executable with compression" -ForegroundColor Gray
Write-Host "  ✓ ReadyToRun (R2R) native compilation" -ForegroundColor Gray
Write-Host ""
Write-Host "To run: $exePath" -ForegroundColor Cyan
Write-Host ""
