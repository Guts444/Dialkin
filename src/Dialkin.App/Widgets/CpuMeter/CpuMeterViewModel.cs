using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using Dialkin.App.WidgetHost;

namespace Dialkin.App.Widgets.CpuMeter;

public sealed class CpuMeterViewModel : INotifyPropertyChanged
{
    private readonly WidgetSettings _settings;
    private readonly CpuUsageProvider _cpuProvider;
    private readonly MemoryUsageProvider _memoryProvider;
    private readonly DispatcherTimer _timer;

    private double _cpuUsage;
    private double _memoryUsage;
    private ulong _memoryUsedBytes;
    private ulong _memoryTotalBytes;
    private string _debugTooltip = "CPU: 0%\nRAM: 0%";

    public CpuMeterViewModel(WidgetSettings settings)
        : this(settings, new CpuUsageProvider(), new MemoryUsageProvider())
    {
    }

    public CpuMeterViewModel(WidgetSettings settings, CpuUsageProvider cpuProvider, MemoryUsageProvider memoryProvider)
    {
        _settings = settings;
        _cpuProvider = cpuProvider;
        _memoryProvider = memoryProvider;
        _timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(Math.Clamp(settings.UpdateIntervalSeconds, 1, 60))
        };
        _timer.Tick += (_, _) => Sample();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public double CpuUsage
    {
        get => _cpuUsage;
        private set => SetField(ref _cpuUsage, Math.Clamp(value, 0, 100));
    }

    public double MemoryUsage
    {
        get => _memoryUsage;
        private set => SetField(ref _memoryUsage, Math.Clamp(value, 0, 100));
    }

    public ulong MemoryUsedBytes
    {
        get => _memoryUsedBytes;
        private set => SetField(ref _memoryUsedBytes, value);
    }

    public ulong MemoryTotalBytes
    {
        get => _memoryTotalBytes;
        private set => SetField(ref _memoryTotalBytes, value);
    }

    public string DebugTooltip
    {
        get => _debugTooltip;
        private set => SetField(ref _debugTooltip, value);
    }

    public void Start()
    {
        ApplySettings();
        Sample();
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
    }

    public void ApplySettings()
    {
        _settings.Normalize();
        _timer.Interval = TimeSpan.FromSeconds(_settings.UpdateIntervalSeconds);
    }

    public void ResetCpuBaseline()
    {
        _cpuProvider.Reset();
        Sample();
    }

    private void Sample()
    {
        if (_cpuProvider.TryGetUsage(out var cpu))
        {
            CpuUsage = cpu;
        }

        if (_memoryProvider.TryGetMemoryUsage(out var memory))
        {
            MemoryUsage = memory.Percentage;
            MemoryUsedBytes = memory.UsedBytes;
            MemoryTotalBytes = memory.TotalBytes;
        }

        DebugTooltip = $"CPU: {CpuUsage:0}%\nRAM: {MemoryUsage:0}% ({FormatBytes(MemoryUsedBytes)} / {FormatBytes(MemoryTotalBytes)})";
    }

    private static string FormatBytes(ulong bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        var value = (double)bytes;
        var unit = 0;

        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return unit == 0 ? $"{value:0} {units[unit]}" : $"{value:0.0} {units[unit]}";
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
