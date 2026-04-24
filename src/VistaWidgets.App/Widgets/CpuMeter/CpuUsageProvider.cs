using System.Runtime.InteropServices;

namespace VistaWidgets.App.Widgets.CpuMeter;

public sealed class CpuUsageProvider
{
    private CpuTimes? _previousTimes;
    private double _lastPercentage;

    public bool TryGetUsage(out double percentage)
    {
        percentage = _lastPercentage;

        if (!TryReadSystemTimes(out var currentTimes))
        {
            percentage = 0;
            _lastPercentage = 0;
            _previousTimes = null;
            return false;
        }

        if (_previousTimes is not { } previousTimes)
        {
            _previousTimes = currentTimes;
            percentage = 0;
            _lastPercentage = 0;
            return true;
        }

        percentage = CalculateUsagePercentage(previousTimes, currentTimes, _lastPercentage);
        _previousTimes = currentTimes;
        _lastPercentage = percentage;
        return true;
    }

    public void Reset()
    {
        _previousTimes = null;
        _lastPercentage = 0;
    }

    public static double CalculateUsagePercentage(CpuTimes previous, CpuTimes current, double fallback = 0)
    {
        var previousSystem = previous.Kernel + previous.User;
        var currentSystem = current.Kernel + current.User;

        if (currentSystem <= previousSystem || current.Idle < previous.Idle)
        {
            return ClampPercentage(fallback);
        }

        var systemDelta = currentSystem - previousSystem;
        var idleDelta = current.Idle - previous.Idle;

        if (systemDelta == 0 || idleDelta > systemDelta)
        {
            return ClampPercentage(fallback);
        }

        var busyDelta = systemDelta - idleDelta;
        return ClampPercentage(busyDelta * 100.0 / systemDelta);
    }

    private static bool TryReadSystemTimes(out CpuTimes times)
    {
        times = default;

        if (!GetSystemTimes(out var idleTime, out var kernelTime, out var userTime))
        {
            return false;
        }

        times = new CpuTimes(idleTime.ToUInt64(), kernelTime.ToUInt64(), userTime.ToUInt64());
        return true;
    }

    private static double ClampPercentage(double value)
    {
        return Math.Clamp(double.IsFinite(value) ? value : 0.0, 0.0, 100.0);
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetSystemTimes(out FileTime idleTime, out FileTime kernelTime, out FileTime userTime);

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct FileTime
    {
        private readonly uint _lowDateTime;
        private readonly uint _highDateTime;

        public ulong ToUInt64() => ((ulong)_highDateTime << 32) | _lowDateTime;
    }
}

public readonly record struct CpuTimes(ulong Idle, ulong Kernel, ulong User);
