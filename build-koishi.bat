@echo off
setlocal
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0tools\build-koishi.ps1"
set "EXITCODE=%ERRORLEVEL%"
echo.
if "%EXITCODE%"=="0" (
  echo Build completed successfully.
) else (
  echo Build failed with exit code %EXITCODE%.
)
echo.
pause
exit /b %EXITCODE%
