@echo off
net session >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo Requesting admin rights...
    powershell -Command "Start-Process '%~f0' -Verb RunAs"
    exit /b
)

chcp 65001 >nul 2>&1
echo ============================================
echo   WinFfmpeg Uninstaller
echo ============================================
echo.

set "INSTALL=%ProgramFiles%\WinFfmpeg"

echo Unregistering...
"%INSTALL%\WinFfmpeg.exe" /unregister

echo.
echo Removing files...
del /q "%INSTALL%\WinFfmpeg.exe" >nul 2>&1
del /q "%INSTALL%\ffmpeg.exe" >nul 2>&1
del /q "%INSTALL%\ffprobe.exe" >nul 2>&1
rd /q "%INSTALL%" >nul 2>&1

echo.
echo Done. WinFfmpeg removed.
echo.
pause
