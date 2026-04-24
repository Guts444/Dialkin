# Vista Widgets

Native Windows 11 recreation of the Windows Vista / Windows 7 CPU Meter gadget, built with C#/.NET 8 and WPF. It does not use the legacy `.gadget` platform, Internet Explorer hosting, ActiveX, or copied Microsoft assets.

## Current Widget

- Vista-style CPU Meter with two analog gauges.
- CPU gauge shows total system CPU utilization.
- RAM gauge shows physical memory usage.
- Values update once per second by default.
- Needles animate smoothly between samples.
- Widget is frameless, transparent, draggable, high-DPI aware, and saved/restored between launches.
- Size and opacity can be adjusted from Settings.
- Tray icon and right-click menus are included.

## Build And Run

```powershell
dotnet build
dotnet run --project src/VistaWidgets.App
```

Run the lightweight test console:

```powershell
dotnet run --project tests/VistaWidgets.Tests
```

Settings are stored at:

```text
%APPDATA%\VistaWidgets\settings.json
```

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

## Dependencies And Licenses

The app uses only .NET 8, WPF, Windows Forms tray support, and Windows native APIs. There are no third-party runtime packages, no telemetry, no ads, and no network calls.

.NET is distributed by Microsoft under the MIT license. WPF and Windows Forms are part of the Windows Desktop runtime.

## Research

See [docs/research-cpu-meter.md](docs/research-cpu-meter.md) for notes on the original CPU Meter behavior and the rationale for avoiding the legacy gadget platform.
