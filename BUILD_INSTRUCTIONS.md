# CHAOS LAUNCHER - Build & Publish Instructions

## Prerequisites

1. **Visual Studio 2022** (Community Edition is free)
   - Download: https://visualstudio.microsoft.com/downloads/
   - Install with ".NET Desktop Development" workload

2. **.NET 6.0 SDK** (or higher)
   - Download: https://dotnet.microsoft.com/download

3. **Windows 10/11** (64-bit)

## Build Steps

### Step 1: Open Project in Visual Studio

1. Open Visual Studio 2022
2. Click **File → Open → Folder**
3. Select the `ChaosLauncher` folder
4. Wait for Visual Studio to load the project

### Step 2: Restore Dependencies

1. Press `Ctrl + Shift + B` or go to **Build → Build Solution**
2. Wait for the build to complete

### Step 3: Run/Debug (Optional)

1. Press `F5` or **Debug → Start Debugging**
2. Application will launch with admin privileges request
3. Click "Yes" when prompted for admin access

## Publishing (Create Single EXE)

### Method 1: Visual Studio UI

1. Right-click on project in Solution Explorer
2. Select **Publish**
3. Click **New Profile**
4. Choose **Folder** as target
5. Click **Next**
6. Set folder location (e.g., `C:\ChaosLauncher\publish`)
7. Click **Finish**
8. Click **Publish** button

The executable will be created as a single `.exe` file.

### Method 2: Command Line

Open PowerShell in the project directory and run:

```powershell
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

The executable will be in: `bin/Release/net6.0-windows/win-x64/publish/ChaosLauncher.exe`

## Final Executable

After publishing, you'll have:
- **ChaosLauncher.exe** - Single executable file (~150-200MB)
- No additional dependencies needed
- Can be run on any Windows 10/11 64-bit system
- Automatically requests admin privileges

## Distribution

1. Copy `ChaosLauncher.exe` to a folder
2. Create a ZIP file with the executable
3. Share the ZIP file for download
4. Users can extract and run directly

## Troubleshooting

### Build Fails
- Ensure .NET 6.0 SDK is installed: `dotnet --version`
- Clean project: **Build → Clean Solution**
- Rebuild: **Build → Rebuild Solution**

### Admin Privileges Not Working
- Verify `app.manifest` is in project folder
- Check that `ApplicationManifest` is set in `.csproj`
- Rebuild solution

### Publish Creates Multiple Files
- Ensure `PublishSingleFile=true` is set in `.csproj`
- Use command line method above

### Application Won't Start
- Check Windows Defender isn't blocking it
- Run as Administrator manually
- Check Event Viewer for errors

## Optimization Tips

### Reduce File Size
Add to `.csproj`:
```xml
<PublishTrimmed>true</PublishTrimmed>
<PublishReadyToRun>true</PublishReadyToRun>
<DebugType>embedded</DebugType>
```

### Faster Startup
Add to `.csproj`:
```xml
<PublishReadyToRun>true</PublishReadyToRun>
```

## Version Updates

To update version:
1. Edit `ChaosLauncher.csproj`
2. Change `<Version>1.0.1</Version>` to new version
3. Rebuild and republish

## Support

For issues:
1. Check build output for errors
2. Verify all prerequisites are installed
3. Try clean rebuild
4. Check Windows Event Viewer for runtime errors

---

**CHAOS LAUNCHER** - Build v1.0.0
