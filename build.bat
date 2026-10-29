# RepoAdminMenu

Repo Admin Menu is a R.E.P.O. in-game admin menu mod built around BepInEx, MenuLib and REPOLib.

The project in this repository is intended to be rebuilt in a local R.E.P.O. installation with the required references present. The source is structured around the current Modding environment used by R.E.P.O. and the mod is designed to remain compatible with the 0.4.x-era APIs while removing the most obvious stability issues in config, upgrades, and multiplayer state handling.

## Supported environment

- Game: R.E.P.O.
- Modding framework: BepInEx 5
- UI library: MenuLib
- Game utility library: REPOLib
- .NET target: net472

## Required local dependencies

The repository does not include the actual game assemblies or BepInEx files. Before building, install or locate:

- R.E.P.O. game directory
- BepInEx installed in the game folder
- MenuLib and REPOLib DLLs in the BepInEx plugins folder or a configured custom mod library directory

This repository expects either:

- environment variables: `REPO_GAME_ROOT`, `BEPINEX_DIR`, `REPO_MOD_LIBS_DIR`
- or command-line properties: `-p:GameRoot=... -p:BepInExDir=... -p:ModLibDir=...`

## Build

Windows build script:

```bat
build.bat
```

The build script tries to detect common R.E.P.O. installation locations and then executes:

```bat
dotnet build RepoAdminMenu.csproj -c Release -p:GameRoot="..." -p:BepInExDir="..." -p:ModLibDir="..."
```

PowerShell equivalent:

```powershell
./build.ps1
```

## Manual build

```powershell
$env:REPO_GAME_ROOT = "C:\Games\R.E.P.O"
$env:BEPINEX_DIR = "C:\Games\R.E.P.O\BepInEx"
$env:REPO_MOD_LIBS_DIR = "C:\Games\R.E.P.O\BepInEx\plugins"

dotnet build RepoAdminMenu.csproj -c Release
```

## Important notes

- The build will fail with a clear message if the game root, BepInEx, or mod libraries are missing.
- The mod relies on the actual R.E.P.O. assembly names and class layout. This repository is intentionally built to use real game references when they are available locally.
- For a runtime install, copy the built DLL into your BepInEx plugins folder after a successful build.

## Known build limitation

This environment does not include a local copy of the R.E.P.O. binaries, BepInEx, MenuLib, or REPOLib. Because of that, the project cannot be fully compiled and validated in this GitHub session without those dependencies being present locally.

## Menu features

- Player list and per-player controls
- Spawn items, valuables, and enemies
- Map selection and level manipulation
- Settings sync for host/client scenarios
- Upgrade controls for player upgrades
- Confirmation dialogs for destructive actions

## Configuration

The mod uses BepInEx configuration values. The default hotkey is `F8` and commands are enabled unless disabled in the config.

## Troubleshooting

If the build fails:

1. Confirm the game directory is correct.
2. Confirm BepInEx is installed under the game directory.
3. Confirm MenuLib and REPOLib are present in the mod library folder.
4. Re-run the script with explicit `-p:...` arguments if needed.

