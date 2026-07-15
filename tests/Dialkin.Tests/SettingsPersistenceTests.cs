using System.Text.Json;
using Dialkin.App.Infrastructure;
using Dialkin.App.WidgetHost;
using Xunit;

namespace Dialkin.Tests;

public sealed class SettingsPersistenceTests
{
    [Fact]
    public void SecondSaveKeepsPreviousSettingsAsBackup()
    {
        using var temp = new TemporaryDirectory();
        var path = Path.Combine(temp.Path, "settings.json");
        var service = new SettingsService(path);

        var first = AppSettings.CreateDefault();
        first.StartWithWindows = false;
        service.Save(first);

        var second = AppSettings.CreateDefault();
        second.StartWithWindows = true;
        service.Save(second);

        var backupPath = path + ".bak";
        Assert.True(File.Exists(backupPath));
        var backup = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(backupPath));
        Assert.NotNull(backup);
        Assert.False(backup.StartWithWindows);
    }

    [Fact]
    public void LoadFallsBackToBackupWhenPrimarySettingsAreCorrupt()
    {
        using var temp = new TemporaryDirectory();
        var path = Path.Combine(temp.Path, "settings.json");
        var service = new SettingsService(path);

        var first = AppSettings.CreateDefault();
        first.StartWithWindows = true;
        service.Save(first);
        service.Save(AppSettings.CreateDefault());
        File.WriteAllText(path, "{ definitely not valid json");

        var loaded = service.Load();

        Assert.True(loaded.StartWithWindows);
        Assert.False(File.Exists(path));
        Assert.True(File.Exists(path + ".bak"));
    }

    [Fact]
    public void LoadFallsBackToBackupWhenWidgetSettingsAreNull()
    {
        using var temp = new TemporaryDirectory();
        var path = Path.Combine(temp.Path, "settings.json");
        var service = new SettingsService(path);

        var valid = AppSettings.CreateDefault();
        valid.StartWithWindows = true;
        service.Save(valid);
        service.Save(AppSettings.CreateDefault());
        File.WriteAllText(path, "{\"Widgets\":{\"cpu-meter\":null}}");

        var loaded = service.Load();

        Assert.True(loaded.StartWithWindows);
        Assert.False(File.Exists(path));
        Assert.True(File.Exists(path + ".bak"));
    }

    [Fact]
    public void LoadQuarantinesCorruptPrimaryWithoutBackup()
    {
        using var temp = new TemporaryDirectory();
        var path = Path.Combine(temp.Path, "settings.json");
        File.WriteAllText(path, "{ invalid json");

        var loaded = new SettingsService(path).Load();

        Assert.NotNull(loaded);
        Assert.False(File.Exists(path));
        Assert.Single(Directory.GetFiles(temp.Path, "settings.json.corrupt-*"));
    }

    [Fact]
    public void LegacySettingsAreCopiedIntoNewBrandDirectory()
    {
        using var temp = new TemporaryDirectory();
        var legacy = Path.Combine(temp.Path, "VistaWidgets");
        var current = Path.Combine(temp.Path, "Dialkin");
        Directory.CreateDirectory(legacy);
        File.WriteAllText(Path.Combine(legacy, "settings.json"), "{\"StartWithWindows\":true}");

        AppPaths.MigrateLegacySettings(legacy, current);

        Assert.True(File.Exists(Path.Combine(current, "settings.json")));
        Assert.True(JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(Path.Combine(current, "settings.json")))?.StartWithWindows);
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Dialkin.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
