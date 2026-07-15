# Changelog

All notable public changes to this project are documented here. The project follows semantic versioning where practical.

## [0.2.0] - Unreleased

### Added

- New **Dialkin** product identity, original twin-gauge icon, and Store/package artwork.
- Repeatable self-contained x64 MSIX packaging with packaged startup-task support.
- A real xUnit test suite and pinned Windows CI workflow.
- Single-instance enforcement.
- Per-monitor work-area placement and settings migration from the former Vista Widgets directory.
- Accessibility names and current-value help text for gauges and settings controls.
- Privacy policy and Microsoft Store readiness documentation.

### Changed

- Migrated from .NET 8 to .NET 10 LTS.
- Replaced direct settings overwrites with atomic writes, backups, and corrupt-file recovery.
- Derived global lock state consistently from per-widget lock settings.
- Debounced settings writes caused by slider movement.
- Reworked dragging to use native WPF movement and reliably save the final position.
- Cached static gauge artwork and shortened needle animation, reducing measured rendering CPU use by roughly one third on the development machine.
- Renamed solution, projects, assemblies, namespaces, application-data directory, and repository branding to Dialkin.

### Fixed

- Alt+F4 now routes through widget hiding so sampling stops and visibility is persisted.
- Lost drag capture no longer drops the final saved widget position.
- A second application launch no longer creates duplicate widgets and tray icons.
- Startup registration now distinguishes packaged and unpackaged execution.

## [0.1.0] - 2026-04-24

Released under the former **Vista Widgets** name.

### Added

- First public preview.
- Native Windows 11 WPF app shell for desktop widgets.
- Vista/Windows 7-inspired CPU Meter widget with paired CPU and RAM gauges.
- Original vector-rendered chrome bezels, gauge faces, tick marks, warning color bands, red needles, shadows, and percent readouts.
- CPU usage sampling through Windows `GetSystemTimes`.
- RAM usage sampling through Windows `GlobalMemoryStatusEx`.
- Smooth needle animation between metric samples.
- Frameless transparent widget window with saved position.
- Widget options for opacity, size, lock position, always on top, click-through mode, update interval, and Start with Windows.
- Tray icon, tray menu, widget right-click menu, and settings window.

### Notes

- This release used portable `dotnet publish` output and did not include an installer.
- The app did not use the retired Windows Sidebar platform, copied Microsoft assets, telemetry, ads, or network calls.
