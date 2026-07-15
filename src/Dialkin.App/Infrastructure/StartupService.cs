using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Win32;
using Windows.ApplicationModel;

namespace Dialkin.App.Infrastructure;

public sealed class StartupService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string RunValueName = "Dialkin";
    private const string LegacyRunValueName = "VistaWidgets";
    private const string PackagedStartupTaskId = "Dialkin.Startup";

    public bool IsEnabled()
    {
        if (IsPackaged())
        {
            return GetPackagedStartupTask()?.State == StartupTaskState.Enabled;
        }

        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
            if (key is null)
            {
                return false;
            }

            var command = key.GetValue(RunValueName) as string;
            if (command is null && key.GetValue(LegacyRunValueName) is string)
            {
                command = $"\"{GetExecutablePath()}\"";
                key.SetValue(RunValueName, command);
                key.DeleteValue(LegacyRunValueName, throwOnMissingValue: false);
            }

            if (command is null)
            {
                return false;
            }

            var configuredPath = ExtractExecutablePath(Environment.ExpandEnvironmentVariables(command));
            return string.Equals(configuredPath, GetExecutablePath(), StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or SecurityException or IOException)
        {
            return false;
        }
    }

    public bool SetEnabled(bool enabled)
    {
        if (IsPackaged())
        {
            return SetPackagedStartupEnabled(enabled);
        }

        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true);
            if (key is null)
            {
                return false;
            }

            if (enabled)
            {
                key.SetValue(RunValueName, $"\"{GetExecutablePath()}\"");
                key.DeleteValue(LegacyRunValueName, throwOnMissingValue: false);
            }
            else
            {
                key.DeleteValue(RunValueName, throwOnMissingValue: false);
                key.DeleteValue(LegacyRunValueName, throwOnMissingValue: false);
            }

            return IsEnabled() == enabled;
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or SecurityException or IOException)
        {
            return false;
        }
    }

    internal static string ExtractExecutablePath(string command)
    {
        var trimmed = command.Trim();
        if (trimmed.StartsWith('"'))
        {
            var closingQuote = trimmed.IndexOf('"', 1);
            return closingQuote > 1 ? trimmed[1..closingQuote] : trimmed.Trim('"');
        }

        var firstSpace = trimmed.IndexOf(' ');
        return firstSpace > 0 ? trimmed[..firstSpace] : trimmed;
    }

    private static bool SetPackagedStartupEnabled(bool enabled)
    {
        try
        {
            var task = GetPackagedStartupTask();
            if (task is null)
            {
                return false;
            }

            if (enabled)
            {
                var state = task.RequestEnableAsync().AsTask().GetAwaiter().GetResult();
                return state == StartupTaskState.Enabled;
            }

            task.Disable();
            return task.State != StartupTaskState.Enabled;
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or InvalidOperationException or COMException)
        {
            return false;
        }
    }

    private static StartupTask? GetPackagedStartupTask()
    {
        try
        {
            return StartupTask.GetAsync(PackagedStartupTaskId).AsTask().GetAwaiter().GetResult();
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or InvalidOperationException or COMException)
        {
            return null;
        }
    }

    private static bool IsPackaged()
    {
        try
        {
            _ = Package.Current.Id;
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        catch (COMException)
        {
            return false;
        }
    }

    private static string GetExecutablePath()
    {
        var processPath = Environment.ProcessPath;
        if (!string.IsNullOrWhiteSpace(processPath) &&
            !processPath.EndsWith("dotnet.exe", StringComparison.OrdinalIgnoreCase))
        {
            return processPath;
        }

        var assemblyPath = Assembly.GetEntryAssembly()?.Location;
        if (!string.IsNullOrWhiteSpace(assemblyPath))
        {
            var exePath = Path.ChangeExtension(assemblyPath, ".exe");
            if (File.Exists(exePath))
            {
                return exePath;
            }
        }

        return Process.GetCurrentProcess().MainModule?.FileName ?? "Dialkin.App.exe";
    }
}
