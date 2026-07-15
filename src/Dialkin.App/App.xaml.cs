using Microsoft.Win32;
using Dialkin.App.Infrastructure;
using Dialkin.App.WidgetHost;

namespace Dialkin.App;

public partial class App
{
    private AppSettings? _appSettings;
    private SettingsService? _settingsService;
    private SingleInstanceService? _singleInstanceService;
    private StartupService? _startupService;
    private WidgetManager? _widgetManager;
    private TrayIconService? _trayIconService;

    protected override void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);

        _singleInstanceService = new SingleInstanceService(@"Local\Dialkin.App");
        if (!_singleInstanceService.IsPrimaryInstance)
        {
            Shutdown();
            return;
        }

        ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;

        _settingsService = new SettingsService();
        _appSettings = _settingsService.Load();
        _startupService = new StartupService();
        _appSettings.StartWithWindows = _startupService.IsEnabled();

        _widgetManager = new WidgetManager(_appSettings, _settingsService, _startupService);
        _widgetManager.Initialize();

        _trayIconService = new TrayIconService(_widgetManager);

        SystemEvents.PowerModeChanged += OnPowerModeChanged;
        SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
    }

    protected override void OnExit(System.Windows.ExitEventArgs e)
    {
        SystemEvents.PowerModeChanged -= OnPowerModeChanged;
        SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;

        _trayIconService?.Dispose();
        _widgetManager?.Shutdown();

        if (_settingsService is not null && _appSettings is not null)
        {
            _settingsService.Save(_appSettings);
        }

        _singleInstanceService?.Dispose();

        base.OnExit(e);
    }

    private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
        if (e.Mode == PowerModes.Resume)
        {
            _widgetManager?.HandleSystemResume();
        }
    }

    private void OnDisplaySettingsChanged(object? sender, EventArgs e)
    {
        _widgetManager?.HandleDisplaySettingsChanged();
    }
}
