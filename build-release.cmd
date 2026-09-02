@echo off
cd /d "%~dp0"
echo ========================================
echo   PvZWSTools One-Click Build + Release
echo ========================================
echo.
echo   1) Build only (no upload)
echo   2) Build and upload to GitHub Release
echo   3) Build only (skip Android + Setup, quick test)
echo.
set /p choice=Select [1-3] (default 1): 

if "%choice%"=="2" (
    powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0build-release.ps1" -Upload
) else if "%choice%"=="3" (
    powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0build-release.ps1" -SkipAndroid -SkipSetup
) else (
    powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0build-release.ps1"
)

echo.
pause
