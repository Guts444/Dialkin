using System.IO;

namespace Dialkin.App.Infrastructure;

public static class AppPaths
{
    public static string AppDataDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dialkin");

    public static string SettingsPath
    {
        get
        {
            var roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            MigrateLegacySettings(Path.Combine(roaming, "VistaWidgets"), AppDataDirectory);
            return Path.Combine(AppDataDirectory, "settings.json");
        }
    }

    public static void MigrateLegacySettings(string legacyDirectory, string targetDirectory)
    {
        var targetSettings = Path.Combine(targetDirectory, "settings.json");
        if (File.Exists(targetSettings))
        {
            return;
        }

        var legacySettings = Path.Combine(legacyDirectory, "settings.json");
        if (!File.Exists(legacySettings))
        {
            return;
        }

        try
        {
            Directory.CreateDirectory(targetDirectory);
            File.Copy(legacySettings, targetSettings, overwrite: false);

            var legacyBackup = legacySettings + ".bak";
            if (File.Exists(legacyBackup))
            {
                File.Copy(legacyBackup, targetSettings + ".bak", overwrite: false);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            // Migration is best-effort; a fresh settings file remains a safe fallback.
        }
    }
}
