# OnTopReplica Build Guide

This guide provides instructions for building OnTopReplica from source code.

## Prerequisites

### Required Software

1. **Visual Studio** (2015 or later recommended)
   - Community Edition (free) is sufficient
   - Download: https://visualstudio.microsoft.com/downloads/

2. **.NET Framework 4.7**
   - Usually included with Visual Studio
   - Target framework: .NET Framework 4.7

3. **Windows Operating System**
   - Windows Vista or later
   - Required for DWM (Desktop Window Manager) features

### Optional Tools

- **Git** for version control
- **NuGet Package Manager** (included with Visual Studio)

## Building the Project

### Method 1: Using Visual Studio (Recommended)

1. **Open the Solution**
   ```
   Open: src/OnTopReplica.sln
   ```

2. **Restore NuGet Packages**
   - Visual Studio should automatically restore packages on first build
   - Or manually: Right-click solution → "Restore NuGet Packages"
   - Required package: `Windows-Forms-Aero` (version 3.0.1)

3. **Select Build Configuration**
   - **Debug**: For development with debugging symbols
   - **Release**: For optimized production builds

4. **Build the Solution**
   - Press `F6` or
   - Menu: Build → Build Solution
   - Or: Right-click solution → Build

5. **Output Location**
   - Debug build: `src/OnTopReplica/bin/Debug/OnTopReplica.exe`
   - Release build: `src/OnTopReplica/bin/Release/OnTopReplica.exe`

### Method 2: Using MSBuild (Command Line)

1. **Open Developer Command Prompt**
   - Start → Visual Studio → Developer Command Prompt

2. **Navigate to Solution Directory**
   ```cmd
   cd path\to\OnTopReplica\src
   ```

3. **Build the Solution**
   ```cmd
   msbuild OnTopReplica.sln /p:Configuration=Release /t:Build
   ```

   Or for Debug build:
   ```cmd
   msbuild OnTopReplica.sln /p:Configuration=Debug /t:Build
   ```

4. **Clean Build (if needed)**
   ```cmd
   msbuild OnTopReplica.sln /t:Clean
   msbuild OnTopReplica.sln /p:Configuration=Release /t:Rebuild
   ```

## Running the Application

### From Visual Studio

1. Press `F5` (Debug mode) or `Ctrl+F5` (Without debugging)
2. Or: Debug → Start Debugging / Start Without Debugging

### From Command Line

```cmd
cd src\OnTopReplica\bin\Release
OnTopReplica.exe
```

### Command Line Arguments

OnTopReplica supports various command-line options:

```cmd
OnTopReplica.exe --help
```

Common options:
- `--window-id <HWND>` - Clone window by handle
- `--window-title <title>` - Clone window by title
- `--window-class <class>` - Clone window by class name
- `--opacity <0-255>` - Set initial opacity
- `--disable-chrome` - Start without window border
- `--enable-click-forwarding` - Enable click forwarding
- `--fullscreen` - Start in fullscreen mode

Example:
```cmd
OnTopReplica.exe --window-title "Notepad" --opacity 200
```

## Project Structure

```
OnTopReplica/
├── src/
│   ├── OnTopReplica/           # Main application
│   │   ├── MainForm.cs         # Core UI form (partial class)
│   │   ├── MainForm_Features.cs    # Feature implementations
│   │   ├── MainForm_MenuEvents.cs  # Menu event handlers
│   │   ├── ThumbnailPanel.cs   # Window cloning display
│   │   ├── CustomResizeDialog.cs   # Custom resize feature (NEW)
│   │   ├── CustomPositionDialog.cs # Custom position feature (NEW)
│   │   ├── SidePanels/         # Side panel UI components
│   │   ├── MessagePumpProcessors/  # Event handlers
│   │   ├── WindowSeekers/      # Window discovery
│   │   ├── Native/             # Windows API wrappers
│   │   └── Platforms/          # OS-specific implementations
│   ├── OnTopReplica.sln        # Visual Studio solution
│   └── packages/               # NuGet packages
├── Installer/                  # Installation project
└── BUILD.md                    # This file
```

## New Features (This Branch)

### 1. Custom Resize Ratio

**File**: `src/OnTopReplica/CustomResizeDialog.cs`

Allows users to enter custom resize ratios beyond the preset sizes.

**Usage**:
1. Right-click window → Resize → Custom...
2. Choose input mode:
   - **Percentage**: 1-1000% (e.g., 75%, 150%)
   - **Decimal**: 0.01-10.0 (e.g., 1.5, 0.33)
3. Enter desired ratio and click OK

**Implementation Details**:
- Menu item: MainForm.Designer.cs:271, 310-315
- Event handler: MainForm_MenuEvents.cs:112-118
- Dialog class: CustomResizeDialog.cs
- Reuses existing `FitToThumbnail(double)` method

### 2. Custom Position Lock

