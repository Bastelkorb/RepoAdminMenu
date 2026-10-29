@echo off
setlocal enabledelayedexpansion

set "SCRIPT_DIR=%~dp0"
set "GAME_ROOT="
set "BEPINEX_DIR="
set "MOD_LIBS_DIR="

rem Prefer common Steam install locations.
for %%D in (
  "%ProgramFiles(x86)%\Steam\steamapps\common\R.E.P.O"
  "%ProgramFiles%\Steam\steamapps\common\R.E.P.O"
  "%USERPROFILE%\Desktop\R.E.P.O"
  "%USERPROFILE%\Games\R.E.P.O"
  "%USERPROFILE%\AppData\Local\Programs\R.E.P.O"
) do (
  if exist "%%~D\R.E.P.O.exe" set "GAME_ROOT=%%~D"
)

if "%GAME_ROOT%"=="" if defined REPO_GAME_ROOT set "GAME_ROOT=%REPO_GAME_ROOT%"
if "%GAME_ROOT%"=="" if defined GAME_ROOT set "GAME_ROOT=%GAME_ROOT%"

if "%GAME_ROOT%"=="" (
  echo [RepoAdminMenu] Could not find a local R.E.P.O installation.
  echo [RepoAdminMenu] Set REPO_GAME_ROOT or pass -p:GameRoot to the game directory.
  exit /b 1
)

if "%BEPINEX_DIR%"=="" if exist "%GAME_ROOT%\BepInEx\core\BepInEx.dll" set "BEPINEX_DIR=%GAME_ROOT%\BepInEx"
if "%BEPINEX_DIR%"=="" if defined BEPINEX_DIR set "BEPINEX_DIR=%BEPINEX_DIR%"
if "%BEPINEX_DIR%"=="" (
  echo [RepoAdminMenu] Could not find BepInEx in %GAME_ROOT%.
  echo [RepoAdminMenu] Install BepInEx or set BEPINEX_DIR.
  exit /b 1
)

if "%MOD_LIBS_DIR%"=="" if exist "%GAME_ROOT%\BepInEx\plugins\MenuLib.dll" set "MOD_LIBS_DIR=%GAME_ROOT%\BepInEx\plugins"
if "%MOD_LIBS_DIR%"=="" if exist "%GAME_ROOT%\BepInEx\plugins\REPOLib.dll" set "MOD_LIBS_DIR=%GAME_ROOT%\BepInEx\plugins"
if "%MOD_LIBS_DIR%"=="" if defined REPO_MOD_LIBS_DIR set "MOD_LIBS_DIR=%REPO_MOD_LIBS_DIR%"
if "%MOD_LIBS_DIR%"=="" (
  echo [RepoAdminMenu] Could not find MenuLib or REPOLib.
  echo [RepoAdminMenu] Install these dependencies or set REPO_MOD_LIBS_DIR.
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

if not exist "%SCRIPT_DIR%bin\Release" mkdir "%SCRIPT_DIR%bin\Release"

echo [RepoAdminMenu] Build completed successfully.
