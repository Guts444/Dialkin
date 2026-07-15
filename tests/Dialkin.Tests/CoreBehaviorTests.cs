using Dialkin.App.Infrastructure;
using Dialkin.App.WidgetHost;
using Dialkin.App.Widgets.CpuMeter;
using Xunit;

namespace Dialkin.Tests;

public sealed class CoreBehaviorTests
{
    [Theory]
    [InlineData(0, GaugeMath.MinimumAngle)]
    [InlineData(50, 0)]
    [InlineData(100, GaugeMath.MaximumAngle)]
    public void GaugeMapsExpectedAngles(double value, double expected)
    {
        Assert.Equal(expected, GaugeMath.MapValueToAngle(value), 4);
    }

    [Theory]
    [InlineData(-40, GaugeMath.MinimumAngle)]
    [InlineData(400, GaugeMath.MaximumAngle)]
    public void GaugeClampsOutOfRangeValues(double value, double expected)
    {
        Assert.Equal(expected, GaugeMath.MapValueToAngle(value), 4);
    }

    [Fact]
    public void GaugeTreatsNaNAsZero()
    {
        Assert.Equal(GaugeMath.MinimumAngle, GaugeMath.MapValueToAngle(double.NaN), 4);
    }

    [Theory]
    [InlineData(100, 75, 25)]
    [InlineData(0, 0, 0)]
    [InlineData(100, 150, 0)]
    public void MemoryPercentageIsStable(ulong total, ulong available, double expected)
    {
        Assert.Equal(expected, MemoryUsageProvider.CalculatePhysicalMemoryLoad(total, available), 4);
    }

    [Fact]
    public void SettingsRoundTrip()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "Dialkin.Tests", Guid.NewGuid().ToString("N"));
        var path = Path.Combine(tempDir, "settings.json");

        try
        {
            var service = new SettingsService(path);
            var settings = AppSettings.CreateDefault();
            settings.StartWithWindows = true;
            settings.Widgets["cpu-meter"] = new WidgetSettings
            {
                Left = 123,
                Top = 456,
                Locked = true,
                Opacity = 0.75,
                Scale = 1.35,
                AlwaysOnTop = true,
                ClickThrough = true,
                UpdateIntervalSeconds = 5
            };

            service.Save(settings);
            var loaded = service.Load();
            var widget = loaded.Widgets["cpu-meter"];

            Assert.True(loaded.StartWithWindows);
            Assert.Equal(123, widget.Left);
            Assert.Equal(456, widget.Top);
            Assert.True(widget.Locked);
            Assert.Equal(0.75, widget.Opacity, 4);
            Assert.Equal(1.35, widget.Scale, 4);
            Assert.True(widget.AlwaysOnTop);
            Assert.True(widget.ClickThrough);
            Assert.Equal(5, widget.UpdateIntervalSeconds);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Fact]
    public void AppSettingsSynchronizesGlobalLockStateFromWidgets()
    {
        var settings = AppSettings.CreateDefault();
        settings.Widgets["cpu"] = new WidgetSettings { Locked = true };
        settings.Widgets["clock"] = new WidgetSettings { Locked = true };

        settings.SynchronizeLockState();
        Assert.True(settings.AllWidgetsLocked);

        settings.Widgets["clock"].Locked = false;
        settings.SynchronizeLockState();
        Assert.False(settings.AllWidgetsLocked);
    }

    [Fact]
    public void CpuCalculationUsesFallbackForRegressingSamples()
    {
        var previous = new CpuTimes(Idle: 200, Kernel: 600, User: 400);
        var current = new CpuTimes(Idle: 150, Kernel: 500, User: 350);

        Assert.Equal(42, CpuUsageProvider.CalculateUsagePercentage(previous, current, fallback: 42), 4);
    }

    [Fact]
    public void CpuCalculationComputesBusyPercentage()
    {
        var previous = new CpuTimes(Idle: 100, Kernel: 600, User: 400);
        var current = new CpuTimes(Idle: 150, Kernel: 700, User: 500);

        Assert.Equal(75, CpuUsageProvider.CalculateUsagePercentage(previous, current), 4);
    }
}
