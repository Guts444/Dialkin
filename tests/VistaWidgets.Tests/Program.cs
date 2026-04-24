using VistaWidgets.App.Infrastructure;
using VistaWidgets.App.WidgetHost;
using VistaWidgets.App.Widgets.CpuMeter;

var tests = new (string Name, Action Test)[]
{
    ("Gauge maps 0/50/100 to the expected sweep", GaugeMapsExpectedAngles),
    ("Gauge clamps invalid values", GaugeClampsValues),
    ("Memory percentage calculation is stable", MemoryCalculationIsStable),
    ("Settings save/load round-trips JSON", SettingsRoundTrips),
    ("CPU usage calculation clamps bad samples", CpuCalculationHandlesBadSamples)
};

var failures = 0;
foreach (var (name, test) in tests)
{
    try
    {
        test();
        Console.WriteLine($"PASS {name}");
    }
    catch (Exception ex)
    {
        failures++;
        Console.Error.WriteLine($"FAIL {name}: {ex.Message}");
    }
}

return failures == 0 ? 0 : 1;

static void GaugeMapsExpectedAngles()
{
    AssertNear(GaugeMath.MinimumAngle, GaugeMath.MapValueToAngle(0));
    AssertNear(0, GaugeMath.MapValueToAngle(50));
    AssertNear(GaugeMath.MaximumAngle, GaugeMath.MapValueToAngle(100));
}

static void GaugeClampsValues()
{
    AssertNear(GaugeMath.MinimumAngle, GaugeMath.MapValueToAngle(-40));
    AssertNear(GaugeMath.MaximumAngle, GaugeMath.MapValueToAngle(400));
    AssertNear(GaugeMath.MinimumAngle, GaugeMath.MapValueToAngle(double.NaN));
}

static void MemoryCalculationIsStable()
{
    AssertNear(25, MemoryUsageProvider.CalculatePhysicalMemoryLoad(100, 75));
    AssertNear(0, MemoryUsageProvider.CalculatePhysicalMemoryLoad(0, 0));
    AssertNear(0, MemoryUsageProvider.CalculatePhysicalMemoryLoad(100, 150));
}

static void SettingsRoundTrips()
{
    var tempDir = Path.Combine(Path.GetTempPath(), "VistaWidgets.Tests", Guid.NewGuid().ToString("N"));
    var path = Path.Combine(tempDir, "settings.json");
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

    AssertTrue(loaded.StartWithWindows, "StartWithWindows should round-trip.");
    AssertNear(123, widget.Left ?? 0);
    AssertNear(456, widget.Top ?? 0);
    AssertTrue(widget.Locked, "Locked should round-trip.");
    AssertNear(0.75, widget.Opacity);
    AssertNear(1.35, widget.Scale);
    AssertTrue(widget.AlwaysOnTop, "AlwaysOnTop should round-trip.");
    AssertTrue(widget.ClickThrough, "ClickThrough should round-trip.");
    AssertTrue(widget.UpdateIntervalSeconds == 5, "UpdateIntervalSeconds should round-trip.");

    Directory.Delete(tempDir, recursive: true);
}

static void CpuCalculationHandlesBadSamples()
{
    var previous = new CpuTimes(Idle: 200, Kernel: 600, User: 400);
    var current = new CpuTimes(Idle: 150, Kernel: 500, User: 350);
    AssertNear(42, CpuUsageProvider.CalculateUsagePercentage(previous, current, fallback: 42));

    previous = new CpuTimes(Idle: 100, Kernel: 600, User: 400);
    current = new CpuTimes(Idle: 150, Kernel: 700, User: 500);
    AssertNear(75, CpuUsageProvider.CalculateUsagePercentage(previous, current));
}

static void AssertNear(double expected, double actual, double tolerance = 0.0001)
{
    if (Math.Abs(expected - actual) > tolerance)
    {
        throw new InvalidOperationException($"Expected {expected}, got {actual}.");
    }
}

static void AssertTrue(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}
