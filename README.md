# WinFfmpeg

FFmpeg converter in Windows "Open with" menu.

Right-click any media file → Open with → WinFfmpeg → choose format → done.

## Build

```
dotnet publish -c Release -r win-x64 --self-contained false -o release
```

## Install

Double-click `release\install.bat`. It will:
1. Copy WinFfmpeg to Program Files
2. Download FFmpeg automatically
3. Register in "Open with" menu

## Uninstall

Double-click `release\uninstall.bat`.

## Usage

Right-click any video/audio/image → **Open with** → **WinFfmpeg**

## Structure

```
src/WinFfmpeg/
├── .gitignore
├── README.md
├── WinFfmpeg.csproj
├── Program.cs
├── release/
│   ├── WinFfmpeg.exe
│   ├── install.bat
│   └── uninstall.bat
├── Forms/
│   ├── MainForm.cs
│   └── MainForm.Designer.cs
├── Models/
│   ├── FormatCategory.cs
│   ├── FormatInfo.cs
│   └── ConversionTask.cs
└── Services/
    ├── FormatRegistry.cs
    ├── FileTypeDetector.cs
    ├── FfmpegBuilder.cs
    ├── FfmpegRunner.cs
    └── ShellRegistrar.cs
```
