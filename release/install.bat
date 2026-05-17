@echo off
net session >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo Requesting admin rights...
    powershell -Command "Start-Process '%~f0' -Verb RunAs"
    exit /b
)

chcp 65001 >nul 2>&1
echo ============================================
echo   WinFfmpeg Installer
echo ============================================
echo.

set "INSTALL=%ProgramFiles%\WinFfmpeg"

echo [1/4] Installing to %INSTALL%...
if not exist "%INSTALL%" mkdir "%INSTALL%"
copy /y "%~dp0WinFfmpeg.exe" "%INSTALL%\WinFfmpeg.exe" >nul
if %ERRORLEVEL% neq 0 (
    echo ERROR: Failed to copy WinFfmpeg.exe
    pause
    exit /b 1
)
echo   Done.

echo.
echo [2/4] Checking FFmpeg...
if exist "%INSTALL%\ffmpeg.exe" (
    echo   Already installed, skipping.
) else (
    echo   Downloading FFmpeg...
    powershell -NoProfile -Command ^
        "Write-Host '  Downloading...';" ^
        "$ProgressPreference='SilentlyContinue';" ^
        "Invoke-WebRequest -Uri 'https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip' -OutFile \"$env:TEMP\ffmpeg.zip\";" ^
        "Write-Host '  Extracting...';" ^
        "Expand-Archive -Path \"$env:TEMP\ffmpeg.zip\" -DestinationPath \"$env:TEMP\ffmpeg_ext\" -Force;" ^
        "$bin=(Get-ChildItem \"$env:TEMP\ffmpeg_ext\ffmpeg-*\bin\ffmpeg.exe\").FullName;" ^
        "Copy-Item $bin '%INSTALL%\ffmpeg.exe';" ^
        "$probe=(Get-ChildItem \"$env:TEMP\ffmpeg_ext\ffmpeg-*\bin\ffprobe.exe\").FullName;" ^
        "Copy-Item $probe '%INSTALL%\ffprobe.exe';" ^
        "Remove-Item \"$env:TEMP\ffmpeg.zip\"; Remove-Item \"$env:TEMP\ffmpeg_ext\" -Recurse;"
    if exist "%INSTALL%\ffmpeg.exe" (
        echo   Done.
    ) else (
        echo   WARNING: Download failed. Place ffmpeg.exe in %INSTALL% manually.
    )
)

echo.
echo [3/4] Registering in "Open with" menu...
"%INSTALL%\WinFfmpeg.exe" /register

echo.
echo [4/4] Done!
echo.
echo   Right-click any media file - Open with - WinFfmpeg
echo.
pause
