using System.Windows;
using VistaWidgets.App.WidgetHost;
using Size = System.Windows.Size;
using UserControl = System.Windows.Controls.UserControl;

namespace VistaWidgets.App.Widgets.CpuMeter;

public partial class CpuMeterWidget : UserControl, IWidget, IWidgetSystemEvents
{
    public const string WidgetId = "cpu-meter";

    private readonly WidgetSettings _settings;
    private readonly CpuMeterViewModel _viewModel;

    public CpuMeterWidget(WidgetSettings settings)
    {
        _settings = settings;
        _viewModel = new CpuMeterViewModel(settings);
        InitializeComponent();
        DataContext = _viewModel;
    }

    public string Id => WidgetId;

    public string DisplayName => "CPU Meter";

    public Size DefaultSize => new(184, 152);

    public FrameworkElement CreateView()
    {
        return this;
    }

    public void Start()
    {
        _viewModel.ApplySettings();
        _viewModel.Start();
    }

    public void Stop()
    {
        _viewModel.Stop();
    }

    public void OnSystemResume()
    {
        _viewModel.ResetCpuBaseline();
    }

    public void OnDisplaySettingsChanged()
    {
        _viewModel.ApplySettings();
    }
}
