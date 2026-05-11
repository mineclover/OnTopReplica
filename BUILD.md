# Build Instructions

This document describes how to build **OnTopReplica** from source.

## Prerequisites

### Required Software

1. **.NET Framework 4.7 or higher**
   - Download from: https://dotnet.microsoft.com/download/dotnet-framework

2. **MSBuild 4.8 (from Visual Studio 2022 Build Tools or Visual Studio 2022)**
   - Download Visual Studio 2022 Build Tools from: https://visualstudio.microsoft.com/downloads/
   - Or install Visual Studio 2022 Community Edition
   - Location: `C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe`

3. **Windows SDK 10.0A** (for .NET Framework 4.7 Tools)
   - Included with Visual Studio or Visual Studio Build Tools
   - Required for Assembly Linker (AL.exe) to generate satellite assemblies for localization
   - Location: `C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.7 Tools\`

### NuGet Packages

The project uses the following NuGet packages (automatically restored during build):

- **System.Resources.Extensions** (8.0.0): For resource handling
- **WindowsFormsAero** (3.0.1): For Windows Aero UI components

## Build Environment Setup

### 1. Install Build Tools

Install Visual Studio 2022 Build Tools with the following workloads:
- .NET desktop build tools
- Windows 10 SDK

### 2. Verify Paths

Make sure the following paths exist:

```
C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe
C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.7 Tools\
```

### 3. Environment Variables

The build script automatically sets the required environment variable:
- `TargetFrameworkSDKToolsDirectory`: Points to Windows SDK tools directory for AL.exe

## Building the Project

### Method 1: Using the Build Script (Recommended)

A PowerShell build script (`build.ps1`) is provided for convenience:

```powershell
# Run from repository root
powershell -ExecutionPolicy Bypass -File build.ps1
```

The build script:
1. Sets the `TargetFrameworkSDKToolsDirectory` environment variable
2. Runs NuGet restore (if needed)
3. Builds the project using MSBuild 4.8
4. Generates localized satellite assemblies

### Method 2: Manual Build

```powershell
# Set environment variable for Assembly Linker
$env:TargetFrameworkSDKToolsDirectory = "C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.7 Tools\"

# Build using MSBuild
& "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe" `
  "src\OnTopReplica\OnTopReplica.csproj" `
  /p:Configuration=Release `
  /v:m
```

### Method 3: Using Visual Studio

1. Open `OnTopReplica.sln` in Visual Studio 2022
2. Restore NuGet packages (right-click solution → Restore NuGet Packages)
3. Build → Build Solution (or press F6)

## Build Output

After a successful build, the executable and resources will be located at:

```
src\OnTopReplica\bin\Release\OnTopReplica.exe
```

Language resource files (satellite assemblies) will be in subdirectories:
```
src\OnTopReplica\bin\Release\ko\OnTopReplica.resources.dll
src\OnTopReplica\bin\Release\ja-JP\OnTopReplica.resources.dll
src\OnTopReplica\bin\Release\zh-CN\OnTopReplica.resources.dll
... (other languages)
```

## Troubleshooting

### Error: MSB3086 - Cannot find AL.exe

**Problem**: Assembly Linker (AL.exe) not found.

**Solution**:
1. Ensure Windows SDK 10.0A is installed
2. Verify the path: `C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.7 Tools\AL.exe`
3. Make sure `TargetFrameworkSDKToolsDirectory` environment variable is set correctly

### Error: MSB3823 - Non-string resources require GenerateResourceUsePreserializedResources

**Problem**: Resource generation error.

**Solution**:
1. Ensure `System.Resources.Extensions` NuGet package is installed
2. The project file already includes `<GenerateResourceUsePreserializedResources>true</GenerateResourceUsePreserializedResources>`

### Error: File is in use / Cannot copy to bin\Release

**Problem**: OnTopReplica.exe is currently running.

**Solution**:
```cmd
taskkill /F /IM OnTopReplica.exe
```

### Error: WindowsFormsAero namespace not found

**Problem**: WindowsFormsAero NuGet package not restored.

**Solution**:
```cmd
nuget restore OnTopReplica.sln
```

## Development Notes

### Project Structure

