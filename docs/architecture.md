# Architecture

Vista Widgets is a Windows-only .NET 8 WPF app. It recreates Vista-style desktop gadgets without using the legacy Windows Gadget Platform.

## Layers

- `Infrastructure`: app paths, JSON settings, startup registration, DPI helpers, tray icon, and native extended window styles.
- `WidgetHost`: generic widget contracts, widget settings, `WidgetManager`, frameless `WidgetWindow`, and settings UI.
- `Widgets/CpuMeter`: the first widget implementation, including the view model, providers, reusable gauge control, and WPF view.

## Widget Contract

Widgets implement `IWidget`:

- `Id`
- `DisplayName`
- `DefaultSize`
- `CreateView()`
- `Start()`
- `Stop()`

Widgets can optionally implement `IWidgetSystemEvents` to react to sleep/resume or monitor/DPI changes.

## Settings

Settings are saved as JSON at:

`%APPDATA%\VistaWidgets\settings.json`

Persisted values include position, lock state, opacity, always-on-top, click-through mode, startup preference, update interval, and theme variant.

## CPU Meter Data

CPU usage is sampled with the Windows `GetSystemTimes` API. The provider stores a previous sample and computes total system busy time from kernel/user/idle deltas. The first sample establishes a baseline and returns 0%.

RAM usage is sampled with `GlobalMemoryStatusEx` and calculated from physical total and available memory. Both values are clamped to `0..100`.

## Window Behavior

`WidgetWindow` is transparent, frameless, hidden from the taskbar, and marked as a tool window to avoid normal Alt+Tab behavior. Click-through mode uses `WS_EX_TRANSPARENT`.

The app does not use WorkerW or fragile Explorer-parenting tricks for the MVP. A future desktop-layer mode should only be added if it can be done reliably without depending on undocumented Explorer internals.
