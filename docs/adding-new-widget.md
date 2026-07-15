# Adding a New Widget

1. Create a folder under `src/Dialkin.App/Widgets/<WidgetName>`.
2. Add a WPF view and view model for the widget.
3. Implement `IWidget`.
4. Use `WidgetSettings` for persisted state instead of writing widget-specific files unless the widget needs extra data.
5. Register the widget in `WidgetManager.Initialize()`.
6. Add tray/settings UI entries only for actions users need globally.

Minimal widget shape:

```csharp
public sealed class ExampleWidget : IWidget
{
    public string Id => "example";
    public string DisplayName => "Example";
    public Size DefaultSize => new(170, 110);

    public FrameworkElement CreateView() => new ExampleWidgetView();
    public void Start() { }
    public void Stop() { }
}
```

Keep each widget focused. CPU, RAM, GPU, network, weather, stocks, currency, feeds, and clocks should be separate widgets rather than one crowded panel.