**File**: `src/OnTopReplica/CustomPositionDialog.cs`

Allows users to lock the window to custom fixed coordinates.

**Usage**:
1. Right-click window → Position Lock → Custom...
2. Enter X and Y coordinates (or click "Use Current Position")
3. Click OK to lock window to those coordinates

**Implementation Details**:
- Menu item: MainForm.Designer.cs:343, 399-404
- Event handler: MainForm_MenuEvents.cs:153-162
- Dialog class: CustomPositionDialog.cs
- Extended position lock system: MainForm_Features.cs:137-189
  - New field: `_customPositionLock`
  - New property: `IsCustomPositionLocked`
  - New methods: `SetCustomPositionLock()`, `ClearCustomPositionLock()`

## Dependencies

### NuGet Packages

- **Windows-Forms-Aero** (version 3.0.1)
  - Provides DWM (Desktop Window Manager) integration
  - Required for thumbnail rendering
  - Auto-installed via NuGet package restore

### System Requirements

- **Windows Vista or later**
  - DWM (Desktop Window Manager) support required
  - Aero theme features on Vista/7
  - Windows 8/10/11 fully supported

- **.NET Framework 4.7 Runtime**
  - Required for end users
  - Included in Windows 10 version 1803+

## Troubleshooting

### Build Errors

**Error: Missing NuGet packages**
```
Solution: Right-click solution → Restore NuGet Packages
```

**Error: Cannot find Windows-Forms-Aero**
```
Solution:
1. Open Package Manager Console (Tools → NuGet Package Manager)
2. Run: Install-Package Windows-Forms-Aero -Version 3.0.1
```

**Error: Target framework not found**
```
Solution:
1. Install .NET Framework 4.7 SDK
2. Or modify OnTopReplica.csproj to target an installed framework version
```

### Runtime Errors

**Application doesn't start**
- Ensure .NET Framework 4.7 is installed
- Check Windows Event Viewer for error details
- Try running as Administrator

**Thumbnail doesn't appear**
- Verify DWM is enabled (Windows Vista+)
- Check that target window is not minimized
- Ensure target window still exists

**Click forwarding not working**
- Some applications block injected input
- Try running OnTopReplica as Administrator
- Check that target window accepts input

### Development Issues

**IntelliSense not working**
- Close and reopen solution
- Delete `.vs` folder and restart Visual Studio
- Run: Build → Clean Solution, then rebuild

**Designer errors**
- Close Designer view
- Rebuild project
- Reopen Designer

## Testing

### Manual Testing Checklist

**Basic Functionality**:
- [ ] Application launches without errors
- [ ] Window list populates correctly
- [ ] Clone window displays thumbnail
- [ ] Right-click context menu appears
- [ ] Window resizing preserves aspect ratio

**New Features**:
- [ ] Custom Resize dialog opens and accepts input
  - [ ] Percentage mode works (1-1000%)
  - [ ] Decimal mode works (0.01-10.0)
  - [ ] Window resizes to custom ratio correctly
- [ ] Custom Position dialog opens and accepts input
  - [ ] Manual X/Y input works
  - [ ] "Use Current Position" button works
  - [ ] Position lock maintains coordinates on resize
  - [ ] Disable option clears custom position

**Advanced Features**:
- [ ] Click forwarding routes clicks to original window
- [ ] Click-through mode makes window transparent to input
- [ ] Region selection crops thumbnail correctly
- [ ] Fullscreen mode works on all monitors
- [ ] Position lock presets work (Top Left, Center, etc.)
- [ ] Opacity changes apply correctly
- [ ] Chrome toggle shows/hides border
- [ ] Hotkeys work (if configured)

## Contributing

When contributing code:

1. **Follow existing code style**
   - Partial classes for MainForm features
   - Regions for code organization
   - XML documentation comments

2. **Test thoroughly**
   - Run manual testing checklist
   - Test on different Windows versions if possible

3. **Update documentation**
   - Add new features to this build guide
   - Update comments in code

4. **Commit messages**
   - Clear, descriptive commit messages
   - Reference issue numbers if applicable

## Additional Resources

- **Original Project**: [OnTopReplica GitHub](https://github.com/LorenzCK/OnTopReplica)
- **Windows-Forms-Aero**: [NuGet Package](https://www.nuget.org/packages/Windows-Forms-Aero/)
- **DWM Documentation**: [Microsoft Docs](https://docs.microsoft.com/en-us/windows/win32/dwm/dwm-overview)

## License

OnTopReplica is licensed under the MS-PL (Microsoft Public License).
See LICENSE.txt for full details.

## Support

For issues and questions:
- Check existing GitHub issues
- Create new issue with detailed description
- Include Windows version and .NET Framework version
- Attach screenshots if applicable

---

**Version**: Current branch with custom resize and position features
**Last Updated**: 2025
**Build Status**: Development build with new features