- **src/OnTopReplica/**: Main application source code
  - `MainForm.cs` / `MainForm_*.cs`: Main window and partial-class feature splits (GUI / Features / MenuEvents / ChildForms)
  - `ThumbnailPanel.cs`: DWM-thumbnail rendering panel (live window mirror)
  - `ImagePanel.cs`: Static-image rendering panel (new, supports mouse pass-through and placement-mode internal drag)
  - `ImagePreset.cs` / `ImagePresetArray.cs`: Image overlay preset model + XML serialization
  - `FullscreenFormManager.cs`: Fullscreen state with cover-monitor variant
  - `*InputForm.cs`: Pixel-precise input dialogs (Position, Size, Scale) with live preview + Cancel-restore
  - `SidePanels/`: Side panel UI components (Options, About, Region, GroupSwitch, **ImagePresetPanel**)
  - `MessagePumpProcessors/`: Native message handlers — `HotKeyManager` registers global hotkeys (clone-current, show-hide, image-panel toggle, and one per `ImagePreset.Hotkey`)
  - `Native/`: Win32 P/Invoke
  - `StartupOptions/`: Command-line parsing + TypeConverters
  - `Properties/`: Settings (user-scoped XML, including `ImagePresets` and `HotKeyImagePanel`) and resources
  - `Assets/`: Image resources (flags, icons)
- **src/OnTopReplica.Tests/**: Console-runner unit tests (see "Running Tests" above)

### Adding New Languages

1. Create new resource file: `Strings.{culture}.resx` (e.g., `Strings.ko.resx`)
2. Add flag image to `Assets/` directory
3. Add flag reference to `Properties/Resources.resx`
4. Update `OptionsPanel.cs` to include the new language in `_languageList`
5. Build to generate satellite assembly

### Code Signing

The project previously used code signing but it has been disabled. If you need to re-enable:
1. Obtain a code signing certificate
2. Update `OnTopReplica.csproj` with:
   - `<ManifestCertificateThumbprint>`
   - `<ManifestKeyFile>`

## Continuous Integration

For automated builds, ensure your CI environment has:
- .NET Framework 4.7 SDK
- MSBuild 4.8 or higher
- Windows SDK 10.0A
- NuGet CLI tool

Example CI build command:
```bash
nuget restore
powershell -ExecutionPolicy Bypass -File build.ps1
```

## Running Tests

A standalone console test project lives under `src/OnTopReplica.Tests/`. It uses a custom `[Test]` attribute and a reflection-based runner — no MSTest/VSTest infrastructure required, which keeps the toolchain simple on .NET Framework 4.7.

### Run all tests

```powershell
# From repository root
./test.ps1
```

This builds the test project with VS 2022 BuildTools MSBuild and runs the resulting executable. Output:

```
== ImagePresetTests
  [PASS] ArrayRoundTrip_EmptyList
  ...
Result: 21 passed, 0 failed
```

### Manual

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe" `
  "src\OnTopReplica.Tests\OnTopReplica.Tests.csproj" `
  /p:Configuration=Debug /t:Build /v:minimal
& "src\OnTopReplica.Tests\bin\Debug\OnTopReplica.Tests.exe"
```

### Coverage

| Test class | What it verifies |
|------------|------------------|
| `SizeConverterTests` | Negative-value regex regression (commit 63e9487), round-trip |
| `ScaleInputFormTests` | `ToScaleFactor` percent→ratio conversion |
| `ResizeLockTests` | Lock pins Min/Max to current; unlock restores prior values; idempotency |
| `ImagePresetTests` | XML round-trip incl. per-preset `Hotkey`, defaults, drops empty-Path entries, opacity clamping |

### Adding new tests

1. Create `src/OnTopReplica.Tests/<Name>Tests.cs` with a public class
2. Add `[Test]` to public parameter-less methods
3. Add the file to `<ItemGroup><Compile Include="..." /></ItemGroup>` in `OnTopReplica.Tests.csproj`
4. Use `Assert.AreEqual`, `Assert.AreClose`, `Assert.IsTrue`, or `Assert.Throws<T>`

The runner auto-discovers via reflection — no registration needed.

## Additional Resources

- [.NET Framework Downloads](https://dotnet.microsoft.com/download/dotnet-framework)
- [Visual Studio Downloads](https://visualstudio.microsoft.com/downloads/)
- [MSBuild Reference](https://docs.microsoft.com/en-us/visualstudio/msbuild/msbuild)
- [Windows SDK Information](https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/)
