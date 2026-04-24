namespace VistaWidgets.App.WidgetHost;

public sealed class WidgetWindowOptions
{
    public required Action SaveSettings { get; init; }

    public required Action OpenSettings { get; init; }

    public required Action ExitApplication { get; init; }

    public required Func<bool> GetStartWithWindows { get; init; }

    public required Action<bool> SetStartWithWindows { get; init; }
}
