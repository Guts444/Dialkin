using System.Runtime.InteropServices;

namespace Dialkin.App.Infrastructure;

public static class NativeWindowStyles
{
    private const int GwlExStyle = -20;
    private const int SwpNoMove = 0x0002;
    private const int SwpNoSize = 0x0001;
    private const int SwpNoZOrder = 0x0004;
    private const int SwpFrameChanged = 0x0020;

    private const long WsExAppWindow = 0x00040000L;
    private const long WsExToolWindow = 0x00000080L;
    private const long WsExTransparent = 0x00000020L;

    public static void ApplyWidgetStyles(nint hwnd, bool clickThrough)
    {
        if (hwnd == nint.Zero)
        {
            return;
        }

        var style = GetWindowLongPtr(hwnd, GwlExStyle).ToInt64();
        style |= WsExToolWindow;
        style &= ~WsExAppWindow;

        if (clickThrough)
        {
            style |= WsExTransparent;
        }
        else
        {
            style &= ~WsExTransparent;
        }

        SetWindowLongPtr(hwnd, GwlExStyle, new nint(style));
        SetWindowPos(hwnd, nint.Zero, 0, 0, 0, 0, SwpNoMove | SwpNoSize | SwpNoZOrder | SwpFrameChanged);
    }

    private static nint GetWindowLongPtr(nint hwnd, int index)
    {
        return nint.Size == 8
            ? GetWindowLongPtr64(hwnd, index)
            : new nint(GetWindowLong32(hwnd, index));
    }

    private static nint SetWindowLongPtr(nint hwnd, int index, nint value)
    {
        return nint.Size == 8
            ? SetWindowLongPtr64(hwnd, index, value)
            : new nint(SetWindowLong32(hwnd, index, value.ToInt32()));
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
    private static extern nint GetWindowLongPtr64(nint hwnd, int index);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    private static extern nint SetWindowLongPtr64(nint hwnd, int index, nint value);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongW", SetLastError = true)]
    private static extern int GetWindowLong32(nint hwnd, int index);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongW", SetLastError = true)]
    private static extern int SetWindowLong32(nint hwnd, int index, int value);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(nint hwnd, nint hwndInsertAfter, int x, int y, int cx, int cy, int flags);
}
