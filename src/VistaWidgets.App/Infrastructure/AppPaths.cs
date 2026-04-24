using System.IO;

namespace VistaWidgets.App.Infrastructure;

public static class AppPaths
{
    public static string AppDataDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VistaWidgets");

    public static string SettingsPath => Path.Combine(AppDataDirectory, "settings.json");
}
