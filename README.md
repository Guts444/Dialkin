# Vista Widgets

Vista Widgets is a native Windows 11 recreation of the Windows Vista / Windows 7 desktop gadget experience, starting with a faithful CPU Meter widget.

The project is built with C#/.NET 8 and WPF. It does not use the retired `.gadget` platform, Internet Explorer hosting, ActiveX, Sidebar APIs, or copied Microsoft assets.

## Current Release

Version: `0.1.0`

The first public release focuses on the classic CPU Meter:

- Vista/Windows 7-inspired paired CPU and RAM analog gauges.
- Freshly drawn chrome bezels, gauge faces, ticks, warning color bands, needles, shadows, and percent readouts.
- CPU gauge for total system CPU usage.
- RAM gauge for physical memory usage.
- Smooth needle animation between samples.
- Frameless transparent desktop widget.
- Dragging, saved position, opacity, size, always-on-top, lock, and click-through options.
- Tray icon, tray menu, widget context menu, and settings window.
- Settings saved in `%APPDATA%\VistaWidgets\settings.json`.
- No telemetry, ads, network calls, or third-party runtime packages.

## Requirements

- Windows 10/11.
- .NET 8 SDK for building from source.
- .NET 8 Desktop Runtime if running a framework-dependent build.

## Build And Run

```powershell
dotnet build
dotnet run --project src/VistaWidgets.App
```

Run the lightweight test console:

```powershell
dotnet run --project tests/VistaWidgets.Tests
```

## Share Locally

There is no MSI installer yet. The easiest local sharing path for `v0.1.0` is to publish a Windows build and zip the output folder.

Framework-dependent build, smaller download, requires .NET 8 Desktop Runtime on the target PC:

```powershell
dotnet publish src/VistaWidgets.App -c Release -r win-x64 --self-contained false -o artifacts\VistaWidgets-0.1.0-win-x64
Compress-Archive -Path artifacts\VistaWidgets-0.1.0-win-x64\* -DestinationPath artifacts\VistaWidgets-0.1.0-win-x64.zip -Force
```

Self-contained build, larger download, does not require users to install .NET first:

```powershell
dotnet publish src/VistaWidgets.App -c Release -r win-x64 --self-contained true -o artifacts\VistaWidgets-0.1.0-win-x64-self-contained
Compress-Archive -Path artifacts\VistaWidgets-0.1.0-win-x64-self-contained\* -DestinationPath artifacts\VistaWidgets-0.1.0-win-x64-self-contained.zip -Force
```

Users can unzip the folder and run `VistaWidgets.App.exe`.

An MSI/MSIX installer is not included yet. A future installer can be added with WiX Toolset, MSIX packaging, or another Windows installer pipeline.

## Menus

Widget right-click menu:

- Lock position
- Always on top
- Click-through mode
- Reset position
- Start with Windows
- Open settings
- Exit

Tray menu:

- Show/Hide CPU Meter
- Lock/Unlock widgets
- Click-through CPU Meter
- Start with Windows
- Settings
- Exit

## Manual Test Checklist

- Launch on Windows 11.
- Drag the widget, exit, relaunch, and confirm position is restored.
- Lock position and confirm dragging stops.
- Enable/disable always-on-top.
- Enable/disable click-through from the tray after testing it from the widget menu.
- Compare CPU and RAM values roughly against Task Manager.
- Test at 125%, 150%, and 200% display scaling.
- Move across monitors and confirm position remains valid.
- Sleep/resume the PC and confirm CPU sampling recovers.

## Project Notes

- [Architecture](docs/architecture.md)
- [CPU Meter research](docs/research-cpu-meter.md)
- [Adding a new widget](docs/adding-new-widget.md)
- [Release process](docs/releasing.md)
- [Changelog](CHANGELOG.md)

## License

MIT License

Copyright (c) 2026 Vista Widgets contributors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
