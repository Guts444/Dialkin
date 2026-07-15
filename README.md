# Dialkin

**Little instruments for your desktop.**

Dialkin is a lightweight native Windows widget app, beginning with a faithful reimagining of the Windows Vista / Windows 7 CPU Meter. It uses original vector rendering and current Windows APIs rather than the retired Sidebar gadget platform or copied Microsoft assets.

> The current development version is `0.2.0`. It is technically prepared for Microsoft Store packaging, but has not been submitted or published there yet.

## Highlights

- Paired analog gauges for total CPU and physical memory usage.
- Native C#/.NET 10 WPF application—no Electron or browser runtime.
- Original chrome, faces, ticks, warning bands, needles, shadows, and icons.
- Smooth animated needles with cached static gauge artwork.
- Frameless transparent widget with multi-monitor-aware placement.
- Saved position, opacity, size, visibility, update interval, and lock state.
- Always-on-top and click-through modes.
- Tray menu, widget context menu, and accessible settings controls.
- Start-with-Windows support for both portable and MSIX installations.
- Atomic settings writes with backup and corrupt-file recovery.
- Single-instance enforcement.
- No telemetry, ads, accounts, network calls, or third-party runtime packages.

## Requirements

### Running

- Windows 10 version 2004 or newer, or Windows 11.
- x64 processor.
- No separate .NET installation is needed for the self-contained release or MSIX package.

### Building

- Windows 10/11.
- .NET SDK `10.0.301` or a compatible servicing release. `global.json` selects it automatically.
- Windows SDK with `MakeAppx.exe` and `SignTool.exe` for MSIX packaging.

Regenerating artwork additionally requires Python 3 and Pillow:

```powershell
python -m pip install Pillow
python scripts\generate-assets.py
```

## Build, test, and run

```powershell
dotnet restore
dotnet test Dialkin.sln -c Release
dotnet run --project src/Dialkin.App
```

Create a self-contained portable build:

```powershell
dotnet publish src/Dialkin.App -c Release -r win-x64 --self-contained true -o artifacts\Dialkin-0.2.0-win-x64-self-contained
```

Create the development MSIX package:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build-msix.ps1 -SignWithTestCertificate
```

See [Release and packaging](docs/releasing.md) for package identity, local testing, and Partner Center instructions.

## Settings and migration

Portable settings are stored at:

```text
%APPDATA%\Dialkin\settings.json
```

MSIX installations use the equivalent Windows-managed package data location. On first launch, Dialkin copies compatible settings from the former `%APPDATA%\VistaWidgets` directory when no Dialkin settings exist.

## Controls

Widget right-click menu:

- Lock position
- Always on top
- Click-through mode
- Reset position
- Start with Windows
- Open settings
- Exit

Tray menu:

- Show or hide CPU Meter
- Lock or unlock widgets
- Toggle click-through mode
- Start with Windows
- Settings
- Exit

## Privacy

CPU and memory values are processed locally and never transmitted. Dialkin contains no telemetry, analytics, advertising, accounts, or networking. See the full [privacy policy](PRIVACY.md).

## Documentation

- [Architecture](docs/architecture.md)
- [Adding a new widget](docs/adding-new-widget.md)
- [Release and MSIX packaging](docs/releasing.md)
- [Microsoft Store readiness](docs/store-readiness.md)
- [Changelog](CHANGELOG.md)

## License

[MIT](LICENSE) © 2026 Dialkin contributors.
