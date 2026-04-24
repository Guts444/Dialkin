using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using VistaWidgets.App.Infrastructure;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;

namespace VistaWidgets.App.WidgetHost;

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
    private Point _dragStartScreen;
    private Point _dragStartWindow;
    private double _dragDpiScale = 1.0;

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
        MouseMove += OnMouseMove;
        MouseLeftButtonUp += OnMouseLeftButtonUp;
        LostMouseCapture += (_, _) => _isDragging = false;
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
            Hide();
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
        _dragStartScreen = PointToScreen(e.GetPosition(this));
        _dragStartWindow = new Point(Left, Top);
        _dragDpiScale = Math.Max(0.1, DpiHelper.GetScale(this));
        CaptureMouse();
        e.Handled = true;
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging || e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        var currentScreen = PointToScreen(e.GetPosition(this));
        var left = _dragStartWindow.X + (currentScreen.X - _dragStartScreen.X) / _dragDpiScale;
        var top = _dragStartWindow.Y + (currentScreen.Y - _dragStartScreen.Y) / _dragDpiScale;
        var clamped = ClampPointToVirtualScreen(left, top);

        Left = clamped.X;
        Top = clamped.Y;
        e.Handled = true;
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDragging)
        {
            return;
        }

        _isDragging = false;
        ReleaseMouseCapture();
        SavePosition();
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
        var clamped = ClampPointToVirtualScreen(Left, Top);
        if (Math.Abs(clamped.X - Left) < 0.01 && Math.Abs(clamped.Y - Top) < 0.01)
        {
            return;
        }

        _isApplyingBounds = true;
        Left = clamped.X;
        Top = clamped.Y;
        _isApplyingBounds = false;
        SavePosition();
    }

    private Point ClampPointToVirtualScreen(double left, double top)
    {
        var bounds = DpiHelper.GetVirtualScreenDipBounds();
        if (!double.IsFinite(left))
        {
            left = Math.Max(bounds.Left, bounds.Right - Width - 48);
        }

        if (!double.IsFinite(top))
        {
            top = bounds.Top + 80;
        }

        var maxLeft = Math.Max(bounds.Left, bounds.Right - Width);
        var maxTop = Math.Max(bounds.Top, bounds.Bottom - Height);

        return new Point(
            Math.Clamp(left, bounds.Left, maxLeft),
            Math.Clamp(top, bounds.Top, maxTop));
    }

    private ContextMenu BuildContextMenu()
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
        startupItem.Click += (_, _) => _options.SetStartWithWindows(startupItem.IsChecked);

        var settingsItem = new MenuItem { Header = "Open settings" };
        settingsItem.Click += (_, _) => _options.OpenSettings();

        var exitItem = new MenuItem { Header = "Exit" };
        exitItem.Click += (_, _) => _options.ExitApplication();

        return new ContextMenu
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
