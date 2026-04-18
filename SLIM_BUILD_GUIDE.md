# Slim Build Optimization Summary

## Objective
Create the smallest possible single-file executable (~30MB) by removing Store and WebView functionality.

## Changes Made

### 1. **Project Configuration (.csproj)**
- ✅ Added `<PublishSingleFile>true</PublishSingleFile>` - Single .exe output
- ✅ Added `<PublishReadyToRun>true</PublishReadyToRun>` - AOT compilation for faster startup and smaller download
- ✅ Added `<IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>` - Self-extracting archive
- ✅ Added `<EnableCompressionInSingleFile>true</EnableCompressionInSingleFile>` - Maximum compression
- ✅ Set `<RuntimeIdentifier>win-x64</RuntimeIdentifier>` - Platform-specific optimization
- ✅ Added `<DebugSymbols>false</DebugSymbols>` - Remove debug symbols
- ✅ Set `<DebugType>embedded</DebugType>` - Embed remaining debug info if needed
- ✅ Added `<CopyLocalLockFileAssemblies>false</CopyLocalLockFileAssemblies>` - Skip redundant assemblies
- ✅ Excluded `store.html` from Assets

### 2. **Package Removals**
Removed the following heavy dependencies:
- ❌ `WebView.Avalonia` (v11.0.0.1)
- ❌ `WebView.Avalonia.Desktop` (v11.0.0.1)

These WebView packages add significant size due to Chromium/Edge runtime dependencies.

### 3. **Files Removed**
- ❌ `Views/StoreView.axaml` - Store UI component
- ❌ `Views/StoreView.axaml.cs` - Store code-behind
- ❌ `Views/MainWindow.Store.axaml.cs` - Store navigation handler
- ❌ `Assets/store.html` - Store website asset (~1.1MB)

### 4. **Code Changes**

#### Program.cs
- Removed `using Avalonia.WebView.Desktop;`
- Removed `.UseDesktopWebView()` from AppBuilder chain

#### MainWindow.axaml
- Removed "🛒 Store" navigation button
- Removed Store page Grid (replaced with empty content)

#### MainWindow.axaml.cs
- Removed `NavStore` from navigation button array
- Removed `PageStore.IsVisible` assignment
- Cleaned up `SetNav()` method

## Size Reduction Strategy

### Primary Removals
1. **WebView Runtime** (~15-20MB)
   - Chromium/Edge WebView2 runtime
   - JavaScript engine
   - HTML/CSS rendering engine

2. **Store Assets** (~1.1MB)
   - store.html file
   - Related Node.js backend code

3. **Dependencies** (~5-10MB)
   - LocalWebServer assemblies no longer needed
   - Web-related libraries

### Build Optimizations
- **ReadyToRun (R2R)**: Pre-compiled native code for instant startup
- **Single-File**: Combines all assemblies into one executable
- **Compression**: Built-in ZIP compression reduces final size
- **Platform-Specific**: win-x64 build without unnecessary platforms

## Expected Results

**Before:** ~60-80MB (with WebView and Store)
**After:** ~25-35MB (without WebView and Store)

## Publishing

Use the provided `publish-slim.bat` script:

```batch
publish-slim.bat
```

This creates an optimized executable at: `bin\Publish\AccessGames Manager.exe`

### Manual Publishing

Alternatively, publish manually:

```powershell
dotnet publish "AccessGames Manager\AccessGames Manager.csproj" `
  -c Release `
  -r win-x64 `
  --self-contained `
  -p:PublishSingleFile=true `
  -p:PublishReadyToRun=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:EnableCompressionInSingleFile=true `
  -p:DebugSymbols=false `
  -o "bin\Publish"
```

## Functionality Retained

✅ Games library browsing
✅ Account management
✅ Steam integration
✅ Settings and configuration
✅ Firewall blocking/unblocking
✅ Auto-updater
✅ Analytics

❌ In-app Store (removed)
❌ WebView browsing (removed)

## Deployment

The single executable can be:
- ✅ Run directly without installation
- ✅ Copied to any Windows machine (no .NET SDK required)
- ✅ Included in installers
- ✅ Updated via auto-updater without re-downloading WebView

## Notes

- Build time: ~2-3 minutes for first publish
- Startup time: Fast due to ReadyToRun compilation
- Size varies by .NET 9 runtime included (~25-35MB total)
- No external dependencies required on end-user machines
