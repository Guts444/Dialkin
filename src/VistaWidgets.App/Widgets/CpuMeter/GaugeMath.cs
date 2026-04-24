namespace VistaWidgets.App.Widgets.CpuMeter;

public static class GaugeMath
{
    public const double MinimumAngle = -125.0;
    public const double MaximumAngle = 125.0;

    public static double MapValueToAngle(double value)
    {
        var clamped = Math.Clamp(double.IsFinite(value) ? value : 0.0, 0.0, 100.0);
        return MinimumAngle + (MaximumAngle - MinimumAngle) * (clamped / 100.0);
    }
}
