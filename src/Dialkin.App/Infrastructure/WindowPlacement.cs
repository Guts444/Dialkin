using System.Windows;

namespace Dialkin.App.Infrastructure;

public static class WindowPlacement
{
    public static Rect ClampToVisibleWorkArea(Rect windowBounds, IReadOnlyList<Rect> workAreas)
    {
        if (workAreas.Count == 0)
        {
            return windowBounds;
        }

        var target = workAreas
            .OrderByDescending(area => IntersectionArea(windowBounds, area))
            .ThenBy(area => CenterDistanceSquared(windowBounds, area))
            .First();

        var maxLeft = Math.Max(target.Left, target.Right - windowBounds.Width);
        var maxTop = Math.Max(target.Top, target.Bottom - windowBounds.Height);

        return new Rect(
            Math.Clamp(windowBounds.Left, target.Left, maxLeft),
            Math.Clamp(windowBounds.Top, target.Top, maxTop),
            windowBounds.Width,
            windowBounds.Height);
    }

    private static double IntersectionArea(Rect first, Rect second)
    {
        var intersection = Rect.Intersect(first, second);
        return intersection.IsEmpty ? 0 : intersection.Width * intersection.Height;
    }

    private static double CenterDistanceSquared(Rect first, Rect second)
    {
        var deltaX = first.Left + first.Width / 2 - (second.Left + second.Width / 2);
        var deltaY = first.Top + first.Height / 2 - (second.Top + second.Height / 2);
        return deltaX * deltaX + deltaY * deltaY;
    }
}
