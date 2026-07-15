using System.IO;
using System.Text;
using System.Text.Json;
using Dialkin.App.WidgetHost;

namespace Dialkin.App.Infrastructure;

public sealed class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly string _settingsPath;

    public SettingsService()
        : this(AppPaths.SettingsPath)
    {
    }

    public SettingsService(string settingsPath)
    {
        _settingsPath = settingsPath;
    }

    public AppSettings Load()
    {
        var primaryExists = File.Exists(_settingsPath);
        if (TryLoad(_settingsPath, out var settings))
        {
            return settings;
        }

        if (TryLoad(BackupPath, out settings))
        {
            QuarantineCorruptPrimary();
            return settings;
        }

        if (primaryExists)
        {
            QuarantineCorruptPrimary();
        }

        return AppSettings.CreateDefault();
    }

    public bool Save(AppSettings settings)
    {
        var directory = Path.GetDirectoryName(_settingsPath);
        if (string.IsNullOrWhiteSpace(directory))
        {
            return false;
        }

        var tempPath = Path.Combine(directory, $".{Path.GetFileName(_settingsPath)}.{Guid.NewGuid():N}.tmp");

        try
        {
            Directory.CreateDirectory(directory);
            var json = JsonSerializer.Serialize(settings.Normalize(), JsonOptions);

            using (var stream = new FileStream(
                       tempPath,
                       FileMode.CreateNew,
                       FileAccess.Write,
                       FileShare.None,
                       bufferSize: 4096,
                       FileOptions.WriteThrough))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
            {
                writer.Write(json);
                writer.Flush();
                stream.Flush(flushToDisk: true);
            }

            if (File.Exists(_settingsPath))
            {
                File.Replace(tempPath, _settingsPath, BackupPath, ignoreMetadataErrors: true);
            }
            else
            {
                File.Move(tempPath, _settingsPath);
            }

            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException or JsonException)
        {
            return false;
        }
        finally
        {
            try
            {
                File.Delete(tempPath);
            }
            catch
            {
                // A failed cleanup must not take down the widget.
            }
        }
    }

    private string BackupPath => _settingsPath + ".bak";

    private void QuarantineCorruptPrimary()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                return;
            }

            var quarantinePath = $"{_settingsPath}.corrupt-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
            File.Move(_settingsPath, quarantinePath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            // Recovery can still continue from the backup if quarantine is unavailable.
        }
    }

    private static bool TryLoad(string path, out AppSettings settings)
    {
        settings = AppSettings.CreateDefault();

        try
        {
            if (!File.Exists(path))
            {
                return false;
            }

            var json = File.ReadAllText(path);
            var loaded = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
            if (loaded is null)
            {
                return false;
            }

            if (loaded.Widgets?.Values.Any(widgetSettings => widgetSettings is null) == true)
            {
                return false;
            }

            settings = loaded.Normalize();
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or NotSupportedException)
        {
            return false;
        }
    }
}
