# Better Song List

The smaller brother of [Better Song Search](https://github.com/kinsi55/BeatSaber_BetterSongSearch#better-song-search). Adds Various improvements to the Basegame Map list like Filters, a persisted state and much more. Aims to be highly optimized.

---

The Game version(s) specific releases are compatible with are mentioned in the Release title (Its obviously possible latest is not supported assuming its been released recently). If you need the plugin for an older version - Grab an older release that fits 🤯

## Install

#### You can always find the latest download in [The Releases](https://github.com/kinsi55/BeatSaber_BetterSongList/releases), simply drag the Plugin DLL into the Plugins folder

### Dependencies

- BeatSaberMarkupLanguage (Available in ModAssistant)
- SongCore (Available in ModAssistant)
- Optional: If you want to be able to use extended features like sorting by BeatSaver date / Showing Star ratings and the like, you need [SongDetailsCache](https://github.com/kinsi55/BeatSaber_SongDetails/releases/latest) (Available in ModAssistant)

## Building from Source

### Prerequisites

1. **Visual Studio 2019 or newer** (or JetBrains Rider)
   - Install the .NET desktop development workload
   - Ensure .NET Framework 4.8 SDK is installed

2. **Beat Saber Installation**
   - You need a modded Beat Saber installation with IPA (BepInEx Mod Loader)
   - Install required dependencies via ModAssistant:
     - BSML (BeatSaberMarkupLanguage)
     - SongCore
     - SongDetailsCache (optional)

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/kinsi55/BeatSaber_BetterSongList.git
   cd BeatSaber_BetterSongList
   ```

2. **Configure your Beat Saber directory**
   
   Create a file named `BetterSongList.csproj.user` in the project root (copy from the example):
   ```bash
   cp BetterSongList.csproj.user.example BetterSongList.csproj.user
   ```
   
   Edit `BetterSongList.csproj.user` and set your Beat Saber installation path:
   ```xml
   <BeatSaberDir>C:\Program Files (x86)\Steam\steamapps\common\Beat Saber</BeatSaberDir>
   ```

3. **Alternative: Use a Refs folder**
   
   Instead of pointing to your Beat Saber installation, you can copy all required DLLs to a `Refs` folder:
   ```
   BeatSaber_BetterSongList/
   └── Refs/
       ├── Beat Saber_Data/
       │   └── Managed/
       │       ├── Main.dll
       │       ├── HMLib.dll
       │       └── ... (other game DLLs)
       ├── Libs/
       │   ├── 0Harmony.dll
       │   └── ... (other mod loader DLLs)
       └── Plugins/
           ├── BSML.dll
           ├── SongCore.dll
           └── ... (other mod DLLs)
   ```

### Building

**Note**: This project requires references to Beat Saber game DLLs and mod dependencies. It will not build in a CI environment without these references. This is normal for Beat Saber mods.

#### Using Visual Studio
1. Open `BetterSongList.sln`
2. Select your build configuration (Debug or Release)
3. Build → Build Solution (Ctrl+Shift+B)

#### Using Command Line
```bash
# Debug build
dotnet build BetterSongList.sln -c Debug

# Release build
dotnet build BetterSongList.sln -c Release
```

The compiled DLL will be:
- **Debug**: `bin/Debug/BetterSongList.dll`
- **Release**: `bin/BetterSongList.dll`

If you configured `BeatSaberDir` correctly, the build process will automatically:
- Copy the DLL to your Beat Saber `Plugins` folder
- If Beat Saber is running, copy to `IPA/Pending/Plugins` instead

### Running and Testing

1. **Install the mod**: The built DLL should be automatically copied to your Plugins folder
2. **Launch Beat Saber**: Run the game normally
3. **Check logs**: Look at `Beat Saber/Logs/` for any errors
4. **Test in-game**: Navigate to song selection to see Better Song List in action

### Debugging

#### Using Visual Studio Debugger

1. **Attach to Process**:
   - Start Beat Saber
   - In Visual Studio: Debug → Attach to Process (Ctrl+Alt+P)
   - Search for "Beat Saber.exe"
   - Click "Attach"

2. **Set Breakpoints**:
   - Open any .cs file and click in the left margin to set breakpoints
   - The debugger will pause execution when breakpoints are hit

3. **Alternative: dnSpy**
   
   For more advanced debugging, use [dnSpy](https://github.com/dnSpy/dnSpy):
   - Open Beat Saber.exe in dnSpy
   - Debug → Start Debugging
   - Set breakpoints in your mod's code

#### Logging

Use the built-in logger in your code:
```csharp
Plugin.Log.Debug("Debug message");
Plugin.Log.Info("Info message");
Plugin.Log.Warn("Warning message");
Plugin.Log.Error("Error message");
```

Logs are written to `Beat Saber/Logs/[PluginName].log`

### Troubleshooting

**Build Error: "The type or namespace name 'X' could not be found"**
- Ensure all dependencies are installed in your Beat Saber installation
- Check that `BeatSaberDir` points to the correct directory
- Verify all required DLLs exist in the referenced locations

**Build Warning: "Unable to copy to Plugins folder"**
- This is normal if you haven't set up `BetterSongList.csproj.user`
- The DLL will still build successfully in the `bin` folder
- Manually copy the DLL to test it

**Build Error: "BeatSaberModdingTools.Tasks nuget package doesn't seem to be installed"**
- This is a warning, not an error - the mod will still build
- Advanced features like auto-zipping releases won't work
- You can safely ignore this for development

**Changes not appearing in-game**
- Make sure Beat Saber is closed when copying the DLL
- If the game was running, the DLL goes to `IPA/Pending/Plugins`
- Restart Beat Saber to load the updated mod

### Project Structure

- `Plugin.cs` - Main plugin entry point
- `Configuration/` - Config management
- `Filters/` - Song filtering logic
- `Sorters/` - Song sorting implementations
- `UI/` - User interface components (BSML)
- `HarmonyPatches/` - Harmony patches for game modifications
- `Util/` - Utility classes

## Features

- Adds various sorting and filtering methods as well as a "Random Song" button
- Rembers 😁 your last selected Category / Playlist / Song and returns there
- Context-Aware legend-scrollbar whose steps are determined by your sorting method
- Fixes the shifting around of Practice / Play buttons when using Scoresaber
- Displays the map Default Jump Distance (Can be changed to show the maps Offset if you prefer that)
- Extended Scroll buttons
- Adds various extra details about the Song Like Ranked Information and NJS
- Plugin system - You can add your own Sorts & Filters to Better Song List!
- Probably other minor things I Forgor 💀

![Main UI](Screenshots/Main.jpg)
