using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Color = System.Windows.Media.Color;
using FontFamily = System.Windows.Media.FontFamily;
using Pen = System.Windows.Media.Pen;
using Point = System.Windows.Point;
using UserControl = System.Windows.Controls.UserControl;

namespace VistaWidgets.App.Widgets.CpuMeter;

public partial class GaugeControl : UserControl
{
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
        var radius = size / 2 - 3;
        var faceRadius = radius * 0.88;

        DrawFace(drawingContext, center, radius, faceRadius);
        DrawTicks(drawingContext, center, faceRadius);
        DrawText(drawingContext, center, faceRadius, dpi.PixelsPerDip);
        DrawNeedle(drawingContext, center, faceRadius);
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
            Duration = TimeSpan.FromMilliseconds(320),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut },
            FillBehavior = FillBehavior.HoldEnd
        };

        gauge.BeginAnimation(NeedleAngleProperty, animation, HandoffBehavior.SnapshotAndReplace);
        gauge.InvalidateVisual();
    }

    private static void DrawFace(DrawingContext dc, Point center, double radius, double faceRadius)
    {
        var outerBrush = new LinearGradientBrush(
            Color.FromArgb(245, 32, 38, 45),
            Color.FromArgb(245, 6, 9, 12),
            new Point(0.25, 0),
            new Point(0.75, 1));
        var outerPen = new Pen(new SolidColorBrush(Color.FromArgb(220, 124, 146, 160)), 1.0);
        dc.DrawEllipse(outerBrush, outerPen, center, radius, radius);

        var faceBrush = new RadialGradientBrush
        {
            GradientOrigin = new Point(0.35, 0.28),
            Center = new Point(0.5, 0.55),
            RadiusX = 0.72,
            RadiusY = 0.72
        };
        faceBrush.GradientStops.Add(new GradientStop(Color.FromArgb(255, 58, 67, 74), 0));
        faceBrush.GradientStops.Add(new GradientStop(Color.FromArgb(255, 20, 25, 30), 0.54));
        faceBrush.GradientStops.Add(new GradientStop(Color.FromArgb(255, 2, 4, 7), 1));
        dc.DrawEllipse(faceBrush, new Pen(new SolidColorBrush(Color.FromArgb(190, 0, 0, 0)), 1.0), center, faceRadius, faceRadius);

        var highlightBrush = new LinearGradientBrush(
            Color.FromArgb(120, 255, 255, 255),
            Color.FromArgb(0, 255, 255, 255),
            new Point(0.5, 0),
            new Point(0.5, 1));
        dc.PushOpacity(0.72);
        dc.DrawEllipse(highlightBrush, null, new Point(center.X - faceRadius * 0.18, center.Y - faceRadius * 0.35), faceRadius * 0.52, faceRadius * 0.24);
        dc.Pop();
    }

    private static void DrawTicks(DrawingContext dc, Point center, double radius)
    {
        for (var i = 0; i <= 10; i++)
        {
            var value = i * 10;
            var angle = GaugeMath.MapValueToAngle(value);
            var start = PointOnGauge(center, radius * 0.74, angle);
            var end = PointOnGauge(center, radius * 0.91, angle);
            var pen = i % 5 == 0
                ? new Pen(new SolidColorBrush(Color.FromArgb(230, 235, 244, 247)), 1.6)
                : new Pen(new SolidColorBrush(Color.FromArgb(185, 171, 192, 198)), 1.0);
            dc.DrawLine(pen, start, end);
        }
    }

    private void DrawText(DrawingContext dc, Point center, double radius, double pixelsPerDip)
    {
        var label = CreateText(Label, Math.Max(9, radius * 0.26), FontWeights.SemiBold, Color.FromArgb(245, 239, 247, 249), pixelsPerDip);
        dc.DrawText(label, new Point(center.X - label.Width / 2, center.Y + radius * 0.24));

        if (ShowPercent)
        {
            var valueText = CreateText($"{Value:0}%", Math.Max(9, radius * 0.24), FontWeights.Bold, Color.FromArgb(245, 145, 229, 255), pixelsPerDip);
            dc.DrawText(valueText, new Point(center.X - valueText.Width / 2, center.Y + radius * 0.48));
        }
    }

    private void DrawNeedle(DrawingContext dc, Point center, double radius)
    {
        var angle = NeedleAngle;
        var needleEnd = PointOnGauge(center, radius * 0.66, angle);
        var needleTail = PointOnGauge(center, radius * 0.17, angle + 180);

        var shadowPen = new Pen(new SolidColorBrush(Color.FromArgb(150, 0, 0, 0)), Math.Max(2.8, radius * 0.065))
        {
            StartLineCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round
        };
        dc.DrawLine(shadowPen, new Point(needleTail.X + 1.2, needleTail.Y + 1.2), new Point(needleEnd.X + 1.2, needleEnd.Y + 1.2));

        var needlePen = new Pen(new SolidColorBrush(Color.FromArgb(255, 245, 65, 53)), Math.Max(2.2, radius * 0.055))
        {
            StartLineCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Triangle
        };
        dc.DrawLine(needlePen, needleTail, needleEnd);

        dc.DrawEllipse(new SolidColorBrush(Color.FromArgb(255, 222, 230, 235)), new Pen(new SolidColorBrush(Color.FromArgb(210, 30, 35, 38)), 1), center, radius * 0.09, radius * 0.09);
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

    private static Point PointOnGauge(Point center, double radius, double angle)
    {
        var radians = angle * Math.PI / 180.0;
        return new Point(
            center.X + Math.Sin(radians) * radius,
            center.Y - Math.Cos(radians) * radius);
    }
}
