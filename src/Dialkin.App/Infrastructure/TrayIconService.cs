using Drawing = System.Drawing;
using Forms = System.Windows.Forms;
using Dialkin.App.WidgetHost;
using Dialkin.App.Widgets.CpuMeter;

namespace Dialkin.App.Infrastructure;

public sealed class TrayIconService : IDisposable
{
    private readonly WidgetManager _widgetManager;
    private readonly Forms.NotifyIcon _notifyIcon;
    private readonly Drawing.Icon? _ownedIcon;
    private readonly Forms.ContextMenuStrip _menu;
    private readonly Forms.ToolStripMenuItem _toggleCpuMeterItem;
    private readonly Forms.ToolStripMenuItem _lockWidgetsItem;
    private readonly Forms.ToolStripMenuItem _clickThroughItem;
    private readonly Forms.ToolStripMenuItem _startWithWindowsItem;

    public TrayIconService(WidgetManager widgetManager)
    {
        _widgetManager = widgetManager;
        _menu = new Forms.ContextMenuStrip();

        _toggleCpuMeterItem = new Forms.ToolStripMenuItem("Hide CPU Meter");
        _toggleCpuMeterItem.Click += (_, _) => _widgetManager.ToggleWidgetVisibility(CpuMeterWidget.WidgetId);

        _lockWidgetsItem = new Forms.ToolStripMenuItem("Lock widgets") { CheckOnClick = true };
        _lockWidgetsItem.Click += (_, _) => _widgetManager.SetAllLocked(_lockWidgetsItem.Checked);

        _clickThroughItem = new Forms.ToolStripMenuItem("Click-through CPU Meter") { CheckOnClick = true };
        _clickThroughItem.Click += (_, _) =>
            _widgetManager.UpdateWidgetSettings(CpuMeterWidget.WidgetId, settings => settings.ClickThrough = _clickThroughItem.Checked);

        _startWithWindowsItem = new Forms.ToolStripMenuItem("Start with Windows") { CheckOnClick = true };
        _startWithWindowsItem.Click += (_, _) =>
        {
            _widgetManager.StartWithWindows = _startWithWindowsItem.Checked;
            _startWithWindowsItem.Checked = _widgetManager.StartWithWindows;
        };

        var settingsItem = new Forms.ToolStripMenuItem("Settings");
        settingsItem.Click += (_, _) => _widgetManager.OpenSettingsWindow();

        var exitItem = new Forms.ToolStripMenuItem("Exit");
        exitItem.Click += (_, _) => _widgetManager.RequestExit();

        _menu.Items.AddRange(
        [
            _toggleCpuMeterItem,
            _lockWidgetsItem,
            _clickThroughItem,
            _startWithWindowsItem,
            new Forms.ToolStripSeparator(),
            settingsItem,
            new Forms.ToolStripSeparator(),
            exitItem
        ]);
        _menu.Opening += (_, _) => RefreshMenuState();

        if (!string.IsNullOrWhiteSpace(Environment.ProcessPath))
        {
            _ownedIcon = Drawing.Icon.ExtractAssociatedIcon(Environment.ProcessPath);
        }

        _notifyIcon = new Forms.NotifyIcon
        {
            Text = "Dialkin",
            Icon = _ownedIcon ?? Drawing.SystemIcons.Application,
            Visible = true,
            ContextMenuStrip = _menu
        };
        _notifyIcon.DoubleClick += (_, _) => _widgetManager.ToggleWidgetVisibility(CpuMeterWidget.WidgetId);

        RefreshMenuState();
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _ownedIcon?.Dispose();
        _menu.Dispose();
    }

    private void RefreshMenuState()
    {
        var visible = _widgetManager.IsWidgetVisible(CpuMeterWidget.WidgetId);
        var settings = _widgetManager.GetWidgetSettings(CpuMeterWidget.WidgetId);

        _toggleCpuMeterItem.Text = visible ? "Hide CPU Meter" : "Show CPU Meter";
        _lockWidgetsItem.Checked = _widgetManager.AllWidgetsLocked;
        _clickThroughItem.Checked = settings.ClickThrough;
        _startWithWindowsItem.Checked = _widgetManager.StartWithWindows;
    }
}
