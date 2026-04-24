using System.Runtime.InteropServices;

namespace VistaWidgets.App.Widgets.CpuMeter;

public sealed class MemoryUsageProvider
{
    public bool TryGetMemoryUsage(out MemoryUsage usage)
    {
        var status = new MemoryStatus
        {
            Length = (uint)Marshal.SizeOf<MemoryStatus>()
        };

        if (!GlobalMemoryStatusEx(ref status) || status.TotalPhysical == 0)
        {
            usage = new MemoryUsage(0, 0, 0);
            return false;
        }

        var used = status.TotalPhysical > status.AvailablePhysical
            ? status.TotalPhysical - status.AvailablePhysical
            : 0;

        usage = new MemoryUsage(
            CalculatePhysicalMemoryLoad(status.TotalPhysical, status.AvailablePhysical),
            used,
            status.TotalPhysical);

        return true;
    }

    public static double CalculatePhysicalMemoryLoad(ulong totalPhysical, ulong availablePhysical)
    {
        if (totalPhysical == 0)
        {
            return 0;
        }

        var used = totalPhysical > availablePhysical ? totalPhysical - availablePhysical : 0;
        return Math.Clamp(used * 100.0 / totalPhysical, 0.0, 100.0);
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatus buffer);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MemoryStatus
    {
        public uint Length;
        public uint MemoryLoad;
        public ulong TotalPhysical;
        public ulong AvailablePhysical;
        public ulong TotalPageFile;
        public ulong AvailablePageFile;
        public ulong TotalVirtual;
        public ulong AvailableVirtual;
        public ulong AvailableExtendedVirtual;
    }
}

public readonly record struct MemoryUsage(double Percentage, ulong UsedBytes, ulong TotalBytes);
