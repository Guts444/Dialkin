using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Dialkin.App.Widgets.CpuMeter;

namespace Dialkin.App.WidgetHost;

public partial class SettingsWindow : Window
{
    private readonly WidgetManager _manager;
    private readonly DispatcherTimer _saveTimer;
    private bool _isLoading = true;

    public SettingsWindow(WidgetManager manager)
    {
        _manager = manager;
        InitializeComponent();
        _saveTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(250)
        };
        _saveTimer.Tick += (_, _) => FlushPendingSettings();
        LoadState();
    }

    private void LoadState()
    {
        _isLoading = true;

        var settings = _manager.GetWidgetSettings(CpuMeterWidget.WidgetId);
        ShowCpuMeterCheckBox.IsChecked = _manager.IsWidgetVisible(CpuMeterWidget.WidgetId);
        LockedCheckBox.IsChecked = settings.Locked;
        AlwaysOnTopCheckBox.IsChecked = settings.AlwaysOnTop;
        ClickThroughCheckBox.IsChecked = settings.ClickThrough;
        StartWithWindowsCheckBox.IsChecked = _manager.StartWithWindows;
        OpacitySlider.Value = settings.Opacity;
        ScaleSlider.Value = settings.Scale;
        UpdateValueLabels();

        foreach (ComboBoxItem item in UpdateIntervalComboBox.Items)
        {
            if (int.TryParse(item.Tag?.ToString(), out var seconds) && seconds == settings.UpdateIntervalSeconds)
            {
                UpdateIntervalComboBox.SelectedItem = item;
                break;
            }
        }

        UpdateIntervalComboBox.SelectedItem ??= UpdateIntervalComboBox.Items[0];
        _isLoading = false;
    }

    private void ShowCpuMeterCheckBox_OnChanged(object sender, RoutedEventArgs e)
    {
        if (_isLoading)
        {
            return;
        }

        if (ShowCpuMeterCheckBox.IsChecked == true)
        {
            _manager.ShowWidget(CpuMeterWidget.WidgetId);
        }
        else
        {
            _manager.HideWidget(CpuMeterWidget.WidgetId);
        }
    }

    private void WidgetSettingsCheckBox_OnChanged(object sender, RoutedEventArgs e)
    {
        if (_isLoading)
        {
            return;
        }

        _manager.UpdateWidgetSettings(
            CpuMeterWidget.WidgetId,
            settings =>
            {
                settings.Locked = LockedCheckBox.IsChecked == true;
                settings.AlwaysOnTop = AlwaysOnTopCheckBox.IsChecked == true;
                settings.ClickThrough = ClickThroughCheckBox.IsChecked == true;
            });
    }

    private void StartWithWindowsCheckBox_OnChanged(object sender, RoutedEventArgs e)
    {
        if (_isLoading)
        {
            return;
        }

        var requested = StartWithWindowsCheckBox.IsChecked == true;
        _manager.StartWithWindows = requested;

        _isLoading = true;
        StartWithWindowsCheckBox.IsChecked = _manager.StartWithWindows;
        _isLoading = false;

        if (_manager.StartWithWindows != requested)
        {
            System.Windows.MessageBox.Show(
                this,
                "Windows did not apply the startup setting. If startup was disabled in Windows, re-enable Dialkin under Settings > Apps > Startup.",
                "Startup setting not changed",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }

    private void OpacitySlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isLoading)
        {
            UpdateValueLabels();
            return;
        }

        _manager.UpdateWidgetSettings(CpuMeterWidget.WidgetId, settings => settings.Opacity = e.NewValue, saveSettings: false);
        ScheduleSettingsSave();
        UpdateValueLabels();
    }

    private void ScaleSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isLoading)
        {
            UpdateValueLabels();
            return;
        }

        _manager.UpdateWidgetSettings(CpuMeterWidget.WidgetId, settings => settings.Scale = e.NewValue, saveSettings: false);
        ScheduleSettingsSave();
        UpdateValueLabels();
    }

    private void UpdateIntervalComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isLoading || UpdateIntervalComboBox.SelectedItem is not ComboBoxItem item)
        {
            return;
        }

        if (int.TryParse(item.Tag?.ToString(), out var seconds))
        {
            _manager.UpdateWidgetSettings(CpuMeterWidget.WidgetId, settings => settings.UpdateIntervalSeconds = seconds);
        }
    }

    private void ResetPositionButton_OnClick(object sender, RoutedEventArgs e)
    {
        _manager.ResetWidgetPosition(CpuMeterWidget.WidgetId);
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        FlushPendingSettings();
        base.OnClosed(e);
    }

    private void ScheduleSettingsSave()
    {
        _saveTimer.Stop();
        _saveTimer.Start();
    }

    private void FlushPendingSettings()
    {
        if (!_saveTimer.IsEnabled)
        {
            return;
        }

        _saveTimer.Stop();
        _manager.SaveSettings();
    }

    private void UpdateValueLabels()
    {
        if (OpacityValueText is not null && OpacitySlider is not null)
        {
            OpacityValueText.Text = $"{OpacitySlider.Value:P0}";
        }

        if (ScaleValueText is not null && ScaleSlider is not null)
        {
            ScaleValueText.Text = $"{ScaleSlider.Value:P0}";
        }
    }
}
