using System.Windows;
using Size = System.Windows.Size;

namespace VistaWidgets.App.WidgetHost;

public interface IWidget
{
    string Id { get; }

    string DisplayName { get; }

    Size DefaultSize { get; }

    FrameworkElement CreateView();

    void Start();

    void Stop();
}

public interface IWidgetSystemEvents
{
    void OnSystemResume();

    void OnDisplaySettingsChanged();
}
