using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using FontFamily = System.Windows.Media.FontFamily;
using Pen = System.Windows.Media.Pen;
using Point = System.Windows.Point;
using Rect = System.Windows.Rect;
using Size = System.Windows.Size;
using UserControl = System.Windows.Controls.UserControl;

namespace Dialkin.App.Widgets.CpuMeter;

public partial class GaugeControl : UserControl
{
    private DrawingGroup? _backgroundDrawing;
    private DrawingGroup? _foregroundDrawing;
    private Size _cachedRenderSize;
    private string _cachedLabel = string.Empty;

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(GaugeControl),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, OnValueChanged, CoercePercentage));

    public static readonly DependencyProperty NeedleAngleProperty =
        DependencyProperty.Register(
            nameof(NeedleAngle),
            typeof(double),
            typeof(GaugeControl),
            new FrameworkPropertyMetadata(GaugeMath.MinimumAngle, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(
            nameof(Label),
            typeof(string),
            typeof(GaugeControl),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ShowPercentProperty =
        DependencyProperty.Register(
            nameof(ShowPercent),
            typeof(bool),
            typeof(GaugeControl),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    public GaugeControl()
    {
        InitializeComponent();
        NeedleAngle = GaugeMath.MapValueToAngle(0);
    }

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double NeedleAngle
    {
        get => (double)GetValue(NeedleAngleProperty);
        set => SetValue(NeedleAngleProperty, value);
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public bool ShowPercent
    {
        get => (bool)GetValue(ShowPercentProperty);
        set => SetValue(ShowPercentProperty, value);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        var size = Math.Min(ActualWidth, ActualHeight);
        if (size <= 0)
        {
            return;
        }

        var dpi = VisualTreeHelper.GetDpi(this);
        var center = new Point(ActualWidth / 2, ActualHeight / 2);
        var radius = size / 2 - Math.Max(4.2, size * 0.043);
        var faceRadius = radius * 0.842;

        EnsureStaticLayers(center, radius, faceRadius);
        drawingContext.DrawDrawing(_backgroundDrawing);
        DrawNeedle(drawingContext, center, faceRadius);
        drawingContext.DrawDrawing(_foregroundDrawing);
        DrawPercentWindow(drawingContext, center, faceRadius, dpi.PixelsPerDip);
    }

    private void EnsureStaticLayers(Point center, double radius, double faceRadius)
    {
        var renderSize = new Size(ActualWidth, ActualHeight);
        if (_backgroundDrawing is not null &&
            _foregroundDrawing is not null &&
            _cachedRenderSize == renderSize &&
            string.Equals(_cachedLabel, Label, StringComparison.Ordinal))
        {
            return;
        }

        var background = new DrawingGroup();
        using (var context = background.Open())
        {
            DrawChromeBezel(context, center, radius);
            DrawFace(context, center, faceRadius);
            DrawInnerBezelShadow(context, center, faceRadius);
            DrawWarningBands(context, center, faceRadius);
            DrawTicks(context, center, faceRadius);
        }

        background.Freeze();

        var foreground = new DrawingGroup();
        using (var context = foreground.Open())
        {
            DrawCenterIcon(context, center, faceRadius);
            DrawGlassHighlight(context, center, radius);
        }

        foreground.Freeze();
        _backgroundDrawing = background;
        _foregroundDrawing = foreground;
        _cachedRenderSize = renderSize;
        _cachedLabel = Label;
    }

    private static object CoercePercentage(DependencyObject d, object baseValue)
    {
        return Math.Clamp(double.IsFinite((double)baseValue) ? (double)baseValue : 0.0, 0.0, 100.0);
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var gauge = (GaugeControl)d;
        var targetAngle = GaugeMath.MapValueToAngle((double)e.NewValue);

        if (!gauge.IsLoaded)
        {
            gauge.NeedleAngle = targetAngle;
            gauge.InvalidateVisual();
            return;
        }

        var animation = new DoubleAnimation
        {
            From = gauge.NeedleAngle,
            To = targetAngle,
            Duration = TimeSpan.FromMilliseconds(180),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut },
            FillBehavior = FillBehavior.HoldEnd
        };

        gauge.BeginAnimation(NeedleAngleProperty, animation, HandoffBehavior.SnapshotAndReplace);
        gauge.InvalidateVisual();
    }

    private static void DrawChromeBezel(DrawingContext dc, Point center, double radius)
    {
        dc.DrawEllipse(
            new SolidColorBrush(Color.FromArgb(62, 0, 0, 0)),
            null,
            new Point(center.X + radius * 0.06, center.Y + radius * 0.07),
            radius * 0.94,
            radius * 0.94);

        var outerChrome = new RadialGradientBrush
        {
            GradientOrigin = new Point(0.30, 0.22),
            Center = new Point(0.52, 0.56),
            RadiusX = 0.82,
            RadiusY = 0.82
        };
        outerChrome.GradientStops.Add(new GradientStop(Color.FromArgb(255, 255, 255, 255), 0.0));
        outerChrome.GradientStops.Add(new GradientStop(Color.FromArgb(255, 246, 250, 251), 0.14));
        outerChrome.GradientStops.Add(new GradientStop(Color.FromArgb(255, 163, 174, 177), 0.40));
        outerChrome.GradientStops.Add(new GradientStop(Color.FromArgb(255, 64, 69, 71), 0.63));
        outerChrome.GradientStops.Add(new GradientStop(Color.FromArgb(255, 232, 237, 238), 0.80));
        outerChrome.GradientStops.Add(new GradientStop(Color.FromArgb(255, 22, 24, 25), 1.0));
        dc.DrawEllipse(outerChrome, new Pen(new SolidColorBrush(Color.FromArgb(245, 10, 11, 11)), Math.Max(0.9, radius * 0.02)), center, radius, radius);

        DrawMetalReflectionBands(dc, center, radius);

        var innerChrome = new LinearGradientBrush(
            Color.FromArgb(255, 252, 254, 254),
            Color.FromArgb(255, 65, 70, 72),
            new Point(0.28, 0.05),
            new Point(0.72, 0.96));
        dc.DrawEllipse(innerChrome, null, center, radius * 0.95, radius * 0.95);

        dc.DrawEllipse(
            null,
            new Pen(new SolidColorBrush(Color.FromArgb(225, 244, 248, 249)), Math.Max(0.8, radius * 0.018)),
            center,
            radius * 0.92,
            radius * 0.92);

        DrawArcStroke(
            dc,
            center,
            radius * 0.955,
            170,
            180,
            new Pen(new SolidColorBrush(Color.FromArgb(215, 24, 27, 29)), Math.Max(0.8, radius * 0.018)));
        DrawArcStroke(
            dc,
            center,
            radius * 0.955,
            -180,
            -170,
            new Pen(new SolidColorBrush(Color.FromArgb(215, 24, 27, 29)), Math.Max(0.8, radius * 0.018)));

        dc.DrawEllipse(
            null,
            new Pen(new SolidColorBrush(Color.FromArgb(190, 18, 21, 22)), Math.Max(0.6, radius * 0.014)),
            center,
            radius * 0.985,
            radius * 0.985);
    }

    private static void DrawMetalReflectionBands(DrawingContext dc, Point center, double radius)
    {
        var outer = radius * 0.985;
        var inner = radius * 0.865;

        DrawArcBand(dc, center, outer, inner, -170, -114, Color.FromArgb(165, 10, 12, 13));
        DrawArcBand(dc, center, outer, inner, -114, -71, Color.FromArgb(178, 202, 212, 215));
        DrawArcBand(dc, center, outer, inner, -71, -7, Color.FromArgb(210, 255, 255, 255));
        DrawArcBand(dc, center, outer, inner, -7, 38, Color.FromArgb(158, 221, 229, 232));
        DrawArcBand(dc, center, outer, inner, 38, 116, Color.FromArgb(188, 35, 39, 41));
        DrawArcBand(dc, center, outer, inner, 116, 170, Color.FromArgb(170, 31, 35, 37));

        dc.DrawEllipse(
            null,
            new Pen(new SolidColorBrush(Color.FromArgb(135, 255, 255, 255)), Math.Max(0.45, radius * 0.01)),
            center,
            radius * 0.78,
            radius * 0.78);
    }

    private static void DrawFace(DrawingContext dc, Point center, double radius)
    {
        var face = new RadialGradientBrush
        {
            GradientOrigin = new Point(0.34, 0.26),
            Center = new Point(0.52, 0.56),
            RadiusX = 0.76,
            RadiusY = 0.76
        };
        face.GradientStops.Add(new GradientStop(Color.FromArgb(255, 255, 255, 255), 0.0));
        face.GradientStops.Add(new GradientStop(Color.FromArgb(255, 254, 253, 248), 0.52));
        face.GradientStops.Add(new GradientStop(Color.FromArgb(255, 232, 231, 225), 0.83));
        face.GradientStops.Add(new GradientStop(Color.FromArgb(255, 176, 177, 173), 1.0));
        dc.DrawEllipse(face, new Pen(new SolidColorBrush(Color.FromArgb(185, 69, 70, 70)), 0.8), center, radius, radius);

        dc.DrawEllipse(
            null,
            new Pen(new SolidColorBrush(Color.FromArgb(120, 255, 255, 255)), 1.0),
            center,
            radius * 0.92,
            radius * 0.92);
    }

    private static void DrawInnerBezelShadow(DrawingContext dc, Point center, double radius)
    {
        dc.DrawEllipse(
            null,
            new Pen(new SolidColorBrush(Color.FromArgb(58, 0, 0, 0)), Math.Max(1.0, radius * 0.048)),
            center,
            radius * 1.01,
            radius * 1.01);

        DrawArcBand(dc, center, radius * 1.03, radius * 0.90, -88, 0, Color.FromArgb(54, 0, 0, 0));
        DrawArcBand(dc, center, radius * 1.02, radius * 0.91, 44, 112, Color.FromArgb(38, 0, 0, 0));

        dc.DrawEllipse(
            null,
            new Pen(new SolidColorBrush(Color.FromArgb(95, 255, 255, 255)), Math.Max(0.55, radius * 0.018)),
            center,
            radius * 0.93,
            radius * 0.93);
    }

    private static void DrawWarningBands(DrawingContext dc, Point center, double radius)
    {
        var warningStart = GaugeMath.MapValueToAngle(70);
        var warningEnd = GaugeMath.MaximumAngle;
        var markerOuterRadius = radius;
        var markerInnerRadius = radius * 0.70;
        var colorOuterRadius = markerOuterRadius;
        var warningInnerRadius = markerInnerRadius;
        var startInnerRadius = markerInnerRadius;
        var warningBrush = new LinearGradientBrush
        {
            StartPoint = new Point(0.58, 0.10),
            EndPoint = new Point(0.94, 0.96)
        };
        warningBrush.GradientStops.Add(new GradientStop(Color.FromArgb(210, 255, 226, 92), 0.00));
        warningBrush.GradientStops.Add(new GradientStop(Color.FromArgb(224, 249, 159, 55), 0.36));
        warningBrush.GradientStops.Add(new GradientStop(Color.FromArgb(236, 238, 82, 43), 0.62));
        warningBrush.GradientStops.Add(new GradientStop(Color.FromArgb(246, 218, 28, 38), 1.00));

        DrawArcBand(dc, center, colorOuterRadius, warningInnerRadius, warningStart, warningEnd, warningBrush);

        DrawArcBand(dc, center, colorOuterRadius, startInnerRadius, GaugeMath.MinimumAngle, GaugeMath.MapValueToAngle(5), Color.FromArgb(205, 196, 37, 41));
    }

    private static void DrawTicks(DrawingContext dc, Point center, double radius)
    {
        var tickBrush = new SolidColorBrush(Color.FromArgb(230, 61, 46, 43));
        var majorPen = new Pen(tickBrush, Math.Max(1.0, radius * 0.034))
        {
            StartLineCap = PenLineCap.Flat,
            EndLineCap = PenLineCap.Flat
        };
        var minorPen = new Pen(tickBrush, Math.Max(0.75, radius * 0.020));

        for (var i = 0; i <= 20; i++)
        {
            var value = i * 5;
            var angle = GaugeMath.MapValueToAngle(value);
            var isMajor = i % 2 == 0;
            var start = PointOnGauge(center, radius * (isMajor ? 0.70 : 0.80), angle);
            var end = PointOnGauge(center, radius, angle);
            dc.DrawLine(isMajor ? majorPen : minorPen, start, end);
        }
    }

    private void DrawNeedle(DrawingContext dc, Point center, double radius)
    {
        var angle = NeedleAngle;
        var needleEnd = PointOnGauge(center, radius * 0.72, angle);
        var needleTail = PointOnGauge(center, radius * 0.13, angle + 180);

        var left = PointOnGauge(center, radius * 0.096, angle - 92);
        var right = PointOnGauge(center, radius * 0.096, angle + 92);

        var needle = new StreamGeometry();
        using (var context = needle.Open())
        {
            context.BeginFigure(needleEnd, isFilled: true, isClosed: true);
            context.LineTo(left, isStroked: true, isSmoothJoin: true);
            context.LineTo(needleTail, isStroked: true, isSmoothJoin: true);
            context.LineTo(right, isStroked: true, isSmoothJoin: true);
        }

        dc.PushTransform(new TranslateTransform(1.0, 1.3));
        dc.DrawGeometry(new SolidColorBrush(Color.FromArgb(120, 0, 0, 0)), null, needle);
        dc.Pop();

        var needleBrush = new LinearGradientBrush(
            Color.FromArgb(255, 255, 47, 38),
            Color.FromArgb(255, 139, 0, 0),
            new Point(0.3, 0.0),
            new Point(0.8, 1.0));
        dc.DrawGeometry(needleBrush, new Pen(new SolidColorBrush(Color.FromArgb(210, 112, 0, 0)), Math.Max(0.45, radius * 0.012)), needle);

        dc.DrawLine(
            new Pen(new SolidColorBrush(Color.FromArgb(95, 255, 171, 156)), Math.Max(0.35, radius * 0.008)),
            PointOnGauge(center, radius * 0.10, angle - 2),
            PointOnGauge(center, radius * 0.60, angle - 2));
    }

    private void DrawCenterIcon(DrawingContext dc, Point center, double radius)
    {
        var bossCenter = new Point(center.X, center.Y - radius * 0.02);
        var bossRadius = radius * 0.23;

        var bossBrush = new RadialGradientBrush
        {
            GradientOrigin = new Point(0.34, 0.20),
            Center = new Point(0.50, 0.55),
            RadiusX = 0.74,
            RadiusY = 0.74
        };
        bossBrush.GradientStops.Add(new GradientStop(Color.FromArgb(255, 255, 255, 255), 0.0));
        bossBrush.GradientStops.Add(new GradientStop(Color.FromArgb(255, 210, 215, 216), 0.45));
        bossBrush.GradientStops.Add(new GradientStop(Color.FromArgb(255, 91, 97, 100), 1.0));
        dc.DrawEllipse(
            bossBrush,
            new Pen(new SolidColorBrush(Color.FromArgb(210, 77, 80, 82)), Math.Max(0.55, radius * 0.016)),
            bossCenter,
            bossRadius,
            bossRadius);
        dc.DrawEllipse(
            null,
            new Pen(new SolidColorBrush(Color.FromArgb(130, 255, 255, 255)), Math.Max(0.35, radius * 0.01)),
            bossCenter,
            bossRadius * 0.72,
            bossRadius * 0.72);

        DrawHubDimpleRing(dc, bossCenter, bossRadius);

        var iconSize = bossRadius * 1.02;
        var iconRect = new Rect(
            bossCenter.X - iconSize / 2,
            bossCenter.Y - iconSize * 0.42,
            iconSize,
            iconSize * 0.88);

        var isRam = Label.Contains("RAM", StringComparison.OrdinalIgnoreCase) ||
                    Label.Contains("MEM", StringComparison.OrdinalIgnoreCase);
        if (isRam)
        {
            DrawMemoryBars(dc, iconRect);
        }
        else
        {
            DrawCpuCore(dc, iconRect);
        }

    }

    private static void DrawHubDimpleRing(DrawingContext dc, Point center, double radius)
    {
        var dotBrush = new SolidColorBrush(Color.FromArgb(115, 77, 83, 85));
        var dotRadius = Math.Max(0.38, radius * 0.026);

        for (var i = 0; i < 26; i++)
        {
            var angle = i * (360.0 / 26.0);
            var point = PointOnGauge(center, radius * 0.90, angle);
            dc.DrawEllipse(dotBrush, null, point, dotRadius, dotRadius);
        }
    }

    private static void DrawCpuCore(DrawingContext dc, Rect iconRect)
    {
        var chipBrush = new LinearGradientBrush(
            Color.FromArgb(255, 236, 239, 240),
            Color.FromArgb(255, 55, 60, 63),
            new Point(0.2, 0.0),
            new Point(0.85, 1.0));
        DrawChipPins(dc, iconRect);

        dc.DrawRoundedRectangle(
            chipBrush,
            new Pen(new SolidColorBrush(Color.FromArgb(235, 38, 41, 43)), Math.Max(0.45, iconRect.Width * 0.045)),
            iconRect,
            0.8,
            0.8);

        var core = InflateByFactor(iconRect, 0.42);
        dc.DrawRoundedRectangle(
            new SolidColorBrush(Color.FromArgb(255, 35, 39, 41)),
            null,
            core,
            0.5,
            0.5);
        dc.DrawRoundedRectangle(
            new SolidColorBrush(Color.FromArgb(170, 230, 238, 240)),
            null,
            InflateByFactor(core, 0.44),
            0.2,
            0.2);
    }

    private static void DrawMemoryBars(DrawingContext dc, Rect iconRect)
    {
        var chipBrush = new LinearGradientBrush(
            Color.FromArgb(255, 230, 234, 236),
            Color.FromArgb(255, 70, 76, 80),
            new Point(0.2, 0.0),
            new Point(0.85, 1.0));
        DrawChipPins(dc, iconRect);

        dc.DrawRoundedRectangle(
            chipBrush,
            new Pen(new SolidColorBrush(Color.FromArgb(235, 38, 41, 43)), Math.Max(0.45, iconRect.Width * 0.045)),
            iconRect,
            0.8,
            0.8);

        var brush = new SolidColorBrush(Color.FromArgb(255, 34, 38, 41));
        var inset = iconRect.Width * 0.24;
        var barWidth = iconRect.Width * 0.115;
        for (var i = 0; i < 3; i++)
        {
            var x = iconRect.Left + inset + i * barWidth * 1.7;
            dc.DrawRoundedRectangle(
                brush,
                null,
                new Rect(x, iconRect.Top + iconRect.Height * 0.24, barWidth, iconRect.Height * 0.52),
                0.4,
                0.4);
        }
    }

    private static void DrawChipPins(DrawingContext dc, Rect iconRect)
    {
        var pinBrush = new SolidColorBrush(Color.FromArgb(210, 238, 242, 243));
        var pinPen = new Pen(new SolidColorBrush(Color.FromArgb(190, 32, 35, 37)), Math.Max(0.25, iconRect.Width * 0.018));
        var pinLength = iconRect.Width * 0.14;
        var pinThickness = Math.Max(0.65, iconRect.Width * 0.052);

        for (var i = 0; i < 4; i++)
        {
            var y = iconRect.Top + iconRect.Height * (0.18 + i * 0.21);
            dc.DrawRectangle(pinBrush, pinPen, new Rect(iconRect.Left - pinLength * 0.74, y, pinLength, pinThickness));
            dc.DrawRectangle(pinBrush, pinPen, new Rect(iconRect.Right - pinLength * 0.26, y, pinLength, pinThickness));
        }
    }

    private void DrawPercentWindow(DrawingContext dc, Point center, double radius, double pixelsPerDip)
    {
        if (!ShowPercent)
        {
            return;
        }

        var text = CreateText($"{Value:0}%", Math.Max(8, radius * 0.268), FontWeights.Bold, Color.FromArgb(255, 255, 255, 255), pixelsPerDip);
        var width = Math.Max(radius * 0.77, text.WidthIncludingTrailingWhitespace + radius * 0.18);
        var height = radius * 0.31;
        var rect = new Rect(center.X - width / 2, center.Y + radius * 0.45, width, height);

        dc.DrawRoundedRectangle(
            new SolidColorBrush(Color.FromArgb(95, 0, 0, 0)),
            null,
            new Rect(rect.Left + 1.0, rect.Top + 1.1, rect.Width, rect.Height),
            1.0,
            1.0);

        dc.DrawRoundedRectangle(
            new SolidColorBrush(Color.FromArgb(244, 16, 15, 15)),
            new Pen(new SolidColorBrush(Color.FromArgb(225, 245, 245, 245)), 0.55),
            rect,
            1.2,
            1.2);

        var textBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
        DrawCenteredText(dc, text, textBrush, rect);
    }

    private static void DrawGlassHighlight(DrawingContext dc, Point center, double radius)
    {
        var highlight = new LinearGradientBrush(
            Color.FromArgb(172, 255, 255, 255),
            Color.FromArgb(0, 255, 255, 255),
            new Point(0.35, 0.0),
            new Point(0.72, 0.8));

        dc.PushOpacity(0.78);
        dc.PushClip(new EllipseGeometry(center, radius * 0.84, radius * 0.84));
        dc.DrawEllipse(
            highlight,
            null,
            new Point(center.X - radius * 0.22, center.Y - radius * 0.30),
            radius * 0.50,
            radius * 0.22);
        dc.Pop();
        dc.Pop();
    }

    private static void DrawArcBand(DrawingContext dc, Point center, double outerRadius, double innerRadius, double startAngle, double endAngle, Color color)
    {
        DrawArcBand(dc, center, outerRadius, innerRadius, startAngle, endAngle, new SolidColorBrush(color));
    }

    private static void DrawArcStroke(DrawingContext dc, Point center, double radius, double startAngle, double endAngle, Pen pen)
    {
        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            var start = PointOnGauge(center, radius, startAngle);
            var end = PointOnGauge(center, radius, endAngle);
            var largeArc = Math.Abs(endAngle - startAngle) > 180;

            context.BeginFigure(start, isFilled: false, isClosed: false);
            context.ArcTo(end, new Size(radius, radius), 0, largeArc, SweepDirection.Clockwise, isStroked: true, isSmoothJoin: true);
        }

        geometry.Freeze();
        dc.DrawGeometry(null, pen, geometry);
    }

    private static void DrawArcBand(DrawingContext dc, Point center, double outerRadius, double innerRadius, double startAngle, double endAngle, Brush brush)
    {
        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            var outerStart = PointOnGauge(center, outerRadius, startAngle);
            var outerEnd = PointOnGauge(center, outerRadius, endAngle);
            var innerEnd = PointOnGauge(center, innerRadius, endAngle);
            var innerStart = PointOnGauge(center, innerRadius, startAngle);
            var largeArc = Math.Abs(endAngle - startAngle) > 180;

            context.BeginFigure(outerStart, isFilled: true, isClosed: true);
            context.ArcTo(outerEnd, new Size(outerRadius, outerRadius), 0, largeArc, SweepDirection.Clockwise, isStroked: true, isSmoothJoin: true);
            context.LineTo(innerEnd, isStroked: true, isSmoothJoin: true);
            context.ArcTo(innerStart, new Size(innerRadius, innerRadius), 0, largeArc, SweepDirection.Counterclockwise, isStroked: true, isSmoothJoin: true);
        }

        geometry.Freeze();
        dc.DrawGeometry(brush, null, geometry);
    }

    private static Rect InflateByFactor(Rect rect, double factor)
    {
        var width = rect.Width * factor;
        var height = rect.Height * factor;
        return new Rect(rect.Left + (rect.Width - width) / 2, rect.Top + (rect.Height - height) / 2, width, height);
    }

    private static FormattedText CreateText(string text, double size, FontWeight weight, Color color, double pixelsPerDip)
    {
        return new FormattedText(
            text,
            CultureInfo.InvariantCulture,
            System.Windows.FlowDirection.LeftToRight,
            new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, weight, FontStretches.Normal),
            size,
            new SolidColorBrush(color),
            pixelsPerDip)
        {
            TextAlignment = TextAlignment.Center
        };
    }

    private static void DrawCenteredText(DrawingContext dc, FormattedText text, Brush brush, Rect rect)
    {
        var geometry = text.BuildGeometry(new Point(0, 0));
        var bounds = geometry.Bounds;
        var x = rect.Left + (rect.Width - bounds.Width) / 2 - bounds.Left;
        var y = rect.Top + (rect.Height - bounds.Height) / 2 - bounds.Top;

        dc.PushTransform(new TranslateTransform(x, y));
        dc.DrawGeometry(brush, null, geometry);
        dc.Pop();
    }

    private static Point PointOnGauge(Point center, double radius, double angle)
    {
        var radians = angle * Math.PI / 180.0;
        return new Point(
            center.X + Math.Sin(radians) * radius,
            center.Y - Math.Cos(radians) * radius);
    }
}
