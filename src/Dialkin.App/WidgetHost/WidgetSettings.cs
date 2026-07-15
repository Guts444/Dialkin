namespace Dialkin.App.WidgetHost;

public sealed class AppSettings
{
    public bool StartWithWindows { get; set; }

    public bool AllWidgetsLocked { get; set; }

    public Dictionary<string, WidgetSettings> Widgets { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public static AppSettings CreateDefault()
    {
        return new AppSettings();
    }

    public AppSettings SynchronizeLockState()
    {
        AllWidgetsLocked = Widgets.Count > 0 && Widgets.Values.All(settings => settings.Locked);
        return this;
    }

    public AppSettings Normalize()
    {
        Widgets ??= new Dictionary<string, WidgetSettings>(StringComparer.OrdinalIgnoreCase);

        var normalized = new Dictionary<string, WidgetSettings>(Widgets, StringComparer.OrdinalIgnoreCase);
        Widgets = normalized;

        foreach (var widgetId in Widgets.Keys.ToArray())
        {
            Widgets[widgetId] = (Widgets[widgetId] ?? WidgetSettings.CreateDefault()).Normalize();
        }

        return this;
    }
}

public sealed class WidgetSettings
{
    public bool IsVisible { get; set; } = true;

    public double? Left { get; set; }

    public double? Top { get; set; }

    public bool Locked { get; set; }

    public double Opacity { get; set; } = 1.0;

    public double Scale { get; set; } = 1.15;

    public bool AlwaysOnTop { get; set; }

    public bool ClickThrough { get; set; }

    public bool StartWithWindows { get; set; }

    public int UpdateIntervalSeconds { get; set; } = 1;

    public string ThemeVariant { get; set; } = "VistaDark";

    public static WidgetSettings CreateDefault() => new();

    public WidgetSettings Normalize()
    {
        Opacity = Math.Clamp(double.IsFinite(Opacity) ? Opacity : 1.0, 0.35, 1.0);
        Scale = Math.Clamp(double.IsFinite(Scale) && Scale > 0 ? Scale : 1.15, 0.8, 1.8);
        UpdateIntervalSeconds = Math.Clamp(UpdateIntervalSeconds, 1, 60);
        ThemeVariant = string.IsNullOrWhiteSpace(ThemeVariant) ? "VistaDark" : ThemeVariant;
        return this;
    }
}
