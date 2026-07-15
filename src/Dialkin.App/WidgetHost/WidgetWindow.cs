using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Dialkin.App.Infrastructure;
using MenuItem = System.Windows.Controls.MenuItem;
using Separator = System.Windows.Controls.Separator;

namespace Dialkin.App.WidgetHost;

public sealed class WidgetWindow : Window
{
    private readonly IWidget _widget;
    private readonly WidgetSettings _settings;
    private readonly WidgetWindowOptions _options;
    private readonly Viewbox _scaledContent;
    private nint _hwnd;
    private bool _allowClose;
    private bool _isDragging;
    private bool _isApplyingBounds;

    public WidgetWindow(IWidget widget, WidgetSettings settings, WidgetWindowOptions options)
    {
        _widget = widget;
        _settings = settings;
        _options = options;

        _scaledContent = new Viewbox
        {
            Stretch = Stretch.Uniform,
            StretchDirection = StretchDirection.Both,
            Child = widget.CreateView()
        };

        SetScaledSize();
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = System.Windows.Media.Brushes.Transparent;
        ResizeMode = ResizeMode.NoResize;
        ShowInTaskbar = false;
        SizeToContent = SizeToContent.Manual;
        UseLayoutRounding = true;
        SnapsToDevicePixels = true;
        Title = widget.DisplayName;
        Content = _scaledContent;

        ContextMenu = BuildContextMenu();
        ContextMenuOpening += (_, _) => ContextMenu = BuildContextMenu();

        Loaded += (_, _) => RestorePosition();
        LocationChanged += (_, _) => SavePosition();
        MouseLeftButtonDown += OnMouseLeftButtonDown;
        SourceInitialized += OnSourceInitialized;

        ApplySettings();
    }

    public bool Locked
    {
        get => _settings.Locked;
        set
        {
            _settings.Locked = value;
            _options.SaveSettings();
        }
    }

    public bool ClickThrough
    {
        get => _settings.ClickThrough;
        set
        {
            _settings.ClickThrough = value;
            ApplySettings();
            _options.SaveSettings();
        }
    }

    public bool AlwaysOnTop
    {
        get => _settings.AlwaysOnTop;
        set
        {
            _settings.AlwaysOnTop = value;
            ApplySettings();
            _options.SaveSettings();
        }
    }

    public void ApplySettings()
    {
        _settings.Normalize();
        SetScaledSize();
        Topmost = _settings.AlwaysOnTop;
        Opacity = _settings.Opacity;
        ClampToVirtualScreen();

        if (_hwnd != nint.Zero)
        {
            NativeWindowStyles.ApplyWidgetStyles(_hwnd, _settings.ClickThrough);
        }
    }

    public void ResetPosition()
    {
        var workArea = SystemParameters.WorkArea;
        Left = Math.Max(workArea.Left + 20, workArea.Right - Width - 48);
        Top = workArea.Top + 80;
        SavePosition();
    }

    public void EnsureVisibleOnAnyMonitor()
    {
        ClampToVirtualScreen();
    }

    public void CloseForShutdown()
    {
        _allowClose = true;
        Close();
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (!_allowClose)
        {
            e.Cancel = true;
            _options.HideWidget();
            return;
        }

        base.OnClosing(e);
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        _hwnd = new WindowInteropHelper(this).Handle;
        NativeWindowStyles.ApplyWidgetStyles(_hwnd, _settings.ClickThrough);
    }

    private void RestorePosition()
    {
        if (_settings.Left is double left &&
            _settings.Top is double top &&
            double.IsFinite(left) &&
            double.IsFinite(top))
        {
            Left = left;
            Top = top;
        }
        else
        {
            ResetPosition();
        }

        EnsureVisibleOnAnyMonitor();
    }

    private void SavePosition()
    {
        if (!IsLoaded || _isDragging || _isApplyingBounds)
        {
            return;
        }

        _settings.Left = Left;
        _settings.Top = Top;
        _options.SaveSettings();
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_settings.Locked || e.ButtonState != MouseButtonState.Pressed)
        {
            return;
        }

        _isDragging = true;
        try
        {
            DragMove();
        }
        catch (InvalidOperationException)
        {
            // DragMove can fail if the button is released before WPF begins the drag.
        }
        finally
        {
            _isDragging = false;
            ClampToVirtualScreen();
            SavePosition();
        }

        e.Handled = true;
    }

    private void SetScaledSize()
    {
        var width = Math.Round(_widget.DefaultSize.Width * _settings.Scale);
        var height = Math.Round(_widget.DefaultSize.Height * _settings.Scale);

        MinWidth = width;
        MinHeight = height;
        MaxWidth = width;
        MaxHeight = height;
        Width = width;
        Height = height;
    }

    private void ClampToVirtualScreen()
    {
        if (_hwnd == nint.Zero)
        {
            return;
        }

        bool changed;
        _isApplyingBounds = true;
        try
        {
            changed = NativeWindowPlacement.ClampToVisibleWorkArea(_hwnd);
        }
        finally
        {
            _isApplyingBounds = false;
        }

        if (changed)
        {
            SavePosition();
        }
    }

    private System.Windows.Controls.ContextMenu BuildContextMenu()
    {
        var lockItem = new MenuItem
        {
            Header = "Lock position",
            IsCheckable = true,
            IsChecked = _settings.Locked
        };
        lockItem.Click += (_, _) =>
        {
            _settings.Locked = lockItem.IsChecked;
            _options.SaveSettings();
        };

        var topmostItem = new MenuItem
        {
            Header = "Always on top",
            IsCheckable = true,
            IsChecked = _settings.AlwaysOnTop
        };
        topmostItem.Click += (_, _) => AlwaysOnTop = topmostItem.IsChecked;

        var clickThroughItem = new MenuItem
        {
            Header = "Click-through mode",
            IsCheckable = true,
            IsChecked = _settings.ClickThrough
        };
        clickThroughItem.Click += (_, _) => ClickThrough = clickThroughItem.IsChecked;

        var resetItem = new MenuItem { Header = "Reset position" };
        resetItem.Click += (_, _) => ResetPosition();

        var startupItem = new MenuItem
        {
            Header = "Start with Windows",
            IsCheckable = true,
            IsChecked = _options.GetStartWithWindows()
        };
        startupItem.Click += (_, _) =>
        {
            _options.SetStartWithWindows(startupItem.IsChecked);
            startupItem.IsChecked = _options.GetStartWithWindows();
        };

        var settingsItem = new MenuItem { Header = "Open settings" };
        settingsItem.Click += (_, _) => _options.OpenSettings();

        var exitItem = new MenuItem { Header = "Exit" };
        exitItem.Click += (_, _) => _options.ExitApplication();

        return new System.Windows.Controls.ContextMenu
        {
            Items =
            {
                lockItem,
                topmostItem,
                clickThroughItem,
                new Separator(),
                resetItem,
                startupItem,
                settingsItem,
                new Separator(),
                exitItem
            }
        };
    }
}
