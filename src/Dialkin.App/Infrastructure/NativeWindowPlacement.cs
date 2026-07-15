using System.Runtime.InteropServices;
using System.Windows;
using Forms = System.Windows.Forms;

namespace Dialkin.App.Infrastructure;

public static class NativeWindowPlacement
{
    private const uint SwpNoSize = 0x0001;
    private const uint SwpNoZOrder = 0x0004;
    private const uint SwpNoActivate = 0x0010;

    public static bool ClampToVisibleWorkArea(nint hwnd)
    {
        if (hwnd == nint.Zero || !GetWindowRect(hwnd, out var nativeBounds))
        {
            return false;
        }

        var bounds = new Rect(
            nativeBounds.Left,
            nativeBounds.Top,
            nativeBounds.Right - nativeBounds.Left,
            nativeBounds.Bottom - nativeBounds.Top);
        var workAreas = Forms.Screen.AllScreens
            .Select(screen => new Rect(
                screen.WorkingArea.Left,
                screen.WorkingArea.Top,
                screen.WorkingArea.Width,
                screen.WorkingArea.Height))
            .ToArray();
        var clamped = WindowPlacement.ClampToVisibleWorkArea(bounds, workAreas);

        if (Math.Abs(clamped.Left - bounds.Left) < 0.5 &&
            Math.Abs(clamped.Top - bounds.Top) < 0.5)
        {
            return false;
        }

        return SetWindowPos(
            hwnd,
            nint.Zero,
            (int)Math.Round(clamped.Left),
            (int)Math.Round(clamped.Top),
            0,
            0,
            SwpNoSize | SwpNoZOrder | SwpNoActivate);
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(nint hwnd, out NativeRect rectangle);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(
        nint hwnd,
        nint insertAfter,
        int x,
        int y,
        int width,
        int height,
        uint flags);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
