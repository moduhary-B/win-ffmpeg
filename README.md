# WinFfmpeg

**FFmpeg converter right in the Windows context menu.**

No more online converters. No command line. Right-click any media file → Open with → WinFfmpeg → pick a format → done.

---

### How it works

1. Right-click a video, audio, or image file
2. Select **Open with** → **WinFfmpeg**
3. Choose the output format (popular formats shown first, search available)
4. Optionally add custom ffmpeg parameters
5. Click **Convert**

The app generates and runs the ffmpeg command for you. Advanced users can tweak any ffmpeg option — nothing is hidden.

---

### Install

1. [Build](#build) or download the release
2. Double-click **`install.bat`** — it will:
   - Copy WinFfmpeg to Program Files
   - Download FFmpeg automatically (if not already present)
   - Register in the "Open with" menu

### Uninstall

Double-click **`uninstall.bat`**

---

### Build

Requires [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0):

```
dotnet publish -c Release -r win-x64 --self-contained false -o release
```

---

### Features

- **32 formats** — video (MP4, AVI, MKV, MOV, WEBM...), audio (MP3, WAV, FLAC, OGG...), image (GIF, PNG, JPG, WEBP...)
- **Smart categories** — popular formats shown first, cross-category conversion available (e.g. video → audio)
- **Search** — find any format instantly
- **Custom parameters** — full ffmpeg control for advanced users
- **Live command preview** — see the exact ffmpeg command before running
- **Progress bar** — real-time conversion progress

---

### Project structure

```
├── WinFfmpeg.csproj
├── Program.cs                Entry point, /register /unregister
├── Forms/
│   ├── MainForm.cs           UI: format picker, search, preview, progress
│   └── MainForm.Designer.cs
├── Models/
│   ├── FormatCategory.cs     Video / Audio / Image enum
│   ├── FormatInfo.cs         Format + default codecs + params
│   └── ConversionTask.cs     Task + output path generation
├── Services/
│   ├── FormatRegistry.cs     Format definitions and presets
│   ├── FileTypeDetector.cs   Detect file category by extension
│   ├── FfmpegBuilder.cs      Build ffmpeg arguments
│   ├── FfmpegRunner.cs       Run ffmpeg, parse progress
│   └── ShellRegistrar.cs     Register in "Open with" menu
└── release/                  Built output
    ├── WinFfmpeg.exe
    ├── install.bat
    └── uninstall.bat
```
