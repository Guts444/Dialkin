using System.Windows;
using System.Windows.Media;

namespace VistaWidgets.App.Infrastructure;

public static class DpiHelper
{
    public static DpiScale GetDpi(Visual visual) => VisualTreeHelper.GetDpi(visual);

    public static double GetScale(Visual visual) => GetDpi(visual).DpiScaleX;

    public static Rect GetVirtualScreenDipBounds()
    {
        return new Rect(
            SystemParameters.VirtualScreenLeft,
            SystemParameters.VirtualScreenTop,
            SystemParameters.VirtualScreenWidth,
            SystemParameters.VirtualScreenHeight);
    }
}
