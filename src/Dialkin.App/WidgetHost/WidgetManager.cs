using System.Windows;
using Dialkin.App.Infrastructure;
using Dialkin.App.Widgets.CpuMeter;

namespace Dialkin.App.WidgetHost;

public sealed class WidgetManager
{
    private readonly AppSettings _settings;
    private readonly SettingsService _settingsService;
    private readonly StartupService _startupService;
    private readonly Dictionary<string, IWidget> _widgets = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, WidgetWindow> _windows = new(StringComparer.OrdinalIgnoreCase);

    private SettingsWindow? _settingsWindow;
    private bool _isShuttingDown;

    public WidgetManager(AppSettings settings, SettingsService settingsService, StartupService startupService)
    {
        _settings = settings.Normalize();
        _settingsService = settingsService;
        _startupService = startupService;
    }

    public bool StartWithWindows
    {
        get => _settings.StartWithWindows;
        set
        {
            _startupService.SetEnabled(value);
            _settings.StartWithWindows = _startupService.IsEnabled();

            foreach (var widgetSettings in _settings.Widgets.Values)
            {
                widgetSettings.StartWithWindows = _settings.StartWithWindows;
            }

            SaveSettings();
        }
    }

    public bool AllWidgetsLocked => _settings.AllWidgetsLocked;

    public void Initialize()
    {
        var cpuSettings = EnsureWidgetSettings(CpuMeterWidget.WidgetId);
        Register(new CpuMeterWidget(cpuSettings));

        if (cpuSettings.IsVisible)
        {
            ShowWidget(CpuMeterWidget.WidgetId);
        }

        SaveSettings();
    }

    public void Register(IWidget widget)
    {
        _widgets[widget.Id] = widget;
    }

    public void ShowWidget(string widgetId)
    {
        if (!_widgets.TryGetValue(widgetId, out var widget))
        {
            return;
        }

        var settings = EnsureWidgetSettings(widgetId);
        settings.IsVisible = true;

        if (!_windows.TryGetValue(widgetId, out var window))
        {
            window = CreateWindow(widget, settings);
            _windows[widgetId] = window;
        }

        window.ApplySettings();
        window.Show();
        widget.Start();
        SaveSettings();
    }

    public void HideWidget(string widgetId)
    {
        if (_windows.TryGetValue(widgetId, out var window))
        {
            window.Hide();
        }

        if (_widgets.TryGetValue(widgetId, out var widget))
        {
            widget.Stop();
        }

        EnsureWidgetSettings(widgetId).IsVisible = false;
        SaveSettings();
    }

    public void ToggleWidgetVisibility(string widgetId)
    {
        if (IsWidgetVisible(widgetId))
        {
            HideWidget(widgetId);
        }
        else
        {
            ShowWidget(widgetId);
        }
    }

    public bool IsWidgetVisible(string widgetId)
    {
        return EnsureWidgetSettings(widgetId).IsVisible &&
               _windows.TryGetValue(widgetId, out var window) &&
               window.IsVisible;
    }

    public WidgetSettings GetWidgetSettings(string widgetId)
    {
        return EnsureWidgetSettings(widgetId);
    }

    public void UpdateWidgetSettings(string widgetId, Action<WidgetSettings> update, bool saveSettings = true)
    {
        var settings = EnsureWidgetSettings(widgetId);
        update(settings);
        settings.Normalize();
        _settings.SynchronizeLockState();

        if (_windows.TryGetValue(widgetId, out var window))
        {
            window.ApplySettings();
        }

        if (_widgets.TryGetValue(widgetId, out var widget) && widget is IWidgetSystemEvents awareWidget)
        {
            awareWidget.OnDisplaySettingsChanged();
        }

        if (saveSettings)
        {
            SaveSettings();
        }
    }

    public void SetAllLocked(bool locked)
    {
        _settings.AllWidgetsLocked = locked;

        foreach (var widgetId in _widgets.Keys)
        {
            EnsureWidgetSettings(widgetId).Locked = locked;
        }

        foreach (var window in _windows.Values)
        {
            window.Locked = locked;
        }

        SaveSettings();
    }

    public void ResetWidgetPosition(string widgetId)
    {
        if (_windows.TryGetValue(widgetId, out var window))
        {
            window.ResetPosition();
        }
        else
        {
            var settings = EnsureWidgetSettings(widgetId);
            settings.Left = null;
            settings.Top = null;
        }

        SaveSettings();
    }

    public void OpenSettingsWindow()
    {
        if (_settingsWindow is { IsVisible: true })
        {
            _settingsWindow.Activate();
            return;
        }

        _settingsWindow = new SettingsWindow(this);
        _settingsWindow.Closed += (_, _) => _settingsWindow = null;
        _settingsWindow.Show();
    }

    public void HandleSystemResume()
    {
        foreach (var widget in _widgets.Values.OfType<IWidgetSystemEvents>())
        {
            widget.OnSystemResume();
        }

        EnsureWindowsAreVisibleOnAnyMonitor();
    }

    public void HandleDisplaySettingsChanged()
    {
        foreach (var widget in _widgets.Values.OfType<IWidgetSystemEvents>())
        {
            widget.OnDisplaySettingsChanged();
        }

        EnsureWindowsAreVisibleOnAnyMonitor();
    }

    public void RequestExit()
    {
        if (_isShuttingDown)
        {
            return;
        }

        SaveSettings();
        System.Windows.Application.Current.Shutdown();
    }

    public void Shutdown()
    {
        if (_isShuttingDown)
        {
            return;
        }

        _isShuttingDown = true;

        foreach (var widget in _widgets.Values)
        {
            widget.Stop();
        }

        foreach (var window in _windows.Values.ToArray())
        {
            window.CloseForShutdown();
        }

        _settingsWindow?.Close();
        SaveSettings();
    }

    public void SaveSettings()
    {
        _settings.SynchronizeLockState();
        _settingsService.Save(_settings);
    }

    private WidgetWindow CreateWindow(IWidget widget, WidgetSettings settings)
    {
        var window = new WidgetWindow(
            widget,
            settings,
            new WidgetWindowOptions
            {
                SaveSettings = SaveSettings,
                HideWidget = () => HideWidget(widget.Id),
                OpenSettings = OpenSettingsWindow,
                ExitApplication = RequestExit,
                GetStartWithWindows = () => StartWithWindows,
                SetStartWithWindows = value => StartWithWindows = value
            });

        return window;
    }

    private WidgetSettings EnsureWidgetSettings(string widgetId)
    {
        if (!_settings.Widgets.TryGetValue(widgetId, out var settings))
        {
            settings = WidgetSettings.CreateDefault();
            settings.Locked = _settings.AllWidgetsLocked;
            settings.StartWithWindows = _settings.StartWithWindows;
            _settings.Widgets[widgetId] = settings;
        }

        return settings.Normalize();
    }

    private void EnsureWindowsAreVisibleOnAnyMonitor()
    {
        foreach (var window in _windows.Values)
        {
            window.EnsureVisibleOnAnyMonitor();
        }
    }
}
