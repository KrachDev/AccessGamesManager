@echo off
REM ═══════════════════════════════════════════════════════════════════
REM SLIM BUILD SCRIPT - Creates optimized single-file executable
REM Target: ~30MB (removed WebView, Store functionality)
REM ═══════════════════════════════════════════════════════════════════

setlocal enabledelayedexpansion

cd /d "%~dp0"

REM Configuration
set PROJECT_DIR=AccessGames Manager
set CONFIG=Release
set OUTPUT_DIR=bin\Publish

echo.
echo ═══════════════════════════════════════════════════════════════════
echo  SLIM BUILD: Creating optimized single-file executable
echo ═══════════════════════════════════════════════════════════════════
echo.

REM Clean previous builds
echo Cleaning previous builds...
if exist "%PROJECT_DIR%\bin" rmdir /s /q "%PROJECT_DIR%\bin" 2>nul
if exist "%PROJECT_DIR%\obj" rmdir /s /q "%PROJECT_DIR%\obj" 2>nul
if exist "%OUTPUT_DIR%" rmdir /s /q "%OUTPUT_DIR%" 2>nul

echo.
echo Publishing slim build (this may take a minute)...
echo.

REM Publish with ReadyToRun (R2R) for faster startup and smaller download
dotnet publish "%PROJECT_DIR%\AccessGames Manager.csproj" ^
  -c %CONFIG% ^
  -r win-x64 ^
  --self-contained ^
  -p:PublishSingleFile=true ^
  -p:PublishReadyToRun=true ^
  -p:PublishTrimmed=false ^
  -p:IncludeNativeLibrariesForSelfExtract=true ^
  -p:EnableCompressionInSingleFile=true ^
  -p:DebugType=embedded ^
  -p:DebugSymbols=false ^
  -p:Deterministic=true ^
  -o "%OUTPUT_DIR%"

if errorlevel 1 (
  echo.
  echo ERROR: Build failed!
  exit /b 1
)

echo.
echo ═══════════════════════════════════════════════════════════════════
echo  BUILD COMPLETE!
echo ═══════════════════════════════════════════════════════════════════
echo.

REM Get file size
for %%I in ("%OUTPUT_DIR%\AccessGames Manager.exe") do (
  set SIZE_BYTES=%%~zI
  set SIZE_MB=!SIZE_BYTES:~0,-6!
  if "!SIZE_MB!"=="" set SIZE_MB=0
)

echo Output: %OUTPUT_DIR%\AccessGames Manager.exe
echo.
echo Size: !SIZE_MB! MB
echo.
echo Changes made for slim build:
echo   - Removed WebView.Avalonia packages
echo   - Removed Store functionality (StoreView, store.html)
echo   - Removed LocalWebServer dependencies
echo   - Single-file executable with compression
echo   - ReadyToRun compilation for faster startup
echo.
echo To run: %OUTPUT_DIR%\AccessGames Manager.exe
echo.

pause
