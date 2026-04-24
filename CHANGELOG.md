# Changelog

All notable public changes to this project will be documented in this file.

This project follows semantic versioning where practical.

## [0.1.0] - 2026-04-24

### Added

- First public preview of Vista Widgets.
- Native Windows 11 WPF app shell for desktop widgets.
- Vista/Windows 7-inspired CPU Meter widget with paired CPU and RAM gauges.
- Fresh vector-rendered chrome bezels, gauge faces, tick marks, warning color bands, red needles, shadows, and percent readouts.
- CPU usage sampling through Windows `GetSystemTimes`.
- RAM usage sampling through Windows `GlobalMemoryStatusEx`.
- Smooth needle animation between metric samples.
- Frameless transparent widget window with saved position.
- Widget options for opacity, size, lock position, always on top, click-through mode, update interval, and Start with Windows.
- Tray icon, tray menu, widget right-click menu, and settings window.
- JSON settings stored in `%APPDATA%\VistaWidgets\settings.json`.
- Lightweight console test project for core calculations and settings behavior.

### Notes

- There is no MSI/MSIX installer in this release. Use `dotnet publish` and zip the output folder for local sharing.
- The app does not use the retired Windows Sidebar gadget platform, copied Microsoft assets, telemetry, ads, or network calls.
