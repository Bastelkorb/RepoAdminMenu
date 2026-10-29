@echo off
setlocal enabledelayedexpansion

set "SCRIPT_DIR=%~dp0"
set "GAME_ROOT="
set "BEPINEX_DIR="
set "MOD_LIBS_DIR="

rem Search typical Steam installations first.
for %%D in (
  "%ProgramFiles(x86)%\Steam\steamapps\common\R.E.P.O"
  "%ProgramFiles%\Steam\steamapps\common\R.E.P.O"
  "%USERPROFILE%\Desktop\R.E.P.O"
  "%USERPROFILE%\Games\R.E.P.O"
  "%USERPROFILE%\AppData\Local\Programs\R.E.P.O"
) do (
  if exist "%%~D\R.E.P.O.exe" set "GAME_ROOT=%%~D"
)

rem Search Thunderstore Mod Manager profile paths used by many REPO mod setups.
if "%GAME_ROOT%"=="" (
  for /d %%P in ("%USERPROFILE%\AppData\Roaming\Thunderstore Mod Manager\DataFolder\REPO\profiles\*") do (
    if exist "%%~P\BepInEx\core\BepInEx.dll" (
      set "GAME_ROOT=%%~P"
      set "BEPINEX_DIR=%%~P\BepInEx"
    )
    if "%GAME_ROOT%"=="" if exist "%%~P\R.E.P.O_Data\Managed\Assembly-CSharp.dll" set "GAME_ROOT=%%~P"
  )
)

if "%GAME_ROOT%"=="" if defined REPO_GAME_ROOT set "GAME_ROOT=%REPO_GAME_ROOT%"
if "%GAME_ROOT%"=="" if defined GAME_ROOT set "GAME_ROOT=%GAME_ROOT%"

if "%GAME_ROOT%"=="" (
  echo [RepoAdminMenu] Could not find a local R.E.P.O installation.
  echo [RepoAdminMenu] Searched Steam, standard folders and Thunderstore profile paths.
  echo [RepoAdminMenu] Set REPO_GAME_ROOT or pass -p:GameRoot to the game directory.
  exit /b 1
)

rem Determine the active BepInEx location.
if "%BEPINEX_DIR%"=="" if exist "%GAME_ROOT%\BepInEx\core\BepInEx.dll" set "BEPINEX_DIR=%GAME_ROOT%\BepInEx"
if "%BEPINEX_DIR%"=="" if exist "%GAME_ROOT%\core\BepInEx.dll" set "BEPINEX_DIR=%GAME_ROOT%"
if "%BEPINEX_DIR%"=="" if defined BEPINEX_DIR set "BEPINEX_DIR=%BEPINEX_DIR%"

if "%BEPINEX_DIR%"=="" (
  echo [RepoAdminMenu] Could not find BepInEx in %GAME_ROOT%.
  echo [RepoAdminMenu] This often means the game is using a Thunderstore profile at:
  echo [RepoAdminMenu] %USERPROFILE%\AppData\Roaming\Thunderstore Mod Manager\DataFolder\REPO\profiles\
  echo [RepoAdminMenu] Install BepInEx or set BEPINEX_DIR.
  exit /b 1
)

rem Determine the mod library folder.
if "%MOD_LIBS_DIR%"=="" if exist "%BEPINEX_DIR%\plugins\MenuLib.dll" set "MOD_LIBS_DIR=%BEPINEX_DIR%\plugins"
if "%MOD_LIBS_DIR%"=="" if exist "%BEPINEX_DIR%\plugins\REPOLib.dll" set "MOD_LIBS_DIR=%BEPINEX_DIR%\plugins"
if "%MOD_LIBS_DIR%"=="" if exist "%GAME_ROOT%\BepInEx\plugins\MenuLib.dll" set "MOD_LIBS_DIR=%GAME_ROOT%\BepInEx\plugins"
if "%MOD_LIBS_DIR%"=="" if defined REPO_MOD_LIBS_DIR set "MOD_LIBS_DIR=%REPO_MOD_LIBS_DIR%"
if "%MOD_LIBS_DIR%"=="" (
  echo [RepoAdminMenu] Could not find MenuLib or REPOLib.
  echo [RepoAdminMenu] Install these dependencies in %BEPINEX_DIR%\plugins or set REPO_MOD_LIBS_DIR.
  exit /b 1
)

where dotnet >nul 2>nul
if errorlevel 1 (
  echo [RepoAdminMenu] .NET SDK was not found on PATH.
  exit /b 1
)

echo [RepoAdminMenu] Using GameRoot: %GAME_ROOT%
echo [RepoAdminMenu] Using BepInExDir: %BEPINEX_DIR%
echo [RepoAdminMenu] Using ModLibDir: %MOD_LIBS_DIR%

dotnet build "%SCRIPT_DIR%RepoAdminMenu.csproj" -c Release -p:GameRoot="%GAME_ROOT%" -p:BepInExDir="%BEPINEX_DIR%" -p:ModLibDir="%MOD_LIBS_DIR%"

if errorlevel 1 (
  exit /b 1
)

echo [RepoAdminMenu] Build completed successfully.
