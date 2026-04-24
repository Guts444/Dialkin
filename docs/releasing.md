# Release Process

This project does not include an MSI or MSIX installer yet. For the first public release, publish the app to a folder and upload a zip archive to GitHub Releases.

## Version

Current public version: `0.1.0`

Version metadata lives in `src/VistaWidgets.App/VistaWidgets.App.csproj`.

## Verify

```powershell
dotnet build
dotnet run --project tests/VistaWidgets.Tests
```

Manual checks:

- Launch the app on Windows 11.
- Confirm the CPU Meter appears with both gauges visible.
- Confirm CPU and RAM percentages update.
- Drag the widget, exit, relaunch, and confirm the saved position.
- Test lock, always-on-top, click-through, opacity, size, update interval, and Start with Windows.
- Confirm no unexpected network activity or external assets are required.

## Build Release Artifacts

Framework-dependent build:

```powershell
dotnet publish src/VistaWidgets.App -c Release -r win-x64 --self-contained false -o artifacts\VistaWidgets-0.1.0-win-x64
Compress-Archive -Path artifacts\VistaWidgets-0.1.0-win-x64\* -DestinationPath artifacts\VistaWidgets-0.1.0-win-x64.zip -Force
```

Self-contained build:

```powershell
dotnet publish src/VistaWidgets.App -c Release -r win-x64 --self-contained true -o artifacts\VistaWidgets-0.1.0-win-x64-self-contained
Compress-Archive -Path artifacts\VistaWidgets-0.1.0-win-x64-self-contained\* -DestinationPath artifacts\VistaWidgets-0.1.0-win-x64-self-contained.zip -Force
```

## GitHub Release

1. Tag the commit as `v0.1.0`.
2. Use the local `RELEASE_NOTES.md` file as the GitHub release body.
3. Attach the zip artifact.
4. Mark the release as a first public preview.

## Future Installer Options

Good Windows installer paths for later releases:

- WiX Toolset for a traditional MSI.
- MSIX for modern Windows package deployment.
- A small bootstrapper that installs or checks the .NET Desktop Runtime for framework-dependent builds.
